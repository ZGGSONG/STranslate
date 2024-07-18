using System.Windows;
using System.Windows.Controls;

namespace STranslate.Style.Styles.Navigation;

public class Btn : RadioButton
{
    static Btn()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Btn), new FrameworkPropertyMetadata(typeof(Btn)));
    }
}