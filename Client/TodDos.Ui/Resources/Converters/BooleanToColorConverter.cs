using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Todos.Ui.Resources.Converters
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colorString)
            {
                var colors = colorString.Split('|');
                if (colors.Length >= 2)
                {
                    var trueColor = colors[0].Trim();
                    var falseColor = colors[1].Trim();
                    
                    return boolValue ? 
                        (SolidColorBrush)new BrushConverter().ConvertFromString(trueColor) : 
                        (SolidColorBrush)new BrushConverter().ConvertFromString(falseColor);
                }
            }
            
            return Brushes.Gray; // Default fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 