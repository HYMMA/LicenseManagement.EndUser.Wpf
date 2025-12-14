// =============================================================================
// WPF Sample Application for License Management
// =============================================================================
// This sample demonstrates how to integrate license management into a WPF
// application using the Hymma.Lm.EndUser.Wpf package.
//
// Features demonstrated:
// - License validation at startup
// - Handling different license states (Valid, Trial, Expired)
// - Showing the built-in license management UI
// - Feature gating based on license status
// =============================================================================

using Hymma.Lm.EndUser;
using Hymma.Lm.EndUser.License;
using Hymma.Lm.EndUser.Models;
using Hymma.Lm.EndUser.Wpf.ViewModels;
using Hymma.Lm.EndUser.Wpf.Views;
using LicenseManagement.Sample.Wpf.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace LicenseManagement.Sample.Wpf
{
    /// <summary>
    /// Sample WPF application demonstrating license management integration.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly LicenseService _licenseService;
        private LicHandlingContext _currentContext;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the license service with your credentials
            _licenseService = new LicenseService();

            // Check license when window loads
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ValidateLicense();
        }

        /// <summary>
        /// Validates the license and updates the UI accordingly.
        /// </summary>
        private void ValidateLicense()
        {
            try
            {
                StatusText.Text = "Validating...";

                // Get the handling context from license service
                _currentContext = _licenseService.ValidateLicense(
                    onLicFileNotFound: DownloadLicenseFile,
                    onTrialEnded: HandleTrialEnded,
                    onCustomerMustEnterProductKey: PromptForProductKey
                );

                // Update UI based on license status
                UpdateLicenseUI(_currentContext);
                UpdateFeatureAccess(_currentContext.LicenseModel.Status);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error";
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
                FeatureMessage.Text = $"License validation failed: {ex.Message}";

                // Disable all features on error
                UpdateFeatureAccess(LicenseStatusTitles.InValidTrial);
            }
        }

        /// <summary>
        /// Updates the license information display.
        /// </summary>
        private void UpdateLicenseUI(LicHandlingContext context)
        {
            var license = context.LicenseModel;

            // Update status with color coding
            StatusText.Text = license.Status.ToString();
            StatusText.Foreground = license.Status switch
            {
                LicenseStatusTitles.Valid => System.Windows.Media.Brushes.Green,
                LicenseStatusTitles.ValidTrial => System.Windows.Media.Brushes.Orange,
                _ => System.Windows.Media.Brushes.Red
            };

            // Update other fields
            ProductText.Text = license.Product?.Name ?? "Unknown";
            ExpiresText.Text = license.Expires?.ToString("yyyy-MM-dd") ?? "-";
            ComputerText.Text = license.Computer?.Name ?? Environment.MachineName;
            ReceiptText.Text = license.Receipt?.Code ?? "(No receipt - Trial mode)";

            // Update feature message
            FeatureMessage.Text = license.Status switch
            {
                LicenseStatusTitles.Valid => "Full access enabled. All features are available.",
                LicenseStatusTitles.ValidTrial => $"Trial mode. {GetTrialDaysRemaining(license)} days remaining.",
                LicenseStatusTitles.InValidTrial => "Trial has expired. Please purchase a license.",
                LicenseStatusTitles.ReceiptExpired => "Subscription expired. Please renew.",
                LicenseStatusTitles.ReceiptUnregistered => "License unregistered. Please enter a product key.",
                _ => "Unknown license status."
            };
        }

        /// <summary>
        /// Enables/disables features based on license status.
        /// </summary>
        private void UpdateFeatureAccess(LicenseStatusTitles status)
        {
            switch (status)
            {
                case LicenseStatusTitles.Valid:
                    // Full access
                    BasicFeature.IsChecked = true;
                    BasicFeature.IsEnabled = true;
                    ProFeature.IsChecked = true;
                    ProFeature.IsEnabled = true;
                    TrialFeature.IsChecked = true;
                    TrialFeature.IsEnabled = true;
                    break;

                case LicenseStatusTitles.ValidTrial:
                    // Trial features only
                    BasicFeature.IsChecked = true;
                    BasicFeature.IsEnabled = true;
                    ProFeature.IsChecked = false;
                    ProFeature.IsEnabled = false;
                    TrialFeature.IsChecked = true;
                    TrialFeature.IsEnabled = true;
                    break;

                default:
                    // No access
                    BasicFeature.IsChecked = false;
                    BasicFeature.IsEnabled = false;
                    ProFeature.IsChecked = false;
                    ProFeature.IsEnabled = false;
                    TrialFeature.IsChecked = false;
                    TrialFeature.IsEnabled = false;
                    break;
            }
        }

        /// <summary>
        /// Calculates remaining trial days.
        /// </summary>
        private int GetTrialDaysRemaining(LicenseModel license)
        {
            if (license.TrialEndDate == null)
                return 0;

            var remaining = (license.TrialEndDate.Value - DateTime.UtcNow).Days;
            return Math.Max(0, remaining);
        }

        /// <summary>
        /// Callback when license file is not found - downloads a new one.
        /// </summary>
        private void DownloadLicenseFile(LicHandlingContext context)
        {
            try
            {
                // Download a fresh license file
                var installHandler = new LicenseHandlingInstall(
                    new LicHandlingContext(context.PublisherPreferences),
                    OnLicenseHandledSuccessfully: null
                );
                installHandler.HandleLicense();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not download license: {ex.Message}",
                    "License Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        /// <summary>
        /// Callback when trial period has ended.
        /// </summary>
        private void HandleTrialEnded(PublisherPreferences preferences)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    "Your trial period has ended. Please purchase a license to continue using the application.",
                    "Trial Expired",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            });
        }

        /// <summary>
        /// Callback to prompt user for product key when needed.
        /// </summary>
        private string PromptForProductKey()
        {
            // In a real application, you might show a dialog here
            // For this sample, we'll show the built-in license management window
            return null; // Return null to skip - user can use "Manage License" button
        }

        /// <summary>
        /// Refreshes the license from the server.
        /// </summary>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ValidateLicense();
        }

        /// <summary>
        /// Opens the built-in license management window.
        /// </summary>
        private void ManageLicenseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentContext == null)
            {
                MessageBox.Show("Please wait for license validation to complete.");
                return;
            }

            // Create products collection for the license view
            var products = new ObservableCollection<ProductViewModel>();
            if (_currentContext.LicenseModel.Product != null)
            {
                products.Add(ProductViewModel.FromProductModel(_currentContext.LicenseModel.Product));
            }

            // Create license view model from the current context
            var licenseViewModel = LicenseViewModel.FromContext(_currentContext, products);

            // Show the built-in license management window from the Hymma package
            var licenseWindow = new Hymma.Lm.EndUser.Wpf.Views.MainWindow
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                License = licenseViewModel
            };

            // Refresh after closing
            licenseWindow.Closed += (s, args) => ValidateLicense();
            licenseWindow.ShowDialog();
        }
    }
}
