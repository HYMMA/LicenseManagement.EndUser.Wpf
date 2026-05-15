using System;
using System.Globalization;
using System.Windows.Data;

namespace LicenseManagement.EndUser.Wpf.Converters
{
    internal class UtcToLocalTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                // XML-serialised DateTime arrives with Kind=Unspecified; treat it as UTC
                // (server timestamps are always UTC) before converting to local time.
                var utc = date.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
                    : date;
                return utc.ToLocalTime().ToString(culture ?? CultureInfo.CurrentCulture);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
