using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using static STranslate.Style.Commons.EnumerationExtension;

namespace STranslate.Style.Converters;

public class MultiLangFilterConverter : IMultiValueConverter
{
    // 静态构造函数，订阅语言变更事件
    static MultiLangFilterConverter()
    {
        // 订阅语言变更事件
        Model.AppLanguageManager.OnAppLanguageChanged += OnLanguageChanged;
    }

    // 存储所有使用此转换器的 ComboBox 的弱引用
    private static readonly List<WeakReference<ComboBox>> _comboBoxes = [];

    // 语言变更事件处理
    private static void OnLanguageChanged()
    {
        // 刷新所有注册的 ComboBox
        for (int i = _comboBoxes.Count - 1; i >= 0; i--)
        {
            if (_comboBoxes[i].TryGetTarget(out var comboBox))
            {
                // 保存当前选中项
                var selectedValue = comboBox.SelectedValue;

                // 刷新 ItemsSource
                var bindingExpression = BindingOperations.GetMultiBindingExpression(comboBox, ComboBox.ItemsSourceProperty);
                bindingExpression?.UpdateTarget();

                // 恢复选中项
                comboBox.SelectedValue = selectedValue;
            }
            else
            {
                // 移除失效的弱引用
                _comboBoxes.RemoveAt(i);
            }
        }
    }

    // 注册 ComboBox 以便在语言变更时刷新
    public void RegisterComboBox(ComboBox comboBox)
    {
        _comboBoxes.Add(new WeakReference<ComboBox>(comboBox));
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
            var desc = Model.AppLanguageManager.GetString(fullPath);
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