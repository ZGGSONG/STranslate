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

    public ICommand AddCommand
    {
        get { return (ICommand)GetValue(AddCommandProperty); }
        set { SetValue(AddCommandProperty, value); }
    }
    public static readonly DependencyProperty AddCommandProperty =
        DependencyProperty.Register("AddCommand", typeof(ICommand), typeof(TermsControl), new PropertyMetadata(null));

    public ICommand DeleteCommand
    {
        get { return (ICommand)GetValue(DeleteCommandProperty); }
        set { SetValue(DeleteCommandProperty, value); }
    }
    public static readonly DependencyProperty DeleteCommandProperty =
        DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(TermsControl), new PropertyMetadata(null));

    public ICommand ExportCommand
    {
        get { return (ICommand)GetValue(ExportCommandProperty); }
        set { SetValue(ExportCommandProperty, value); }
    }
    public static readonly DependencyProperty ExportCommandProperty =
        DependencyProperty.Register("ExportCommand", typeof(ICommand), typeof(TermsControl), new PropertyMetadata(null));

    public ICommand ImportCommand
    {
        get { return (ICommand)GetValue(ImportCommandProperty); }
        set { SetValue(ImportCommandProperty, value); }
    }
    public static readonly DependencyProperty ImportCommandProperty =
        DependencyProperty.Register("ImportCommand", typeof(ICommand), typeof(TermsControl), new PropertyMetadata(null));
}