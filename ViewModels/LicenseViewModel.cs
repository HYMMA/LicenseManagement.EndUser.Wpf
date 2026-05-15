using LicenseManagement.EndUser.License;
using LicenseManagement.EndUser.Models;
using LicenseManagement.EndUser.Wpf.Commands;
using LicenseManagement.EndUser.Wpf.Configuration;
using LicenseManagement.EndUser.Wpf.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace LicenseManagement.EndUser.Wpf.ViewModels
{
    [XmlType("License")]
    public class LicenseViewModel : BaseViewModel
    {
        private DateTime? _expires;
        private LicenseStatusTitles _status;
        private string _fullFileName;
        private DateTime? _receiptExpires;
        private string _mac;
        private string _code;
        private string _message;
        private string _vendorName;
        private string _vendorId;
        private DateTime? _trialExpires;
        private string _compName;
        private uint _validDays;
        private ProductViewModel _product;
        private string _customerEmail;
        private ObservableCollection<ProductViewModel> _products;
        private string _apiKey;
        private bool _isBusy;

        private readonly RelayCommand _showRegisterView;
        private readonly RelayCommand _showUnregisterView;
        private readonly RelayCommand _renewLicenseFile;

        public LicenseViewModel()
        {
            _showRegisterView = new RelayCommand(ShowRegisterWindow, _ => !IsBusy);
            _showUnregisterView = new RelayCommand(ShowUnregisterWindow, _ => !IsBusy);
            _renewLicenseFile = new RelayCommand(RenewLicenseFileAction, _ => !IsBusy && _apiKey != null);
        }

        public static LicenseViewModel FromContext(LicHandlingContext context, ObservableCollection<ProductViewModel> products)
        {
            var lic = new LicenseViewModel
            {
                ValidDays = context.PublisherPreferences.ValidDays,
                VendorId = context.PublisherPreferences.VendorId,
                PublicKey = context.PublisherPreferences.PublicKey,
                TrialExpires = context.LicenseModel.TrialEndDate,
                Created = context.LicenseModel.Created,
                Expires = context.LicenseModel.Expires,
                Updated = context.LicenseModel.Updated,
                Status = context.LicenseModel.Status
            };
            lic.SetApiKey(context.PublisherPreferences.ApiKey);
            if (context.LicenseModel.Receipt != null)
            {
                lic.ReceiptCode = context.LicenseModel.Receipt.Code;
                lic.ReceiptExpires = context.LicenseModel.Receipt.Expires;
                lic.CustomerEmail = context.LicenseModel.Receipt.BuyerEmail;
            }
            if (context.LicenseModel.Product != null)
            {
                lic.Product = ProductViewModel.FromProductModel(context.LicenseModel.Product);
                lic.VendorName = context.LicenseModel.Product.Vendor?.Name;
            }
            if (context.LicenseModel.Computer != null)
            {
                lic.ComputerName = context.LicenseModel.Computer.Name;
                lic.MacAddress = context.LicenseModel.Computer.MacAddress;
            }
            lic.Products = products;
            return lic;
        }

        public string FullFileName
        {
            get => _fullFileName;
            set { if (_fullFileName != value) { _fullFileName = value; OnPropertyChanged(); } }
        }

        public string CustomerEmail
        {
            get => _customerEmail;
            set { if (_customerEmail != value) { _customerEmail = value; OnPropertyChanged(); } }
        }

        public DateTime? Expires
        {
            get => _expires;
            set { if (_expires != value) { _expires = value; OnPropertyChanged(); } }
        }

        public DateTime? ReceiptExpires
        {
            get => _receiptExpires;
            set { if (_receiptExpires != value) { _receiptExpires = value; OnPropertyChanged(); } }
        }

        public DateTime? TrialExpires
        {
            get => _trialExpires;
            set { if (_trialExpires != value) { _trialExpires = value; OnPropertyChanged(); } }
        }

        public string ComputerName
        {
            get => _compName;
            set { if (_compName != value) { _compName = value; OnPropertyChanged(); } }
        }

        public string MacAddress
        {
            get => _mac;
            set { if (_mac != value) { _mac = value; OnPropertyChanged(); } }
        }

        public string VendorName
        {
            get => _vendorName;
            set { if (_vendorName != value) { _vendorName = value; OnPropertyChanged(); } }
        }

        public ObservableCollection<ProductViewModel> Products
        {
            get => _products;
            set { if (_products != value) { _products = value; OnPropertyChanged(); } }
        }

        public ProductViewModel Product
        {
            get => _product;
            set
            {
                if (!ReferenceEquals(_product, value))
                {
                    _product = value;
                    OnPropertyChanged();
                }
            }
        }

        public string VendorId
        {
            get => _vendorId;
            set { if (_vendorId != value) { _vendorId = value; OnPropertyChanged(); } }
        }

        public string ReceiptCode
        {
            get => _code;
            set { if (_code != value) { _code = value; OnPropertyChanged(); } }
        }

        public string Message
        {
            get => _message;
            set { if (_message != value) { _message = value; OnPropertyChanged(); } }
        }

        public uint ValidDays
        {
            get => _validDays;
            set { if (_validDays != value) { _validDays = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// API key used to authenticate with the license server. Kept out of the
        /// bindable property surface so it is not visible to UI inspectors (Snoop,
        /// Live Visual Tree). Use <see cref="SetApiKey(string)"/> to configure it.
        /// </summary>
        [XmlIgnore]
        [Browsable(false)]
        internal string ApiKey => _apiKey;

        /// <summary>
        /// Configures the API key used by this view model. The key is stored in a
        /// non-bindable field so it is not reflected by XAML diagnostic tools.
        /// </summary>
        public void SetApiKey(string value) => _apiKey = value;

        /// <summary>
        /// Indicates whether an API key has been configured. Bind to this property
        /// instead of <c>ApiKey</c> to drive button visibility / IsEnabled state.
        /// </summary>
        public bool HasApiKey => _apiKey != null;

        public LicenseStatusTitles Status
        {
            get => _status;
            set { if (_status != value) { _status = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// Set to <c>true</c> while a server-bound license operation is in flight.
        /// Used by commands to prevent double-submission (e.g. double-click on Register).
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                    _showRegisterView.RaiseCanExecuteChanged();
                    _showUnregisterView.RaiseCanExecuteChanged();
                    _renewLicenseFile.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand ShowRegisterView => _showRegisterView;
        public ICommand ShowUnregisterView => _showUnregisterView;
        public ICommand RenewLicenseFile => _renewLicenseFile;

        public string PublicKey { get; internal set; }

        public void CheckLiceneFile(object obj)
        {
            var source = obj as DependencyObject;
            IsBusy = true;
            try
            {
                var context = new LicHandlingContext(PublisherPreferencesFactory.Build(this));
                var handler = new LicenseHandlingLaunch(context, OnLicenseHandledSuccessfully: UpdateFromLicenseModel);
                LicenseOperationRunner.Run(handler, ex =>
                {
                    UpdateFromLicenseModel(handler.HandlingContext.LicenseModel);
                    ShowErrorView(source, context.Exception ?? ex);
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        internal void RenewLicenseFileAction(object obj)
        {
            var source = obj as DependencyObject;
            IsBusy = true;
            try
            {
                var context = new LicHandlingContext(PublisherPreferencesFactory.Build(this));
                var handler = new LicenseHandlingInstall(context, UpdateFromLicenseModel);
                LicenseOperationRunner.Run(handler, ex => ShowErrorView(source, ex));
            }
            finally
            {
                IsBusy = false;
            }
        }

        internal void UpdateFromLicenseModel(LicenseModel model)
        {
            if (model == null) return;
            TrialExpires = model.TrialEndDate;
            Created = model.Created;
            Expires = model.Expires;
            Status = model.Status;
            MacAddress = model.Computer?.MacAddress ?? MacAddress;
            ComputerName = model.Computer?.Name ?? ComputerName;
            VendorId = model.Product?.Vendor?.Id ?? VendorId;
            VendorName = model.Product?.Vendor?.Name ?? VendorName;
            Product = ProductViewModel.FromProductModel(model.Product) ?? Product;
            Updated = model.Updated;
            if (model.Receipt != null)
            {
                ReceiptCode = model.Receipt.Code;
                ReceiptExpires = model.Receipt.Expires;
                CustomerEmail = model.Receipt.BuyerEmail;
            }
        }

        private void ShowRegisterWindow(object obj)
        {
            var source = obj as DependencyObject;
            var owner = source as Window ?? (source != null ? Window.GetWindow(source) : null);
            var registerVm = new RegisterLicenseViewModel
            {
                ProductId = Product?.Id,
                VendorId = VendorId,
                PublicKey = PublicKey,
                ValidDays = ValidDays,
            };
            registerVm.SetApiKey(_apiKey);

            var view = new RegisterLicenseView
            {
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                DataContext = registerVm
            };
            EventHandler onClosed = null;
            onClosed = (s, e) =>
            {
                view.Closed -= onClosed;
                CheckLiceneFile(source ?? owner);
            };
            view.Closed += onClosed;
            view.ShowDialog();
        }

        private void ShowUnregisterWindow(object obj)
        {
            var source = obj as DependencyObject;
            var owner = source as Window ?? (source != null ? Window.GetWindow(source) : null);
            var unregisterVm = new UnregisterViewModel
            {
                ProductId = Product?.Id,
                VendorId = VendorId,
                PublicKey = PublicKey,
            };
            unregisterVm.SetApiKey(_apiKey);

            var view = new UnregisterView
            {
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                DataContext = unregisterVm
            };
            EventHandler onClosed = null;
            onClosed = (s, e) =>
            {
                view.Closed -= onClosed;
                CheckLiceneFile(source ?? owner);
            };
            view.Closed += onClosed;
            view.ShowDialog();
        }
    }
}
