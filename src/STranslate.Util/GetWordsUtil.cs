using System.Runtime.InteropServices;
using System.Windows.Input;

namespace STranslate.Util;

[Obsolete]
public class GetWordsUtil
{
    public static string Get(int interval = 100)
    {
        SendCtrlCV();
        Thread.Sleep(interval);

        return GetText();
    }

    public static string MouseSlidGet(int interval = 100)
    {
        var oldTxt = GetText();
        SendCtrlCV();
        Thread.Sleep(interval);

        //为了鼠标划词做对比
        var newTxt = GetText();
        return newTxt == oldTxt ? string.Empty : newTxt.Trim();
    }

    /// <summary>
    ///     模拟粘贴操作
    /// </summary>
    public static void Paste()
    {
        SendCtrlCV(true);
    }

    [Obsolete]
    public static string WIN32SetText(string text)
    {
        SetText(text, out var ret);
        return ret;
    }

    private static void SendCtrlCV(bool isPaste = false)
    {
        uint KEYEVENTF_KEYUP = 2;

        CommonUtil.keybd_event(Keys.ControlKey, 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(KeyInterop.VirtualKeyFromKey(Key.LeftAlt), 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(KeyInterop.VirtualKeyFromKey(Key.RightAlt), 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(Keys.LWin, 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(Keys.RWin, 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(Keys.ShiftKey, 0, KEYEVENTF_KEYUP, 0);

        CommonUtil.keybd_event(Keys.ControlKey, 0, 0, 0);
        CommonUtil.keybd_event(isPaste ? Keys.V : Keys.C, 0, 0, 0);
        CommonUtil.keybd_event(isPaste ? Keys.V : Keys.C, 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(Keys.ControlKey, 0, KEYEVENTF_KEYUP, 0); // 'Left Control Up
    }

    #region Clipboard

    /// <summary>
    ///     向剪贴板中添加文本
    /// </summary>
    /// <param name="text">文本</param>
    internal static void SetText(string text, out string error)
    {
        try
        {
            if (!CommonUtil.OpenClipboard(IntPtr.Zero))
                throw new InvalidOperationException("Unable to open the clipboard.");
            CommonUtil.EmptyClipboard();

            // 获取 Unicode 文本格式的常量
            var cfUnicodeText = 13; // CF_UNICODETEXT
            // 将文本分配到非托管内存
            var hGlobal = Marshal.StringToHGlobalUni(text);
            // 将文本添加到剪贴板
            CommonUtil.SetClipboardData(cfUnicodeText, hGlobal);
            // 关闭剪贴板
            CommonUtil.CloseClipboard();

            // 在使用后释放非托管内存
            Marshal.FreeHGlobal(hGlobal);

            error = "";
        }
        catch (Exception ex)
        {
            // 处理异常，例如记录错误信息
            Console.WriteLine($"Error: {ex.Message}");
            error = ex.Message;
        }
    }

    internal static string GetText()
    {
        var value = string.Empty;
        CommonUtil.OpenClipboard(IntPtr.Zero);
        if (CommonUtil.IsClipboardFormatAvailable(13))
        {
            var ptr = CommonUtil.GetClipboardData(13);
            if (ptr != IntPtr.Zero) value = Marshal.PtrToStringUni(ptr) ?? string.Empty;
        }

        CommonUtil.CloseClipboard();
        return value;
    }

    #endregion Clipboard
}