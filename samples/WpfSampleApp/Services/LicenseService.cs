// =============================================================================
// License Service - Centralized License Management
// =============================================================================
// This service encapsulates all license management logic, making it easy to
// integrate into your application architecture (MVVM, DI containers, etc.)
//
// Credentials are loaded from App.config <appSettings>. NEVER hard-code an
// API key in a class file — committing secrets to source control is the
// #1 source of credential leaks for desktop apps.
// =============================================================================

using LicenseManagement.EndUser;
using LicenseManagement.EndUser.Models;
using System;
using System.Configuration;

namespace LicenseManagement.Sample.Wpf.Services
{
    /// <summary>
    /// Centralized service for license management operations.
    ///
    /// In a production application, you would typically:
    /// - Register this as a singleton in your DI container
    /// - Implement caching to avoid repeated API calls
    /// </summary>
    public class LicenseService
    {
        private readonly string _vendorId;
        private readonly string _productId;
        private readonly string _apiKey;
        private readonly string _publicKey;
        private readonly uint _validDays;

        private LicHandlingContext _cachedContext;

        public LicenseService()
        {
            _vendorId  = ConfigurationManager.AppSettings["vendorId"]  ?? throw new ConfigurationErrorsException("Missing appSetting 'vendorId'");
            _productId = ConfigurationManager.AppSettings["productId"] ?? throw new ConfigurationErrorsException("Missing appSetting 'productId'");
            _apiKey    = ConfigurationManager.AppSettings["ApiKey"]    ?? throw new ConfigurationErrorsException("Missing appSetting 'ApiKey'");
            _publicKey = ConfigurationManager.AppSettings["publicKey"] ?? throw new ConfigurationErrorsException("Missing appSetting 'publicKey'");
            _validDays = uint.TryParse(ConfigurationManager.AppSettings["validDays"], out var d) ? d : 90;
        }

        /// <summary>
        /// Creates the publisher preferences configuration.
        /// </summary>
        public PublisherPreferences GetPreferences() =>
            new PublisherPreferences(_vendorId, _productId, _apiKey)
            {
                PublicKey = _publicKey,
                ValidDays = _validDays
            };

        /// <summary>
        /// Validates the license at application launch.
        /// </summary>
        public LicHandlingContext ValidateLicense(
            Action<LicHandlingContext> onLicFileNotFound = null,
            Action<PublisherPreferences> onTrialEnded = null,
            Func<string> onCustomerMustEnterProductKey = null)
        {
            var context = new LicHandlingContext(GetPreferences());

            var handler = new LicenseHandlingLaunch(
                context,
                OnCustomerMustEnterProductKey: onCustomerMustEnterProductKey,
                OnLicFileNotFound: onLicFileNotFound,
                OnTrialEnded: onTrialEnded,
                OnLicenseHandledSuccessfully: _ => _cachedContext = context);

            handler.HandleLicense();
            _cachedContext = context;
            return context;
        }

        /// <summary>
        /// Downloads a fresh license file from the server.
        /// </summary>
        public LicHandlingContext DownloadLicense(Action<LicenseModel> onSuccess = null)
        {
            var context = new LicHandlingContext(GetPreferences());
            var handler = new LicenseHandlingInstall(context, onSuccess);
            handler.HandleLicense();
            _cachedContext = context;
            return context;
        }

        /// <summary>
        /// Unregisters this computer from the license.
        /// </summary>
        public void UnregisterLicense()
        {
            var context = new LicHandlingContext(GetPreferences());
            var handler = new LicenseHandlingUninstall(context);
            handler.HandleLicense();
        }

        /// <summary>
        /// Activates a license with a receipt code (product key).
        /// </summary>
        public LicHandlingContext ActivateLicense(string receiptCode)
        {
            var context = new LicHandlingContext(GetPreferences());

            var handler = new LicenseHandlingLaunch(
                context,
                OnCustomerMustEnterProductKey: () => receiptCode);

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
        public bool HasProAccess() =>
            _cachedContext?.LicenseModel?.Status == LicenseManagement.EndUser.License.LicenseStatusTitles.Valid;

        /// <summary>
        /// Checks if the current license is in trial mode.
        /// </summary>
        public bool IsTrialMode() =>
            _cachedContext?.LicenseModel?.Status == LicenseManagement.EndUser.License.LicenseStatusTitles.ValidTrial;

        /// <summary>
        /// Gets the number of days remaining in the trial period.
        /// </summary>
        public int GetTrialDaysRemaining()
        {
            var trialEndDate = _cachedContext?.LicenseModel?.TrialEndDate;
            if (!trialEndDate.HasValue || trialEndDate == default)
                return 0;

            var remaining = (trialEndDate.Value - DateTime.UtcNow).Days;
            return Math.Max(0, remaining);
        }
    }
}
