using LicenseManagement.EndUser.Wpf.Configuration;
using LicenseManagement.EndUser.Wpf.ViewModels;
using System.Windows;

namespace LicenseManagement.EndUser.Wpf
{
    /// <summary>
    /// Standalone license window. A thin host around
    /// <see cref="Views.LicenseControl"/> — all UI and behaviour live in the control.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region dependency property set up
        public static readonly DependencyProperty LicenseProperty =
            DependencyProperty.Register(
                nameof(License),
                typeof(LicenseViewModel),
                typeof(MainWindow),
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
    }
}
