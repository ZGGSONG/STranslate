using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class PromptConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var empty = string.Empty;
        if (value is not BindingList<UserDefinePrompt> up) return empty;
        var prompt = up.FirstOrDefault(p => p.Enabled);
        return prompt != null ? prompt.Name : empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}