using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace STranslate.Style.Commons;

public class MouseWheelBehavior
{
    public static readonly DependencyProperty EnableSmoothScrollProperty =
        DependencyProperty.RegisterAttached(
            "EnableSmoothScroll",
            typeof(bool),
            typeof(MouseWheelBehavior),
            new PropertyMetadata(false, OnEnableSmoothScrollChanged));

    public static bool GetEnableSmoothScroll(DependencyObject obj)
    {
        return (bool)obj.GetValue(EnableSmoothScrollProperty);
    }

    public static void SetEnableSmoothScroll(DependencyObject obj, bool value)
    {
        obj.SetValue(EnableSmoothScrollProperty, value);
    }

    private static void OnEnableSmoothScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if ((bool)e.OldValue)
        {
            element.PreviewMouseWheel -= UIElement_OnPreviewMouseWheel;
        }

        if ((bool)e.NewValue)
        {
            element.PreviewMouseWheel += UIElement_OnPreviewMouseWheel;
        }
    }
    private static void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        switch (sender)
        {
            case ScrollViewer scrollViewer:
                HandleScrollViewerMouseWheel(scrollViewer, e);
                break;
            case ListBox listBox:
                HandleListBoxMouseWheel(listBox, e);
                break;
        }
    }

    private static void HandleScrollViewerMouseWheel(ScrollViewer scrollViewer, MouseWheelEventArgs e)
    {
        // 调整滚动速度
        var newOffset = scrollViewer.VerticalOffset - e.Delta / 3.0;
        if (newOffset < 0)
            newOffset = 0;
        else if (newOffset > scrollViewer.ExtentHeight) newOffset = scrollViewer.ExtentHeight;
        scrollViewer.ScrollToVerticalOffset(newOffset);
        e.Handled = true;
    }

    private static void HandleListBoxMouseWheel(ListBox listBox, MouseWheelEventArgs e)
    {
        var scrollViewer = GetScrollViewer(listBox);
        if (scrollViewer != null)
        {
            HandleScrollViewerMouseWheel(scrollViewer, e);
        }
    }

    private static ScrollViewer? GetScrollViewer(DependencyObject depObj)
    {
        if (depObj is ScrollViewer scrollViewer) return scrollViewer;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);
            var result = GetScrollViewer(child);
            if (result != null) return result;
        }
        return null;
    }
}