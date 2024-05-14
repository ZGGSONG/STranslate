using STranslate.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace STranslate.Style.Converters;

public class ParamEqualToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is LangDetectType pType && parameter is string str && Enum.TryParse(str, out LangDetectType lType))
        {
            return pType == lType ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}