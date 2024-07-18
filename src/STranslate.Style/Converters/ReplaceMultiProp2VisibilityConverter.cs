using System.Globalization;
using System.Windows;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

internal class ReplaceMultiProp2VisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (values.Count() != 2 || values[0] is not LangDetectType dType || values[1] is not LangEnum lang)
                return Visibility.Collapsed;


            return dType == LangDetectType.Local && lang == LangEnum.auto ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception)
        {
            return Visibility.Collapsed;
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}