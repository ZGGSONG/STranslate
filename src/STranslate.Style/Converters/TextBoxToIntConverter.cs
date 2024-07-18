using System.Globalization;
using System.Windows.Data;

namespace STranslate.Style.Converters;

public class TextBoxToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str && int.TryParse(str, out var port) && port > 0 && port < 65535)
            return port;
        if (int.TryParse(parameter.ToString(), out var defaultPort)) return defaultPort;
        return 50020;
    }
}