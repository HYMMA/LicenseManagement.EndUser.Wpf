using LicenseManagement.EndUser.Wpf.ViewModels;
using System.Windows;

namespace LicenseManagement.EndUser.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ErrorView.xaml
    /// </summary>
    public partial class ErrorView : Window
    {
        public ErrorView()
        {
            InitializeComponent();
        }

        private void OnOpenPortalClick(object sender, RoutedEventArgs e)
        {
            // the browser opens alongside; keep this dialog so the user can come back to Install
            (DataContext as ErrorViewModel)?.OpenPortal?.Invoke();
        }

        private void OnInstallFileClick(object sender, RoutedEventArgs e)
        {
            // close first: the file picker and the refreshed card belong to the license window
            var install = (DataContext as ErrorViewModel)?.InstallLicenseFile;
            Close();
            install?.Invoke();
        }
    }
}
