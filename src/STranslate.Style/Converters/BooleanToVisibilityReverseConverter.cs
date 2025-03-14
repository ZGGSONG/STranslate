using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace STranslate.Style.Converters;

public sealed class BooleanToVisibilityReverseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var bValue = false;
        if (value is bool v)
        {
            bValue = v;
        }
        else if (value is bool?)
        {
            var tmp = (bool?)value;
            bValue = tmp ?? false;
        }

        return bValue ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visi) return visi == Visibility.Collapsed;

        return true;
    }
}