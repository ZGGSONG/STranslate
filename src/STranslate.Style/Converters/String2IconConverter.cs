using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class String2IconConverter : IValueConverter
{
    private readonly Dictionary<IconType, string> IconMap =
        Application.Current.Resources.MergedDictionaries
            .FirstOrDefault(x =>
                x.Source == new Uri("pack://application:,,,/STranslate.Style;component/Styles/IconStyle.xaml",
                    UriKind.Absolute))
            !.OfType<DictionaryEntry>()
            .ToDictionary(
                entry => (IconType)Enum.Parse(typeof(IconType), entry.Key.ToString() ?? "STranslate"),
                entry => entry.Value!.ToString() ?? ""
            ) ?? [];

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IconType name)
            if (IconMap.ContainsKey(name))
                return IconMap[name];
        return IconMap[IconType.STranslate];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}