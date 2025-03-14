using System.Windows;
using System.Windows.Controls;

namespace STranslate.Style.Commons;

public static class ExpanderHelper
{
    // Data.Result Attached Property
    public static readonly DependencyProperty DataResultProperty =
        DependencyProperty.RegisterAttached("DataResult", typeof(bool), typeof(ExpanderHelper),
            new PropertyMetadata(true, OnAnyPropertyChanged));


    public static void SetDataResult(UIElement element, bool value)
    {
        element.SetValue(DataResultProperty, value);
    }

    public static bool GetDataResult(UIElement element)
    {
        return (bool)element.GetValue(DataResultProperty);
    }

    // Property Changed Callback
    private static void OnAnyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Expander expander) return;
        var dataResult = GetDataResult(expander);

        // Custom logic to determine the IsExpanded state
        expander.IsExpanded = dataResult;
    }
}