using System.Globalization;
using System.Windows.Data;

namespace STranslate.Style.Converters;

/// <summary>
///     用于从Xml中Command事件命令传递多参数转换
/// </summary>
internal class MultiValue2ListConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.ToList();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}