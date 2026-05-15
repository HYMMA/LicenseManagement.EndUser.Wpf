# Webapp fixes that address `audit2026-05-15.md`

**Date:** 2026-05-15
**Webapp branch:** `audit/fix` (worktree at `C:\Users\Ashra\source\repos\audit-fix`)
**Scope:** Server-side changes made in the `LicenseManagement` webapp in response to findings in the WPF wrapper audit.

The WPF wrapper audit is overwhelmingly UI-local (MVVM hygiene, XAML resource duplication, RelayCommand bugs, `OnPropertyChanged` misses, etc.). Almost nothing requires a webapp change. The two places where the wrapper's audit indirectly surfaces webapp work are below.

---

## Findings indirectly addressed in the webapp

| Audit finding | Severity | Webapp change |
|---|---|---|
| `external-integrations review` — "No retry policy / no timeout configuration is observable at this layer" + "ApiException broadly caught with no differentiation". The WPF wrapper depends on the SDK + webapp to surface differentiated error types. | High | The webapp now returns **RFC 7807 problem+json** on every non-2xx, with stable `detail / status / correlationId`. Once the SDK reads these, the WPF VMs can switch on `ApiException.Detail` / `StatusCode` for differentiated UX without text matching. Wired in `LicenseManagement/Program.cs:30-35` and `LicenseManagement/Utilities/AppConfig/ServiceProviders.cs:50-65`. |
| `external-integrations review` — "No observability — none of the catch blocks log anything; intermittent server errors cannot be diagnosed." | High | Every API response now carries an **`X-Correlation-Id` header**. The WPF wrapper should log this in its catch blocks so support can pin a customer-reported failure to a specific request. `LicenseManagement/Utilities/AppConfig/SecurityMiddleware.cs:104-118`. |
| `external-integrations review` — "Double-submit on Register: `RelayCommand.CanExecute` doesn't disable while a request is in flight." Server-side mitigation: idempotency. | Medium | New **`Idempotency-Key` middleware** (`LicenseManagement/Utilities/AppConfig/IdempotencyMiddleware.cs`). The SDK should send a deterministic key on `POST /api/license` and `POST /api/computer`; the webapp caches and replays the response for 24 h. Even if the user double-clicks Register, the server creates the resource exactly once. |

---

## Findings the webapp cannot fix from its side

Everything else in this audit is local to the WPF library or the sample app:

- `MainWindow.xaml.cs:70` null reference on `ps.AllKeys`
- `RelayCommand.CanExecuteChanged` never wired to `CommandManager.RequerySuggested`
- `ProductViewModel.Id` / `Name` setters missing `OnPropertyChanged`
- `LicenseStatusConverter` zoo (seven near-identical switches)
- `Window1.xaml` scaffolding leftover
- `ApiKey` exposed on bindable VM (data-context inspectable)
- `AppResources.xaml` duplicated across all five Views
- Asset path / pack URI in `ErrorView.xaml`
- All packaging / CI / version-drift findings
- Sample-app credential placeholders

These all belong in a wrapper-side PR.

---

## Suggested wrapper companion changes

When migrating the wrapper to consume the updated webapp:

1. **Catch SDK exceptions by type.** The SDK already exposes `LicenseExpiredException`, `ReceiptExpiredException`, `ComputerOfflineException`, `ApiException`. The webapp now provides distinct status codes + `detail` in problem+json. Use both: SDK exception type → UX category, problem `detail` → message.
2. **Read and log `X-Correlation-Id`** in every catch block; include it in the `ErrorView` so users can quote it to support.
3. **Disable Register/Unregister buttons via `RaiseCanExecuteChanged`** *and* send `Idempotency-Key = ReceiptCode + MacAddress` so a double-click can never burn a seat.
4. **Respect `Retry-After`** if the SDK returns it on 429.

---

## Build status

- `dotnet build LicenseManagement/LicenseManagement.csproj` — **clean** (0 errors).
- `dotnet test WebApi.Test/WebApi.Test.csproj` — **48/48 passing**.
