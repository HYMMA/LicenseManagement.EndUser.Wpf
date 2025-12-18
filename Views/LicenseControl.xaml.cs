using LicenseManagement.EndUser.Wpf.ViewModels;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LicenseManagement.EndUser.Wpf.Views
{
    /// <summary>
    /// A UserControl for displaying and managing license information.
    /// Can be embedded directly in any WPF application.
    /// </summary>
    /// <example>
    /// XAML usage:
    /// <code>
    /// &lt;Window xmlns:lm="clr-namespace:LicenseManagement.EndUser.Wpf.Views;assembly=LicenseManagement.EndUser.Wpf"&gt;
    ///     &lt;lm:LicenseControl /&gt;
    /// &lt;/Window&gt;
    /// </code>
    /// </example>
    public partial class LicenseControl : UserControl
    {
        public LicenseControl()
        {
            InitializeComponent();
        }

        #region dependency property set up
        public static DependencyProperty LicenseProperty { get; }

        /// <summary>
        /// CLR wrapper for dependency property
        /// </summary>
        public LicenseViewModel License
        {
            get => GetValue(LicenseProperty) as LicenseViewModel;
            set => SetValue(LicenseProperty, value);
        }

        public static void LicenseChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as LicenseControl;
            control.License = e.NewValue as LicenseViewModel;
        }

        public virtual void OnLicenseChanged(DependencyPropertyChangedEventArgs e)
        {
            var newVal = e.NewValue as LicenseViewModel;
        }

        public static PropertyMetadata Meta
        {
            get
            {
                if (ConfigurationManager.AppSettings.Count != 0)
                {
                    if (!uint.TryParse(ConfigurationManager.AppSettings.Get("validDays"), out uint valid))
                        valid = 90;

                    var defaultValue = new LicenseViewModel()
                    {
                        Expires = DateTime.Now,
                        ValidDays = valid,
                        VendorId = ConfigurationManager.AppSettings.Get("vendorId") ?? "",
                        PublicKey = ConfigurationManager.AppSettings.Get("publicKey") ?? "",
                        ApiKey = ConfigurationManager.AppSettings.Get("ApiKey") ?? "",
                    };
                    var ps = (NameValueCollection)ConfigurationManager.GetSection("Products");
                    defaultValue.Products = new System.Collections.ObjectModel.ObservableCollection<ProductViewModel>();
                    if (ps != null)
                    {
                        foreach (var item in ps.AllKeys)
                        {
                            defaultValue.Products.Add(new ProductViewModel { Id = item, Name = ps[item] });
                        }
                    }
                    defaultValue.Product = defaultValue.Products.FirstOrDefault();
                    var model = new PropertyMetadata(defaultValue, propertyChangedCallback: LicenseChanged);
                    return model;
                }
                else
                {
                    return null;
                }
            }
        }

        static LicenseControl()
        {
            LicenseProperty = DependencyProperty.Register(nameof(License),
                typeof(LicenseViewModel),
                ownerType: typeof(LicenseControl),
                typeMetadata: Meta);
        }
        #endregion

        private void OnProductsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var newSelection = e.AddedItems[0] as ProductViewModel;
                License?.CheckLiceneFile(this);
            }
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            productsComboBox.SelectedIndex = 0;

            if (Application.ResourceAssembly == null)
            {
                Application.ResourceAssembly = Assembly.GetExecutingAssembly();
            }
            if (licenseImage.Source == null)
            {
                licenseImage.Source = new BitmapImage(new Uri("pack://application:,,,/LicenseManagement.EndUser.Wpf;component/Assets/license.png", UriKind.Absolute));
            }
        }
    }
}
