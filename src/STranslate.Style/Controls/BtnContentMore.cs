using System.Windows;
using System.Windows.Controls;

namespace STranslate.Style.Controls;

public class BtnContentMore : Button
{
    public static readonly DependencyProperty ContentMoreProperty = DependencyProperty.Register(nameof(ContentMore),
        typeof(string), typeof(BtnContentMore), new FrameworkPropertyMetadata(string.Empty, null));

    public string ContentMore
    {
        get => (string)GetValue(ContentMoreProperty);
        set => SetValue(ContentMoreProperty, value);
    }
}