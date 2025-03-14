using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace STranslate.Style.Converters;

public class StringToVisibilityHiddenConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
            return string.IsNullOrEmpty(str) ? Visibility.Hidden : Visibility.Visible;
        return Visibility.Hidden;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null && (Visibility)value == Visibility.Hidden;
    }
}