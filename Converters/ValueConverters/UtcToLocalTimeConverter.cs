using System;
using System.Globalization;
using System.Windows.Data;

namespace Hymma.Lm.EndUser.Wpf.Converters
{
    internal class UtcToLocalTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                return date.ToLocalTime().ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                return null;
            }
            //return DateTime.SpecifyKind(DateTime.Parse(value as string), DateTimeKind.Utc).ToLocalTime();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
