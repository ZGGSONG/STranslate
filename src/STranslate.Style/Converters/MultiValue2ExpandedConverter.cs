using System.Globalization;
using System.Windows.Data;

namespace STranslate.Style.Converters;

[Obsolete("通过附加属性绑定避免与界面点击操作冲突")]
public class MultiValue2ExpandedConverter : IMultiValueConverter
{
    /// <summary>
    ///     将内容值转换为 Expander 的 IsExpanded 布尔值。
    /// </summary>
    /// <param name="value">内容值。</param>
    /// <param name="targetType">要转换到的类型。</param>
    /// <param name="parameter">可选参数。</param>
    /// <param name="culture">在转换中要使用的文化。</param>
    /// <returns>如果内容不为 null 或空，则为 true；否则为 false。</returns>
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        var content = (string)value[0];
        var autoExpander = (bool)value[1];
        var isExecuting = (bool)value[2];
        // 如果值是非空字符串，则返回 true；否则返回 false。
        return (!string.IsNullOrEmpty(content) && autoExpander) || isExecuting;
    }

    /// <summary>
    ///     将 IsExpanded 布尔值转换回原始内容值。
    /// </summary>
    /// <param name="value">IsExpanded 布尔值。</param>
    /// <param name="targetType">要转换到的类型。</param>
    /// <param name="parameter">可选参数。</param>
    /// <param name="culture">在转换中要使用的文化。</param>
    /// <returns>Binding.DoNothing 表示无需执行任何操作。</returns>
    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
    {
        // 返回填充了 Binding.DoNothing 的数组
        return Enumerable.Repeat(Binding.DoNothing, targetType.Length).ToArray();
    }
}