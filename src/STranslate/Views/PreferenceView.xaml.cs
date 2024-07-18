using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using STranslate.Model;
using STranslate.ViewModels;

namespace STranslate.Views;

public partial class PreferenceView : Window
{
    //Windows消息: https://www.cnblogs.com/cncc/articles/8004771.html
    /// <summary>
    ///     执行系统命令，如移动、最小化、最大化
    /// </summary>
    private const int WM_SYSCOMMAND = 0x0112;

    /// <summary>
    ///     移动窗口的系统命令
    /// </summary>
    private const int SC_MOVE = 0xF010;

    /// <summary>
    ///     一个窗口被销毁
    /// </summary>
    private const int WM_DESTROY = 0x0002;

    /// <summary>
    ///     空消息
    /// </summary>
    private const int WM_NULL = 0x0000;

    /// <summary>
    ///     ViewModel
    /// </summary>
    private readonly PreferenceViewModel vm = new();

    public PreferenceView()
    {
        InitializeComponent();

        DataContext = vm;
#if false
            Topmost = true;
#endif
    }

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

    public void UpdateNavigation(PerferenceType type = PerferenceType.Common)
    {
        vm.UpdateNavigation(type);
    }

    /// <summary>
    ///     左键按住拖动
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var wndHelper = new WindowInteropHelper(this);
        ReleaseCapture();
        SendMessage(wndHelper.Handle, WM_SYSCOMMAND, SC_MOVE + WM_DESTROY, WM_NULL);
    }

    /// <summary>
    ///     双击最大/恢复
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }
    }
}