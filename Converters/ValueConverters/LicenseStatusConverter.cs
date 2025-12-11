using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Hymma.Lm.EndUser.License;

namespace Hymma.Lm.EndUser.Wpf.Converters
{

    [ValueConversion(typeof(LicenseStatusTitles), typeof(string))]
    internal class LicenseStatusConverter : IValueConverter
    {
        private const string COMPUTER_UNREGISTERED = "Computer has been unregistered.";
        private const string RECEIPT_EXPIRED = "Payment is suspended or subscription needs renewal.";
        private const string INVALID_TRIAL = "Trial ended and requires activation.";
        private const string ACTIVE_TRIAL = "Trial and active.";
        private const string ACTIVE_PAID = "Paid and active.";
        private const string EXPIRED = "License file expired.";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var st = (LicenseStatusTitles)value;
            switch (st)
            {
                case LicenseStatusTitles.Expired:
                    return EXPIRED;
                case LicenseStatusTitles.Valid:
                    return ACTIVE_PAID;
                case LicenseStatusTitles.ValidTrial:
                    return ACTIVE_TRIAL;
                case LicenseStatusTitles.InValidTrial:
                    return INVALID_TRIAL;
                case LicenseStatusTitles.ReceiptExpired:
                    return RECEIPT_EXPIRED;
                case LicenseStatusTitles.ReceiptUnregistered:
                    return COMPUTER_UNREGISTERED;
                default:
                    return "Unknown";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(LicenseStatusTitles), typeof(Visibility))]
    internal class LicenseStatusVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var st = ((LicenseStatusTitles)value);
            switch (st)
            {
                case LicenseStatusTitles.Expired:
                    return Visibility.Collapsed;
                case LicenseStatusTitles.Valid:
                    return Visibility.Collapsed;
                case LicenseStatusTitles.ValidTrial:
                    return Visibility.Visible;
                case LicenseStatusTitles.InValidTrial:
                    return Visibility.Visible;
                case LicenseStatusTitles.ReceiptExpired:
                    return Visibility.Visible;
                case LicenseStatusTitles.ReceiptUnregistered:
                    return Visibility.Visible;
                default:
                    return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(LicenseStatusTitles), typeof(SolidColorBrush))]
    internal class LicenseStatusBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var st = (LicenseStatusTitles)value;
            switch (st)
            {
                case LicenseStatusTitles.Expired:
                    return new SolidColorBrush(Colors.LightSalmon);
                case LicenseStatusTitles.Valid:
                    return new SolidColorBrush(Colors.Azure);
                case LicenseStatusTitles.ValidTrial:
                    return new SolidColorBrush(Colors.Azure);
                case LicenseStatusTitles.InValidTrial:
                    return new SolidColorBrush(Colors.LightSalmon);
                case LicenseStatusTitles.ReceiptExpired:
                    return new SolidColorBrush(Colors.LightSalmon);
                case LicenseStatusTitles.ReceiptUnregistered:
                    return new SolidColorBrush(Colors.LightGray);
                default:
                    return new SolidColorBrush();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(LicenseStatusTitles), typeof(Brush))]
    internal class LicenseStatusBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var st = (LicenseStatusTitles)value;
            switch (st)
            {
                case LicenseStatusTitles.Expired:
                    return new SolidColorBrush(Colors.DarkRed);
                case LicenseStatusTitles.Valid:
                    return new SolidColorBrush(Colors.Green);
                case LicenseStatusTitles.ValidTrial:
                    return new SolidColorBrush(Colors.Blue);
                case LicenseStatusTitles.InValidTrial:
                    return new SolidColorBrush(Colors.DarkRed);
                case LicenseStatusTitles.ReceiptExpired:
                    return new SolidColorBrush(Colors.DarkRed);
                case LicenseStatusTitles.ReceiptUnregistered:
                    return new SolidColorBrush(Colors.Black);
                default:
                    return new SolidColorBrush();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    [ValueConversion(typeof(LicenseStatusTitles), typeof(Visibility))]
    internal class UnregisterVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var st = ((LicenseStatusTitles)value);
            switch (st)
            {
                case LicenseStatusTitles.Valid:
                    return Visibility.Visible;
                default:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(LicenseStatusTitles), typeof(Visibility))]
    internal class RenewLicenseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var st = ((LicenseStatusTitles)value);
            switch (st)
            {
                case LicenseStatusTitles.Expired:
                    return Visibility.Visible;
                default:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(LicenseStatusTitles), typeof(Visibility))]
    internal class SubscriptionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var st = ((LicenseStatusTitles)value);
            switch (st)
            {
                case LicenseStatusTitles.ReceiptExpired: return Visibility.Collapsed;
                case LicenseStatusTitles.ValidTrial: return Visibility.Collapsed;
                case LicenseStatusTitles.InValidTrial: return Visibility.Collapsed;
                case LicenseStatusTitles.ReceiptUnregistered: return Visibility.Collapsed;
                default:
                    return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(LicenseStatusTitles), typeof(Visibility))]
    internal class TrialVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var st = ((LicenseStatusTitles)value);
            switch (st)
            {
                case LicenseStatusTitles.ValidTrial:
                    return Visibility.Visible;
                default:
                    return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}