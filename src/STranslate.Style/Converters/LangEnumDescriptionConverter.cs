using System.Globalization;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class LangEnumDescriptionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str) return "UNKNOWN";
        return GetDescription(str);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    private string GetDescription(string str)
    {
        var @enum = str.GetEnumByDescription<LangEnum>() ?? LangEnum.auto;

        var fieldName = @enum.ToString() ?? "";

        // 尝试从资源获取本地化描述
        var fullPath = $"{@enum.GetType().Name}.{fieldName}";
        var localizedDesc = AppLanguageManager.GetString(fullPath);

        // 如果找到本地化描述，则使用它
        if (localizedDesc != fullPath)
            return localizedDesc;

        // 否则使用Description特性或枚举值名称
        return @enum.GetDescription();
    }
}