using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using STranslate.Util;
using STranslate.ViewModels.Preference;
using WebDav;

namespace STranslate.Views.Preference;

public partial class WebDavDialog : Window
{
    private readonly WebDavViewModel vm = Singleton<WebDavViewModel>.Instance;

    public WebDavDialog(WebDavClient client, string absolutePath, string tmpPath)
    {
        InitializeComponent();

        vm.UpdateParam(client, absolutePath, tmpPath);
        DataContext = vm;
    }

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        WindowInteropHelper wndHelper = new(this);
        ReleaseCapture();
        SendMessage(wndHelper.Handle, 0x0112, 0xF010 + 0x0002, 0);
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
                { RoutedEvent = MouseWheelEvent, Source = sender };
            var parent = ((Control)sender).Parent as UIElement;
            parent!.RaiseEvent(eventArg);
        }
    }
}