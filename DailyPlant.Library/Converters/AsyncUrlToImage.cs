using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using DailyPlant.Library.Services;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace DailyPlant.Library.Converters
{
    public class AsyncUrlToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}