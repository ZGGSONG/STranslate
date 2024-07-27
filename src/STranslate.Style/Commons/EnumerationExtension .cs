using System.ComponentModel;
using System.Windows.Markup;

namespace STranslate.Style.Commons;

/// <summary>
///     https://blog.csdn.net/kristen_dou/article/details/133675830
/// </summary>
public class EnumerationExtension : MarkupExtension
{
    private Type? _enumType;

    public EnumerationExtension()
    {
    }

    public EnumerationExtension(Type enumType)
    {
        EnumType = enumType;
    }

    public Type? EnumType
    {
        get => _enumType;
        set
        {
            if (_enumType != value)
            {
                if (value != null)
                {
                    var enumType = Nullable.GetUnderlyingType(value) ?? value;
                    if (!enumType.IsEnum)
                        throw new ArgumentException("Type must be an Enum.");
                }

                _enumType = value;
            }
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var enumValues = Enum.GetValues(EnumType!);
        return (from object enumValue in enumValues
            select new EnumerationMember { Value = enumValue, Description = GetDescription(enumValue) }).ToArray();
    }

    private string GetDescription(object enumValue)
    {
        if (EnumType == null)
            throw new InvalidOperationException("The EnumType must be specified.");

        var descriptionAttribute =
            EnumType.GetField(enumValue.ToString() ?? "")?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
        return descriptionAttribute != null ? descriptionAttribute.Description : enumValue.ToString() ?? "";
    }

    public class EnumerationMember
    {
        public string Description { get; set; } = "";
        public object? Value { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}