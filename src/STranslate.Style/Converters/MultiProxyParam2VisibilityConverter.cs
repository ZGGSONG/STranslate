using System.Globalization;
using System.Windows;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

/// <summary>
///     多条数据确定显示隐藏
/// </summary>
internal class MultiProxyParam2VisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var proxyMethod = (ProxyMethodEnum)values[0];
            var isAuth = (bool)values[1];

            var proxyVisibility = proxyMethod switch
            {
                ProxyMethodEnum.不使用代理 => Visibility.Collapsed,
                ProxyMethodEnum.系统代理 => Visibility.Collapsed,
                ProxyMethodEnum.HTTP => Visibility.Visible,
                ProxyMethodEnum.SOCKS5 => Visibility.Visible,
                _ => Visibility.Collapsed
            };

            var authVisibility = isAuth ? Visibility.Visible : Visibility.Collapsed;

            return proxyVisibility | authVisibility;
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