using STranslate.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace STranslate.Style.Converters
{
    public sealed class ProxyMethodToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProxyMethodEnum proxyMethodEnum)
            {
                return proxyMethodEnum switch
                {
                    ProxyMethodEnum.不使用代理 => Visibility.Collapsed,
                    ProxyMethodEnum.系统代理 => Visibility.Collapsed,
                    ProxyMethodEnum.HTTP => Visibility.Visible,
                    ProxyMethodEnum.SOCKS5 => Visibility.Visible,
                    _ => Visibility.Collapsed
                };
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
