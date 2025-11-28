using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DailyPlant.Converters
{
    public class MenuItemEqualsConverter : IValueConverter
    {
        public static MenuItemEqualsConverter Instance { get; } = new MenuItemEqualsConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
