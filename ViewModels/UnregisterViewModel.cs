using Hymma.Lm.EndUser.Wpf.Commands;
using Hymma.Lm.EndUser.Wpf.Views;
using System;
using System.Collections.ObjectModel;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;

namespace Hymma.Lm.EndUser.Wpf.ViewModels
{
    public class UnregisterViewModel : BaseViewModel
    {
        public ICommand Unregister => new RelayCommand(UnregisterLicense, CanUnregisterLicense);

        public string VendorId { get; internal set; }
        public string ProductId { get; internal set; }
        public string ApiKey { get; internal set; }
        public string PublicKey { get; internal set; }

        private void UnregisterLicense(object obj)
        {
            var view = obj as UnregisterView;
            var pref = new PublisherPreferences(VendorId, ProductId, ApiKey) { PublicKey = PublicKey };
            var context = new LicHandlingContext(pref);
            var handler = new LicenseHandlingUninstall(
                context: context,
                OnLicenseHandledSuccessfully: (c) =>
            {
                //view.Owner.DataContext = LicenseViewModel.FromContext(context, Products);
                view.Close();
            });
            try
            {
                handler.HandleLicense();
            }
            catch (Exception e)
            {
                ShowErrorView(obj as UnregisterView, e);
            }
        }

        private bool CanCallApi() =>
            ApiKey != null;


        private bool CanUnregisterLicense(object obj) =>
            CanCallApi();
    }
}
