using LicenseManagement.EndUser.Wpf.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace LicenseManagement.EndUser.Wpf.Configuration
{
    /// <summary>
    /// Reads license-related publisher settings (vendor, public key, products...) from
    /// <see cref="ConfigurationManager.AppSettings"/> and the optional &lt;Products&gt;
    /// section. Returns <c>null</c> when no app settings are present, so callers can
    /// run without a host config file (e.g. unit tests, designer).
    /// </summary>
    internal static class LicenseConfigurationLoader
    {
        private const uint DefaultValidDays = 90;

        /// <summary>
        /// Build a default <see cref="LicenseViewModel"/> from app settings, or return
        /// <c>null</c> if no settings are configured or config reads throw.
        /// </summary>
        public static LicenseViewModel TryLoad()
        {
            try
            {
                if (ConfigurationManager.AppSettings.Count == 0)
                    return null;

                if (!uint.TryParse(ConfigurationManager.AppSettings.Get("validDays"), out uint validDays))
                    validDays = DefaultValidDays;

                var vm = new LicenseViewModel
                {
                    Expires = DateTime.Now,
                    ValidDays = validDays,
                    VendorId = ConfigurationManager.AppSettings.Get("vendorId") ?? string.Empty,
                    PublicKey = ConfigurationManager.AppSettings.Get("publicKey") ?? string.Empty,
                };
                vm.SetApiKey(ConfigurationManager.AppSettings.Get("ApiKey") ?? string.Empty);

                vm.Products = new ObservableCollection<ProductViewModel>();
                var products = (NameValueCollection)ConfigurationManager.GetSection("Products");
                if (products != null)
                {
                    foreach (var id in products.AllKeys)
                    {
                        vm.Products.Add(new ProductViewModel { Id = id, Name = products[id] });
                    }
                }
                vm.Product = vm.Products.FirstOrDefault();
                return vm;
            }
            catch (ConfigurationErrorsException ex)
            {
                Trace.TraceWarning($"LicenseConfigurationLoader: failed to read app settings — {ex.Message}");
                return null;
            }
        }
    }
}
