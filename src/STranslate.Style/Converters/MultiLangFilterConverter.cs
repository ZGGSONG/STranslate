using System.Globalization;
using System.Windows.Data;
using static STranslate.Style.Commons.EnumerationExtension;

namespace STranslate.Style.Converters;

public class MultiLangFilterConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [EnumerationMember[] enums, string content])
            return values;

        var langEnables = (content.Length != enums.Length || string.IsNullOrEmpty(content))
            ? Enumerable.Repeat(true, enums.Length).ToArray()
            : content.Select(c => c == '1').ToArray();

        for (var i = 0; i < enums.Length; i++)
        {
            enums[i].IsEnabled = langEnables[i];
        }
        return enums.Where(x => x.IsEnabled).ToArray();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return Enumerable.Repeat(Binding.DoNothing, targetTypes.Length).ToArray();
    }
}
