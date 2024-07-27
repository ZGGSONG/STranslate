using System.Globalization;
using System.Windows.Data;
using static STranslate.Style.Commons.EnumerationExtension;

namespace STranslate.Style.Converters;

public class MultiLangFilterConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2 || values[0] is not EnumerationMember[] enums || values[1] is not string content)
            return values;

        var langEnableds = (content.Length != enums.Length || string.IsNullOrEmpty(content))
            ? Enumerable.Repeat(true, enums.Length).ToArray()
            : content.Select(c => c == '1').ToArray();

        for (int i = 0; i < enums.Length; i++)
        {
            enums[i].IsEnabled = langEnableds[i];
        }
        return enums.Where(x => x.IsEnabled).ToArray();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return Enumerable.Repeat(Binding.DoNothing, targetTypes.Length).ToArray();
    }
}
