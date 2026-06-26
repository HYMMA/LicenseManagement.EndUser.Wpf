using LicenseManagement.EndUser.Wpf.Configuration;
using LicenseManagement.EndUser.Wpf.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LicenseManagement.EndUser.Wpf.Views
{
    /// <summary>
    /// A UserControl for displaying and managing license information as grouped product
    /// cards. Can be embedded directly in any WPF application.
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

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Control_Loaded;

            var vm = License;
            if (vm == null) return;

            // Build a default single group if the developer never applied grouping,
            // so existing app-settings consumers still render as cards.
            if (vm.ProductGroups == null)
                vm.ApplyGrouping(null);

            if (vm.Products == null || vm.Products.Count == 0 || !vm.HasApiKey)
                return;

            // Read every not-yet-loaded card's licence in the BACKGROUND, each showing its own
            // spinner while it loads. The reads run off the UI thread (and one at a time, so
            // they don't race on the licence file), so the window paints immediately instead of
            // freezing. Cards the host already seeded are skipped.
            vm.LoadAllProducts(this);
        }

        private void OnCheckClick(object sender, RoutedEventArgs e)
        {
            var product = (sender as FrameworkElement)?.DataContext as ProductViewModel;
            if (product != null)
                License?.RefreshProduct(product, this);   // async; shows the card spinner
        }

        private void OnRegisterClick(object sender, RoutedEventArgs e) =>
            InvokeOnProduct(sender, License?.ShowRegisterView);

        private void OnUnregisterClick(object sender, RoutedEventArgs e) =>
            InvokeOnProduct(sender, License?.ShowUnregisterView);

        private void OnRenewClick(object sender, RoutedEventArgs e) =>
            InvokeOnProduct(sender, License?.RenewLicenseFile);

        private void InvokeOnProduct(object sender, ICommand command)
        {
            var product = (sender as FrameworkElement)?.DataContext as ProductViewModel;
            if (product == null || License == null || command == null) return;

            License.MakeActive(product);
            if (command.CanExecute(this))
                command.Execute(this);
        }
    }
}
