using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace STranslate.Helper;

public class WindowHelper
{
    private const string WINDOW_CLASS_CONSOLE = "ConsoleWindowClass";
    private const string WINDOW_CLASS_WINTAB = "Flip3D";
    private const string WINDOW_CLASS_PROGMAN = "Progman";
    private const string WINDOW_CLASS_WORKERW = "WorkerW";


    private static IntPtr _hwnd_shell;
    private static IntPtr _hwnd_desktop;

    //Accessors for shell and desktop handlers
    //Will set the variables once and then will return them
    private static IntPtr HWND_SHELL => _hwnd_shell != IntPtr.Zero ? _hwnd_shell : _hwnd_shell = GetShellWindow();

    private static IntPtr HWND_DESKTOP =>
        _hwnd_desktop != IntPtr.Zero ? _hwnd_desktop : _hwnd_desktop = GetDesktopWindow();

    /// <summary>
    ///     窗口是否可见
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public static bool IsWindowVisible(Window window)
    {
        return IsWindowVisible(new WindowInteropHelper(window).Handle);
    }

    /// <summary>
    ///     窗口是否在最上层
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public static bool IsWindowInForeground(Window window)
    {
        return new WindowInteropHelper(window).Handle == GetForegroundWindow();
    }

    /// <summary>
    ///     设置窗口在最上层
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public static bool SetWindowInForeground(Window window)
    {
        return SetForegroundWindow(new WindowInteropHelper(window).Handle);
    }

    /// <summary>
    ///     是否全屏
    /// </summary>
    /// <returns></returns>
    public static bool IsWindowFullscreen()
    {
        //get current active window
        var hWnd = GetForegroundWindow();

        if (hWnd.Equals(IntPtr.Zero)) return false;

        //if current active window is desktop or shell, exit early
        if (hWnd.Equals(HWND_DESKTOP) || hWnd.Equals(HWND_SHELL)) return false;

        var sb = new StringBuilder(256);
        GetClassName(hWnd, sb, sb.Capacity);
        var windowClass = sb.ToString();

        //for Win+Tab (Flip3D)
        if (windowClass == WINDOW_CLASS_WINTAB) return false;

        RECT appBounds;
        GetWindowRect(hWnd, out appBounds);

        //for console (ConsoleWindowClass), we have to check for negative dimensions
        if (windowClass == WINDOW_CLASS_CONSOLE) return appBounds.Top < 0 && appBounds.Bottom < 0;

        //for desktop (Progman or WorkerW, depends on the system), we have to check
        if (windowClass is WINDOW_CLASS_PROGMAN or WINDOW_CLASS_WORKERW)
        {
            var hWndDesktop = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
            hWndDesktop = FindWindowEx(hWndDesktop, IntPtr.Zero, "SysListView32", "FolderView");
            if (!hWndDesktop.Equals(IntPtr.Zero)) return false;
        }

        var screenBounds = Screen.FromHandle(hWnd).Bounds;
        return appBounds.Bottom - appBounds.Top == screenBounds.Height &&
               appBounds.Right - appBounds.Left == screenBounds.Width;
    }

    [DllImport("user32.dll")]
    internal static extern IntPtr GetShellWindow();

    [DllImport("user32.dll")]
    internal static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern int GetWindowRect(IntPtr hwnd, out RECT rc);

    [DllImport("user32.DLL")]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass,
        string? lpszWindow);

    [DllImport("user32.dll")]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}