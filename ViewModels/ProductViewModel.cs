using LicenseManagement.EndUser.License;
using LicenseManagement.EndUser.Models;
using System;

namespace LicenseManagement.EndUser.Wpf.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        // Fallback window (days) used for the meter when the license has no Created date.
        private const double FallbackWindowDays = 90.0;

        public static ProductViewModel FromProductModel(ProductModel model)
        {
            if (model == null)
                return null;

            return new ProductViewModel
            {
                Id = model.Id,
                Name = model.Name,
                Created = model.Created,
                Updated = model.Updated
            };
        }

        private string _id;
        private string _name;
        private string _groupKey;
        private bool _isActive;

        private LicenseStatusTitles? _status;
        private DateTime? _expires;
        private DateTime? _receiptExpires;
        private DateTime? _trialExpires;
        private string _customerEmail;
        private string _receiptCode;

        // Period boundaries used to size the validity meter. These come from the LICENSE
        // and RECEIPT (not the product definition), so the bar reflects the real window.
        private DateTime? _licenseCreated;
        private DateTime? _licenseUpdated;
        private DateTime? _receiptCreated;
        private uint _validDays;

        /// <summary>
        /// this is actually the Ulid of the product with prefix PRD_
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// this is the user defined name of the product
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Key of the <see cref="ProductGroupViewModel"/> this product belongs to.
        /// Assigned by <c>LicenseViewModel.ApplyGrouping</c>; purely presentational.
        /// </summary>
        public string GroupKey
        {
            get => _groupKey;
            set { if (_groupKey != value) { _groupKey = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// True for the card the user last selected. Drives the selected-card highlight.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set { if (_isActive != value) { _isActive = value; OnPropertyChanged(); } }
        }

        private bool _isLoading;
        /// <summary>
        /// True while this card is fetching its licence state from the SDK. Drives the
        /// per-card spinner so the user sees work happening during the (async) read.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set { if (_isLoading != value) { _isLoading = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// This product's license status, or <c>null</c> until it has been checked.
        /// Setting any snapshot value refreshes the computed card values.
        /// </summary>
        public LicenseStatusTitles? Status
        {
            get => _status;
            private set { if (_status != value) { _status = value; OnPropertyChanged(); } }
        }

        public DateTime? Expires
        {
            get => _expires;
            private set { if (_expires != value) { _expires = value; OnPropertyChanged(); } }
        }

        public DateTime? ReceiptExpires
        {
            get => _receiptExpires;
            private set { if (_receiptExpires != value) { _receiptExpires = value; OnPropertyChanged(); } }
        }

        public DateTime? TrialExpires
        {
            get => _trialExpires;
            private set { if (_trialExpires != value) { _trialExpires = value; OnPropertyChanged(); } }
        }

        public string CustomerEmail
        {
            get => _customerEmail;
            private set { if (_customerEmail != value) { _customerEmail = value; OnPropertyChanged(); } }
        }

        public string ReceiptCode
        {
            get => _receiptCode;
            private set { if (_receiptCode != value) { _receiptCode = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// True once this product has a <em>determinate</em> license status. A check that
        /// came back <see cref="LicenseStatusTitles.Unknown"/> (couldn't be verified) is
        /// treated as not-yet-checked so the card offers to check again rather than
        /// pretending it loaded.
        /// </summary>
        public bool IsChecked => _status.HasValue && _status.Value != LicenseStatusTitles.Unknown;

        /// <summary>The expiry date that matters for the current status (trial / subscription / file).</summary>
        public DateTime? PrimaryExpiry
        {
            get
            {
                switch (_status)
                {
                    case LicenseStatusTitles.ValidTrial:
                    case LicenseStatusTitles.InvalidTrial:
                        return _trialExpires;
                    case LicenseStatusTitles.Valid:
                        return _receiptExpires ?? _expires;
                    case LicenseStatusTitles.ReceiptExpired:
                        return _receiptExpires ?? _expires;
                    case LicenseStatusTitles.Expired:
                        return _expires ?? _receiptExpires;
                    case LicenseStatusTitles.ReceiptUnregistered:
                    case LicenseStatusTitles.Unknown:
                        return null;
                    default:
                        return _expires ?? _receiptExpires;
                }
            }
        }

        /// <summary>True when there is a date to show a meter / countdown for.</summary>
        public bool HasMeter => PrimaryExpiry.HasValue;

        /// <summary>Whole days remaining until <see cref="PrimaryExpiry"/> (never negative).</summary>
        public int DaysLeft
        {
            get
            {
                var expiry = PrimaryExpiry;
                if (!expiry.HasValue) return 0;
                var remaining = (ToUtc(expiry.Value) - DateTime.UtcNow).TotalDays;
                return remaining <= 0 ? 0 : (int)Math.Floor(remaining);
            }
        }

        /// <summary>
        /// Remaining fraction (0..1) of the current validity window, for the meter bar.
        /// The window is the real period for the status: the subscription term
        /// (<c>Receipt.Created → Receipt.Expires</c>) for a paid license, the trial window
        /// (<c>License.Created → TrialEndDate</c>) for a trial, or the issued file window
        /// (<c>License.Updated → Expires</c>, falling back to the publisher's ValidDays).
        /// </summary>
        public double MeterFraction
        {
            get
            {
                var expiry = PrimaryExpiry;
                if (!expiry.HasValue) return 0;

                var expiryUtc = ToUtc(expiry.Value);
                var now = DateTime.UtcNow;
                if (expiryUtc <= now) return 0;

                var start = WindowStart(expiryUtc);
                var window = (expiryUtc - start).TotalDays;
                if (window <= 0) return 1;

                var remaining = (expiryUtc - now).TotalDays / window;
                return remaining < 0 ? 0 : (remaining > 1 ? 1 : remaining);
            }
        }

        /// <summary>
        /// Start of the validity window that <see cref="PrimaryExpiry"/> closes, chosen
        /// from the real period boundaries for the current status. Falls back to
        /// "<paramref name="expiryUtc"/> minus the publisher's ValidDays" when the period
        /// start is unknown, so the bar is always meaningful rather than guessed.
        /// </summary>
        private DateTime WindowStart(DateTime expiryUtc)
        {
            DateTime? start;
            switch (_status)
            {
                case LicenseStatusTitles.ValidTrial:
                case LicenseStatusTitles.InvalidTrial:
                    start = _licenseCreated;                                   // trial began at license creation
                    break;
                case LicenseStatusTitles.Valid:
                case LicenseStatusTitles.ReceiptExpired:
                    start = _receiptCreated ?? _licenseUpdated ?? _licenseCreated; // subscription term
                    break;
                default:
                    start = _licenseUpdated ?? _licenseCreated;                // last file issue
                    break;
            }

            if (start.HasValue)
            {
                var s = ToUtc(start.Value);
                if (s < expiryUtc)
                    return s;
            }

            var days = _validDays > 0 ? _validDays : (uint)FallbackWindowDays;
            return expiryUtc.AddDays(-(double)days);
        }

        /// <summary>Caption shown before the date (e.g. "Renews", "Trial ends").</summary>
        public string ExpiryCaption
        {
            get
            {
                switch (_status)
                {
                    case LicenseStatusTitles.ValidTrial: return "Trial ends";
                    case LicenseStatusTitles.InvalidTrial: return "Trial ended";
                    case LicenseStatusTitles.Valid: return _receiptExpires.HasValue ? "Renews" : "Expires";
                    case LicenseStatusTitles.ReceiptExpired: return "Renewal due";
                    case LicenseStatusTitles.Expired: return "Expired";
                    default: return "Expires";
                }
            }
        }

        /// <summary>Local short-date string of <see cref="PrimaryExpiry"/>, or empty.</summary>
        public string PrimaryExpiryText
        {
            get
            {
                var expiry = PrimaryExpiry;
                if (!expiry.HasValue) return string.Empty;
                return ToUtc(expiry.Value).ToLocalTime().ToString("d");
            }
        }

        /// <summary>One-line description of the licensing state, shown under the name.</summary>
        public string MetaText
        {
            get
            {
                if (!_status.HasValue)
                    return "Status not loaded — choose Check to load it";
                switch (_status.Value)
                {
                    case LicenseStatusTitles.Valid:
                        return string.IsNullOrEmpty(_customerEmail)
                            ? "Licensed on this computer"
                            : "Licensed to " + _customerEmail;
                    case LicenseStatusTitles.ValidTrial:
                        return "Free trial — not purchased yet";
                    case LicenseStatusTitles.InvalidTrial:
                        return "Trial ended — activation required";
                    case LicenseStatusTitles.Expired:
                        return "License file expired";
                    case LicenseStatusTitles.ReceiptExpired:
                        return "Subscription needs renewal";
                    case LicenseStatusTitles.ReceiptUnregistered:
                        return "This computer has been unregistered";
                    case LicenseStatusTitles.Unknown:
                        return "Couldn't verify — check your connection and try again";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>"19 days" style countdown, or "expired" / "—".</summary>
        public string DaysLeftText
        {
            get
            {
                if (!HasMeter) return "—";
                var days = DaysLeft;
                if (days <= 0) return "expired";
                return days == 1 ? "1 day" : days + " days";
            }
        }

        /// <summary>
        /// Copies the resolved license result for this product onto the card, capturing the
        /// real license/receipt period boundaries so the meter is accurate, then refreshes
        /// every computed value. <paramref name="validDays"/> is the publisher's configured
        /// license length, used only as a last-resort window when no period start is known.
        /// </summary>
        public void UpdateLicenseSnapshot(LicenseModel model, uint validDays)
        {
            if (model == null) return;

            Status = model.Status;
            Expires = model.Expires;
            // Period-start dates feed the validity-meter window. A server model can leave any
            // of these as default(DateTime) (0001-01-01); used as-is the window spans ~2000
            // years, so the meter reads ~0% even with weeks left. Treat default as unknown
            // (null) — WindowStart then falls back to the publisher's ValidDays.
            _licenseCreated = NullIfDefault(model.Created);
            _licenseUpdated = NullIfDefault(model.Updated);
            _validDays = validDays;

            var receipt = model.Receipt;
            _receiptCreated = receipt != null ? NullIfDefault(receipt.Created) : null;
            ReceiptExpires = receipt != null ? receipt.Expires : null;
            CustomerEmail = receipt != null ? receipt.BuyerEmail : null;
            ReceiptCode = receipt != null ? receipt.Code : null;

            // TrialEndDate is non-nullable on the model; treat the default as "no trial".
            TrialExpires = NullIfDefault(model.TrialEndDate);

            RaiseComputed();
        }

        /// <summary>
        /// Marks this product as checked-but-unverifiable (e.g. the check threw or returned
        /// nothing). The card then shows an "Unverified" state and offers to try again,
        /// instead of looking like it was never loaded.
        /// </summary>
        internal void MarkUnverified()
        {
            Status = LicenseStatusTitles.Unknown;
            Expires = null;
            ReceiptExpires = null;
            TrialExpires = null;
            RaiseComputed();
        }

        private void RaiseComputed()
        {
            OnPropertyChanged(nameof(IsChecked));
            OnPropertyChanged(nameof(PrimaryExpiry));
            OnPropertyChanged(nameof(HasMeter));
            OnPropertyChanged(nameof(DaysLeft));
            OnPropertyChanged(nameof(MeterFraction));
            OnPropertyChanged(nameof(ExpiryCaption));
            OnPropertyChanged(nameof(PrimaryExpiryText));
            OnPropertyChanged(nameof(DaysLeftText));
            OnPropertyChanged(nameof(MetaText));
        }

        /// <summary>Treats <c>default(DateTime)</c> (0001-01-01) as "no date" so a missing
        /// server date never produces a meaningless multi-century validity window.</summary>
        private static DateTime? NullIfDefault(DateTime? value) =>
            (value == null || value.Value == default(DateTime)) ? (DateTime?)null : value;

        private static DateTime ToUtc(DateTime dt)
        {
            // Server timestamps arrive as Unspecified; treat them as UTC (matches UtcToLocalTimeConverter).
            return dt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                : dt.ToUniversalTime();
        }
    }
}
