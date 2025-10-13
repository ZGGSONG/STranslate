using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STranslate.Style.Controls;

public class TermsControl : ListBox
{
    static TermsControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TermsControl), new FrameworkPropertyMetadata(typeof(TermsControl)));
    }

    public ICommand DeleteCommand
    {
        get { return (ICommand)GetValue(DeleteCommandProperty); }
        set { SetValue(DeleteCommandProperty, value); }
    }
    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(TermsControl), new PropertyMetadata(null));
}