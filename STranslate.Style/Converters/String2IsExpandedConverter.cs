using System;
using System.Globalization;
using System.Windows.Data;

namespace STranslate.Style.Converters
{
    public class String2IsExpandedConverter : IValueConverter
    {
        /// <summary>
        /// 将内容值转换为 Expander 的 IsExpanded 布尔值。
        /// </summary>
        /// <param name="value">内容值。</param>
        /// <param name="targetType">要转换到的类型。</param>
        /// <param name="parameter">可选参数。</param>
        /// <param name="culture">在转换中要使用的文化。</param>
        /// <returns>如果内容不为 null 或空，则为 true；否则为 false。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 如果值是非空字符串，则返回 true；否则返回 false。
            return value is string str && !string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// 将 IsExpanded 布尔值转换回原始内容值。
        /// </summary>
        /// <param name="value">IsExpanded 布尔值。</param>
        /// <param name="targetType">要转换到的类型。</param>
        /// <param name="parameter">可选参数。</param>
        /// <param name="culture">在转换中要使用的文化。</param>
        /// <returns>Binding.DoNothing 表示无需执行任何操作。</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 用户通过 UI 更改了 Expander 的状态，不需要执行任何操作。
            return Binding.DoNothing;
        }
    }
}
