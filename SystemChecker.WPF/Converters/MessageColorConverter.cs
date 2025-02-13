using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SystemChecker.WPF.Converters;

public class MessageColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string message)
        {
            if (message.StartsWith("Error"))
                return new SolidColorBrush(Colors.Red);
            if (message.StartsWith("Valid") || message.StartsWith("Configuration saved"))
                return new SolidColorBrush(Colors.Green);
        }
        return new SolidColorBrush(Colors.Black);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 