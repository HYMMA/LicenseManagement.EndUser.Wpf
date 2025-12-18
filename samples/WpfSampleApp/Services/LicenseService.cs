// =============================================================================
// License Service - Centralized License Management
// =============================================================================
// This service encapsulates all license management logic, making it easy to
// integrate into your application architecture (MVVM, DI containers, etc.)
// =============================================================================

using LicenseManagement.EndUser;
using LicenseManagement.EndUser.Models;
using System;

namespace LicenseManagement.Sample.Wpf.Services
{
    /// <summary>
    /// Centralized service for license management operations.
    ///
    /// In a production application, you would typically:
    /// - Register this as a singleton in your DI container
    /// - Store credentials in app settings or secure storage
    /// - Implement caching to avoid repeated API calls
    /// </summary>
    public class LicenseService
    {
        // =================================================================
        // CONFIGURATION - Replace with your actual values from the dashboard
        // =================================================================

        /// <summary>
        /// Your vendor ID from the License Management dashboard.
        /// Found at: Dashboard > Account Settings
        /// </summary>
        private const string VendorId = "VDR_YOUR_VENDOR_ID";

        /// <summary>
        /// Your product ID from the License Management dashboard.
        /// Found at: Dashboard > Products > Your Product
        /// </summary>
        private const string ProductId = "PRD_YOUR_PRODUCT_ID";

        /// <summary>
        /// Your CLIENT API key (NOT master key!) from the dashboard.
        /// Found at: Dashboard > API Keys > Client Keys
        ///
        /// IMPORTANT: Never use your Master API key in client applications!
        /// </summary>
        private const string ApiKey = "PUB_YOUR_CLIENT_API_KEY";

        /// <summary>
        /// Your public key for offline license validation.
        /// Found at: Dashboard > Signing Keys
        ///
        /// This allows the app to validate licenses without API calls.
        /// </summary>
        private const string PublicKey = @"<RSAKeyValue>
            <Modulus>YOUR_MODULUS_HERE</Modulus>
            <Exponent>AQAB</Exponent>
        </RSAKeyValue>";

        /// <summary>
        /// Trial period in days. Set to your desired trial length.
        /// </summary>
        private const uint TrialDays = 14;

        /// <summary>
        /// License validity period in days.
        /// The license file is valid for this many days before requiring re-validation.
        /// </summary>
        private const uint ValidDays = 90;

        // =================================================================
        // SERVICE IMPLEMENTATION
        // =================================================================

        private LicHandlingContext _cachedContext;

        /// <summary>
        /// Creates the publisher preferences configuration.
        /// </summary>
        public PublisherPreferences GetPreferences()
        {
            return new PublisherPreferences(VendorId, ProductId, ApiKey)
            {
                PublicKey = PublicKey,
                ValidDays = ValidDays
            };
        }

        /// <summary>
        /// Validates the license at application launch.
        /// </summary>
        /// <param name="onLicFileNotFound">Called when license file is missing</param>
        /// <param name="onTrialEnded">Called when trial period has expired</param>
        /// <param name="onCustomerMustEnterProductKey">Called when user needs to enter a key</param>
        /// <returns>The license handling context with current license state</returns>
        public LicHandlingContext ValidateLicense(
            Action<LicHandlingContext> onLicFileNotFound = null,
            Action<PublisherPreferences> onTrialEnded = null,
            Func<string> onCustomerMustEnterProductKey = null)
        {
            var preferences = GetPreferences();
            var context = new LicHandlingContext(preferences);

            var handler = new LicenseHandlingLaunch(
                context,
                OnCustomerMustEnterProductKey: onCustomerMustEnterProductKey,
                OnLicFileNotFound: onLicFileNotFound,
                OnTrialEnded: onTrialEnded,
                OnLicenseHandledSuccessfully: (license) =>
                {
                    // Cache the context for later use
                    _cachedContext = context;
                }
            );

            handler.HandleLicense();
            _cachedContext = context;

            return context;
        }

        /// <summary>
        /// Downloads a fresh license file from the server.
        /// Call this during installation or when license is missing.
        /// </summary>
        /// <param name="onSuccess">Called when license is successfully downloaded</param>
        public LicHandlingContext DownloadLicense(Action<LicenseModel> onSuccess = null)
        {
            var preferences = GetPreferences();
            var context = new LicHandlingContext(preferences);

            var handler = new LicenseHandlingInstall(context, onSuccess);
            handler.HandleLicense();

            _cachedContext = context;
            return context;
        }

        /// <summary>
        /// Unregisters this computer from the license.
        /// Call this during application uninstall to free up the seat.
        /// </summary>
        public void UnregisterLicense()
        {
            var preferences = GetPreferences();
            var context = new LicHandlingContext(preferences);

            var handler = new LicenseHandlingUninstall(context);
            handler.HandleLicense();
        }

        /// <summary>
        /// Activates a license with a receipt code (product key).
        /// </summary>
        /// <param name="receiptCode">The receipt code from the customer's purchase</param>
        public LicHandlingContext ActivateLicense(string receiptCode)
        {
            var preferences = GetPreferences();
            var context = new LicHandlingContext(preferences);

            var handler = new LicenseHandlingLaunch(
                context,
                OnCustomerMustEnterProductKey: () => receiptCode
            );

            handler.HandleLicense();
            _cachedContext = context;

            return context;
        }

        /// <summary>
        /// Gets the cached license context from the last validation.
        /// </summary>
        public LicHandlingContext GetCachedContext() => _cachedContext;

        /// <summary>
        /// Checks if the current license allows access to pro features.
        /// </summary>
        public bool HasProAccess()
        {
            if (_cachedContext?.LicenseModel == null)
                return false;

            return _cachedContext.LicenseModel.Status == LicenseManagement.EndUser.License.LicenseStatusTitles.Valid;
        }

        /// <summary>
        /// Checks if the current license is in trial mode.
        /// </summary>
        public bool IsTrialMode()
        {
            if (_cachedContext?.LicenseModel == null)
                return false;

            return _cachedContext.LicenseModel.Status == LicenseManagement.EndUser.License.LicenseStatusTitles.ValidTrial;
        }

        /// <summary>
        /// Gets the number of days remaining in the trial period.
        /// </summary>
        public int GetTrialDaysRemaining()
        {
            if (_cachedContext?.LicenseModel?.TrialEndDate == null)
                return 0;

            var remaining = (_cachedContext.LicenseModel.TrialEndDate.Value - DateTime.UtcNow).Days;
            return Math.Max(0, remaining);
        }
    }
}
