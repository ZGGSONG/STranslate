using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class PromptToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not BindingList<UserDefinePrompt> up) return Visibility.Collapsed;
        var prompt = up.FirstOrDefault(p => p.Enabled);
        return prompt != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

public class PromptToVisibilityMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var up = (BindingList<UserDefinePrompt>)values[0];
            var isPromptToggleVisible = (bool)values[1];

            var haveEnabledPrompt = up.FirstOrDefault(p => p.Enabled) != null;

            return isPromptToggleVisible && haveEnabledPrompt ? Visibility.Visible : Visibility.Collapsed;
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