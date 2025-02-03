using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SystemChecker.WPF.Converters
{
    public class LogLevelColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LogLevel level)
            {
                return level switch
                {
                    LogLevel.Trace => new SolidColorBrush(Colors.Gray),
                    LogLevel.Debug => new SolidColorBrush(Colors.Gray),
                    LogLevel.Information => new SolidColorBrush(Colors.Green),
                    LogLevel.Warning => new SolidColorBrush(Colors.Orange),
                    LogLevel.Error => new SolidColorBrush(Colors.Red),
                    LogLevel.Critical => new SolidColorBrush(Colors.DarkRed),
                    _ => new SolidColorBrush(Colors.Black)
                };
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 