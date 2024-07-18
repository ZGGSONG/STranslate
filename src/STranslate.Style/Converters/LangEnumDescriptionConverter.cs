using System.Globalization;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class LangEnumDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not LangEnum @enum) return "UNKNOWN";
        return @enum.GetDescription();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}