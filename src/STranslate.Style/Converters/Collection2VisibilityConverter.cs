using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class Collection2VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BindingList<WebDavResult> list) return list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}