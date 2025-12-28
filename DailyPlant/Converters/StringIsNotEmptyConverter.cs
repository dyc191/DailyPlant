using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace DailyPlant.Views;

public class StringIsNotEmptyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 如果字符串不为空，返回 true（显示）
        // 如果字符串为空或null，返回 false（隐藏）
        return !string.IsNullOrEmpty(value as string);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}