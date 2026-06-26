using LicenseManagement.EndUser.License;
using LicenseManagement.EndUser.Models;
using LicenseManagement.EndUser.Wpf.Commands;
using LicenseManagement.EndUser.Wpf.Configuration;
using LicenseManagement.EndUser.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
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
        private ObservableCollection<ProductGroupViewModel> _productGroups;
        private ProductLayout _layout = ProductLayout.Bands;
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
            set { if (_vendorName != value) { _vendorName = value; OnPropertyChanged(); OnPropertyChanged(nameof(VendorInitial)); } }
        }

        /// <summary>First letter of the vendor name, for the identity badge.</summary>
        public string VendorInitial =>
            string.IsNullOrWhiteSpace(_vendorName) ? "•" : _vendorName.Trim().Substring(0, 1).ToUpperInvariant();

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

        /// <summary>
        /// Products arranged into developer-defined groups (e.g. Monthly / Annual) for
        /// the card UI. Built by <see cref="ApplyGrouping(IEnumerable{ProductGroupDefinition})"/>;
        /// the control builds a single default group if grouping is never applied.
        /// </summary>
        public ObservableCollection<ProductGroupViewModel> ProductGroups
        {
            get => _productGroups;
            private set { if (!ReferenceEquals(_productGroups, value)) { _productGroups = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// Arrangement used to present the product groups. Set by the integrating
        /// developer at startup; defaults to <see cref="ProductLayout.Bands"/>.
        /// </summary>
        public ProductLayout Layout
        {
            get => _layout;
            set { if (_layout != value) { _layout = value; OnPropertyChanged(); } }
        }

        /// <summary>True once <see cref="ProductGroups"/> has been built.</summary>
        public bool HasGroups => _productGroups != null && _productGroups.Count > 0;

        /// <summary>
        /// Arranges <see cref="Products"/> into the supplied groups and selects the
        /// layout. A product not named in any group falls into a trailing default group.
        /// Safe to call with <c>null</c> to build a single default group from all products.
        /// </summary>
        public void ApplyGrouping(IEnumerable<ProductGroupDefinition> definitions, ProductLayout layout)
        {
            Layout = layout;
            ApplyGrouping(definitions);
        }

        /// <summary>
        /// Arranges <see cref="Products"/> into the supplied groups without changing the
        /// current <see cref="Layout"/>.
        /// </summary>
        public void ApplyGrouping(IEnumerable<ProductGroupDefinition> definitions)
        {
            var products = Products ?? new ObservableCollection<ProductViewModel>();
            var groups = new ObservableCollection<ProductGroupViewModel>();

            var byId = new Dictionary<string, ProductViewModel>();
            foreach (var p in products)
                if (p.Id != null && !byId.ContainsKey(p.Id))
                    byId[p.Id] = p;

            var assigned = new HashSet<ProductViewModel>();

            if (definitions != null)
            {
                foreach (var def in definitions)
                {
                    if (def == null) continue;
                    var members = new List<ProductViewModel>();
                    if (def.ProductIds != null)
                    {
                        foreach (var id in def.ProductIds)
                        {
                            ProductViewModel p;
                            if (id != null && byId.TryGetValue(id, out p) && assigned.Add(p))
                            {
                                p.GroupKey = def.Key;
                                members.Add(p);
                            }
                        }
                    }
                    if (members.Count > 0)
                        groups.Add(new ProductGroupViewModel(def.Key, def.Label, def.Caption, def.Accent, members));
                }
            }

            var leftovers = new List<ProductViewModel>();
            foreach (var p in products)
                if (!assigned.Contains(p))
                    leftovers.Add(p);

            if (leftovers.Count > 0)
            {
                // No definitions at all => one unlabeled group (just cards, no band header).
                // Some definitions but stragglers => a trailing "Other" group.
                var hadDefinitions = groups.Count > 0;
                const string key = "__default";
                foreach (var p in leftovers) p.GroupKey = key;
                groups.Add(new ProductGroupViewModel(key, hadDefinitions ? "Other" : null, null, null, leftovers));
            }

            ProductGroups = groups;
        }

        /// <summary>
        /// Makes <paramref name="product"/> the active card and (re)checks its license,
        /// caching the result onto the product so its card shows live status. Called from
        /// the card UI when a product is selected.
        /// </summary>
        internal void SelectProduct(ProductViewModel product, DependencyObject source)
        {
            if (product == null) return;
            SetActiveProduct(product);
            Product = product;
            CheckLiceneFile(source);
        }

        /// <summary>
        /// Makes <paramref name="product"/> the active card without re-checking its
        /// license. Used by the per-card action buttons (register / unregister / renew)
        /// so the existing commands operate on the right product.
        /// </summary>
        internal void MakeActive(ProductViewModel product)
        {
            if (product == null) return;
            SetActiveProduct(product);
            Product = product;
        }

        private void SetActiveProduct(ProductViewModel product)
        {
            if (_products == null) return;
            foreach (var p in _products)
                p.IsActive = ReferenceEquals(p, product);
        }

        private ProductViewModel ResolveProduct(ProductModel model)
        {
            if (model != null && model.Id != null && _products != null)
            {
                foreach (var p in _products)
                    if (p.Id == model.Id)
                        return p;
            }
            return _product;
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

        /// <summary>
        /// Loads every not-yet-loaded product's licence state in the background, showing a
        /// per-card spinner while each read runs. Reads are awaited one at a time so they
        /// never race on the licence file, and so the UI thread stays free (the window paints
        /// immediately and the spinners animate). Cards already seeded with a status by the
        /// host are left untouched.
        /// </summary>
        public async void LoadAllProducts(object source)
        {
            if (_products == null) return;
            var src = source as DependencyObject;

            var pending = new List<ProductViewModel>();
            foreach (var product in _products)
            {
                if (!product.IsChecked)
                {
                    product.IsLoading = true;   // show all spinners up front
                    pending.Add(product);
                }
            }

            foreach (var product in pending)
            {
                try { await LoadProductCoreAsync(product, src); }
                finally { product.IsLoading = false; }
            }
        }

        /// <summary>
        /// Re-reads a single product's licence (per-card Check / Refresh action), showing its
        /// spinner while the async read runs.
        /// </summary>
        public async void RefreshProduct(ProductViewModel product, object source)
        {
            if (product == null) return;
            MakeActive(product);
            product.IsLoading = true;
            try { await LoadProductCoreAsync(product, source as DependencyObject); }
            finally { product.IsLoading = false; }
        }

        /// <summary>
        /// Reads one product's licence off the UI thread and applies the result to that card
        /// only (without changing which card is active), so several cards can load without the
        /// active selection flickering.
        /// </summary>
        private async Task LoadProductCoreAsync(ProductViewModel product, DependencyObject source)
        {
            var prefs = PublisherPreferencesFactory.Build(VendorId, product.Id, _apiKey, PublicKey, ValidDays);
            var context = new LicHandlingContext(prefs);
            var handler = new LicenseHandlingLaunch(context, OnLicenseHandledSuccessfully: null);

            Exception error = null;
            await Task.Run(() =>
            {
                try { handler.HandleLicense(); }
                catch (Exception ex) { error = ex; }
            });

            var model = handler.HandlingContext.LicenseModel;
            if (error == null && model != null)
            {
                if (model.Product != null && !string.IsNullOrEmpty(model.Product.Name))
                    product.Name = model.Product.Name;
                product.UpdateLicenseSnapshot(model, ValidDays);
            }
            else
            {
                if (!product.IsChecked)
                    product.MarkUnverified();
                if (error != null)
                    ShowErrorView(source, context.Exception ?? error);
            }
        }

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
                    // If the failed check left the product without a determinate status,
                    // surface it as "Unverified" rather than an unloaded-looking card.
                    if (_product != null && !_product.IsChecked)
                        _product.MarkUnverified();
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
            Updated = model.Updated;
            if (model.Receipt != null)
            {
                ReceiptCode = model.Receipt.Code;
                ReceiptExpires = model.Receipt.Expires;
                CustomerEmail = model.Receipt.BuyerEmail;
            }

            // Cache the resolved license onto the matching product card (rather than
            // replacing Product with a throwaway VM) so each card keeps its own live
            // status as the user checks different products.
            var target = ResolveProduct(model.Product);
            if (target != null)
            {
                if (model.Product != null && !string.IsNullOrEmpty(model.Product.Name))
                    target.Name = model.Product.Name;
                Product = target;
                SetActiveProduct(target);

                // Don't blank out a card that already has a known status when a refresh
                // comes back with no usable result (Status.Unknown — e.g. offline / failed).
                if (model.Status != LicenseStatusTitles.Unknown || !target.IsChecked)
                    target.UpdateLicenseSnapshot(model, ValidDays);
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
                RefreshProduct(Product, source ?? owner);   // async refresh with the card spinner
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
                RefreshProduct(Product, source ?? owner);   // async refresh with the card spinner
            };
            view.Closed += onClosed;
            view.ShowDialog();
        }
    }
}
