using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using STranslate.Model;

namespace STranslate.Views;

public partial class OutputView
{
    public OutputView()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     ListBox鼠标滚轮事件处理函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        //按住Ctrl滚动时不将滚动冒泡给上一层级的控件
        if (e.Handled || Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) return;

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

    private void TextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // 检查是否按住 Ctrl 键，如果按住则进行缩放等特殊操作，否则进行滚动
        if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;

        var currentFontSize = (double)Application.Current.Resources[Constant.FontSize18TextBox];

        // 根据滚轮事件更改字体大小
        if (e.Delta > 0)
        {
            if (currentFontSize < 40) currentFontSize += 2.0; // 根据需要调整放大的幅度
        }
        else
        {
            if (currentFontSize > 10) currentFontSize -= 2.0; // 根据需要调整放大的幅度
        }

        // 设置新的字体大小
        Application.Current.Resources[Constant.FontSize18TextBox] = currentFontSize;
    }

    private void UI_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true; // 阻止事件继续传递
    }
}