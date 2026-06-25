using LicenseManagement.EndUser.Wpf.ViewModels;
using System;
using System.Collections.Generic;
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

                // Optional, zero-code grouping + layout (developer config at startup).
                // With no <ProductGroups> section every product falls into one default
                // group, so existing consumers keep working unchanged.
                var layout = ParseLayout(ConfigurationManager.AppSettings.Get("licenseLayout"));
                vm.ApplyGrouping(ReadGroupDefinitions(), layout);
                return vm;
            }
            catch (ConfigurationErrorsException ex)
            {
                Trace.TraceWarning($"LicenseConfigurationLoader: failed to read app settings — {ex.Message}");
                return null;
            }
        }

        private static ProductLayout ParseLayout(string value)
        {
            ProductLayout layout;
            return Enum.TryParse(value, ignoreCase: true, result: out layout)
                ? layout
                : ProductLayout.Bands;
        }

        /// <summary>
        /// Reads an optional &lt;ProductGroups&gt; NameValueCollection section. Each entry is
        /// <c>groupKey = "Label | Caption | #Accent | id1,id2,id3"</c>. Returns <c>null</c>
        /// when the section is absent.
        /// </summary>
        private static IEnumerable<ProductGroupDefinition> ReadGroupDefinitions()
        {
            var section = (NameValueCollection)ConfigurationManager.GetSection("ProductGroups");
            if (section == null)
                return null;

            var defs = new List<ProductGroupDefinition>();
            foreach (var key in section.AllKeys)
            {
                var parts = (section[key] ?? string.Empty).Split('|');
                var def = new ProductGroupDefinition
                {
                    Key = key,
                    Label = parts.Length > 0 ? parts[0].Trim() : key,
                    Caption = parts.Length > 1 ? parts[1].Trim() : null,
                    Accent = parts.Length > 2 ? parts[2].Trim() : null,
                };
                if (parts.Length > 3)
                {
                    foreach (var id in parts[3].Split(','))
                    {
                        var trimmed = id.Trim();
                        if (trimmed.Length > 0)
                            def.ProductIds.Add(trimmed);
                    }
                }
                defs.Add(def);
            }
            return defs;
        }
    }
}
