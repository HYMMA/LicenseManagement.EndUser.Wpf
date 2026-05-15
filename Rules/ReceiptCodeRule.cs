using LicenseManagement.EndUser.Wpf.Properties;
using System.Globalization;
using System.Windows.Controls;

namespace LicenseManagement.EndUser.Wpf.Rules
{
    internal class ReceiptCodeRule : ValidationRule
    {
        private const int MaxReceiptCodeLength = 100;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var val = value as string;
            if (string.IsNullOrEmpty(val))
                return new ValidationResult(false, Resources.ReceiptCodeEmpty);

            if (val.Length > MaxReceiptCodeLength)
                return new ValidationResult(false, string.Format(cultureInfo ?? CultureInfo.CurrentCulture, Resources.ReceiptCodeTooLong, MaxReceiptCodeLength));

            return ValidationResult.ValidResult;
        }
    }
}
