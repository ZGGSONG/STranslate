using System.Globalization;
using System.Windows;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public sealed class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            ProxyMethodEnum proxyMethodEnum => proxyMethodEnum switch
            {
                ProxyMethodEnum.不使用代理 => Visibility.Collapsed,
                ProxyMethodEnum.系统代理 => Visibility.Collapsed,
                _ => Visibility.Visible
            },
            BackupType backupType => backupType switch
            {
                BackupType.Local => Visibility.Collapsed,
                _ => Visibility.Visible
            },
            _ => Visibility.Collapsed
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}