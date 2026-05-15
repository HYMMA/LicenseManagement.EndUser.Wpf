using LicenseManagement.EndUser.Wpf.Configuration;
using LicenseManagement.EndUser.Wpf.ViewModels;
using System;
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
        public static readonly DependencyProperty LicenseProperty =
            DependencyProperty.Register(
                nameof(License),
                typeof(LicenseViewModel),
                typeof(LicenseControl),
                new PropertyMetadata(LicenseConfigurationLoader.TryLoad(), OnLicenseChanged));

        /// <summary>
        /// CLR wrapper for the License dependency property.
        /// </summary>
        public LicenseViewModel License
        {
            get => (LicenseViewModel)GetValue(LicenseProperty);
            set => SetValue(LicenseProperty, value);
        }

        private static void OnLicenseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // dependency property already stores the new value; nothing extra to do
        }
        #endregion

        private void OnProductsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                License?.CheckLiceneFile(this);
            }
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Control_Loaded;

            if (productsComboBox.Items.Count > 0)
                productsComboBox.SelectedIndex = 0;

            if (licenseImage.Source == null)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/LicenseManagement.EndUser.Wpf;component/Assets/license.png", UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                licenseImage.Source = bitmap;
            }
        }
    }
}
