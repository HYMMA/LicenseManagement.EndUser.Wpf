# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.3.4] - 2026-06-26

First public release since 2.3.0 — fixes the empty-window regression and several UI issues
from the grouped-cards redesign.

### Fixed
- **`MainWindow` rendered empty when `License` was assigned in code.** The window hosted
  `LicenseControl` with a bare `License='{Binding}'`, which resolved against the control's
  own self-bound `DataContext` (stuck at the `LicenseConfigurationLoader.TryLoad()` default),
  so a `window.License` set in code never reached the control. It now binds directly to the
  window's `License`. (The sample app was unaffected because it populates products via the
  default value / App.config rather than assigning `License`.)
- **Cards no longer sit at a fixed 332px with dead space on the right.** The Bands and
  Switcher layouts lay cards out in a stretch-to-fill grid and the content cap was widened, so
  cards grow with the window.
- **Buttons were inert** — added real hover and pressed states to the primary, ghost and link
  button styles (previously only a faint hover and no pressed feedback).
- **Window could freeze briefly on open.** Reading a licence runs on the UI thread and, for
  non-valid states, reaches the licence server; reading every card synchronously on open left
  the window painted but unresponsive for a beat (see the async-loading change below).
- **Validity meter could read ~0% with weeks left.** A `default(DateTime)` period-start date
  from the model was used as a real window start, making the window span ~2000 years. Such
  dates are now treated as unknown, falling back to the publisher's `ValidDays`.

### Changed
- **Cards load asynchronously with a per-card spinner.** Each not-yet-loaded card is read in
  the background (off the UI thread, one at a time so reads never race on the licence file)
  while showing a "Checking license…" spinner, so the window paints immediately. The per-card
  Check / Refresh action and the post-Register / post-Unregister refresh use the same path. A
  host that has already validated its products can seed the cards directly via
  `ProductViewModel.UpdateLicenseSnapshot(model, validDays)` so they appear instantly.
- **Clearer per-status labels.** `Unknown` → **"Couldn't verify"**, `Expired` → **"File
  Expired"**, `ReceiptExpired` → **"Subscription Ended"**, `ReceiptUnregistered` → **"Computer
  Unregistered"** (with the matching meta line).

### Added
- `ProductViewModel.IsLoading` and `LicenseViewModel.LoadAllProducts(...)` /
  `RefreshProduct(...)` for the async, spinner-driven loading; an `LmSpinner` style.

## [2.3.0] - 2026-06-25

### Added
- **Grouped product cards.** The license window now renders each product as a card instead
  of a dropdown, arranged into developer-defined groups (e.g. Monthly / Annual). Configure
  in code with `LicenseViewModel.ApplyGrouping(IEnumerable<ProductGroupDefinition>, ProductLayout)`,
  or zero-code via the App.config `<ProductGroups>` section + `licenseLayout` app setting.
- **Three selectable layouts** via the new `ProductLayout` enum — `Bands` (default, stacked
  bands), `Lanes` (side-by-side columns), `Switcher` (segmented tabs). The developer picks
  one; the end user cannot change it.
- **Per-card status, validity meter and actions.** Each card shows its own status chip, a
  validity meter, the relevant expiry/countdown, and the status-appropriate action
  (Register / Unregister / Download new file). New public types: `ProductLayout`,
  `ProductGroupDefinition`, `ProductGroupViewModel`.

### Changed
- **Validity meter is now period-accurate.** The bar reflects the real window for the
  status — subscription term (`Receipt.Created → Receipt.Expires`), trial window
  (`License.Created → TrialEndDate`), or issued file validity (`License.Updated → Expires`),
  falling back to the publisher's `ValidDays` only when no period start is known. Previously
  it used the product's DB-creation date, which was not a meaningful window.
- **`MainWindow` is now a thin host of `LicenseControl`**, so the standalone-window and
  embedded experiences are identical. The public `MainWindow.License` / `LicenseControl.License`
  API and `LicenseViewModel.FromContext(...)` are unchanged.
- **Register / Unregister / Error dialogs restyled** to match the card UI (bindings,
  commands and receipt-code validation unchanged).

### Fixed
- A license check that returns `Unknown` (e.g. it threw) now shows a distinct **"Unverified"**
  card state with a retry, instead of being conflated with the never-checked **"Not checked"**
  state — a failed check no longer renders as a blank/unloaded card.

### Notes
- The cards load each product's status lazily (the first on open, the rest on demand) to
  avoid a burst of server calls. No core SDK or server change is required for grouping — it
  is purely a presentation concern over the product ids you already configure.

## [2.2.1] - 2026-06-17

### Fixed
- **Aligned the core `LicenseManagement.EndUser` version across the whole build.** The
  net481 assembly was compiled in CI against 2.0.1 while the package bundled 3.0.2 and
  the net8 asset depended on 3.1.0 — three different versions, risking a runtime
  `MissingMethodException` for net481 consumers (a constructor signature differs between
  2.x and 3.x). Everything now uses **3.1.1**: the CI install/build version, the bundled
  net481 DLL, and the net8 dependency. Also picks up the net8 DNS-staleness and
  HTTPS-guard fixes from EndUser 3.1.1.

## [2.2.0] - 2026-06-16

### Added
- **net8.0-windows target.** The package now ships `lib/net8.0-windows7.0` alongside the
  existing `lib/net481`, so .NET 8 WPF hosts (e.g. the CadShift for Inventor add-in) can
  reuse the shared license Window / LicenseControl — including the products ComboBox that
  binds to the developer-supplied product list. Built by a sibling project (`net8/`) that
  links the same XAML + sources; the net481 assembly is unchanged. On net8 the core SDK
  and `System.Configuration.ConfigurationManager` come in as package dependencies (bundled
  as loose files only for net481), and the unused `Properties.Settings` / System.Drawing
  resx template entries are dropped.

## [2.1.1] - 2026-05-15

### Changed
- Bumped `LicenseManagement.EndUser` dependency from 3.0.1 to 3.0.2. The 3.0.1 uninstall
  disk-read approach had two bugs: deleted file = stuck seat, shared file = wrong computer
  unregistered. 3.0.2 restores the original server chain (identity always from hardware).

## [2.1.0] - 2026-05-15

### Changed
- Bumped `LicenseManagement.EndUser` dependency from 2.0.1 to 3.0.1 to pick up tamper-detection
  fix in `LicenseSignatureValidationHandler` and the uninstall disk-read fix.

### Added
- Unit test project (`Tests/LicenseManagement.EndUser.Wpf.Tests`) covering `LicenseViewModel` and
  `ProductViewModel` property change notifications. Run with `dotnet test` after building the
  library with MSBuild (XAML compilation requires the full MSBuild WPF targets).

## [2.0.2] - 2026-05-15

### Fixed
- **MVVM correctness**: `RelayCommand.CanExecuteChanged` now forwards to `CommandManager.RequerySuggested` so command bindings refresh automatically. `RelayCommand` accepts a null `canExecute` predicate and exposes `RaiseCanExecuteChanged()`.
- **Property change notifications**: `ProductViewModel.Id` and `ProductViewModel.Name` setters now raise `OnPropertyChanged` (previously the setters silently dropped change notifications).
- **Null-reference safety**: `MainWindow` / `LicenseControl` no longer throw when the host has app settings but no `<Products>` section; `ConfigurationErrorsException` during type initialization is caught and logged rather than corrupting the type.
- **UTC handling**: `UtcToLocalTimeConverter` now treats `DateTimeKind.Unspecified` values as UTC before converting (fixes double-conversion when XML serialisation strips the kind).
- **MVVM cleanup**: `RegisterLicenseViewModel` no longer reaches into the View's named `TextBox`; the receipt code is bound via `ReceiptCode` two-way.

### Changed
- **Security**: `ApiKey` is no longer a publicly-bindable property — it is set via `LicenseViewModel.SetApiKey(...)` and held in a non-browsable internal field so binding inspectors (Snoop, Live Visual Tree) cannot read it from the DataContext.
- **Idempotency**: Register / Unregister / RenewLicense view models gained an `IsBusy` flag that disables the command while a server call is in flight, preventing double-submission from a double-click.
- **Observability**: SDK exceptions are now caught by specific type (`ApiException`, `ReceiptCodeException`, `ProductNameException`, `ComputerOfflineException`, etc.) and logged via `Trace` before being surfaced to the user. Generic `catch (Exception)` swallowing was removed.
- **XAML**: Implicit `StackPanel` / `Button` / `TextBox` / `Label` styles in `AppResources.xaml` are now keyed so that merging the dictionary into a host application no longer overrides every control's style. The seven `LicenseStatusTitles` switch-based converters were consolidated into a single dictionary-driven `LicenseStatusConverter` selected via `ConverterParameter`. Duplicate 60-line `Window.Resources` blocks across five views were replaced with a merged dictionary reference.
- **Build**: SDK dependency bumped to `LicenseManagement.EndUser 2.0.1`. The version is now centralised in an MSBuild property (`LicenseEndUserVersion`) used by `csproj`, `nuspec`, and the publish workflow.
- **Sample app**: Hard-coded credentials in `LicenseService.cs` replaced with `ConfigurationManager` lookups against a new `App.config`. Fixed `InValidTrial` typo and broken namespace reference (`LicenseManagement.EndUser.Wpf.Views.MainWindow` → `LicenseManagement.EndUser.Wpf.MainWindow`).

### Removed
- Dead scaffolding files (`Window1.xaml`, `Window1.xaml.cs`, empty `ErrorViewModel`, unused `BooleanToVisibilityConverter` and `ProductNameToIdConverter`).
- Duplicate `padlock.ico` from repo root (kept the copy under `Assets/`).
- Cached state on `ProductNameToIdConverter` (entire converter dropped).

## [2.0.1] - 2026-02-05

### Fixed
- Updated sample app to handle server-provided trial end dates safely.

## [2.0.0] - 2024-12-18

### Changed
- **BREAKING**: Renamed package from `Hymma.Lm.EndUser.Wpf` to `LicenseManagement.EndUser.Wpf`
- **BREAKING**: Renamed namespace from `Hymma.Lm.EndUser.Wpf` to `LicenseManagement.EndUser.Wpf`
- **BREAKING**: Renamed assembly from `Hymma.Lm.EndUser.Wpf.dll` to `LicenseManagement.EndUser.Wpf.dll`
- Updated dependency to `LicenseManagement.EndUser` 2.0.0

### Added
- `LicenseControl` - New embeddable UserControl for integrating license management directly into application UI as an alternative to the standalone `MainWindow` dialog

### Migration
- Update NuGet package reference from `Hymma.Lm.EndUser.Wpf` to `LicenseManagement.EndUser.Wpf`
- Update C# namespaces: `using Hymma.Lm.EndUser.Wpf.*` to `using LicenseManagement.EndUser.Wpf.*`
- Update XAML namespaces: `assembly=Hymma.Lm.EndUser.Wpf` to `assembly=LicenseManagement.EndUser.Wpf`
- Update ResourceDictionary references in App.xaml

## [1.3.3] - 2024-12-15

### Changed
- Bundle all dependencies in NuGet package for single-folder deployment compatibility
- Include LicenseManagement.EndUser.dll, HttpClientFactory.dll, Newtonsoft.Json.dll, DeviceId.*.dll and system dependencies
- Fixed packages.config and csproj HintPath to reference LicenseManagement.EndUser v1.2.0 for CI build

## [1.3.0] - 2024-12-14

### Changed
- Bumped version to align with assembly version 1.3.0.0
- Updated dependency to Hymma.Lm.EndUser 1.2.0+

## [1.0.0] - 2024-12-11

### Added

- Initial public release of LicenseManagement.EndUser.Wpf
- `MainWindow` - Main license management interface
- `RegisterLicenseView` - Receipt code entry dialog
- `UnregisterView` - Computer unregistration confirmation
- `ErrorView` - Error display with details
- ViewModels with full MVVM support
  - `LicenseViewModel`
  - `ProductViewModel`
  - `RegisterLicenseViewModel`
  - `UnregisterViewModel`
  - `ErrorViewModel`
  - `BaseViewModel`
- Value Converters
  - `LicenseStatusConverter`
  - `BooleanToVisibilityConverter`
  - `UtcToLocalTimeConverter`
- `RelayCommand` for MVVM command binding
- `ReceiptCodeRule` for input validation
- `AppResources.xaml` with default styles
- Asset resources (icons, images)
