using LicenseManagement.EndUser.Models;
using LicenseManagement.EndUser.Wpf.Commands;
using LicenseManagement.EndUser.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LicenseManagement.EndUser.Wpf.ViewModels
{
    internal class RegisterLicenseViewModel : BaseViewModel
    {
        private string _code;

        public string PublicKey { get; set; }
        public string ReceiptCode
        {
            get => _code; set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged();

                }
            }
        }

        internal string ApiKey { get; set; }

        public ICommand Register => new RelayCommand(RegisterAction, CanRegisterNewLicense);

        internal string VendorId { get; set; }
        internal string ProductId { get; set; }
        public uint ValidDays { get; set; }

        private bool CanRegisterNewLicense(object obj) =>
            CanCallApi();

        private bool CanCallApi() =>
            ApiKey != null;

        private void RegisterAction(object obj)
        {
            var view = obj as RegisterLicenseView;
            if (string.IsNullOrEmpty(view.receiptCode.Text))
                return;
            var pref = new PublisherPreferences(VendorId, ProductId, ApiKey)
            {
                PublicKey = this.PublicKey,
                ValidDays = this.ValidDays,
            };
            var context = new LicHandlingContext(pref);
            var handler = new LicenseHandlingLaunch(context,
                                                    OnCustomerMustEnterProductKey: GetNewReceiptCode,
                                                    OnLicFileNotFound: GetNewLicFile,
                                                    OnTrialEnded: ChangeDefaultTrail,
                                                    OnComputerUnregistered: ComputerUnregistered,
                                                    OnTrialValidated: GetNewReceiptCode,
                                                    OnLicenseHandledSuccessfully: (l) =>
                                                    {
                                                        view.Close();
                                                    });
            try
            {
                handler.HandleLicense();
            }
            catch (Exception e)
            {
                ShowErrorView(view, e);
            }
        }

        private void GetNewLicFile(LicHandlingContext context)
        {
            var handler = new LicenseHandlingInstall(context, null);
            handler.HandleLicense();
        }

        private string GetNewReceiptCode()
        {
            return ReceiptCode;
        }

        private void ChangeDefaultTrail(PublisherPreferences preferences)
        {

        }

        private void ComputerUnregistered(ComputerModel model)
        {
        }

    }
}
