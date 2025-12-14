# WPF Sample Application

This sample demonstrates how to integrate license management into a WPF desktop application using the `Hymma.Lm.EndUser.Wpf` package.

## Overview

The sample shows:
- License validation at application startup
- Handling different license states (Valid, Trial, Expired)
- Feature gating based on license status
- Using the built-in license management UI
- Centralized license service pattern

## Files

- `WpfSampleApp.csproj` - Project file
- `App.xaml` / `App.xaml.cs` - Application entry point
- `MainWindow.xaml` / `MainWindow.xaml.cs` - Main application window
- `Services/LicenseService.cs` - Centralized license management service

## Prerequisites

- .NET Framework 4.8.1
- [Hymma.Lm.EndUser.Wpf](https://www.nuget.org/packages/Hymma.Lm.EndUser.Wpf) NuGet package
- Account at [license-management.com](https://license-management.com)

## Setup

### 1. Get Your Credentials

From the License Management dashboard, note:
- **Vendor ID**: `VDR_01...`
- **Product ID**: `PRD_01...`
- **Client API Key**: `PUB_01...` (NOT your Master key!)
- **Public Key**: For offline validation

### 2. Configure the License Service

Edit `Services/LicenseService.cs` and replace the placeholder values:

```csharp
private const string VendorId = "VDR_YOUR_VENDOR_ID";
private const string ProductId = "PRD_YOUR_PRODUCT_ID";
private const string ApiKey = "PUB_YOUR_CLIENT_API_KEY";
private const string PublicKey = @"<RSAKeyValue>
    <Modulus>YOUR_MODULUS_HERE</Modulus>
    <Exponent>AQAB</Exponent>
</RSAKeyValue>";
```

### 3. Build and Run

```bash
dotnet build WpfSampleApp.csproj
```

## How It Works

### Startup Flow

1. Application loads `MainWindow`
2. `MainWindow_Loaded` event triggers license validation
3. `LicenseService.ValidateLicense()` checks the local license file
4. UI updates based on the license status

### License States

| Status | Behavior |
|--------|----------|
| `Valid` | Full access to all features |
| `ValidTrial` | Limited features, trial countdown shown |
| `InValidTrial` | No access, prompt to purchase |
| `ReceiptExpired` | No access, prompt to renew |
| `ReceiptUnregistered` | No access, prompt to enter key |

### Feature Gating

```csharp
// In your code, check license status before enabling features
if (_licenseService.HasProAccess())
{
    // Enable pro features
    EnableProFeature();
}
else if (_licenseService.IsTrialMode())
{
    // Enable trial features only
    EnableTrialFeatures();
}
else
{
    // Show purchase prompt
    ShowPurchaseDialog();
}
```

### Built-in License UI

The `Hymma.Lm.EndUser.Wpf` package includes pre-built windows for:
- Viewing license details
- Registering a product key
- Unregistering the computer

```csharp
// Show the license management window
var licenseWindow = new Hymma.Lm.EndUser.Wpf.Views.MainWindow
{
    License = LicenseViewModel.FromContext(context, products)
};
licenseWindow.ShowDialog();
```

## Integration Patterns

### MVVM Pattern

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    private readonly LicenseService _licenseService;

    public LicenseStatus Status => _licenseService.GetCachedContext()?.LicenseModel?.Status;

    public bool CanAccessProFeatures => _licenseService.HasProAccess();

    public ICommand ManageLicenseCommand { get; }
}
```

### Dependency Injection

```csharp
// In App.xaml.cs or your DI setup
services.AddSingleton<LicenseService>();

// Inject into view models
public class MyViewModel
{
    public MyViewModel(LicenseService licenseService)
    {
        _licenseService = licenseService;
    }
}
```

### Async Validation

```csharp
// For async validation (if your UI supports it)
public async Task ValidateLicenseAsync()
{
    var context = new LicHandlingContext(preferences);
    var handler = new LicenseHandlingLaunch(context);
    await handler.HandleLicenseAsync();
}
```

## Customization

### Custom License Dialog

Instead of the built-in UI, create your own:

```csharp
private void ShowCustomLicenseDialog()
{
    var dialog = new MyCustomLicenseDialog();
    if (dialog.ShowDialog() == true)
    {
        var receiptCode = dialog.ReceiptCode;
        _licenseService.ActivateLicense(receiptCode);
        ValidateLicense();
    }
}
```

### Styling the Built-in UI

The built-in views use resources from `AppResources.xaml`. Override styles in your App.xaml:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Hymma.Lm.EndUser.Wpf;component/AppResources.xaml"/>
            <!-- Your overrides -->
            <ResourceDictionary>
                <Style TargetType="Button" BasedOn="{StaticResource {x:Type Button}}">
                    <Setter Property="Background" Value="#0078D7"/>
                </Style>
            </ResourceDictionary>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## Related

- [WiX Custom Action Sample](https://github.com/HYMMA/LicenseManagement.EndUser/tree/master/samples/WixCustomAction)
- [EndUser.Wpf README](../../README.md)
- [Documentation](https://license-management.com/docs/)
