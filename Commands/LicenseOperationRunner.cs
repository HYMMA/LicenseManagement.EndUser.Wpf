using LicenseManagement.EndUser.Exceptions;
using System;
using System.Diagnostics;

namespace LicenseManagement.EndUser.Wpf.Commands
{
    /// <summary>
    /// Runs an <see cref="LicenseHandlingStrategy"/> with narrow exception handling and
    /// trace-level logging. Replaces the four near-identical try / catch / ShowErrorView
    /// blocks that were duplicated across the license view models.
    /// </summary>
    internal static class LicenseOperationRunner
    {
        public static bool Run(LicenseHandlingStrategy handler, Action<Exception> onError)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            try
            {
                handler.HandleLicense();
                return true;
            }
            catch (ApiException apiEx)
            {
                Trace.TraceError($"[LicenseOperationRunner] API call failed — {apiEx.Message}");
                onError?.Invoke(apiEx);
            }
            catch (ReceiptCodeException ex)
            {
                Trace.TraceWarning($"[LicenseOperationRunner] Receipt code rejected — {ex.Message}");
                onError?.Invoke(ex);
            }
            catch (ReceiptNotAvailableException ex)
            {
                Trace.TraceWarning($"[LicenseOperationRunner] Receipt unavailable — {ex.Message}");
                onError?.Invoke(ex);
            }
            catch (ProductNameException ex)
            {
                Trace.TraceWarning($"[LicenseOperationRunner] Product name mismatch — {ex.Message}");
                onError?.Invoke(ex);
            }
            catch (ComputerNameException ex)
            {
                Trace.TraceWarning($"[LicenseOperationRunner] Computer name mismatch — {ex.Message}");
                onError?.Invoke(ex);
            }
            catch (ComputerOfflineException ex)
            {
                Trace.TraceWarning($"[LicenseOperationRunner] Computer offline — {ex.Message}");
                onError?.Invoke(ex);
            }
            catch (CouldNotReadLicenseFromDiskException ex)
            {
                Trace.TraceError($"[LicenseOperationRunner] Cannot read license file — {ex.Message}");
                onError?.Invoke(ex);
            }
            catch (CouldNotWriteLicenseOnDiskException ex)
            {
                Trace.TraceError($"[LicenseOperationRunner] Cannot write license file — {ex.Message}");
                onError?.Invoke(ex);
            }
            catch (CouldNotReadComputerIdException ex)
            {
                Trace.TraceError($"[LicenseOperationRunner] Cannot read computer id — {ex.Message}");
                onError?.Invoke(ex);
            }
            catch (CouldNotUnregisterComputerException ex)
            {
                Trace.TraceError($"[LicenseOperationRunner] Cannot unregister computer — {ex.Message}");
                onError?.Invoke(ex);
            }
            catch (CouldNotPatchLicenseWithReceiptException ex)
            {
                Trace.TraceError($"[LicenseOperationRunner] Cannot patch license with receipt — {ex.Message}");
                onError?.Invoke(ex);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"[LicenseOperationRunner] Unhandled {ex.GetType().Name} — {ex.Message}");
                onError?.Invoke(ex);
            }

            return false;
        }
    }
}
