using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Messaging;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class MultiDetectTypeEnumDescriptionConverter : IMultiValueConverter
{
    private static readonly object _messageHandler = new();
    static MultiDetectTypeEnumDescriptionConverter()
    {
        WeakReferenceMessenger.Default.Register<AppLanguageMessenger>(_messageHandler, (_, _) => OnLanguageChanged());
    }

    // 存储所有使用此转换器的绑定表达式的弱引用
    private static readonly List<WeakReference<TextBlock>> _bindingTargets = [];

    private static void OnLanguageChanged()
    {
        // 刷新所有注册的绑定目标
        for (int i = _bindingTargets.Count - 1; i >= 0; i--)
        {
            if (_bindingTargets[i].TryGetTarget(out var target))
            {
                // 刷新绑定
                var bindingExpression = BindingOperations.GetMultiBindingExpression(target as TextBlock, TextBlock.TextProperty);
                bindingExpression?.UpdateTarget();
            }
            else
            {
                // 移除失效的弱引用
                _bindingTargets.RemoveAt(i);
            }
        }
    }

    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length != 2)
            return values;

        // 注册绑定目标，以便在语言变更时刷新
        if (values[0] is TextBlock target && !_bindingTargets.Any(wr => wr.TryGetTarget(out var t) && t == target))
        {
            _bindingTargets.Add(new WeakReference<TextBlock>(target));
        }

        //if (values[1] is not string str) return "UNKNOWN";
        return GetDescription(values[1].ToString() ?? "");
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        return [.. Enumerable.Repeat(Binding.DoNothing, targetTypes.Length)];
    }

    private string GetDescription(string str)
    {
        if (Enum.TryParse<LangDetectType>(str, out var @enum) != true)
        {
            @enum = LangDetectType.Local;
        }

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