# Senior .NET Audit — LicenseManagement.EndUser.Wpf net8 target (work of 2026-06-16)

## Scope

This audit covers commit range `576cd82..HEAD` — a single commit `d9713d4` ("Add net8.0-windows target via sibling project (net481 unchanged)"). The change adds a sibling SDK-style WPF project under `net8/` that *links* the same XAML and `.cs` sources as the original net4.8.1 project to emit a `net8.0-windows7.0` assembly into the same NuGet package, without altering the net481 assembly consumed by the SolidWorks add-in. Reviewed artifacts: `net8/LicenseManagement.EndUser.Wpf.Net8.csproj`, `net8/Resources.cs`, `nugetSpec.nuspec`, `.github/workflows/publish.yml`, and `CHANGELOG.md`. Four lenses were applied (clean-code, build/packaging correctness, WPF resource-resolution, security + exception-leakage). The stale `fix-audit/` snapshot directory was excluded as it is not part of this change. Verification focused on whether dropping `Settings.cs` and the `.resx` is safe, whether the hand-written `Resources.cs` strings match the originals, whether linked XAML pack-URIs resolve at runtime, strong-naming, and net481 behavioral parity.

## Findings

### Critical

**`.github/workflows/publish.yml:59,74` — CI builds net481 against the wrong SDK version; `nuget pack` will fail.**
The net481 path runs `nuget install LicenseManagement.EndUser -Version 2.0.1` (line 59) and builds with `/p:LicenseEndUserVersion=2.0.1` (line 74), so only `2.0.1` is ever downloaded. But `nugetSpec.nuspec:37` hard-references the loose file `packages\LicenseManagement.EndUser.3.0.2\lib\net481\LicenseManagement.EndUser.dll`, which is never restored. `nuget pack` therefore fails on a missing file; if the file did exist, the package would ship a `2.0.1`-built assembly while declaring a different dependency. This predates the diff but sits on lines directly adjacent to the new net8 build step and blocks the release this commit is meant to produce. **Fix:** set both line 59 and line 74 to the source-of-truth version (`3.0.2`), or align all three (install / build / nuspec bundle) to one intended version. *(Also flagged by the security lens at Medium; the packaging trace confirms it is release-blocking.)*

### High

**`nugetSpec.nuspec:23` + `net8/LicenseManagement.EndUser.Wpf.Net8.csproj:36` — core-SDK version divergence across one package version.**
The net8 asset is built against `LicenseManagement.EndUser` `3.1.0` while the net481 asset targets `3.0.2` (per `packages.config` and the original csproj). A single published package version thus ships two assemblies compiled against different feature versions of the core SDK, undocumented in release notes. **Fix:** align both TFMs to the same core SDK version, or explicitly document the per-TFM divergence in `CHANGELOG.md` / `releaseNotes` and confirm 3.0.2 and 3.1.0 are behaviorally compatible for the shared views.

**`net8/Resources.cs:6` and `net8/LicenseManagement.EndUser.Wpf.Net8.csproj:17` — false justification comment.**
Both comments claim the `.resx` "mixes these with unused System.Drawing-typed template entries (Color1/Bitmap1/Icon1)" that net8 resgen/BinaryFormatter can't process. Those names appear **only** in the ResX schema's boilerplate header comment (`Resources.resx:21,22,25`), not as real `<data>` entries — the data section holds exactly the two string entries and zero Drawing/binary entries. The stated reason for hand-writing `Resources.cs` is factually wrong and will mislead the next maintainer. **Fix:** correct the comment to the real rationale (e.g. "inlined the two strings to drop the `.resx`/`.Designer` pair and avoid resx tooling on net8"); dropping the resx is still defensible, only the explanation is inaccurate.

### Medium

**`nugetSpec.nuspec:5,11,23` — inconsistent version metadata.**
`<version>` is `2.2.0`, `releaseNotes` still describes "2.1.0 ... 3.0.1", and the net8 dependency is `3.1.0`. CI rewrites `<version>` from the tag but does not touch `releaseNotes`, so shipped notes will be stale. **Fix:** update `releaseNotes` to the actual shipped version and its real dependency (`3.1.0`).

### Low

**`net8/Resources.cs:10,12-13` — `public` members on an `internal` type.**
Members are declared `public` inside an `internal static class`, so effective accessibility is `internal` and the `public` is misleading; the original `Resources.Designer.cs` exposed them as `internal`. **Fix:** change both members to `internal static string` to match intent and the original.

**`net8/LicenseManagement.EndUser.Wpf.Net8.csproj:77` (and original `LicenseManagement.EndUser.Wpf.csproj:142`) — `padlock.ico` packaged but unreferenced.**
No XAML `Icon=` attribute or code references `padlock.ico`; it is a dead resource. Pre-existing and identical in the net481 project, so not a net8 regression. **Fix:** remove the `<Resource>` entry from both csproj files, or wire `Icon="pack://.../Assets/padlock.ico"` on the `Window` elements.

**`.github/workflows/publish.yml:121` — release action pinned to a floating major tag.**
`softprops/action-gh-release@v1` is mutable. **Fix:** pin to a commit SHA for supply-chain reproducibility (`dotnet-version: 8.0.x` floating patch is acceptable).

### Nit

**`CHANGELOG.md` / `nugetSpec.nuspec` — `Properties.Settings` referenced only in prose.** The CHANGELOG and csproj comment mention dropping `Properties.Settings`; this is accurate (no production code uses it) but is the only place the name now appears. No action required.

## Dismissed false-positives

- **Culture regression from inlining strings.** Moving from `ResourceManager`-backed properties to string literals loses `CurrentUICulture` lookup, but **no satellite `Resources.*.resx` files exist** (no localization), and `ReceiptCodeRule` formats with `CurrentCulture` independently. Theoretical only — not a defect today; revisit only if localization is added.
- **`Settings.cs` drop unsafe.** Verified safe — `Properties.Settings` / `Settings.Default` is referenced by no production code (only the CHANGELOG and the csproj comment).
- **Resource string values drifted.** Verified byte-identical to `Resources.resx:117-122`, including the `{0}` placeholder.
- **pack-URI / resource-key resolution on net8.** Verified clean. `AssemblyName` is identical (`LicenseManagement.EndUser.Wpf`), pack URIs resolve by assembly name, and every `Link=` logical path matches the `component/<path>` each URI expects (`AppResources.xaml` at root, `Assets/*` under `Assets\`). All `{StaticResource}` / `{DynamicResource}` keys used by the views (`license`, `error`, keyed Thicknesses and styles) exist in `AppResources.xaml`.
- **Strong-naming / `GenerateAssemblyInfo` / item completeness.** The net8 project reuses the same `Enduser.wpf.snk` (same public key token, full-sign), `GenerateAssemblyInfo=false` plus the linked `AssemblyInfo.cs` yields a single `AssemblyVersion("2.0.2.0")` with no double-definition, and under `EnableDefaultItems=false` the Compile/Page/Resource list exactly covers the original minus the correctly-dropped `Settings.cs` and `Resources.Designer.cs` plus the new `Resources.cs`. `System.Configuration.ConfigurationManager 9.0.17` is genuinely needed (`LicenseConfigurationLoader` uses `ConfigurationManager.AppSettings`).
- **`dotnet build` vs `dotnet publish` for net8.** Correct — transitive DLLs come via the dependency group, not the package; Release emits a `.pdb` so the nuspec `.pdb` reference is satisfied. Implicit restore covers the SDK-style project and the packages.config `nuget restore` does not interfere.
- **Security surface.** No BinaryFormatter or unsafe deserialization introduced; no secrets/credentials in any changed file; `NUGET_API_KEY` passed only via `-ApiKey`, `GITHUB_TOKEN` only via `env:`, never echoed; resource strings are now always non-null, eliminating a prior theoretical `ArgumentNullException` path in `ReceiptCodeRule`.

## Overall assessment

The architectural approach is sound and low-risk: a sibling project that links the existing XAML and sources is the right way to add a `net8.0-windows` asset while leaving the SolidWorks-consumed net481 assembly untouched, and the WPF resource-resolution, strong-naming, item-list completeness, and null-safety all check out. The change is **not ship-ready as-is**, however, because the publish workflow still builds and references mismatched core-SDK versions (`2.0.1` install/build vs `3.0.2` bundled vs `3.1.0` net8 dependency), which will fail `nuget pack` or produce a mislabeled package — fix that before tagging a release. Beyond the pipeline, resolve the net481-vs-net8 core-SDK version divergence (or document it deliberately) and correct the inaccurate justification comments in `Resources.cs` / `.csproj`, which currently describe resx contents that do not exist. The remaining items are low-severity cleanup. Once the version alignment is fixed, this is a clean, well-scoped addition.