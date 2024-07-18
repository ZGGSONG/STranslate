using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace STranslate.Style.Converters;

public class VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
            return string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible;
        if (value is ImageSource) return Visibility.Visible;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null && (Visibility)value == Visibility.Collapsed;
    }
}