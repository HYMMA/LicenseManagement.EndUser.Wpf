# LicenseManagement.EndUser.Wpf

[![Build and Test](https://github.com/HYMMA/LicenseManagement.EndUser.Wpf/actions/workflows/build.yml/badge.svg)](https://github.com/HYMMA/LicenseManagement.EndUser.Wpf/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/LicenseManagement.EndUser.Wpf.svg)](https://www.nuget.org/packages/LicenseManagement.EndUser.Wpf)
[![NuGet Downloads](https://img.shields.io/nuget/dt/LicenseManagement.EndUser.Wpf.svg)](https://www.nuget.org/packages/LicenseManagement.EndUser.Wpf)
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
dotnet add package LicenseManagement.EndUser.Wpf
```

Or via NuGet Package Manager:
```
Install-Package LicenseManagement.EndUser.Wpf
```

> [!WARNING]
> **Breaking Change in v2.0.0**: The package has been renamed from `Hymma.Lm.EndUser.Wpf` to `LicenseManagement.EndUser.Wpf`. See the [Migration Guide](#migrating-from-v1x) below.

## Components

### Views

| View | Description |
|------|-------------|
| `MainWindow` | Main license management window (standalone) |
| `LicenseControl` | Embeddable UserControl for license management |
| `RegisterLicenseView` | Product key/receipt code entry |
| `UnregisterView` | Computer unregistration confirmation |
| `ErrorView` | Error display with details |

### Window vs UserControl

This package provides two ways to display license management UI:

- **`MainWindow`** - A standalone `Window` that can be shown as a dialog. Use this when you want to show license management in a separate popup window.

- **`LicenseControl`** - A `UserControl` that can be embedded directly into your application's UI. Use this when you want to integrate license management into an existing window, tab, or settings panel.

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

### Show License Window (Dialog)

```csharp
using LicenseManagement.EndUser.Wpf.Views;
using LicenseManagement.EndUser.Wpf.ViewModels;

var mainWindow = new MainWindow();
mainWindow.License = new LicenseViewModel
{
    Status = LicenseStatusTitles.Valid,
    ExpirationDate = DateTime.UtcNow.AddMonths(6),
    ProductName = "My Product"
};
mainWindow.ShowDialog();
```

### Embed LicenseControl in Your UI

```xml
<Window xmlns:views="clr-namespace:LicenseManagement.EndUser.Wpf.Views;assembly=LicenseManagement.EndUser.Wpf">
    <Grid>
        <!-- Other UI elements -->
        <views:LicenseControl x:Name="licenseControl" />
    </Grid>
</Window>
```

```csharp
// In code-behind or ViewModel
licenseControl.License = new LicenseViewModel
{
    Status = LicenseStatusTitles.Valid,
    ExpirationDate = DateTime.UtcNow.AddMonths(6),
    ProductName = "My Product"
};
```

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
- [LicenseManagement.EndUser](https://www.nuget.org/packages/LicenseManagement.EndUser) (dependency)

## Migrating from v1.x

Version 2.0.0 introduces breaking changes: the namespace has been renamed from `Hymma.Lm.EndUser.Wpf` to `LicenseManagement.EndUser.Wpf`.

### Steps to Migrate

1. **Update NuGet Package Reference**
   ```bash
   # Remove old package
   dotnet remove package Hymma.Lm.EndUser.Wpf

   # Add new package
   dotnet add package LicenseManagement.EndUser.Wpf
   ```

2. **Update Namespace Imports in C#**
   Replace all occurrences:
   ```csharp
   // Old
   using Hymma.Lm.EndUser.Wpf.Views;
   using Hymma.Lm.EndUser.Wpf.ViewModels;
   using Hymma.Lm.EndUser.Wpf.Converters;

   // New
   using LicenseManagement.EndUser.Wpf.Views;
   using LicenseManagement.EndUser.Wpf.ViewModels;
   using LicenseManagement.EndUser.Wpf.Converters;
   ```

3. **Update XAML Namespaces**
   ```xml
   <!-- Old -->
   xmlns:views="clr-namespace:Hymma.Lm.EndUser.Wpf.Views;assembly=Hymma.Lm.EndUser.Wpf"

   <!-- New -->
   xmlns:views="clr-namespace:LicenseManagement.EndUser.Wpf.Views;assembly=LicenseManagement.EndUser.Wpf"
   ```

4. **Update Resource Dictionary References**
   ```xml
   <!-- Old -->
   <ResourceDictionary Source="pack://application:,,,/Hymma.Lm.EndUser.Wpf;component/AppResources.xaml"/>

   <!-- New -->
   <ResourceDictionary Source="pack://application:,,,/LicenseManagement.EndUser.Wpf;component/AppResources.xaml"/>
   ```

### What Changed

| v1.x | v2.0.0 |
|------|--------|
| `Hymma.Lm.EndUser.Wpf` namespace | `LicenseManagement.EndUser.Wpf` namespace |
| `Hymma.Lm.EndUser.Wpf.dll` | `LicenseManagement.EndUser.Wpf.dll` |
| NuGet: `Hymma.Lm.EndUser.Wpf` | NuGet: `LicenseManagement.EndUser.Wpf` |

### New in v2.0.0

- **`LicenseControl`** - A new embeddable UserControl for integrating license management directly into your application's UI, as an alternative to the standalone `MainWindow` dialog.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and release notes.

## License

MIT - See [LICENSE](LICENSE) for details.

## Related Packages

- [LicenseManagement.EndUser](https://www.nuget.org/packages/LicenseManagement.EndUser) - Core end-user SDK (includes [WiX Custom Action sample](https://github.com/HYMMA/LicenseManagement.EndUser/tree/master/samples/WixCustomAction))
- [LicenseManagement.Client](https://www.nuget.org/packages/LicenseManagement.Client) - Server-side SDK for vendors

## Documentation

- [API Documentation](https://license-management.com/docs/)
- [LLMs.txt for AI Tools](https://license-management.com/llms.txt)
