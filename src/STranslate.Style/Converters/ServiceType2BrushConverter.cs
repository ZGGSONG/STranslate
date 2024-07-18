using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class ServiceType2BrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selfBuild = (SolidColorBrush)Application.Current.Resources["UnOfficialServiceColor"];
        var local = (SolidColorBrush)Application.Current.Resources["ThemeAccentColor"];
        var official = (SolidColorBrush)Application.Current.Resources["OfficialServiceColor"];
        if (value is ServiceType sType)
            return sType switch
            {
                ServiceType.ApiService => selfBuild,
                ServiceType.STranslateService => local,
                ServiceType.EcdictService => local,
                _ => official
            };
        if (value is OCRType oType)
            return oType switch
            {
                OCRType.PaddleOCR => selfBuild,
                _ => official
            };
        if (value is TTSType tType)
            return tType switch
            {
                TTSType.OfflineTTS => selfBuild,
                _ => official
            };
        return local;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}