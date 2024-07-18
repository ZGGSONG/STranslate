using System.Windows.Forms;
using STranslate.Log;
using STranslate.Util;

namespace STranslate.Helper;

public class MouseHookHelper
{
    private readonly MouseHookUtil mouseHook;

    private bool isDown;

    private bool isMove;

    private bool isStart;

    public Action<string>? OnGetwordsHandler;

    public MouseHookHelper()
    {
        mouseHook = new MouseHookUtil();
    }

    public void MouseHookStart()
    {
        mouseHook.MouseMove += mouseHook_MouseMove;
        mouseHook.MouseDown += mouseHook_MouseDown;
        mouseHook.MouseUp += mouseHook_MouseUp;
        mouseHook.Start();
        isStart = true;
    }

    public void MouseHookStop()
    {
        mouseHook.MouseMove -= mouseHook_MouseMove;
        mouseHook.MouseDown -= mouseHook_MouseDown;
        mouseHook.MouseUp -= mouseHook_MouseUp;
        mouseHook.Stop();
        isStart = false;
    }

    private void mouseHook_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) isDown = true;
    }

    private void mouseHook_MouseMove(object? sender, MouseEventArgs e)
    {
        if (isDown && isStart) isMove = true;
    }

    private void mouseHook_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (isDown && isMove)
        {
            var interval = Singleton<ConfigHelper>.Instance.CurrentConfig?.WordPickingInterval ?? 100;
            string? content = null;
            try
            {
                content = ClipboardUtil.GetSelectedTextDiff(interval);

                if (string.IsNullOrEmpty(content))
                {
                    LogService.Logger.Debug($"可能是取词内容相同, 或者需要增加取词延迟(当前: {interval}ms)...");
                    return;
                }
            }
            catch (Exception)
            {
                LogService.Logger.Warn("获取剪贴板异常, 请重试");
                return;
            }

            Task.Run(() => OnGetwordsHandler?.Invoke(content));
        }

        isDown = false;
        isMove = false;
    }
}