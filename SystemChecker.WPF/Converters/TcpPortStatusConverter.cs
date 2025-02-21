using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SystemChecker.Core.Models;

namespace SystemChecker.WPF.Converters;

public class TcpPortStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TcpPortStatus status)
        {
            if (targetType == typeof(Brush))
            {
                return status.IsOpen 
                    ? new SolidColorBrush(Colors.Green) 
                    : new SolidColorBrush(Colors.Red);
            }
            
            if (targetType == typeof(string))
            {
                return status.IsOpen 
                    ? $"Open" 
                    : "Closed";
            }
        }
        
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 