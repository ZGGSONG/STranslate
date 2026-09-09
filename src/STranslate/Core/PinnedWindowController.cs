using STranslate.Helpers;
using STranslate.Plugin;
using STranslate.Views;
using System.Windows;
using System.Windows.Media.Imaging;
using DrawingRectangle = System.Drawing.Rectangle;

namespace STranslate.Core;

/// <summary>只在 UI 线程管理静态贴图及截图避让，不参与 OCR 或翻译。</summary>
public sealed class PinnedWindowController(Internationalization i18n, ISnackbar snackbar)
{
    private readonly HashSet<PinnedImageTranslateWindow> _windows = [];
    private readonly PinnedCaptureCoordinator _captureCoordinator = new(
        action => Application.Current.Dispatcher.InvokeAsync(action).Task,
        FlushDesktop);

    internal void CopyText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;
        try
        {
            Clipboard.SetText(text);
        }
        catch (System.Runtime.InteropServices.ExternalException ex)
        {
            snackbar.ShowError($"{i18n.GetTranslation("CopyFailed")}: {ex.Message}");
        }
    }

    internal PinnedImageTranslateWindow CreateWindow(PinnedImageTranslateSnapshot snapshot)
    {
        Application.Current.Dispatcher.VerifyAccess();
        var window = new PinnedImageTranslateWindow(this);
        _windows.Add(window);
        try
        {
            window.Initialize(snapshot);
            window.ShowActivated = !_captureCoordinator.IsActive;
            window.Show();
            return window;
        }
        catch
        {
            window.Close();
            throw;
        }
    }

    internal void Unregister(PinnedImageTranslateWindow window) => _windows.Remove(window);

    internal void OnWindowSourceInitialized(PinnedImageTranslateWindow window)
    {
        _captureCoordinator.OnTargetInitialized(window);
    }

    internal ValueTask<IAsyncDisposable?> BeginCaptureAsync(CancellationToken cancellationToken = default) =>
        _captureCoordinator.BeginAsync(_windows, cancellationToken);

    internal void CloseAll()
    {
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(CloseAll);
            return;
        }
        foreach (var window in _windows.ToArray())
            window.Close();
    }

    private static void FlushDesktop() => Win32Helper.FlushDesktopComposition();
}

internal interface IPinnedCaptureTarget
{
    void CloseTransientUiForCapture();
    bool SetCaptureCloaked(bool cloaked);
}

/// <summary>负责截图期间贴图窗口的并发门控与隐藏恢复，可脱离真实窗口进行测试。</summary>
internal sealed class PinnedCaptureCoordinator(Func<Action, Task> dispatchAsync, Action flushDesktop)
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    internal bool IsActive { get; private set; }

    internal void OnTargetInitialized(IPinnedCaptureTarget target)
    {
        if (IsActive && !target.SetCaptureCloaked(true))
            throw new InvalidOperationException("Failed to cloak a pinned window during capture.");
    }

    internal async ValueTask<IAsyncDisposable?> BeginAsync(
        IReadOnlyCollection<IPinnedCaptureTarget> targets,
        CancellationToken cancellationToken = default)
    {
        // 同一截图尚未结束时忽略重复触发，不把旧输入排队成下一次截图。
        if (!await _gate.WaitAsync(0, cancellationToken))
            return null;

        try
        {
            await dispatchAsync(() =>
            {
                IsActive = true;
                foreach (var target in targets)
                {
                    target.CloseTransientUiForCapture();
                    if (!target.SetCaptureCloaked(true))
                        throw new InvalidOperationException("Failed to cloak a pinned window before capture.");
                }
                if (targets.Count > 0)
                    flushDesktop();
            });
            return new CaptureLease(this, targets);
        }
        catch
        {
            await EndAsync(targets);
            throw;
        }
    }

    private async ValueTask EndAsync(IReadOnlyCollection<IPinnedCaptureTarget> targets)
    {
        try
        {
            await dispatchAsync(() =>
            {
                IsActive = false;
                var restored = true;
                foreach (var target in targets)
                    restored &= target.SetCaptureCloaked(false);
                if (targets.Count > 0)
                    flushDesktop();
                if (!restored)
                    throw new InvalidOperationException("Failed to restore pinned windows after capture.");
            });
        }
        finally
        {
            _gate.Release();
        }
    }

    private sealed class CaptureLease(
        PinnedCaptureCoordinator owner,
        IReadOnlyCollection<IPinnedCaptureTarget> targets) : IAsyncDisposable
    {
        private PinnedCaptureCoordinator? _owner = owner;

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _owner, null) is { } current)
                await current.EndAsync(targets);
        }
    }
}

/// <summary>已生成结果的独立显示快照；图片冻结，选择数据逐项复制。</summary>
internal sealed record PinnedImageTranslateSnapshot(
    BitmapSource SourceImage,
    ImageTranslateOverlayDocument TranslationOverlay,
    IReadOnlyList<OcrWord> OriginalWords,
    IReadOnlyList<OcrWord> TranslatedWords,
    DrawingRectangle PhysicalBounds,
    bool ShowOriginal)
{
    internal static PinnedImageTranslateSnapshot Create(
        BitmapSource source, ImageTranslateOverlayDocument overlay,
        IReadOnlyList<OcrWord> originalWords, IReadOnlyList<OcrWord> translatedWords,
        DrawingRectangle bounds, bool showOriginal = false)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0 ||
            source.PixelWidth != bounds.Width || source.PixelHeight != bounds.Height ||
            !source.IsFrozen || overlay.IsEmpty)
            throw new ArgumentException("Pin requires a frozen, completed result matching the physical image bounds.");

        return new(source, new ImageTranslateOverlayDocument(overlay.Items.ToArray(), []),
            CloneWords(originalWords), CloneWords(translatedWords), bounds, showOriginal);
    }

    internal PinnedImageTranslateDisplayContent GetDisplayContent(bool showOriginal) => showOriginal
        ? new(SourceImage, null)
        : new(SourceImage, TranslationOverlay);

    private static IReadOnlyList<OcrWord> CloneWords(IReadOnlyList<OcrWord> words) =>
        Array.AsReadOnly(words.Select(word => new OcrWord
        {
            Text = word.Text,
            BoundingBox = word.BoundingBox,
            StartIndexInFullText = word.StartIndexInFullText,
            VisualLineIndex = word.VisualLineIndex,
            ParagraphIndex = word.ParagraphIndex,
        }).ToArray());
}

internal sealed record PinnedImageTranslateDisplayContent(
    BitmapSource Image,
    ImageTranslateOverlayDocument? Overlay);
