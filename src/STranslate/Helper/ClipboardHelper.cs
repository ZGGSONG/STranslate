using STranslate.Log;
using STranslate.Util;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace STranslate.Helper
{
    public class ClipboardHelper
    {
        private HwndSource? _source;
        private IntPtr _nextClipboardViewer;

        public event Action<string>? OnClipboardChanged;

        public bool Start(out string error)
        {
            bool result = false;
            _source = PresentationSource.FromVisual(Application.Current.Windows.Cast<Window>().FirstOrDefault()) as HwndSource;
            if (_source is null)
            {
                error = "监听剪贴板失败，请重新监听...";
                LogService.Logger.Error(error);
                goto End;
            }

            // 开启时添加一条空字符串到剪贴板
            // 避免第一条为文本信息而在开启功能时触发
            Clipboard.SetDataObject("", false);

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
            bool result = false;
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
                string content = Clipboard.GetText();
                if (!string.IsNullOrEmpty(content))
                {
                    OnClipboardChanged?.Invoke(content);
                }
            }
            else if (msg == CommonUtil.WM_CHANGECBCHAIN)
            {
                // 下一个观察者已经处理了变化
                if (wParam == _nextClipboardViewer)
                {
                    _nextClipboardViewer = lParam;
                }
                // 将消息传递给下一个观察者
                else if (_nextClipboardViewer != IntPtr.Zero)
                {
                    CommonUtil.SendMessage(_nextClipboardViewer, msg, wParam, lParam);
                }
            }

            return IntPtr.Zero;
        }
    }
}
