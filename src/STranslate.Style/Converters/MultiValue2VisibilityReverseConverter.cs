using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace STranslate.Style.Converters;

/// <summary>
///     多条数据确定显示隐藏
/// </summary>
internal class MultiValue2VisibilityReverseConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            // 检查所有值是否都是 bool 类型
            if (!values.All(x => x is bool))
                return Visibility.Collapsed;
            
            // 检查所有值是否为 true
            var allTrue = values.Cast<bool>().All(x => x);
            return allTrue ? Visibility.Collapsed : Visibility.Visible;
        }
        catch (Exception)
        {
            return Visibility.Collapsed;
        }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}