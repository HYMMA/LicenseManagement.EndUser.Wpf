using LicenseManagement.EndUser.Wpf.Commands;
using LicenseManagement.EndUser.Wpf.Configuration;
using LicenseManagement.EndUser.Wpf.Views;
using System.ComponentModel;
using System.Windows.Input;
using System.Xml.Serialization;

namespace LicenseManagement.EndUser.Wpf.ViewModels
{
    internal class RegisterLicenseViewModel : BaseViewModel
    {
        private string _code;
        private string _apiKey;
        private bool _isBusy;
        private readonly RelayCommand _register;

        public RegisterLicenseViewModel()
        {
            _register = new RelayCommand(RegisterAction, _ => !IsBusy && !string.IsNullOrEmpty(ReceiptCode) && _apiKey != null);
        }

        public string PublicKey { get; set; }

        public string ReceiptCode
        {
            get => _code;
            set
            {
                if (_code != value)
                {
                    _code = value;
                    OnPropertyChanged();
                    _register.RaiseCanExecuteChanged();
                }
            }
        }

        [XmlIgnore]
        [Browsable(false)]
        internal string ApiKey => _apiKey;

        public void SetApiKey(string value) => _apiKey = value;

        public ICommand Register => _register;

        internal string VendorId { get; set; }
        internal string ProductId { get; set; }
        public uint ValidDays { get; set; }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                    _register.RaiseCanExecuteChanged();
                }
            }
        }

        private void RegisterAction(object obj)
        {
            var view = obj as RegisterLicenseView;
            if (view == null || string.IsNullOrEmpty(ReceiptCode))
                return;

            IsBusy = true;
            try
            {
                var pref = PublisherPreferencesFactory.Build(VendorId, ProductId, _apiKey, PublicKey, ValidDays);
                var context = new LicHandlingContext(pref);
                var handler = new LicenseHandlingLaunch(
                    context,
                    OnCustomerMustEnterProductKey: () => ReceiptCode,
                    OnLicFileNotFound: DownloadLicenseFile,
                    OnTrialValidated: () => ReceiptCode,
                    OnLicenseHandledSuccessfully: _ => view.Close());

                LicenseOperationRunner.Run(handler, ex => ShowErrorView(view, ex));
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void DownloadLicenseFile(LicHandlingContext context)
        {
            var handler = new LicenseHandlingInstall(context, null);
            handler.HandleLicense();
        }
    }
}
