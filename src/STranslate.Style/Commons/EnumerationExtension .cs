using STranslate.Model;
using System.ComponentModel;
using System.Windows.Markup;

namespace STranslate.Style.Commons;

/// <summary>
///     https://blog.csdn.net/kristen_dou/article/details/133675830
/// </summary>
public class EnumerationExtension : MarkupExtension
{
    public Type EnumType { get; set; }

    public EnumerationExtension(Type? enumType)
    {
        ArgumentNullException.ThrowIfNull(enumType);
        if (!enumType.IsEnum)
            throw new ArgumentException("Type must be an Enum.");
        EnumType = enumType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var enumValues = Enum.GetValues(EnumType).Cast<object>();

        // 检查枚举的底层类型是否为 int
        if (Enum.GetUnderlyingType(EnumType) == typeof(int))
        {
            enumValues = enumValues.OrderBy(e => (int)e);
        }
        return (from object enumValue in enumValues
            select new EnumerationMember { Root = EnumType.Name, Value = enumValue, Description = GetLocalizedDescription(enumValue) }).ToArray();
    }

    private string GetLocalizedDescription(object enumValue)
    {
        var fullEnumPath = $"{EnumType.GetField(enumValue.ToString() ?? "")?.FieldType.Name}.{enumValue}";
        var localizedDescription = AppLanguageManager.GetString(fullEnumPath);
        if (localizedDescription == fullEnumPath)
            localizedDescription = EnumType.GetField(enumValue.ToString() ?? "")?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() is DescriptionAttribute descriptionAttribute ? descriptionAttribute.Description : enumValue.ToString() ?? "";
        return localizedDescription;
    }

    public class EnumerationMember
    {
        public string Root { get; set; } = "";
        public string Description { get; set; } = "";
        public object? Value { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}