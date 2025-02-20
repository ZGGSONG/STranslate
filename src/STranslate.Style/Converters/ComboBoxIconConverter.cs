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

                if (Enum.TryParse<IconType>(new string(str.TakeWhile(c => c != separator).ToArray()), out var iconType))
                {
                    string icon = new(str.SkipWhile(c => c != separator).Skip(1).ToArray());
                    return param == "0" ? icon : iconType.GetDescription();
                }
                // 添加 OpenAI 配置页面不知道为啥会传入一个空字符串？？？先这么解吧
                return param == "0" ? Constant.Icon : "STranslate";
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