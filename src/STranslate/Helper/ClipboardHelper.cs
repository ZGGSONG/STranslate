using System.Windows;
using System.Windows.Interop;
using STranslate.Log;
using STranslate.Util;
using STranslate.Views;

namespace STranslate.Helper;

public class ClipboardHelper
{
    private IntPtr _nextClipboardViewer;
    private HwndSource? _source;

    public event Action<string>? OnClipboardChanged;

    public bool Start(out string error)
    {
        var result = false;
        _source =
            PresentationSource.FromVisual(Application.Current.Windows.OfType<MainView>().First()) as HwndSource;
        if (_source is null)
        {
            error = "监听剪贴板失败，请重新监听...";
            LogService.Logger.Error(error);
            goto End;
        }

        // 开启时添加一条空字符串到剪贴板
        // 避免第一条为文本信息而在开启功能时触发
        Copy();

        _source.AddHook(WndProc);

        // 设置当前窗口为剪贴板的下一个观察者
        _nextClipboardViewer = CommonUtil.SetClipboardViewer(_source.Handle);

        error = "";
        result = true;
        End:
        return result;
    }

    public bool Stop(out string error)
    {
        var result = false;
        if (_source == null)
        {
            error = "取消监听剪贴板失败，请重启以重置...";
            LogService.Logger.Error(error);
            goto End;
        }

        // 移除窗口作为剪贴板的观察者
        CommonUtil.ChangeClipboardChain(_source.Handle, _nextClipboardViewer);
        _source.RemoveHook(WndProc);
        _source = null;

        error = "";
        result = true;
        End:
        return result;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == CommonUtil.WM_DRAWCLIPBOARD)
        {
            // 剪贴板内容变化事件发生
            var content = Clipboard.GetText();
            if (!string.IsNullOrEmpty(content)) OnClipboardChanged?.Invoke(content);
        }
        else if (msg == CommonUtil.WM_CHANGECBCHAIN)
        {
            // 下一个观察者已经处理了变化
            if (wParam == _nextClipboardViewer)
                _nextClipboardViewer = lParam;
            // 将消息传递给下一个观察者
            else if (_nextClipboardViewer != IntPtr.Zero)
                CommonUtil.SendMessage(_nextClipboardViewer, msg, wParam, lParam);
        }

        return IntPtr.Zero;
    }

    /// <summary>
    ///     复制
    /// </summary>
    /// <param name="content"></param>
    /// <remarks>
    public static void Copy(string content = "", bool copy = false)
    {
        if (Singleton<ConfigHelper>.Instance.CurrentConfig?.UseFormsCopy ?? false)
            //https://referencesource.microsoft.com/#System.Windows.Forms/winforms/Managed/System/WinForms/Clipboard.cs,c6184ddc7e88f288,references
            //看了下源码，似乎只是Forms库下默认每隔100ms重试10次，如果都出错才抛出异常，而底层都是调用的 UnsafeNativeMethods.OleGetClipboard方法
            System.Windows.Forms.Clipboard.SetDataObject(content, copy);
        else
            //https://referencesource.microsoft.com/#PresentationCore/Core/CSharp/System/Windows/Clipboard.cs,68ca81bbc84f706a
            Clipboard.SetDataObject(content, copy);
    }
}