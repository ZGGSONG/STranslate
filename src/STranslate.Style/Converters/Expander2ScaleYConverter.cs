using System.Globalization;
using System.Windows.Data;

namespace STranslate.Style.Converters;

public class Expander2ScaleYConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return b ? 1 : 0;
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}