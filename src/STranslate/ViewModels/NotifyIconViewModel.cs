using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using ScreenGrab;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference;
using STranslate.Views;

namespace STranslate.ViewModels;

public partial class NotifyIconViewModel : ObservableObject
{
    private readonly ConfigHelper _configHelper = Singleton<ConfigHelper>.Instance;

    private readonly InputViewModel _inputViewModel = Singleton<InputViewModel>.Instance;
    private readonly OutputViewModel _outputViewModel = Singleton<OutputViewModel>.Instance;

    private ClipboardHelper? _clipboardHelper;

    [ObservableProperty] private bool _isClipboardMonitor;

    [ObservableProperty] private string _isEnabledClipboardMonitor = Constant.TagFalse;

    [ObservableProperty] private bool _isForbiddenShortcuts;

    [ObservableProperty] private bool _isMousehook;

    [ObservableProperty] private bool _isScreenshotExecuting;

    public NotifyIconViewModel()
    {
        UpdateToolTip();
        SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
        WeakReferenceMessenger.Default.Register<ExternalCallMessenger>(this, (o, e) => ExternalCallHandler(e));
#if DEBUG
        LogService.Logger.OnErrorOccured += ShowBalloonTip;
#endif
    }

    /// <summary>
    ///     图标、提示
    /// </summary>
    public NotifyIconModel NIModel { get; } = new();

    /// <summary>
    ///     屏蔽快捷键事件
    /// </summary>
    public event Action<Window, bool>? OnForbiddenShortcuts;

    /// <summary>
    ///     监听鼠标划词事件
    /// </summary>
    public event Action<Window>? OnMousehook;

    /// <summary>
    ///     退出事件
    /// </summary>
    public event Action? OnExit;

    /// <summary>
    ///     Toast通知事件
    /// </summary>
    public event Action<string>? OnShowBalloonTip;

    /// <summary>
    ///     外部调用注册
    /// </summary>
    /// <param name="e"></param>
    private void ExternalCallHandler(ExternalCallMessenger e)
    {
        var actions = new Dictionary<ExternalCallAction, Action<Window, string>>
        {
            {
                ExternalCallAction.translate,
                (view, content) =>
                {
                    if (string.IsNullOrWhiteSpace(content))
                        InputTranslate(view);
                    else
                        TranslateHandler(view, content);
                }
            },
            {
                ExternalCallAction.translate_force,
                (view, content) =>
                {
                    if (string.IsNullOrWhiteSpace(content))
                        InputTranslate(view);
                    else
                        TranslateHandler(view, content, true); //表示对象非空，强制翻译
                }
            },
            { ExternalCallAction.translate_input, (view, _) => InputTranslate(view) },
            {
                ExternalCallAction.translate_ocr,
                async (_, content) =>
                {
                    if (string.IsNullOrWhiteSpace(content))
                        ScreenShotHandler();
                    else
                        await ScreenshotCallbackAsync(BitmapUtil.ReadImageFile(content));
                }
            },
            { ExternalCallAction.translate_crossword, (view, _) => CrossWordTranslate(view) },
            { ExternalCallAction.translate_mousehook, (view, _) => MousehookTranslate(view) },
            { ExternalCallAction.translate_replace, async (view, _) => await ReplaceTranslateAsync(view) },
            { ExternalCallAction.listenclipboard, (view, _) => ClipboardMonitor(view) },
            {
                ExternalCallAction.ocr,
                async (_, content) =>
                {
                    if (string.IsNullOrWhiteSpace(content))
                        OCRHandler();
                    else
                        await OCRCallbackAsync(BitmapUtil.ReadImageFile(content));
                }
            },
            {
                ExternalCallAction.ocr_silence,
                async (_, content) =>
                {
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        SilentOCRHandler();
                    }
                    else
                    {
                        var bitmap = BitmapUtil.ReadImageFile(content);
                        if (bitmap != null) await SilentOCRCallbackAsync(bitmap);
                    }
                }
            },
            {
                ExternalCallAction.tts_silence,
                async (_, content) =>
                {
                    if (!string.IsNullOrEmpty(content))
                        await SilentTTSHandlerAsync(content);
                }
            },
            {
                ExternalCallAction.ocr_qrcode,
                (_, content) =>
                {
                    if (string.IsNullOrWhiteSpace(content))
                        QRCodeHandler();
                    else
                        QRCodeCallback(BitmapUtil.ReadImageFile(content));
                }
            },
            { ExternalCallAction.open_window, (view, _) => OpenMainWindow(view) },
            { ExternalCallAction.open_preference, (_, _) => OpenPreference() },
            { ExternalCallAction.open_history, (_, _) => OpenHistory() },
            { ExternalCallAction.forbiddenhotkey, (view, _) => ForbiddenShortcuts(view) }
        };

        CommonUtil.InvokeOnUIThread(() =>
        {
            var view = Application.Current.Windows.OfType<MainView>().First();

            if (actions.TryGetValue(e.ECAction, out var handler)) handler?.Invoke(view, e.Content);
        });
    }

    public void UpdateToolTip(string msg = "")
    {
        const int maxLength = 127;
        var isAdmin = CommonUtil.IsUserAdministrator();
        var toolTipFormat = isAdmin ? "STranslate {0}\n{1} #\n{2}" : "STranslate {0} {1}#\n{2}";

        var adminStr = isAdmin ? AppLanguageManager.GetString("Admin") : "";
        // 计算基础提示信息的长度
        var baseToolTip = string.Format(toolTipFormat, Constant.AppVersion, adminStr, string.Empty);
        var baseLength = baseToolTip.Length;

        // 剩余可用长度
        var availableLength = maxLength - baseLength;

        // 如果msg超过可用长度，进行截断并添加省略号
        if (msg.Length > availableLength)
        {
            var truncatedMsg = msg[..(availableLength - 3)] + "...";
            NIModel.ToolTip = string.Format(toolTipFormat, Constant.AppVersion, adminStr, truncatedMsg);
        }
        else
        {
            NIModel.ToolTip = string.Format(toolTipFormat, Constant.AppVersion, adminStr, msg);
        }
    }

    [RelayCommand]
    private void DoubleClick(Window view)
    {
        switch (_configHelper.CurrentConfig?.DoubleTapTrayFunc ?? DoubleTapFuncEnum.InputFunc)
        {
            case DoubleTapFuncEnum.InputFunc:
                InputTranslateCommand.Execute(view);
                break;

            case DoubleTapFuncEnum.ScreenFunc:
                ScreenShotTranslateCommand.Execute(null);
                break;

            case DoubleTapFuncEnum.MouseHookFunc:
                MousehookTranslateCommand.Execute(view);
                break;

            case DoubleTapFuncEnum.OCRFunc:
                OCRCommand.Execute(null);
                break;

            case DoubleTapFuncEnum.ShowViewFunc:
                OpenMainWindowCommand.Execute(view);
                break;

            case DoubleTapFuncEnum.PreferenceFunc:
                OpenPreferenceCommand.Execute(null);
                break;

            case DoubleTapFuncEnum.ForbidShortcutFunc:
                ForbiddenShortcutsCommand.Execute(view);
                break;

            case DoubleTapFuncEnum.ExitFunc:
                ExitCommand.Execute(null);
                break;
        }
    }

    [RelayCommand]
    private void InputTranslate(Window view)
    {
        //如果重复执行先取消上一步操作
        _outputViewModel.SingleTranslateCancelCommand.Execute(null);
        _outputViewModel.SingleTranslateBackCancelCommand.Execute(null);
        _inputViewModel.TranslateCancelCommand.Execute(null);
        _inputViewModel.Save2VocabularyBookCancelCommand.Execute(null);
        ClearAll();
        ShowAndActive(view, _configHelper.CurrentConfig?.IsFollowMouse ?? false);
    }

    [RelayCommand]
    private void CrossWordTranslate(Window view)
    {
        if (!TryGetWord(out var content) || content == null) return;

        content = _configHelper.CurrentConfig?.LineBreakHandler switch
        {
            LineBreakHandlingMode.RemoveExtraLineBreak => StringUtil.NormalizeText(content),
            LineBreakHandlingMode.RemoveAllLineBreak => StringUtil.RemoveLineBreaks(content),
            LineBreakHandlingMode.RemoveAllLineBreakWithoutSpace => StringUtil.RemoveLineBreaks(content, false),
            _ => content,
        };

        TranslateHandler(view, content);
    }

    internal void TranslateHandler(Window view, string content, object? obj = null)
    {
        //如果重复执行先取消上一步操作
        _outputViewModel.SingleTranslateCancelCommand.Execute(null);
        _outputViewModel.SingleTranslateBackCancelCommand.Execute(null);
        _inputViewModel.TranslateCancelCommand.Execute(null);
        _inputViewModel.Save2VocabularyBookCancelCommand.Execute(null);

        //增量翻译
        if (_configHelper.CurrentConfig?.IncrementalTranslation ?? false)
        {
            ClearOutput();
            var input = _inputViewModel.InputContent;
            _inputViewModel.InputContent = string.IsNullOrEmpty(input) ? string.Empty : input + " ";
        }
        else
        {
            ClearAll();
        }

        _inputViewModel.InputContent += content;
        ShowAndActive(view, _configHelper.CurrentConfig?.IsFollowMouse ?? false);

        _inputViewModel.TranslateCommand.Execute(obj);
    }

    [RelayCommand]
    private void MousehookTranslate(Window view)
    {
        ShowAndActive(view);
        OnMousehook?.Invoke(view);
    }

    private void ScreenshotHandler(object? obj, Action? action)
    {
        switch (obj)
        {
            case null:
                var haveActive = Application.Current.Windows.Cast<Window>()
                    .Aggregate(false, (current, window) => current | window.IsActive);
                if (haveActive)
                    break;
                goto Last;
            case "header":
                HideMainView();
                break;
        }

        Task.Delay(200).ContinueWith(_ => CommonUtil.InvokeOnUIThread(() => action?.Invoke()));

        return;

    Last:
        action?.Invoke();
    }

    private async Task ScreenshotHandlerAsync(object? obj, Action<CancellationToken?>? action, CancellationToken token)
    {
        switch (obj)
        {
            case null:
                var haveActive = Application.Current.Windows.Cast<Window>()
                    .Aggregate(false, (current, window) => current | window.IsActive);
                if (haveActive)
                    break;
                goto Last;
            case "header":
                HideMainView();
                break;
        }

        await Task.Delay(200)
            .ContinueWith(_ => CommonUtil.InvokeOnUIThread(() => action?.Invoke(token)), token);

        return;

    Last:
        action?.Invoke(token);
    }

    [RelayCommand]
    private void QRCode(object obj) => ScreenshotHandler(obj, QRCodeHandler);

    internal void QRCodeHandler()
    {
        if (ScreenGrabber.IsCapturing) return;
        ScreenGrabber.OnCaptured = bitmap => QRCodeCallback(bitmap);
        ScreenGrabber.Capture(_configHelper.CurrentConfig?.ShowAuxiliaryLine ?? true);
    }

    private void QRCodeCallback(Bitmap? bitmap)
    {
        if (bitmap == null)
        {
            ShowBalloonTip("图像不存在");
            return;
        }

        //显示OCR窗口
        var view = Application.Current.Windows.OfType<OCRView>().FirstOrDefault();
        view ??= new OCRView();
        ShowAndActive(view);

        //显示截图
        var bs = BitmapUtil.ConvertBitmap2BitmapSource(bitmap, GetImageFormat());

        Singleton<OCRViewModel>.Instance.Bs = bs;
        Singleton<OCRViewModel>.Instance.GetImg = bs;

        Singleton<OCRViewModel>.Instance.QRCodeCommand.Execute(bs);
    }

    [RelayCommand]
    private void OCR(object obj) => ScreenshotHandler(obj, OCRHandler);

    internal void OCRHandler()
    {
        if (ScreenGrabber.IsCapturing) return;
        ScreenGrabber.OnCaptured = async bitmap => await OCRCallbackAsync(bitmap);
        ScreenGrabber.Capture(_configHelper.CurrentConfig?.ShowAuxiliaryLine ?? true);
    }

    private async Task OCRCallbackAsync(Bitmap? bitmap)
    {
        if (bitmap == null)
        {
            ShowBalloonTip("图像不存在");
            return;
        }

        //显示OCR窗口
        var view = Application.Current.Windows.OfType<OCRView>().FirstOrDefault();
        view ??= new OCRView();
        ShowAndActive(view);

        //显示截图
        var bs = BitmapUtil.ConvertBitmap2BitmapSource(bitmap, GetImageFormat());
        Singleton<OCRViewModel>.Instance.ResetImgCommand.Execute(view.FindName("ImgCtl"));
        Singleton<OCRViewModel>.Instance.Bs = bs;

        await Singleton<OCRViewModel>.Instance.RecertificationCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private void SilentOCR(object? obj) => ScreenshotHandler(obj, SilentOCRHandler);

    internal void SilentOCRHandler()
    {
        if (ScreenGrabber.IsCapturing) return;
        ScreenGrabber.OnCaptured = async bitmap => await SilentOCRCallbackAsync(bitmap);
        ScreenGrabber.Capture(_configHelper.CurrentConfig?.ShowAuxiliaryLine ?? true);
    }

    private async Task SilentOCRCallbackAsync(Bitmap bitmap)
    {
        try
        {
            CursorManager.Instance.Execute();
            var bytes = BitmapUtil.ConvertBitmap2Bytes(bitmap, GetImageFormat());
            var ocrResult = await Singleton<OCRScvViewModel>.Instance.ExecuteAsync(bytes, WindowType.Main,
                lang: _configHelper.CurrentConfig?.MainOcrLang ?? LangEnum.auto);
            //判断结果
            if (!ocrResult.Success) throw new Exception(ocrResult.ErrorMsg);
            var getText = ocrResult.Text;

            getText = _configHelper.CurrentConfig?.LineBreakHandler switch
            {
                LineBreakHandlingMode.RemoveExtraLineBreak => StringUtil.NormalizeText(getText),
                LineBreakHandlingMode.RemoveAllLineBreak => StringUtil.RemoveLineBreaks(getText),
                LineBreakHandlingMode.RemoveAllLineBreakWithoutSpace => StringUtil.RemoveLineBreaks(getText, false),
                _ => getText,
            };

            //写入剪贴板
            ClipboardHelper.Copy(getText);
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("静默OCR失败", ex);
        }
        finally
        {
            CursorManager.Instance.Restore();
            MemoUtil.FlushMemory();
        }
    }

    [RelayCommand]
    private async Task SilentTTSAsync()
    {
        if (!TryGetWord(out var content) || content == null)
        {
            return;
        }

        await SilentTTSHandlerAsync(content);
    }

    internal async Task SilentTTSHandlerAsync(string content)
    {
        await Singleton<TTSViewModel>.Instance.SilentSpeakTextAsync(content);
    }


    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ScreenShotTranslateAsync(object obj, CancellationToken token) => await ScreenshotHandlerAsync(obj, ScreenShotHandler, token);

    internal void ScreenShotHandler(CancellationToken? token = null)
    {
        if (ScreenGrabber.IsCapturing) return;
        ScreenGrabber.OnCaptured = async bitmap => await ScreenshotCallbackAsync(bitmap, token);
        ScreenGrabber.Capture(_configHelper.CurrentConfig?.ShowAuxiliaryLine ?? true);
    }

    internal async Task ScreenshotCallbackAsync(Bitmap? bitmap, CancellationToken? token = null)
    {
        if (bitmap == null)
        {
            ShowBalloonTip("图像不存在");
            return;
        }

        //如果重复执行先取消上一步操作
        _outputViewModel.SingleTranslateCancelCommand.Execute(null);
        _outputViewModel.SingleTranslateBackCancelCommand.Execute(null);
        _inputViewModel.TranslateCancelCommand.Execute(null);
        _inputViewModel.Save2VocabularyBookCancelCommand.Execute(null);

        //增量翻译
        if (_configHelper.CurrentConfig?.IncrementalTranslation ?? false)
        {
            ClearOutput();
            var input = _inputViewModel.InputContent;
            _inputViewModel.InputContent = string.IsNullOrEmpty(input) ? string.Empty : input + " ";
        }
        else
        {
            ClearAll();
        }

        var view = Application.Current.Windows.OfType<MainView>().First();
        //var view = Application.Current.MainWindow!;
        ShowAndActive(view, _configHelper.CurrentConfig?.IsFollowMouse ?? false);

        var bytes = BitmapUtil.ConvertBitmap2Bytes(bitmap, GetImageFormat());
        try
        {
            // 显示水印的情况下，如果输入框为空则填充一个空格，以显示动画避免与水印重叠
            if ((_configHelper.CurrentConfig?.IsShowMainPlaceholder ?? true) && _inputViewModel.InputContent == "")
                _inputViewModel.InputContent = " ";
            IsScreenshotExecuting = true;
            var getText = "";
            var ocrResult = await Singleton<OCRScvViewModel>.Instance.ExecuteAsync(bytes, WindowType.Main, token,
                _configHelper.CurrentConfig?.MainOcrLang ?? LangEnum.auto);
            //判断结果
            if (!ocrResult.Success) throw new Exception("OCR失败: " + ocrResult.ErrorMsg);
            getText = ocrResult.Text;

            getText = _configHelper.CurrentConfig?.LineBreakHandler switch
            {
                LineBreakHandlingMode.RemoveExtraLineBreak => StringUtil.NormalizeText(getText),
                LineBreakHandlingMode.RemoveAllLineBreak => StringUtil.RemoveLineBreaks(getText),
                LineBreakHandlingMode.RemoveAllLineBreakWithoutSpace => StringUtil.RemoveLineBreaks(getText, false),
                _ => getText,
            };
            //截图翻译OCR后自动复制
            if (_configHelper.CurrentConfig?.IsScreenshotOcrAutoCopyText ?? false)
                ClipboardHelper.Copy(getText);
            // 如果仅有空格则移除
            if (string.IsNullOrWhiteSpace(_inputViewModel.InputContent))
                _inputViewModel.InputContent = "";
            _inputViewModel.InputContent += getText;
            Singleton<MainViewModel>.Instance.OnInputTbUpdateCaretIndex?.Invoke();
            _inputViewModel.TranslateCommand.Execute(null);
        }
        catch (OperationCanceledException)
        {
            LogService.Logger.Debug("Screenshot Translate 取消");
        }
        catch (Exception ex)
        {
            _inputViewModel.InputContent = ex.Message;
        }
        finally
        {
            IsScreenshotExecuting = false;
            MemoUtil.FlushMemory();
        }
    }

    /// <summary>
    ///     根据配置获取图片格式
    /// </summary>
    /// <returns></returns>
    private ImageFormat GetImageFormat()
    {
        return (_configHelper.CurrentConfig?.OcrImageQuality ?? OcrImageQualityEnum.Medium) switch
        {
            OcrImageQualityEnum.Medium => ImageFormat.Png,
            OcrImageQualityEnum.Low => ImageFormat.Jpeg,
            _ => ImageFormat.Bmp
        };
    }

    /// <summary>
    ///     隐藏主窗口
    /// </summary>
    internal void HideMainView()
    {
        var view = Application.Current.Windows.OfType<MainView>().First();
        // 判断是否置顶，置顶的话则不隐藏
        if (!view.Topmost) view.WindowAnimation(false);
    }

    [RelayCommand]
    private void OpenMainWindow(Window view)
    {
        if ((_configHelper.CurrentConfig?.IsTriggerShowHide ?? false) && WindowHelper.IsWindowVisible(view) &&
            WindowHelper.IsWindowInForeground(view))
            HideMainView();
        else
            ShowAndActive(view);
    }

    public void ClearOutput()
    {
        //清空输出相关
        Singleton<OutputViewModel>.Instance.Clear();
    }

    internal void ClearAll()
    {
        ClearOutput();
        //清空输入相关
        _inputViewModel.Clear();
    }

    private void ShowAndActive(Window view, bool canFollowMouse = false)
    {
        if (canFollowMouse)
        {
            var position = FollowMouseHandler(view);

            view.Left = position.Item1;
            view.Top = position.Item2;
        }

        SpecialWindowActiveHandler(view);

        if (view is MainView mView)
        {
            mView.WindowAnimation();
            Singleton<MainViewModel>.Instance.OnInputTbUpdateCaretIndex?.Invoke();
        }
        else
            view.Show();
        view.Activate();
        WindowHelper.SetWindowInForeground(view);
    }

    [RelayCommand]
    private void OpenPreference()
    {
        var view = Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault();
        // 如果已经打开则不重新导航
        if (view is null)
        {
            view = new PreferenceView();
            view.UpdateNavigation();
        }

        ShowAndActive(view);
    }

    [RelayCommand]
    private void OpenHistory()
    {
        var view = Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault();
        view ??= new PreferenceView();
        view.UpdateNavigation(PerferenceType.History);

        ShowAndActive(view);
    }

    [RelayCommand]
    private void ForbiddenShortcuts(Window view)
    {
        //保存配置
        var commonVm = Singleton<CommonViewModel>.Instance;
        commonVm.DisableGlobalHotkeys = !IsForbiddenShortcuts;
        commonVm.SaveCommand.Execute(null);
    }

    public void InvokeForbiddenShotcuts(Window view, bool isForbiddenShortcuts)
    {
        ForbiddenShortcuts(isForbiddenShortcuts);
        OnForbiddenShortcuts?.Invoke(view, isForbiddenShortcuts);
    }

    public void ForbiddenShortcuts(bool isForbidden)
    {
        IsForbiddenShortcuts = isForbidden;
        NIModel.IconSource = IsForbiddenShortcuts ? Constant.IconForbidden : Constant.Icon;
    }

    [RelayCommand]
    private void ClipboardMonitor(Window view)
    {
        IsClipboardMonitor = !IsClipboardMonitor;
        IsEnabledClipboardMonitor = IsClipboardMonitor ? Constant.TagTrue : Constant.TagFalse;

        if (IsClipboardMonitor)
        {
            _clipboardHelper ??= new ClipboardHelper();
            // 开始监听剪贴板变化
            if (_clipboardHelper.Start(out var error))
            {
                // 清除热键复制标记
                Singleton<MainViewModel>
                    .Instance
                    .IsHotkeyCopy = false;
                _clipboardHelper.OnClipboardChanged += c => ClipboardChanged(c, view);

                ShowBalloonTip("已启用监听剪贴板");
            }
            else
            {
                ShowBalloonTip(error);
            }
        }
        else
        {
            if (_clipboardHelper == null) return;

            if (_clipboardHelper.Stop(out var error))
            {
                _clipboardHelper.OnClipboardChanged -= c => ClipboardChanged(c, view);
                _clipboardHelper = null;
                ShowBalloonTip("已关闭监听剪贴板");
            }
            else
            {
                ShowBalloonTip(error);
            }
        }
    }

    private void ClipboardChanged(string content, Window view)
    {
        //热键复制时略过
        if (Singleton<MainViewModel>.Instance.IsHotkeyCopy)
        {
            Singleton<MainViewModel>.Instance.IsHotkeyCopy = false;
            return;
        }
        //监听剪贴板，剪贴板内容变动时取消保存至生词本
        _inputViewModel.Save2VocabularyBookCancelCommand.Execute(null);

        content = _configHelper.CurrentConfig?.LineBreakHandler switch
        {
            LineBreakHandlingMode.RemoveExtraLineBreak => StringUtil.NormalizeText(content),
            LineBreakHandlingMode.RemoveAllLineBreak => StringUtil.RemoveLineBreaks(content),
            LineBreakHandlingMode.RemoveAllLineBreakWithoutSpace => StringUtil.RemoveLineBreaks(content, false),
            _ => content,
        };

        //如果重复执行先取消上一步操作
        _outputViewModel.SingleTranslateCancelCommand.Execute(null);
        _outputViewModel.SingleTranslateBackCancelCommand.Execute(null);
        _inputViewModel.TranslateCancelCommand.Execute(null);
        //增量翻译
        if (_configHelper.CurrentConfig?.IncrementalTranslation ?? false)
        {
            ClearOutput();
            var input = _inputViewModel.InputContent;
            _inputViewModel.InputContent = string.IsNullOrEmpty(input) ? string.Empty : input + " ";
        }
        else
        {
            ClearAll();
        }

        _inputViewModel.InputContent += content;
        ShowAndActive(view, _configHelper.CurrentConfig?.IsFollowMouse ?? false);

        _inputViewModel.TranslateCommand.Execute(null);
    }

    [RelayCommand]
    private async Task ReplaceTranslateAsync(Window view)
    {
        if (!TryGetWord(out var content) || content == null) return;
        await Singleton<ReplaceViewModel>.Instance.ExecuteAsync(content);
    }

    internal bool TryGetWord(out string? content)
    {
        var interval = _configHelper.CurrentConfig?.WordPickingInterval ?? 100;
        try
        {
            content = ClipboardUtil.GetSelectedText(interval)?.Trim();
            if (string.IsNullOrEmpty(content))
            {
                LogService.Logger.Warn($"取词失败, 可能原因有:\n" +
                    $"1. 未选中文本\n" +
                    $"2. 文本禁止复制\n" +
                    $"3. 取词间隔过短, 尝试延长取词间隔(当前: {interval}ms)\n" +
                    $"4. 被取词软件权限高于本软件权限(管理员权限启动即可解决)");
                return false;
            }
        }
        catch (Exception)
        {
            LogService.Logger.Warn("获取剪贴板异常请重试");
            content = null;
            return false;
        }

        return true;
    }

    private void SaveSelectedLang()
    {
        //写入配置
        var vm = Singleton<MainViewModel>.Instance;
        if (!_configHelper.WriteConfig(vm.SourceLang, vm.TargetLang))
            LogService.Logger.Warn(
                $"保存源语言({vm.SourceLang.GetDescription()})、目标语言({vm.TargetLang.GetDescription()})配置失败...");
    }

    /// <summary>
    ///     系统显示变化
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DisplaySettingsChanged(object? sender, EventArgs e)
    {
        NIModel.IconSource = Constant.Icon;
    }

    /// <summary>
    ///     托盘程序BallonTip消息入口
    /// </summary>
    /// <param name="msg"></param>
    public void ShowBalloonTip(string msg)
    {
        OnShowBalloonTip?.Invoke(msg);
    }

    [RelayCommand]
    private void Exit()
    {
        SystemEvents.DisplaySettingsChanged -= DisplaySettingsChanged;

        OnExit?.Invoke();

        SaveSelectedLang();

        Application.Current.Shutdown();
    }

    /// <summary>
    ///     跟随鼠标处理
    /// </summary>
    /// <param name="view"></param>
    /// <returns></returns>
    private Tuple<double, double> FollowMouseHandler(Window view)
    {
        var infos = CommonUtil.GetPositionInfos();
        var position = infos.Item1;
        var bounds = infos.Item2;
        var left = position.X;
        var top = position.Y;

        //保持页面在屏幕上方三分之一处
        //if ((top - bounds.Top) * 3 > bounds.Height) top = bounds.Height / 3 + bounds.Top;

        ////如果当前高度不足以容纳最大高度的内容，则使用最大高度为窗口Top值（修改为150%缩放似乎有问题）
        //if (bounds.Height - top + bounds.Top < view.MaxHeight) top = bounds.Height - view.MaxHeight + bounds.Top - 48;

        //if (view.MaxHeight > bounds.Height) top = bounds.Top;
        
        //右侧不超出当前屏幕区域
        if (left + view.Width > bounds.Left + bounds.Width) left = bounds.Left + bounds.Width - view.Width;
        return new Tuple<double, double>(left, top);
    }

    /// <summary>
    ///     特定情况下窗口无法激活的问题
    ///     1. 主窗口非置顶并且设置页面已经存在时激活设置页面
    ///     2. 设置页面最小化再激活
    /// </summary>
    /// <param name="view"></param>
    private void SpecialWindowActiveHandler(Window view)
    {
        if (view.WindowState == WindowState.Minimized)
            view.WindowState = WindowState.Normal; // Restore the window if it was minimized.
        if (!view.Topmost) // Ensure the window is topmost if it's not already.
        {
            view.Topmost = true; // Temporarily make the window topmost.
            view.Topmost = false; // Then set it back to normal state, this is a trick to bring it to the front.
        }
    }
}