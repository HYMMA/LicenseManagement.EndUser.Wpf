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

        // --- card presentation (status pill + validity meter) ---
        public string StatusLabel { get; set; }
        public Color PillForeground { get; set; }
        public Color PillBackground { get; set; }
        public Color Meter { get; set; }
    }

    /// <summary>Named palette shared by the status pill and the validity meter.</summary>
    internal static class StatusPalette
    {
        public static readonly Color Ok = Color.FromRgb(0x15, 0x80, 0x3D);
        public static readonly Color OkTint = Color.FromRgb(0xE6, 0xF4, 0xEA);
        public static readonly Color Trial = Color.FromRgb(0xB4, 0x53, 0x09);
        public static readonly Color TrialTint = Color.FromRgb(0xFB, 0xEE, 0xDC);
        public static readonly Color Bad = Color.FromRgb(0xB9, 0x1C, 0x1C);
        public static readonly Color BadTint = Color.FromRgb(0xFB, 0xEA, 0xEA);
        public static readonly Color None = Color.FromRgb(0x47, 0x55, 0x69);
        public static readonly Color NoneTint = Color.FromRgb(0xED, 0xF1, 0xF5);
        public static readonly Color Faint = Color.FromRgb(0x94, 0xA3, 0xB8);
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
            StatusLabel = "Not checked",
            PillForeground = StatusPalette.None,
            PillBackground = StatusPalette.NoneTint,
            Meter = StatusPalette.Faint,
        };

        private static readonly IReadOnlyDictionary<LicenseStatusTitles, LicenseStatusPresentation> Table =
            new Dictionary<LicenseStatusTitles, LicenseStatusPresentation>
            {
                [LicenseStatusTitles.Unknown] = new LicenseStatusPresentation
                {
                    // Reached only when a check completed but couldn't be verified (e.g. it
                    // threw). Distinct from "never checked" (the null Fallback) so a failed
                    // check never masquerades as an unloaded card.
                    Description = "Could not verify the license.",
                    Background = Colors.Transparent,
                    Border = Colors.Gray,
                    StatusLabel = "Couldn't verify",
                    PillForeground = StatusPalette.Trial,
                    PillBackground = StatusPalette.TrialTint,
                    Meter = StatusPalette.Faint,
                },
                [LicenseStatusTitles.Expired] = new LicenseStatusPresentation
                {
                    Description = "License file expired.",
                    Background = Colors.LightSalmon,
                    Border = Colors.DarkRed,
                    ShowRenewFile = true,
                    ShowSubscriptionPanel = true,
                    StatusLabel = "File Expired",
                    PillForeground = StatusPalette.Bad,
                    PillBackground = StatusPalette.BadTint,
                    Meter = StatusPalette.Bad,
                },
                [LicenseStatusTitles.Valid] = new LicenseStatusPresentation
                {
                    Description = "Paid and active.",
                    Background = Colors.Azure,
                    Border = Colors.Green,
                    ShowUnregister = true,
                    ShowSubscriptionPanel = true,
                    StatusLabel = "Licensed",
                    PillForeground = StatusPalette.Ok,
                    PillBackground = StatusPalette.OkTint,
                    Meter = StatusPalette.Ok,
                },
                [LicenseStatusTitles.ValidTrial] = new LicenseStatusPresentation
                {
                    Description = "Trial and active.",
                    Background = Colors.Azure,
                    Border = Colors.Blue,
                    ShowRegisterPrompt = true,
                    ShowTrialPanel = true,
                    StatusLabel = "Trial",
                    PillForeground = StatusPalette.Trial,
                    PillBackground = StatusPalette.TrialTint,
                    Meter = StatusPalette.Trial,
                },
                [LicenseStatusTitles.InvalidTrial] = new LicenseStatusPresentation
                {
                    Description = "Trial ended and requires activation.",
                    Background = Colors.LightSalmon,
                    Border = Colors.DarkRed,
                    ShowRegisterPrompt = true,
                    StatusLabel = "Trial ended",
                    PillForeground = StatusPalette.Bad,
                    PillBackground = StatusPalette.BadTint,
                    Meter = StatusPalette.Bad,
                },
                [LicenseStatusTitles.ReceiptExpired] = new LicenseStatusPresentation
                {
                    Description = "Payment is suspended or subscription needs renewal.",
                    Background = Colors.LightSalmon,
                    Border = Colors.DarkRed,
                    ShowRegisterPrompt = true,
                    StatusLabel = "Subscription Ended",
                    PillForeground = StatusPalette.Bad,
                    PillBackground = StatusPalette.BadTint,
                    Meter = StatusPalette.Bad,
                },
                [LicenseStatusTitles.ReceiptUnregistered] = new LicenseStatusPresentation
                {
                    Description = "Computer has been unregistered.",
                    Background = Colors.LightGray,
                    Border = Colors.Black,
                    ShowRegisterPrompt = true,
                    StatusLabel = "Computer Unregistered",
                    PillForeground = StatusPalette.None,
                    PillBackground = StatusPalette.NoneTint,
                    Meter = StatusPalette.Faint,
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
                case "StatusLabel":
                    return p.StatusLabel ?? p.Description;
                case "PillForeground":
                    return FrozenBrush(p.PillForeground);
                case "PillBackground":
                    return FrozenBrush(p.PillBackground);
                case "Meter":
                    return FrozenBrush(p.Meter);
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
