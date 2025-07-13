using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Todos.Ui.Resources.Converters
{
    public class EnumEqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter != null)
            {
                try
                {
                    var enumValue = Enum.Parse(value.GetType(), parameter.ToString());
                    return value.Equals(enumValue) ? Visibility.Visible : Visibility.Collapsed;
                }
                catch
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 