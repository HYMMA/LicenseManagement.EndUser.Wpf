using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LicenseManagement.EndUser.Wpf.Converters
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool val)
            {
                if (!val)
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// returns id of product when the name is provided
    /// </summary>
    //[ValueConversion(typeof(string), typeof(string))]
    public class ProductNameToIdConverter : IValueConverter
    {
        private NameValueCollection products;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value as LicenseManagement.EndUser.Models.ProductModel;
            if (val != null)
            {
                if (products == null)
                    products = (NameValueCollection)ConfigurationManager.GetSection("Products");
                foreach (string name in products.Keys) //keys are the names
                {
                    if (val.Name == name)
                    {
                        return val.Id;
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string val)
            {
                if (products == null)
                    products = (NameValueCollection)ConfigurationManager.GetSection("Products");

                foreach (var name in products.AllKeys) //keys are the names
                {
                    if (products[name] == val) //if product id is equal to input
                    {
                        return name;
                    }
                }
            }
            return null;
        }
    }
}
