using System.Globalization;
using System.Windows.Data;

namespace STranslate.Style.Converters;

internal class MultiValue2BooleanConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.All(v => v is bool))
        {
            return values.Cast<bool>().Any(b => b);
        }
        return values;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
