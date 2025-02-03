using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SystemChecker.WPF.Converters;

public class ServiceStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.ToLower() switch
            {
                "running" => new SolidColorBrush(Colors.Green),
                "stopped" => new SolidColorBrush(Colors.Red),
                "error" => new SolidColorBrush(Colors.Red),
                _ => new SolidColorBrush(Colors.Orange)
            };
        }
        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 