using LicenseManagement.EndUser.Wpf.ViewModels;

namespace LicenseManagement.EndUser.Wpf.Configuration
{
    /// <summary>
    /// Builds <see cref="PublisherPreferences"/> from the publisher-related slice of a
    /// <see cref="LicenseViewModel"/>. Centralises the repeated initialiser block that
    /// was duplicated across every license operation.
    /// </summary>
    internal static class PublisherPreferencesFactory
    {
        public static PublisherPreferences Build(LicenseViewModel vm) =>
            new PublisherPreferences(vm.VendorId, vm.Product?.Id, vm.ApiKey)
            {
                ValidDays = vm.ValidDays,
                PublicKey = vm.PublicKey
            };

        public static PublisherPreferences Build(string vendorId, string productId, string apiKey, string publicKey, uint validDays) =>
            new PublisherPreferences(vendorId, productId, apiKey)
            {
                ValidDays = validDays,
                PublicKey = publicKey
            };
    }
}
