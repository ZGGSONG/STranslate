using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using STranslate.Util;
using STranslate.ViewModels.Preference;

namespace STranslate.Views.Preference;

public partial class TranslatorPage : UserControl
{
    public TranslatorPage()
    {
        InitializeComponent();
        DataContext = Singleton<TranslatorViewModel>.Instance;
    }

    public static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        do
        {
            if (current is T t) return t;
            current = VisualTreeHelper.GetParent(current);
        } while (current != null);

        return null;
    }

    /// <summary>
    ///     ListBox鼠标滚轮事件处理函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ServiceListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        //按住Ctrl滚动时不将滚动冒泡给上一层级的控件
        if (!e.Handled && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
        {
            // ListBox拦截鼠标滚轮事件
            e.Handled = true;

            // 激发一个鼠标滚轮事件，冒泡给外层ListBox接收到
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                { RoutedEvent = MouseWheelEvent, Source = sender };
            var parent = ((Control)sender).Parent as UIElement;
            parent!.RaiseEvent(eventArg);
        }
    }
}