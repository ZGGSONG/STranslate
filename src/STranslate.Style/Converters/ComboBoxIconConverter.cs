using System.Globalization;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class ComboBoxIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case string pkv when parameter is string param:
            {
                var str = pkv.TrimStart('[').TrimEnd(']');
                const char separator = ',';
                var type = (IconType)Enum.Parse(typeof(IconType),
                    new string(str.TakeWhile(c => c != separator).ToArray()));
                string icon = new(str.SkipWhile(c => c != separator).Skip(1).ToArray());
                return param == "0" ? icon : type.GetDescription();
            }
            case IconType type:
                return type.GetDescription();
            default:
                return "";
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}