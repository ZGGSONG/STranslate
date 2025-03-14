using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using STranslate.ViewModels.Preference.History;

namespace STranslate.Views.Preference.History;

/// <summary>
///     HistoryContentPage.xaml 的交互逻辑
/// </summary>
public partial class HistoryContentPage : UserControl
{
    public HistoryContentPage(HistoryContentViewModel? vm)
    {
        InitializeComponent();

        DataContext = vm;
    }

    public void UpdateVM(HistoryContentViewModel vm)
    {
        DataContext = vm;
    }

    /// <summary>
    ///     ListBox鼠标滚轮事件处理函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        //按住Ctrl滚动时不将滚动冒泡给上一层级的控件
        if (!e.Handled && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
        {
            // ListBox拦截鼠标滚轮事件
            e.Handled = true;

            // 激发一个鼠标滚轮事件，冒泡给外层ListBox接收到
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = sender
            };
            var parent = ((Control)sender).Parent as UIElement;
            parent!.RaiseEvent(eventArg);
        }
    }
}