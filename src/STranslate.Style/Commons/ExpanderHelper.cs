using System.Windows;
using System.Windows.Controls;

namespace STranslate.Style.Commons;

public static class ExpanderHelper
{
    // Data.Result Attached Property
    public static readonly DependencyProperty DataResultProperty =
        DependencyProperty.RegisterAttached("DataResult", typeof(bool), typeof(ExpanderHelper),
            new PropertyMetadata(true, OnAnyPropertyChanged));

    // IsExecuting Attached Property
    public static readonly DependencyProperty IsExecutingProperty =
        DependencyProperty.RegisterAttached("IsExecuting", typeof(bool), typeof(ExpanderHelper),
            new PropertyMetadata(true, OnAnyPropertyChanged));

    public static void SetDataResult(UIElement element, bool value)
    {
        element.SetValue(DataResultProperty, value);
    }

    public static bool GetDataResult(UIElement element)
    {
        return (bool)element.GetValue(DataResultProperty);
    }

    public static void SetIsExecuting(UIElement element, bool value)
    {
        element.SetValue(IsExecutingProperty, value);
    }

    public static bool GetIsExecuting(UIElement element)
    {
        return (bool)element.GetValue(IsExecutingProperty);
    }

    // Property Changed Callback
    private static void OnAnyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Expander expander) return;
        var dataResult = GetDataResult(expander);
        var isExecuting = GetIsExecuting(expander);

        // Custom logic to determine the IsExpanded state
        expander.IsExpanded = dataResult || isExecuting;
    }
}