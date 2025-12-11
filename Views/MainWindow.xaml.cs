using Hymma.Lm.EndUser.Wpf.ViewModels;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Hymma.Lm.EndUser.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
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
            var win = sender as MainWindow;
            win.License = e.NewValue as LicenseViewModel;
            //win.OnLicenseChanged(e);
        }

        public virtual void OnLicenseChanged(DependencyPropertyChangedEventArgs e)
        {
            var newVal = e.NewValue as LicenseViewModel;
            //newVal.
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
                        //ProductId = ConfigurationManager.AppSettings.Get("productId") ?? "",
                        VendorId = ConfigurationManager.AppSettings.Get("vendorId") ?? "",
                        PublicKey = ConfigurationManager.AppSettings.Get("publicKey") ?? "",
                        ApiKey = ConfigurationManager.AppSettings.Get("ApiKey") ?? "",
                    };
                    var ps = (NameValueCollection)ConfigurationManager.GetSection("Products");
                    defaultValue.Products = new System.Collections.ObjectModel.ObservableCollection<ProductViewModel>();
                    foreach (var item in ps.AllKeys)
                    {
                        defaultValue.Products.Add(new ProductViewModel { Id = item, Name = ps[item] });
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

        static MainWindow()
        {
            LicenseProperty = DependencyProperty.Register(nameof(License),
                typeof(LicenseViewModel),
                ownerType: typeof(MainWindow),
                typeMetadata: Meta);
        }
        #endregion

        private void OnProductsSlectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var newSelection = e.AddedItems[0] as ProductViewModel;
                License.CheckLiceneFile(this);
            }
        }


        private void thisView_Loaded(object sender, RoutedEventArgs e)
        {

            productsComboBox.SelectedIndex = 0;
            //var presenter = productsComboBox.Template.FindName("PART_ItemsPresenter", productsComboBox);
            // Example: Access ComboBoxItem safely

            //var item = productsComboBox.ItemContainerGenerator.ContainerFromIndex(0) as ComboBoxItem;
            //if (item != null)
            //{
            //    Debug.WriteLine("ComboBoxItem is ready: " + item.Content);
            //}

            //License.RenewLicenseFileAction(this);
            if (Application.ResourceAssembly == null)
            {
                Application.ResourceAssembly = Assembly.GetExecutingAssembly();
            }
            if (licenseImage.Source == null)
            {
                licenseImage.Source = new BitmapImage(new Uri("pack://application:,,,/Hymma.Lm.EndUser.Wpf;component/Assets/license.png", UriKind.Absolute));
            }
            //var rebuilt = productsComboBox.ApplyTemplate();

            // You can also inspect or adjust bindings here
        }
    }
}
