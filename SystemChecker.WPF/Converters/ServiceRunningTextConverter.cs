using System;
using System.Globalization;
using System.Windows.Data;

namespace SystemChecker.WPF.Converters;

public class ServiceRunningTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool isRunning && isRunning ? "Sim" : "Não";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 