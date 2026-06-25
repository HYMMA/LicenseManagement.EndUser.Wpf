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

            // Check the first product so its card shows live status on open (mirrors the
            // old dropdown selecting index 0). Other cards load when the user opens them.
            // Skip when it is already checked or when no API key is configured (designer /
            // unconfigured host) so we never fire a pointless server call on launch.
            var first = vm.Products != null && vm.Products.Count > 0 ? vm.Products[0] : null;
            if (first != null && !first.IsChecked && vm.HasApiKey)
                vm.SelectProduct(first, this);
        }

        private void OnCheckClick(object sender, RoutedEventArgs e)
        {
            var product = (sender as FrameworkElement)?.DataContext as ProductViewModel;
            if (product != null)
                License?.SelectProduct(product, this);
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
