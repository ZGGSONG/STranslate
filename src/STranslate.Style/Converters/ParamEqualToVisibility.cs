using System.Globalization;
using System.Windows;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class ParamEqualToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            LangDetectType pType when parameter is string str && Enum.TryParse(str, out LangDetectType lType) =>
                pType == lType ? Visibility.Visible : Visibility.Collapsed,
            LangEnum lang when parameter is string paramStr && Enum.TryParse(paramStr, out LangEnum pLang) =>
                lang == pLang ? Visibility.Visible : Visibility.Collapsed,
            _ => Visibility.Collapsed
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}