using System.Globalization;
using System.Windows.Controls;

namespace LicenseManagement.EndUser.Wpf.Rules
{
    internal class ReceiptCodeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var val = value as string;
            if (string.IsNullOrEmpty(val))
            {
                return new ValidationResult(false, "Product code cannot be empty");
            }
            if (val.Length > 100)
            {
                return new ValidationResult(false, "Product code cannot be more than 100 characters.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
