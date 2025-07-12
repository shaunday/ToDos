using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using static Todos.Client.Common.TypesGlobal;

namespace Todos.Ui.Resources.Converters
{
    public class ConnectionStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ConnectionStatus connectionStatus)
            {
                return connectionStatus switch
                {
                    ConnectionStatus.Connected => Brushes.Green,
                    ConnectionStatus.Connecting => Brushes.Orange,
                    ConnectionStatus.Reconnecting => Brushes.Yellow,
                    ConnectionStatus.Failed => Brushes.Red,
                    _ => Brushes.Gray // Disconnected
                };
            }
            
            return Brushes.Gray; // Default fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 