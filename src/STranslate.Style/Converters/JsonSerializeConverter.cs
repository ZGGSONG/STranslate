using System.Globalization;
using System.Windows.Data;
using Newtonsoft.Json;

namespace STranslate.Style.Converters;

public class JsonSerializeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return string.Empty;
        return JsonConvert.SerializeObject(value, Formatting.Indented);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}