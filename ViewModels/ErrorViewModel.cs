using System;

namespace LicenseManagement.EndUser.Wpf.ViewModels
{
    /// <summary>
    /// What the error dialog shows: the failure, and - when the license server could not be
    /// reached - the ways the user can still activate this computer without it (install a
    /// license file they downloaded, or open the vendor's license portal to get one).
    /// </summary>
    public sealed class ErrorViewModel
    {
        public ErrorViewModel(Exception error)
        {
            Error = error;
        }

        public Exception Error { get; }

        public string Message => Error?.Message;

        public string Detail => Error?.InnerException?.Message;

        public string MoreDetail => Error?.InnerException?.InnerException?.Message;

        /// <summary>Runs after the dialog closes; null hides the button.</summary>
        public Action InstallLicenseFile { get; set; }

        /// <summary>Opens the vendor's license portal; null hides the button.</summary>
        public Action OpenPortal { get; set; }

        public bool CanInstallLicenseFile => InstallLicenseFile != null;

        public bool CanOpenPortal => OpenPortal != null;

        public bool HasRecovery => CanInstallLicenseFile || CanOpenPortal;
    }
}
