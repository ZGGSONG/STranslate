using System.Windows.Forms;
using STranslate.Log;
using STranslate.Util;

namespace STranslate.Helper;

/// <summary>
/// 鼠标钩子辅助类,用于处理文本选择
/// </summary>
public sealed class MouseHookHelper
{
    private readonly MouseHookUtil _mouseHook;
    private bool _isDown;
    private bool _isMove;
    private bool _isStarted;

    // 文本选择完成后的事件
    public event Action<string>? WordsSelected;

    public MouseHookHelper()
    {
        _mouseHook = new MouseHookUtil();
    }

    /// <summary>
    /// 启动鼠标钩子
    /// </summary>
    public void Start()
    {
        if (_isStarted)
            return;

        _mouseHook.MouseMove += OnMouseMove;
        _mouseHook.MouseDown += OnMouseDown;
        _mouseHook.MouseUp += OnMouseUp;
        _mouseHook.Start();
        _isStarted = true;
    }

    /// <summary>
    /// 停止鼠标钩子
    /// </summary>
    public void Stop()
    {
        if (!_isStarted)
            return;

        UnsubscribeEvents();
        _mouseHook.Stop();
        _isStarted = false;
    }

    private void UnsubscribeEvents()
    {
        _mouseHook.MouseMove -= OnMouseMove;
        _mouseHook.MouseDown -= OnMouseDown;
        _mouseHook.MouseUp -= OnMouseUp;
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isDown = true;
        }
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (_isDown && _isStarted)
        {
            _isMove = true;
        }
    }

    private async void OnMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || !_isDown || !_isMove)
        {
            ResetState();
            return;
        }

        try
        {
            var interval = Singleton<ConfigHelper>.Instance.CurrentConfig?.WordPickingInterval ?? 100;
            var content = await ClipboardUtil.GetSelectedTextDiffAsync(interval);

            if (string.IsNullOrEmpty(content))
            {
                LogService.Logger.Debug($"可能拖拽窗;可能选中的内容相同,或需要增加取词延迟(当前:{interval}ms)");
                return;
            }

            WordsSelected?.Invoke(content);
        }
        catch (Exception)
        {
            LogService.Logger.Warn("获取剪贴板内容失败,请重试");
        }
        finally
        {
            ResetState();
        }
    }

    private void ResetState()
    {
        _isDown = false;
        _isMove = false;
    }
}