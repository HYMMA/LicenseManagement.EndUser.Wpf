using LicenseManagement.EndUser.Wpf.Commands;
using LicenseManagement.EndUser.Wpf.Configuration;
using LicenseManagement.EndUser.Wpf.Views;
using System.ComponentModel;
using System.Windows.Input;
using System.Xml.Serialization;

namespace LicenseManagement.EndUser.Wpf.ViewModels
{
    public class UnregisterViewModel : BaseViewModel
    {
        private string _apiKey;
        private bool _isBusy;
        private readonly RelayCommand _unregister;

        public UnregisterViewModel()
        {
            _unregister = new RelayCommand(UnregisterLicense, _ => !IsBusy && _apiKey != null);
        }

        public ICommand Unregister => _unregister;

        public string VendorId { get; internal set; }
        public string ProductId { get; internal set; }
        public string PublicKey { get; internal set; }

        [XmlIgnore]
        [Browsable(false)]
        internal string ApiKey => _apiKey;

        public void SetApiKey(string value) => _apiKey = value;

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                    _unregister.RaiseCanExecuteChanged();
                }
            }
        }

        private void UnregisterLicense(object obj)
        {
            var view = obj as UnregisterView;
            if (view == null) return;

            IsBusy = true;
            try
            {
                var pref = PublisherPreferencesFactory.Build(VendorId, ProductId, _apiKey, PublicKey, validDays: 0);
                var context = new LicHandlingContext(pref);
                var handler = new LicenseHandlingUninstall(context, OnLicenseHandledSuccessfully: _ => view.Close());

                LicenseOperationRunner.Run(handler, ex => ShowErrorView(view, ex));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
