using CommunityToolkit.Mvvm.Messaging;
using STranslate.Model;
using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace STranslate.Style.Converters;

public class MultiLangFilterConverter : IMultiValueConverter
{
    private static readonly object _messageHandler = new();
    // 静态构造函数，订阅语言变更事件
    static MultiLangFilterConverter()
    {
        // 订阅语言变更事件
        WeakReferenceMessenger.Default.Register<AppLanguageMessenger>(_messageHandler, (_, _) => OnLanguageChanged());
    }

    // 存储所有使用此转换器的 Selector 的弱引用
    private static readonly List<WeakReference<Selector>> _selectors = [];

    // 语言变更事件处理
    private static void OnLanguageChanged()
    {
        // 刷新所有注册的 Selector
        for (int i = _selectors.Count - 1; i >= 0; i--)
        {
            if (_selectors[i].TryGetTarget(out var selector))
            {
                // 保存当前选中项
                var selectedValue = selector.SelectedValue;

                // 刷新 ItemsSource
                var bindingExpression = BindingOperations.GetMultiBindingExpression(selector, Selector.ItemsSourceProperty);
                bindingExpression.UpdateTarget();

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
    public void RegisterSelector(Selector selector)
    {
        _selectors.Add(new WeakReference<Selector>(selector));
    }


    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [EnumerationMember[] enums, string content])
            return values;

        var langEnables = (content.Length != enums.Length || string.IsNullOrEmpty(content))
            ? Enumerable.Repeat(true, enums.Length).ToArray()
            : content.Select(c => c == '1').ToArray();

        for (var i = 0; i < enums.Length; i++)
        {
            var fullPath = $"{enums[i].Root}.{enums[i].Value}";
            var desc = AppLanguageManager.GetString(fullPath);
            if (desc != fullPath)
                enums[i].Description = desc;
            enums[i].IsEnabled = langEnables[i];
        }
        return enums.Where(x => x.IsEnabled).ToArray();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return Enumerable.Repeat(Binding.DoNothing, targetTypes.Length).ToArray();
    }
}