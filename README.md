# LicenseManagement.EndUser.Wpf

[![Build and Test](https://github.com/HYMMA/LicenseManagement.EndUser.Wpf/actions/workflows/build.yml/badge.svg)](https://github.com/HYMMA/LicenseManagement.EndUser.Wpf/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Hymma.Lm.EndUser.Wpf.svg)](https://www.nuget.org/packages/Hymma.Lm.EndUser.Wpf)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Hymma.Lm.EndUser.Wpf.svg)](https://www.nuget.org/packages/Hymma.Lm.EndUser.Wpf)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

WPF UI components for [license-management.com](https://license-management.com) end-user SDK.

This library provides ready-to-use WPF views and view models for license registration, validation display, and unregistration workflows.

> [!IMPORTANT]
> **Account Required**: This library requires a publisher account at [license-management.com](https://license-management.com).
>
> **Free with Dev Subscription**: A developer subscription is available at no cost, which provides full access to all features for development and testing purposes.

## Features

- **Pre-built Views** - Ready-to-use WPF windows for common license operations
- **MVVM Architecture** - Clean separation with ViewModels and Commands
- **Value Converters** - License status to UI element converters
- **Validation Rules** - Input validation for receipt codes
- **Customizable** - Use as-is or extend with your own styles

## Installation

```bash
dotnet add package Hymma.Lm.EndUser.Wpf
```

Or via NuGet Package Manager:
```
Install-Package Hymma.Lm.EndUser.Wpf
```

## Components

### Views

| View | Description |
|------|-------------|
| `MainWindow` | Main license management window |
| `RegisterLicenseView` | Product key/receipt code entry |
| `UnregisterView` | Computer unregistration confirmation |
| `ErrorView` | Error display with details |

### ViewModels

| ViewModel | Description |
|-----------|-------------|
| `LicenseViewModel` | License status and details |
| `ProductViewModel` | Product information display |
| `RegisterLicenseViewModel` | Registration workflow logic |
| `UnregisterViewModel` | Unregistration workflow logic |
| `ErrorViewModel` | Error handling and display |
| `BaseViewModel` | Base class with INotifyPropertyChanged |

### Converters

| Converter | Description |
|-----------|-------------|
| `LicenseStatusConverter` | Converts LicenseStatusTitles to display strings |
| `BooleanToVisibilityConverter` | Standard bool to Visibility converter |
| `UtcToLocalTimeConverter` | Converts UTC DateTime to local time |

### Commands

| Command | Description |
|---------|-------------|
| `RelayCommand` | ICommand implementation for MVVM |

### Validation Rules

| Rule | Description |
|------|-------------|
| `ReceiptCodeRule` | Validates receipt code format |

## Quick Start

### Show Registration Dialog

```csharp
using LicenseManagement.EndUser.Wpf.Views;
using LicenseManagement.EndUser.Wpf.ViewModels;

var viewModel = new RegisterLicenseViewModel();
var view = new RegisterLicenseView
{
    DataContext = viewModel
};

if (view.ShowDialog() == true)
{
    string receiptCode = viewModel.ReceiptCode;
    // Process the receipt code
}
```

### Display License Status

```csharp
using LicenseManagement.EndUser.Wpf.ViewModels;

var licenseVm = new LicenseViewModel
{
    Status = LicenseStatusTitles.Valid,
    ExpirationDate = DateTime.UtcNow.AddMonths(6),
    ProductName = "My Product"
};
```

### Use Converters in XAML

```xml
<Window xmlns:converters="clr-namespace:LicenseManagement.EndUser.Wpf.Converters.ValueConverters;assembly=LicenseManagement.EndUser.Wpf">
    <Window.Resources>
        <converters:LicenseStatusConverter x:Key="StatusConverter"/>
        <converters:UtcToLocalTimeConverter x:Key="TimeConverter"/>
    </Window.Resources>

    <TextBlock Text="{Binding Status, Converter={StaticResource StatusConverter}}"/>
    <TextBlock Text="{Binding ExpirationDate, Converter={StaticResource TimeConverter}}"/>
</Window>
```

### Include Resource Dictionary

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/LicenseManagement.EndUser.Wpf;component/AppResources.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## Sample Application

We provide a complete sample WPF application demonstrating real-world usage patterns:

### WPF Sample App

The [WPF Sample Application](samples/WpfSampleApp/) demonstrates:
- License validation at application startup
- Handling different license states (Valid, Trial, Expired)
- Feature gating based on license status
- Integration with built-in license management UI
- Centralized license service pattern

```csharp
// Example: Validate license at startup
private void MainWindow_Loaded(object sender, RoutedEventArgs e)
{
    var context = _licenseService.ValidateLicense(
        onLicFileNotFound: DownloadLicenseFile,
        onTrialEnded: HandleTrialEnded
    );
    UpdateFeatureAccess(context.LicenseModel.Status);
}
```

See the [sample README](samples/WpfSampleApp/README.md) for complete setup instructions.

## Requirements

- .NET Framework 4.8.1
- WPF (Windows Presentation Foundation)
- [Hymma.Lm.EndUser](https://www.nuget.org/packages/Hymma.Lm.EndUser) (dependency)

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and release notes.

## License

MIT - See [LICENSE](LICENSE) for details.

## Related Packages

- [LicenseManagement.EndUser](https://www.nuget.org/packages/Hymma.Lm.EndUser) - Core end-user SDK (includes [WiX Custom Action sample](https://github.com/HYMMA/LicenseManagement.EndUser/tree/main/samples/WixCustomAction))
- [LicenseManagement.Client](https://www.nuget.org/packages/LicenseManagement.Client) - Server-side SDK for vendors

## Documentation

- [API Documentation](https://license-management.com/docs/)
- [LLMs.txt for AI Tools](https://license-management.com/llms.txt)
