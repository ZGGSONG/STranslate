using System.Globalization;
using System.Windows.Data;
using STranslate.Model;
using STranslate.Style.Commons;

namespace STranslate.Style.Converters;

public class GlobalFontSizeToDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not EnumerationMember v)
            return 18;
        return v.Description;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

public class GlobalFontSizeToFontSizeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not EnumerationMember v ||
            Enum.TryParse<GlobalFontSizeEnum>(v.Value?.ToString() ?? "", out var @enum) == false)
            return 18;
        return @enum.ToInt() + 18;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}