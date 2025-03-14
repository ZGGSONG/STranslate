using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class VisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            string str => string.IsNullOrEmpty(str) ? Visibility.Collapsed : Visibility.Visible,
            ImageSource => Visibility.Visible,
            IVocabularyBook => Visibility.Visible,
            _ => Visibility.Collapsed
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null && (Visibility)value == Visibility.Collapsed;
    }
}