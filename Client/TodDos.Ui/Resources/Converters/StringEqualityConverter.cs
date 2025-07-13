using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Todos.Ui.Resources.Converters
{
    public class StringEqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string currentValue && parameter is string expectedValue)
            {
                return currentValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase) 
                    ? new SolidColorBrush(Colors.Black) 
                    : new SolidColorBrush(Colors.Transparent);
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 