using LicenseManagement.EndUser.License;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace LicenseManagement.EndUser.Wpf.Converters
{
    /// <summary>
    /// Presentation metadata for each <see cref="LicenseStatusTitles"/> value.
    /// Keeping all status-driven UI in one table eliminates the seven near-identical
    /// switch-based converters the audit flagged.
    /// </summary>
    internal sealed class LicenseStatusPresentation
    {
        public string Description { get; set; }
        public Color Background { get; set; }
        public Color Border { get; set; }
        public bool ShowRegisterPrompt { get; set; }
        public bool ShowSubscriptionPanel { get; set; }
        public bool ShowTrialPanel { get; set; }
        public bool ShowUnregister { get; set; }
        public bool ShowRenewFile { get; set; }
    }

    /// <summary>
    /// Single dictionary-driven converter. The optional ConverterParameter selects
    /// which facet of the presentation to return.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;TextBlock Text="{Binding Status, Converter={StaticResource LicenseStatusConverter}, ConverterParameter=Description}" /&gt;
    /// &lt;Border Background="{Binding Status, Converter={StaticResource LicenseStatusConverter}, ConverterParameter=Background}" /&gt;
    /// </code>
    /// </example>
    [ValueConversion(typeof(LicenseStatusTitles), typeof(object))]
    internal sealed class LicenseStatusConverter : IValueConverter
    {
        private static readonly LicenseStatusPresentation Fallback = new LicenseStatusPresentation
        {
            Description = "Unknown",
            Background = Colors.Transparent,
            Border = Colors.Gray,
        };

        private static readonly IReadOnlyDictionary<LicenseStatusTitles, LicenseStatusPresentation> Table =
            new Dictionary<LicenseStatusTitles, LicenseStatusPresentation>
            {
                [LicenseStatusTitles.Expired] = new LicenseStatusPresentation
                {
                    Description = "License file expired.",
                    Background = Colors.LightSalmon,
                    Border = Colors.DarkRed,
                    ShowRenewFile = true,
                    ShowSubscriptionPanel = true,
                },
                [LicenseStatusTitles.Valid] = new LicenseStatusPresentation
                {
                    Description = "Paid and active.",
                    Background = Colors.Azure,
                    Border = Colors.Green,
                    ShowUnregister = true,
                    ShowSubscriptionPanel = true,
                },
                [LicenseStatusTitles.ValidTrial] = new LicenseStatusPresentation
                {
                    Description = "Trial and active.",
                    Background = Colors.Azure,
                    Border = Colors.Blue,
                    ShowRegisterPrompt = true,
                    ShowTrialPanel = true,
                },
                [LicenseStatusTitles.InvalidTrial] = new LicenseStatusPresentation
                {
                    Description = "Trial ended and requires activation.",
                    Background = Colors.LightSalmon,
                    Border = Colors.DarkRed,
                    ShowRegisterPrompt = true,
                },
                [LicenseStatusTitles.ReceiptExpired] = new LicenseStatusPresentation
                {
                    Description = "Payment is suspended or subscription needs renewal.",
                    Background = Colors.LightSalmon,
                    Border = Colors.DarkRed,
                    ShowRegisterPrompt = true,
                },
                [LicenseStatusTitles.ReceiptUnregistered] = new LicenseStatusPresentation
                {
                    Description = "Computer has been unregistered.",
                    Background = Colors.LightGray,
                    Border = Colors.Black,
                    ShowRegisterPrompt = true,
                },
            };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!TryGetStatus(value, out var status))
                return Resolve(targetType, parameter, Fallback);

            if (!Table.TryGetValue(status, out var p))
            {
                Debug.WriteLine($"[LicenseStatusConverter] No presentation registered for {status}");
                p = Fallback;
            }

            return Resolve(targetType, parameter, p);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();

        private static bool TryGetStatus(object value, out LicenseStatusTitles status)
        {
            if (value is LicenseStatusTitles s)
            {
                status = s;
                return true;
            }
            status = default;
            return false;
        }

        private static object Resolve(Type targetType, object parameter, LicenseStatusPresentation p)
        {
            var facet = parameter as string;
            switch (facet)
            {
                case "Background":
                    return FrozenBrush(p.Background);
                case "Border":
                    return FrozenBrush(p.Border);
                case "ShowRegisterPrompt":
                    return p.ShowRegisterPrompt ? Visibility.Visible : Visibility.Collapsed;
                case "ShowSubscriptionPanel":
                    return p.ShowSubscriptionPanel ? Visibility.Visible : Visibility.Collapsed;
                case "ShowTrialPanel":
                    return p.ShowTrialPanel ? Visibility.Visible : Visibility.Collapsed;
                case "ShowUnregister":
                    return p.ShowUnregister ? Visibility.Visible : Visibility.Collapsed;
                case "ShowRenewFile":
                    return p.ShowRenewFile ? Visibility.Visible : Visibility.Collapsed;
                case "Description":
                default:
                    return p.Description;
            }
        }

        private static SolidColorBrush FrozenBrush(Color c)
        {
            var brush = new SolidColorBrush(c);
            brush.Freeze();
            return brush;
        }
    }
}
