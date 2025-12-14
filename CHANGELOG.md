# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.2] - 2024-12-15

### Changed
- Bundle all dependencies in NuGet package for single-folder deployment compatibility
- Include Hymma.Lm.EndUser.dll, HttpClientFactory.dll, Newtonsoft.Json.dll, DeviceId.*.dll and system dependencies
- Fixed packages.config to reference all bundled dependencies for CI build

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
