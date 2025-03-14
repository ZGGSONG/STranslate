using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace STranslate.Style.Converters;

internal class StringBoolean2VisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (values.Count() != 2 || values[0] is not string str || values[1] is not bool b)
                return Visibility.Collapsed;

            return !string.IsNullOrEmpty(str) && b ? Visibility.Visible : Visibility.Collapsed;
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