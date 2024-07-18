using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace STranslate.Style.Converters;

/// <summary>
///     多条数据确定显示隐藏
/// </summary>
internal class MultiValue2VisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var isExpander = (bool)values[0];
            var isConfOpen = (bool)values[1];
            var isSuccess = (bool)values[2];

            return isConfOpen && !isExpander && isSuccess ? Visibility.Visible : Visibility.Collapsed;
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