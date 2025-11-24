using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace DailyPlant.Converters
{
    public class ObjectToBoolConverter : IValueConverter
    {
        // 单例实例
        public static ObjectToBoolConverter IsNotNull { get; } = new ObjectToBoolConverter();
        public static ObjectToBoolConverter IsNull { get; } = new ObjectToBoolConverter { _invert = true };

        private bool _invert = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value != null;
            return _invert ? !result : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}