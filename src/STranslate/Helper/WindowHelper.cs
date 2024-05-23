using STranslate.Util;
using System.Windows;
using System.Windows.Interop;

namespace STranslate.Helper;

public class WindowHelper
{
    /// <summary>
    /// 窗口是否可见
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public static bool IsWindowVisible(Window window)
    {
        return CommonUtil.IsWindowVisible(new WindowInteropHelper(window).Handle);
    }

    /// <summary>
    /// 窗口是否在最上层
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public static bool IsWindowInForeground(Window window)
    {
        return new WindowInteropHelper(window).Handle == CommonUtil.GetForegroundWindow();
    }

    /// <summary>
    /// 设置窗口在最上层
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public static bool SetWindowInForeground(Window window)
    {
        return CommonUtil.SetForegroundWindow(new WindowInteropHelper(window).Handle);
    }
}
