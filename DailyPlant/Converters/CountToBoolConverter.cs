using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DailyPlant.Converters
{
    public class CountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count && parameter is string paramString)
            {
                int param = int.Parse(paramString);
                return count > param;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
