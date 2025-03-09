using STranslate.Model;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace STranslate.Style.Commons;

/// <summary>
///     https://blog.csdn.net/kristen_dou/article/details/133675830
/// </summary>
public class EnumerationExtension : MarkupExtension
{
    // 静态构造函数，订阅语言变更事件
    static EnumerationExtension()
    {
        // 订阅语言变更事件
        AppLanguageManager.OnAppLanguageChanged += OnLanguageChanged;
    }

    // 存储所有使用此扩展的 Selector 的弱引用
    private static readonly List<(WeakReference<Selector> SelectorRef, Type EnumType)> _selectors = [];

    // 语言变更事件处理
    private static void OnLanguageChanged()
    {
        // 刷新所有注册的 Selector
        for (int i = _selectors.Count - 1; i >= 0; i--)
        {
            var (selectorRef, enumType) = _selectors[i];
            if (selectorRef.TryGetTarget(out var selector))
            {
                // 保存当前选中项
                var selectedValue = selector.SelectedValue;

                // 重新生成枚举成员列表
                var enumValues = Enum.GetValues(enumType).Cast<object>();
                if (Enum.GetUnderlyingType(enumType) == typeof(int))
                {
                    enumValues = enumValues.OrderBy(e => (int)e);
                }

                // 创建新的枚举成员列表，包含更新后的本地化描述
                var newItemsSource = (from object enumValue in enumValues
                                      select new EnumerationMember
                                      {
                                          Value = enumValue,
                                          Description = GetDescription(enumValue, enumType),
                                          Root = enumType.Name
                                      }).ToArray();

                // 更新 ItemsSource
                selector.ItemsSource = newItemsSource;

                // 恢复选中项
                selector.SelectedValue = selectedValue;
            }
            else
            {
                // 移除失效的弱引用
                _selectors.RemoveAt(i);
            }
        }
    }

    // 注册 Selector 以便在语言变更时刷新
    public static void RegisterSelector(Selector selector)
    {
        _selectors.Add((new WeakReference<Selector>(selector), selector.SelectedValue.GetType()));
    }

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
                select new EnumerationMember
                {
                    Value = enumValue,
                    Description = GetDescription(enumValue, EnumType),
                    Root = EnumType.Name
                }).ToArray();
    }

    private static string GetDescription(object enumValue, Type enumType)
    {
        var fieldName = enumValue.ToString() ?? "";
        var field = enumType.GetField(fieldName);

        if (field == null) return fieldName;

        // 获取Description特性
        var descriptionAttribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        // 尝试从资源获取本地化描述
        var fullPath = $"{enumType.Name}.{fieldName}";
        var localizedDesc = AppLanguageManager.GetString(fullPath);

        // 如果找到本地化描述，则使用它
        if (localizedDesc != fullPath)
            return localizedDesc;

        // 否则使用Description特性或枚举值名称
        return descriptionAttribute?.Description ?? fieldName;
    }
}