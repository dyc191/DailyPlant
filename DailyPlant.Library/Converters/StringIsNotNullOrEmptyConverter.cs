using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace DailyPlant.Library.Converters
{
    public class StringIsNotNullOrEmptyConverter : IValueConverter
    {
        // 单例实例
        public static StringIsNotNullOrEmptyConverter Instance { get; } = new StringIsNotNullOrEmptyConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}