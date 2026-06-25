using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace LicenseManagement.EndUser.Wpf.ViewModels
{
    /// <summary>
    /// A presentation group of products (e.g. "Monthly"). Carries the heading text and
    /// the accent brushes derived from the developer-supplied hex so that the card rail,
    /// header glyph and count chip all share one colour and the group reads apart at a
    /// glance. Groups are built once from the configuration and are immutable thereafter.
    /// </summary>
    public sealed class ProductGroupViewModel
    {
        private static readonly Color DefaultAccent = Color.FromRgb(0x47, 0x55, 0x69); // slate

        public ProductGroupViewModel(string key, string label, string caption, string accentHex, IEnumerable<ProductViewModel> products)
        {
            Key = key;
            Label = label;
            Caption = caption;

            var accent = ParseColor(accentHex, DefaultAccent);
            Accent = FrozenBrush(accent);
            AccentTint = FrozenBrush(Blend(accent, Colors.White, 0.12)); // soft fill behind the group

            Products = new ObservableCollection<ProductViewModel>(products ?? new ProductViewModel[0]);
        }

        public string Key { get; private set; }
        public string Label { get; private set; }
        public string Caption { get; private set; }

        /// <summary>First letter of the label, for the group glyph.</summary>
        public string Initial =>
            string.IsNullOrEmpty(Label) ? string.Empty : Label.Substring(0, 1).ToUpperInvariant();

        /// <summary>Solid accent brush (card rail, glyph, count chip text).</summary>
        public Brush Accent { get; private set; }

        /// <summary>Low-alpha tint of the accent for group backgrounds / chips.</summary>
        public Brush AccentTint { get; private set; }

        public ObservableCollection<ProductViewModel> Products { get; private set; }

        private static Color ParseColor(string hex, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return fallback;
            try
            {
                var parsed = ColorConverter.ConvertFromString(hex);
                if (parsed is Color c)
                    return c;
            }
            catch (FormatException)
            {
                // fall through to fallback for malformed hex
            }
            return fallback;
        }

        private static Color Blend(Color color, Color onto, double amount)
        {
            // amount = weight of `color` over `onto` (0..1)
            var inv = 1.0 - amount;
            return Color.FromRgb(
                (byte)Math.Round(color.R * amount + onto.R * inv),
                (byte)Math.Round(color.G * amount + onto.G * inv),
                (byte)Math.Round(color.B * amount + onto.B * inv));
        }

        private static SolidColorBrush FrozenBrush(Color c)
        {
            var brush = new SolidColorBrush(c);
            brush.Freeze();
            return brush;
        }
    }
}
