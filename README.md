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

## Requirements

- .NET Framework 4.8.1
- WPF (Windows Presentation Foundation)
- [Hymma.Lm.EndUser](https://www.nuget.org/packages/Hymma.Lm.EndUser) (dependency)

## License

MIT - See [LICENSE](LICENSE) for details.

## Related Packages

- [LicenseManagement.EndUser](https://www.nuget.org/packages/Hymma.Lm.EndUser) - Core end-user SDK
- [LicenseManagement.Client](https://www.nuget.org/packages/LicenseManagement.Client) - Server-side SDK for vendors
