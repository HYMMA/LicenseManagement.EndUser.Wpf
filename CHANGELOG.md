# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
