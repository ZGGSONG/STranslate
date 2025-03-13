using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class MultiComboBoxIconConverter : IMultiValueConverter
{
    // 静态构造函数，订阅语言变更事件
    static MultiComboBoxIconConverter()
    {
        // 订阅语言变更事件
        AppLanguageManager.OnAppLanguageChanged += OnLanguageChanged;
    }

    // 存储所有使用此转换器的绑定表达式的弱引用
    private static readonly List<WeakReference<TextBlock>> _bindingTargets = [];

    // 语言变更事件处理
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
        if (values.Length == 2)
        {
            // 注册绑定目标，以便在语言变更时刷新
            if (values[1] is TextBlock target && !_bindingTargets.Any(wr => wr.TryGetTarget(out var t) && t == target))
            {
                _bindingTargets.Add(new WeakReference<TextBlock>(target));
            }
        }

        switch (values[0])
        {
            case string pkv when parameter is string param:
            {
                var str = pkv.TrimStart('[').TrimEnd(']');
                const char separator = ',';

                if (Enum.TryParse<IconType>(new string([.. str.TakeWhile(c => c != separator)]), out var iconType))
                {
                    string icon = new([.. str.SkipWhile(c => c != separator).Skip(1)]);
                    return param == "0" ? icon : AppLanguageManager.GetString($"IconType.{iconType}");
                }
                // 添加 OpenAI 配置页面不知道为啥会传入一个空字符串？？？先这么解吧
                return param == "0" ? Constant.Icon : AppLanguageManager.GetString($"IconType.STranslate");
                }
            case IconType type:
                return AppLanguageManager.GetString($"IconType.{type}");
            default:
                return "";
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        return [.. Enumerable.Repeat(Binding.DoNothing, targetTypes.Length)];
    }
}