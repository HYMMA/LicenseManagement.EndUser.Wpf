using Hymma.Lm.EndUser.Exceptions;
using Hymma.Lm.EndUser.License;
using Hymma.Lm.EndUser.Models;
using Hymma.Lm.EndUser.Wpf.Commands;
using Hymma.Lm.EndUser.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Hymma.Lm.EndUser.Wpf.ViewModels{
    [XmlType("License")]
    public class LicenseViewModel : BaseViewModel
    {
        DateTime? expires;
        LicenseStatusTitles status;
        private string _fullFileName;
        private DateTime? receiptExpires;
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

        public static LicenseViewModel FromContext(LicHandlingContext context,ObservableCollection<ProductViewModel> products)
        {
            var lic = new LicenseViewModel()
            {
                ValidDays = context.PublisherPreferences.ValidDays,
                VendorId = context.PublisherPreferences.VendorId,
                PublicKey = context.PublisherPreferences.PublicKey,
                ApiKey = context.PublisherPreferences.ApiKey,
                TrialExpires = context.LicenseModel.TrialEndDate,
                Created = context.LicenseModel.Created,
                Expires = context.LicenseModel.Expires,
                Updated = context.LicenseModel.Updated,
                Status = context.LicenseModel.Status
            };
            if (context.LicenseModel.Receipt != null)
            {
                lic.ReceiptCode = context.LicenseModel.Receipt.Code;
                lic.ReceiptExpires = context.LicenseModel.Receipt.Expires;
                lic.CustomerEmail = context.LicenseModel.Receipt.BuyerEmail;
            }
            if (context.LicenseModel.Product != null)
            {
                lic.Product = ProductViewModel.FromProductModel(context.LicenseModel.Product);
                lic.VendorName = context.LicenseModel.Product?.Vendor.Name;
            }
            if (context.LicenseModel.Computer != null)
            {
                lic.ComputerName = context.LicenseModel.Computer?.Name;
                lic.MacAddress = context.LicenseModel.Computer?.MacAddress;
            }
            lic.Products = products;
            return lic;
        }

        /// <summary>
        /// full file name of the license file
        /// </summary>
        public string FullFileName
        {
            get { return _fullFileName; }
            set
            {
                if (_fullFileName != value)
                {
                    _fullFileName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// the email of the customer who purchased the product
        /// </summary>
        public string CustomerEmail
        {
            get => _customerEmail; set
            {
                _customerEmail = value;
                OnPropertyChanged();
            }
        }

        public DateTime? Expires
        {
            get => expires; set
            {
                if (expires != value)
                {
                    expires = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? ReceiptExpires
        {
            get => receiptExpires; set
            {
                if (receiptExpires != value)
                {
                    receiptExpires = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? TrialExpires
        {
            get => _trialExpires; set
            {
                if (_trialExpires != value)
                {
                    _trialExpires = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ComputerName
        {
            get => _compName; set
            {
                if (_compName != value)
                {
                    _compName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MacAddress
        {
            get => _mac; set
            {
                if (_mac != value)
                {
                    _mac = value;
                    OnPropertyChanged();
                }
            }
        }

        public string VendorName
        {
            get { return _vendorName; }
            set
            {
                if (_vendorName != value)
                {
                    _vendorName = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ProductViewModel> Products
        {
            get => _products; set
            {
                _products = value;
                OnPropertyChanged();
            }
        }
        public ProductViewModel Product
        {
            get => _product; set
            {
                if (_product == null)
                {
                    _product = value;
                    OnPropertyChanged();
                }
                else if (_product.Id != value.Id)
                {
                    _product = value;
                    OnPropertyChanged();
                }
            }
        }

        public string VendorId
        {
            get => _vendorId; set
            {
                if (_vendorId != value)
                {
                    _vendorId = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public string Message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }
        public uint ValidDays
        {
            get => _validDays; set
            {
                if (_validDays != value)
                {
                    _validDays = value;
                    OnPropertyChanged();
                }
            }
        }
        [XmlIgnore]
        public string ApiKey { get; set; }

        public LicenseStatusTitles Status
        {
            get => status;
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// When the user presses on the register button 
        /// </summary>
        public ICommand ShowRegisterView => new RelayCommand(ShowRegisterWindow, (s) => true);
        public ICommand ShowUnregisterView => new RelayCommand(ShowUnregisterWindow, (s) => true);
        public ICommand RenewLicenseFile => new RelayCommand(RenewLicenseFileAction, (s) => ApiKey != null);

        public string PublicKey { get; internal set; }

        public void CheckLiceneFile(object obj)
        {
            var publisher = new PublisherPreferences(VendorId, Product.Id, ApiKey)
            {
                ValidDays = ValidDays,
                PublicKey = PublicKey
            };
            var context = new LicHandlingContext(publisher);
            var handler = new LicenseHandlingLaunch(context, OnLicenseHandledSuccessfully: UpdateFromLicenseModel);
            try
            {
                handler.HandleLicense();
            }
            catch (Exception)
            {
                UpdateFromLicenseModel(handler.HandlingContext.LicenseModel);
                ShowErrorView(obj as MainWindow, context.Exception);
            }
        }

        internal void RenewLicenseFileAction(object obj)
        {
            var publisher = new PublisherPreferences(VendorId, Product.Id, ApiKey)
            {
                ValidDays = ValidDays,
                PublicKey = PublicKey
            };
            var context = new LicHandlingContext(publisher);
            var handler = new LicenseHandlingInstall(context, UpdateFromLicenseModel);
            try
            {
                handler.HandleLicense();
                //this = LicenseViewModel.FromContext()                
            }
            catch (Exception e)
            {
                ShowErrorView(obj as MainWindow, e);
            }

        }

        internal void UpdateFromLicenseModel(LicenseModel model)
        {
            //now read the downloaded file
            TrialExpires = model.TrialEndDate;
            Created = model.Created;
            Expires = model.Expires;
            Status = model.Status;
            MacAddress = model.Computer?.MacAddress ?? MacAddress;
            ComputerName = model.Computer?.Name ?? ComputerName;
            VendorId = model.Product?.Vendor?.Id ?? VendorId;
            VendorName = model.Product?.Vendor?.Name ?? VendorName;
            Product = ProductViewModel.FromProductModel(model.Product) ?? Product;
            Updated = model?.Updated;
            if (model.Receipt != null)
            {
                ReceiptCode = model.Receipt.Code;
                ReceiptExpires = model.Receipt.Expires;
                CustomerEmail = model.Receipt.BuyerEmail;
            }
        }

        private void ShowRegisterWindow(object obj)
        {
            var thisView = obj as MainWindow;
            var view = new RegisterLicenseView
            {
                Owner = thisView,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                DataContext = new RegisterLicenseViewModel()
                {
                    ApiKey = this.ApiKey,
                    ProductId = this.Product.Id,
                    VendorId = this.VendorId,
                    PublicKey = this.PublicKey,
                    ValidDays = this.ValidDays,
                }
            };
            view.Closed += (s, e) =>
            {
                CheckLiceneFile(thisView);
            };
            view.ShowDialog();
        }

        private void ShowUnregisterWindow(object obj)
        {
            var thisView = obj as MainWindow;
            var view = new UnregisterView
            {
                Owner = thisView,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                DataContext = new UnregisterViewModel()
                {
                    ApiKey = ApiKey,
                    ProductId = Product.Id,
                    VendorId = VendorId,
                    PublicKey = PublicKey,
                }
            };
            view.Closed += (s, e) =>
            {
                CheckLiceneFile(thisView);
            };
            view.ShowDialog();
        }
    }
}