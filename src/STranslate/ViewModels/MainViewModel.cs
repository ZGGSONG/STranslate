using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Style.Commons;
using STranslate.Style.Controls;
using STranslate.Util;
using STranslate.ViewModels.Preference;
using STranslate.Views;

namespace STranslate.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private bool _isDebug;

    [ObservableProperty] private string _isEnableIncrementalTranslation = Constant.TagFalse;

    [ObservableProperty] private string _isAutoTranslate = Constant.TagFalse;

    [ObservableProperty] private string _isEnableMosehook = Constant.TagFalse;

    [ObservableProperty] private string _isEnableOnlyShowRet = Constant.TagFalse;

    /// <summary>
    ///     是否为重置状态
    /// </summary>
    private bool _isInitial;

    [ObservableProperty] private bool _isOnlyShowRet;
    
    [ObservableProperty] private bool _isHideLangWhenOnlyShowOutput;

    [ObservableProperty] private string _isTopMost = Constant.TagFalse;

    /// <summary>
    ///     原始语言
    /// </summary>
    private LangEnum _sourceLang;

    /// <summary>
    ///     目标语言
    /// </summary>
    private LangEnum _targetLang;

    [ObservableProperty] private string _topMostContent = Constant.UnTopmostContent;

    public bool IsHotkeyCopy = false;

    public MainViewModel()
    {
#if DEBUG
        IsDebug = true;
#else
        IsDebug = false;
#endif

        SqlHelper.InitializeDB();

        Reset();
    }

    public InputViewModel InputVM => Singleton<InputViewModel>.Instance;
    public OutputViewModel OutputVM => Singleton<OutputViewModel>.Instance;
    public NotifyIconViewModel NotifyIconVM => Singleton<NotifyIconViewModel>.Instance;
    public CommonViewModel CommonSettingVM => Singleton<CommonViewModel>.Instance;
    public OCRScvViewModel OCRVM => Singleton<OCRScvViewModel>.Instance;
    public TTSViewModel TTSVM => Singleton<TTSViewModel>.Instance;
    public ReplaceViewModel ReplaceVm => Singleton<ReplaceViewModel>.Instance;

    private ConfigModel? Config => Singleton<ConfigHelper>.Instance.CurrentConfig;

    public LangEnum SourceLang
    {
        get => _sourceLang;
        set
        {
            if (_sourceLang == value)
                return;
            OnPropertyChanging();
            _sourceLang = value;
            OnPropertyChanged();
            //清空识别缓存
            InputVM.IdentifyLanguage = string.Empty;

            //是否立即翻译
            if (!_isInitial && !string.IsNullOrEmpty(InputVM.InputContent) && (Config?.ChangedLang2Execute ?? false))
                CancelAndTranslate();
        }
    }

    public LangEnum TargetLang
    {
        get => _targetLang;
        set
        {
            if (_targetLang == value)
                return;
            OnPropertyChanging();
            _targetLang = value;
            OnPropertyChanged();
            //清空识别缓存
            InputVM.IdentifyLanguage = string.Empty;

            //是否立即翻译
            if (!_isInitial && !string.IsNullOrEmpty(InputVM.InputContent) && (Config?.ChangedLang2Execute ?? false))
                CancelAndTranslate();
        }
    }

    /// <summary>
    ///     是否忽略全局热键
    /// </summary>
    public bool ShouldIgnoreHotkeys =>
        (Config?.IgnoreHotkeysOnFullscreen ?? false) && WindowHelper.IsWindowFullscreen();

    public void Reset()
    {
        _isInitial = true;
        SourceLang = Config?.SourceLang ?? LangEnum.auto;
        TargetLang = Config?.TargetLang ?? LangEnum.auto;
        InputVM.OftenUsedLang = Config?.OftenUsedLang ?? string.Empty;
        IsEnableIncrementalTranslation = Config?.IncrementalTranslation ?? false ? Constant.TagTrue : Constant.TagFalse;
        IsEnableOnlyShowRet = Config?.IsOnlyShowRet ?? false ? Constant.TagTrue : Constant.TagFalse;
        IsAutoTranslate = Config?.AutoTranslate ?? false ? Constant.TagTrue : Constant.TagFalse;
        _ = ReplaceVm.ReplaceProp.ActiveService; //激活ReplaceVm
        _isInitial = false;
    }

    [RelayCommand]
    private void Loaded(Window view)
    {
        try
        {
            HotkeyHelper.Hotkeys = Config?.Hotkeys ?? throw new Exception("快捷键配置出错，请检查配置后重启...");

            NotifyIconVM.OnMousehook += MouseHook;
            NotifyIconVM.OnForbiddenShortcuts += OnForbiddenShortcutsChanged;
            CommonSettingVM.OnBooleansChanged += OnBooleansChanged;
            // 由于需要使用windows句柄，需要window加载完成后处理热键注册相关逻辑
            // 所以不放在ConfigHelper中处理,而使用读取配置判断,如果禁用则不注册并更新提示
            if (Config?.DisableGlobalHotkeys ?? false)
            {
                NotifyIconVM.ForbiddenShortcuts(true);
                NotifyIconVM.UpdateToolTip("快捷键禁用");
            }
            else
            {
                RegisterHotkeys(view);
            }
        }
        catch (Exception ex)
        {
            LogService.Logger.Error($"[Hotkeys] {ex.Message}");
        }
    }

    [RelayCommand]
    private void Closing(Window view)
    {
        NotifyIconVM.OnMousehook -= MouseHook;
        NotifyIconVM.OnForbiddenShortcuts -= OnForbiddenShortcutsChanged;
        CommonSettingVM.OnBooleansChanged -= OnBooleansChanged;
        UnRegisterHotkeys(view);
    }

    /// <summary>
    ///     禁用/启用快捷键
    /// </summary>
    /// <param name="view"></param>
    /// <param name="forbidden"></param>
    private void OnForbiddenShortcutsChanged(Window view, bool forbidden)
    {
        if (forbidden)
            UnRegisterHotkeys(view);
        else
            RegisterHotkeys(view);
    }

    /// <summary>
    ///     监听增量翻译更新
    /// </summary>
    /// <param name="value"></param>
    private void OnBooleansChanged(bool incrementalTranslation, bool autoTranslate, bool isOnlyShowRet)
    {
        IsEnableIncrementalTranslation = incrementalTranslation ? Constant.TagTrue : Constant.TagFalse;
        IsAutoTranslate = autoTranslate ? Constant.TagTrue : Constant.TagFalse;
        IsEnableOnlyShowRet = isOnlyShowRet ? Constant.TagTrue : Constant.TagFalse;
    }

    private void RegisterHotkeys(Window view)
    {
        HotkeyHelper.InitialHook(view);
        HotkeyHelper.Register(HotkeyHelper.InputTranslateId, () =>
        {
            if (ShouldIgnoreHotkeys) return;
            NotifyIconVM.InputTranslateCommand.Execute(view);
        });
        HotkeyHelper.Register(HotkeyHelper.CrosswordTranslateId, () =>
        {
            if (ShouldIgnoreHotkeys) return;
            NotifyIconVM.CrossWordTranslateCommand.Execute(view);
        });
        HotkeyHelper.Register(HotkeyHelper.ScreenShotTranslateId, () =>
        {
            if (ShouldIgnoreHotkeys) return;
            NotifyIconVM.ScreenShotTranslateCommand.Execute(null);
        });
        HotkeyHelper.Register(HotkeyHelper.ReplaceTranslateId, () =>
        {
            if (ShouldIgnoreHotkeys) return;
            NotifyIconVM.ReplaceTranslateCommand.Execute(view);
        });
        HotkeyHelper.Register(HotkeyHelper.OpenMainWindowId, () =>
        {
            if (ShouldIgnoreHotkeys) return;
            NotifyIconVM.OpenMainWindowCommand.Execute(view);
        });
        HotkeyHelper.Register(HotkeyHelper.MousehookTranslateId, () =>
        {
            if (ShouldIgnoreHotkeys) return;
            NotifyIconVM.MousehookTranslateCommand.Execute(view);
        });
        HotkeyHelper.Register(HotkeyHelper.OCRId, () =>
        {
            if (ShouldIgnoreHotkeys) return;
            NotifyIconVM.OCRCommand.Execute(null);
        });
        HotkeyHelper.Register(HotkeyHelper.SilentOCRId, () =>
        {
            if (ShouldIgnoreHotkeys) return;
            NotifyIconVM.SilentOCRCommand.Execute(null);
        });
        HotkeyHelper.Register(HotkeyHelper.SilentTTSId, () =>
        {
            if (ShouldIgnoreHotkeys)
                return;
            NotifyIconVM.SilentTTSCommand.Execute(null);
        });

        HotkeyHelper.Register(HotkeyHelper.ClipboardMonitorId, () =>
        {
            if (ShouldIgnoreHotkeys) return;
            NotifyIconVM.ClipboardMonitorCommand.Execute(view);
        });
        if (
            HotkeyHelper.Hotkeys!.InputTranslate.Conflict
            || HotkeyHelper.Hotkeys!.CrosswordTranslate.Conflict
            || HotkeyHelper.Hotkeys!.ScreenShotTranslate.Conflict
            || HotkeyHelper.Hotkeys!.ReplaceTranslate.Conflict
            || HotkeyHelper.Hotkeys!.OpenMainWindow.Conflict
            || HotkeyHelper.Hotkeys!.MousehookTranslate.Conflict
            || HotkeyHelper.Hotkeys!.OCR.Conflict
            || HotkeyHelper.Hotkeys!.SilentOCR.Conflict
            || HotkeyHelper.Hotkeys!.SilentTTS.Conflict
            || HotkeyHelper.Hotkeys!.ClipboardMonitor.Conflict
        )
            MessageBox_S.Show("全局热键冲突，请前往软件首选项中修改...");
        var msg = "";
        if (!HotkeyHelper.Hotkeys.InputTranslate.Conflict &&
            !string.IsNullOrEmpty(HotkeyHelper.Hotkeys.InputTranslate.Text))
            msg += $"输入: {HotkeyHelper.Hotkeys.InputTranslate.Text}\n";
        if (!HotkeyHelper.Hotkeys.CrosswordTranslate.Conflict &&
            !string.IsNullOrEmpty(HotkeyHelper.Hotkeys.CrosswordTranslate.Text))
            msg += $"划词: {HotkeyHelper.Hotkeys.CrosswordTranslate.Text}\n";
        if (!HotkeyHelper.Hotkeys.ScreenShotTranslate.Conflict &&
            !string.IsNullOrEmpty(HotkeyHelper.Hotkeys.ScreenShotTranslate.Text))
            msg += $"截图: {HotkeyHelper.Hotkeys.ScreenShotTranslate.Text}\n";
        if (!HotkeyHelper.Hotkeys.ReplaceTranslate.Conflict &&
            !string.IsNullOrEmpty(HotkeyHelper.Hotkeys.ReplaceTranslate.Text))
            msg += $"替换: {HotkeyHelper.Hotkeys.ReplaceTranslate.Text}\n";
        if (!HotkeyHelper.Hotkeys.OpenMainWindow.Conflict &&
            !string.IsNullOrEmpty(HotkeyHelper.Hotkeys.OpenMainWindow.Text))
            msg += $"显示: {HotkeyHelper.Hotkeys.OpenMainWindow.Text}\n";
        if (!HotkeyHelper.Hotkeys.MousehookTranslate.Conflict &&
            !string.IsNullOrEmpty(HotkeyHelper.Hotkeys.MousehookTranslate.Text))
            msg += $"鼠标: {HotkeyHelper.Hotkeys.MousehookTranslate.Text}\n";
        if (!HotkeyHelper.Hotkeys.OCR.Conflict && !string.IsNullOrEmpty(HotkeyHelper.Hotkeys.OCR.Text))
            msg += $"识字: {HotkeyHelper.Hotkeys.OCR.Text}\n";
        if (!HotkeyHelper.Hotkeys.SilentOCR.Conflict && !string.IsNullOrEmpty(HotkeyHelper.Hotkeys.SilentOCR.Text))
            msg += $"静默OCR: {HotkeyHelper.Hotkeys.SilentOCR.Text}\n";
        if (!HotkeyHelper.Hotkeys.ClipboardMonitor.Conflict &&
            !string.IsNullOrEmpty(HotkeyHelper.Hotkeys.ClipboardMonitor.Text))
            msg += $"剪贴板: {HotkeyHelper.Hotkeys.ClipboardMonitor.Text}\n";
        if (!HotkeyHelper.Hotkeys.SilentTTS.Conflict && !string.IsNullOrEmpty(HotkeyHelper.Hotkeys.SilentTTS.Text))
            msg += $"静默TTS: {HotkeyHelper.Hotkeys.SilentTTS.Text}\n";
        NotifyIconVM.UpdateToolTip(msg.TrimEnd('\n'));
        HotkeyHelper.UpdateConflict();
    }

    private void UnRegisterHotkeys(Window view)
    {
        HotkeyHelper.UnRegisterHotKey(view);

        NotifyIconVM.UpdateToolTip("快捷键禁用");
    }

    private void CancelAndTranslate()
    {
        OutputVM.SingleTranslateCancelCommand.Execute(null);
        OutputVM.SingleTranslateBackCancelCommand.Execute(null);
        InputVM.TranslateCancelCommand.Execute(null);
        InputVM.TranslateCommand.Execute(null);
    }

    [RelayCommand]
    private void ExchangeSourceTarget()
    {
        if (SourceLang == TargetLang || string.IsNullOrEmpty(InputVM.InputContent)) return;
        (SourceLang, TargetLang) = (TargetLang, SourceLang);
        CancelAndTranslate();
    }

    [RelayCommand]
    private void MouseHook(Window view)
    {
        NotifyIconVM.IsMousehook = !NotifyIconVM.IsMousehook;
        IsEnableMosehook = NotifyIconVM.IsMousehook ? Constant.TagTrue : Constant.TagFalse;

        if (NotifyIconVM.IsMousehook)
        {
            view.Topmost = true;
            IsTopMost = Constant.TagTrue;
            TopMostContent = Constant.TopmostContent;
            Singleton<MouseHookHelper>.Instance.Start();
            Singleton<MouseHookHelper>.Instance.WordsSelected += OnWordsSelectedChanged;
            ToastHelper.Show("启用鼠标划词");
        }
        else
        {
            if (!(Config?.IsKeepTopmostAfterMousehook ?? false))
            {
                view.Topmost = false;
                IsTopMost = Constant.TagFalse;
                TopMostContent = Constant.UnTopmostContent;
            }

            Singleton<MouseHookHelper>.Instance.Stop();
            Singleton<MouseHookHelper>.Instance.WordsSelected -= OnWordsSelectedChanged;
            ToastHelper.Show("关闭鼠标划词");
        }
    }

    [RelayCommand]
    private void AutoTranslate()
    {
        var conf = Config;
        var common = Singleton<CommonViewModel>.Instance;
        if (conf is null)
            return;

        conf.AutoTranslate = !conf.AutoTranslate;
        IsAutoTranslate = conf?.AutoTranslate ?? false ? Constant.TagTrue : Constant.TagFalse;

        common.AutoTranslate = !common.AutoTranslate;
        common.SaveCommand.Execute(null);

        var msg = (common.AutoTranslate ? "打开" : "关闭") + "自动翻译";
        ToastHelper.Show(msg);
    }

    [RelayCommand]
    private void IncrementalTranslation()
    {
        var conf = Config;
        var common = Singleton<CommonViewModel>.Instance;
        if (conf is null)
            return;

        conf.IncrementalTranslation = !conf.IncrementalTranslation;
        IsEnableIncrementalTranslation = conf?.IncrementalTranslation ?? false ? Constant.TagTrue : Constant.TagFalse;

        common.IncrementalTranslation = !common.IncrementalTranslation;
        common.SaveCommand.Execute(null);

        var msg = (common.IncrementalTranslation ? "打开" : "关闭") + "增量翻译";
        ToastHelper.Show(msg);
    }

    [RelayCommand]
    private void OnlyShowRet()
    {
        var common = Singleton<CommonViewModel>.Instance;
        if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        {
            common.IsHideLangWhenOnlyShowOutput = !common.IsHideLangWhenOnlyShowOutput;
            common.SaveCommand.Execute(null);


            var aMsg = (common.IsHideLangWhenOnlyShowOutput ? "隐藏" : "显示") + "语言框";
            ToastHelper.Show(aMsg);
            return;
        }
        var conf = Config;
        if (conf is null)
            return;

        conf.IsOnlyShowRet = !conf.IsOnlyShowRet;
        IsEnableOnlyShowRet = conf?.IsOnlyShowRet ?? false ? Constant.TagTrue : Constant.TagFalse;

        common.IsOnlyShowRet = !common.IsOnlyShowRet;
        common.SaveCommand.Execute(null);

        var msg = (common.IsOnlyShowRet ? "隐藏" : "显示") + "非输出部分";
        ToastHelper.Show(msg);
    }

    private void OnWordsSelectedChanged(string content)
    {
        if (string.IsNullOrEmpty(content))
            return;
        
        // 先取消可能存在的存生词本的操作
        InputVM.Save2VocabularyBookCancelCommand.Execute(null);

        //处理剪贴板内容格式
        if (Config?.IsPurify ?? true)
            content = StringUtil.NormalizeText(content);

        //取词前移除换行
        if (Config?.IsRemoveLineBreakGettingWords ?? false)
            content = StringUtil.RemoveLineBreaks(content);

        //增量翻译
        if (Config?.IncrementalTranslation ?? false)
        {
            NotifyIconVM.ClearOutput();
            var input = InputVM.InputContent;
            InputVM.InputContent = string.IsNullOrEmpty(input) ? string.Empty : input + " ";
        }
        else
        {
            NotifyIconVM.ClearAll();
        }

        InputVM.InputContent += content;

        //如果重复执行先取消上一步操作
        InputVM.Save2VocabularyBookCancelCommand.Execute(null);
        OutputVM.SingleTranslateCancelCommand.Execute(null);
        OutputVM.SingleTranslateBackCancelCommand.Execute(null);
        InputVM.TranslateCancelCommand.Execute(null);

        InputVM.TranslateCommand.Execute(null);
    }

    /// <summary>
    ///     点击置顶按钮
    /// </summary>
    /// <param name="win"></param>
    [RelayCommand]
    private void Sticky(Window win)
    {
        if (NotifyIconVM.IsMousehook)
        {
            MessageBox_S.Show("当前监听鼠标划词中，请先解除监听...");
            return;
        }

        var tmp = !win.Topmost;
        IsTopMost = tmp ? Constant.TagTrue : Constant.TagFalse;
        TopMostContent = tmp ? Constant.TopmostContent : Constant.UnTopmostContent;
        win.Topmost = tmp;

        ToastHelper.Show(tmp ? "启用置顶" : "关闭置顶");
    }

    /// <summary>
    ///     隐藏窗口
    /// </summary>
    /// <param name="win"></param>
    [RelayCommand]
    private void Esc(MainView win)
    {
        if (NotifyIconVM.IsMousehook)
        {
            MessageBox_S.Show("当前监听鼠标划词中，请先解除监听...");
            return;
        }

        win.Topmost = false;
        IsTopMost = Constant.TagFalse;
        TopMostContent = Constant.UnTopmostContent;
        win.WindowAnimation(false);

        //如果重复执行先取消上一步操作
        OutputVM.SingleTranslateCancelCommand.Execute(null);
        OutputVM.SingleTranslateBackCancelCommand.Execute(null);
        InputVM.TranslateCancelCommand.Execute(null);
        InputVM.Save2VocabularyBookCancelCommand.Execute(null);

        NotifyIconVM.ScreenShotTranslateCancelCommand.Execute(null);

        //取消语音播放
        InputVM.TTSCancelCommand.Execute(null);
        OutputVM.TTSCancelCommand.Execute(null);
    }

    [RelayCommand]
    private async Task ShowHideInputAsync()
    {
        CommonSettingVM.IsOnlyShowRet = !CommonSettingVM.IsOnlyShowRet;
        ToastHelper.Show($"{(CommonSettingVM.IsOnlyShowRet ? "隐藏" : "显示")}非输出部分");
        await CommonSettingVM.SaveCommand.ExecuteAsync(null);
    }

    /// <summary>
    ///     重置文本框字体大小
    /// </summary>
    [RelayCommand]
    private void ResetFontSize()
    {
        Application.Current.Resources[Constant.FontSize18TextBox] = Application.Current.Resources[Constant.FontSize18];
    }

    /// <summary>
    ///     更新主界面图标显示
    /// </summary>
    internal void UpdateMainViewIcons()
    {
        IsShowClose = Config?.IsShowClose ?? false;
        ShowMinimalBtn = Config?.ShowMinimalBtn ?? false;
        IsShowPreference = Config?.IsShowPreference ?? false;
        IsShowConfigureService = Config?.IsShowConfigureService ?? false;
        IsShowMousehook = Config?.IsShowMousehook ?? false;
        IsShowAutoTranslate = Config?.IsShowAutoTranslate ?? false;
        IsShowIncrementalTranslation = Config?.IsShowIncrementalTranslation ?? false;
        IsShowOnlyShowRet = Config?.IsShowOnlyShowRet ?? false;
        IsShowScreenshot = Config?.IsShowScreenshot ?? false;
        IsShowOCR = Config?.IsShowOCR ?? false;
        IsShowSilentOCR = Config?.IsShowSilentOCR ?? false;
        IsShowClipboardMonitor = Config?.IsShowClipboardMonitor ?? false;
        IsShowQRCode = Config?.IsShowQRCode ?? false;
        IsShowHistory = Config?.IsShowHistory ?? false;
        ShowMainOcrLang = Config?.ShowMainOcrLang ?? false;
    }


    [RelayCommand]
    private void SelectedService(List<object> list)
    {
        if (list.Last() is ToggleButton control)
            control.IsChecked = !control.IsChecked;

        var service = list.First();
        switch (service)
        {
            case ITranslator it:
                it.IsEnabled = !it.IsEnabled;
                Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);

                // 如果服务开启并且输入框有内容则立即翻译
                if (it.IsEnabled  && !string.IsNullOrEmpty(InputVM.InputContent))
                    OutputVM.SingleTranslateCommand.Execute(it);

                break;
            case IOCR io:
                Singleton<OCRScvViewModel>.Instance.ActivedOCR = io;
                break;
            case ITTS its:
                its.IsEnabled = !its.IsEnabled;

                Singleton<TTSViewModel>.Instance.SaveCommand.Execute(null);
                break;
        }
    }

    [RelayCommand]
    private void ChangeTheme()
    {
        CommonSettingVM.ThemeType = CommonSettingVM.ThemeType.Increase();
        CommonSettingVM.SaveCommand.Execute(null);

        ToastHelper.Show(CommonSettingVM.ThemeType.GetDescription());
    }

    [RelayCommand]
    private void SelectedMainOcrLanguage(List<object> list)
    {
        if (list.Count != 2 || list.First() is not EnumerationExtension.EnumerationMember member ||
            list.Last() is not ToggleButton tb)
            return;

        tb.IsChecked = false;

        if (!Enum.TryParse(typeof(LangEnum), member.Value?.ToString() ?? "", out var obj) ||
            obj is not LangEnum lang) return;

        CommonSettingVM.MainOcrLang = lang;
        CommonSettingVM.SaveCommand.Execute(null);
    }

    [RelayCommand]
    private void ResetLocation(Window window)
    {
        // 计算窗口左上角在屏幕上的位置
        var left = (SystemParameters.PrimaryScreenWidth - window.Width) / 2;
        var top = (SystemParameters.PrimaryScreenHeight - 600) / 2;

        // 设置窗口位置
        window.Left = left;
        window.Top = top;
    }

    [RelayCommand]
    private void Minimal(Window window)
    {
        window.WindowState = WindowState.Minimized;
    }

    #region 全局字体大小

    /// <summary>
    ///     重置全局字体大小
    /// </summary>
    [RelayCommand]
    private void ResetGlobalFontSize()
    {
        Constant.GlobalFontSizeList.ForEach(font => Application.Current.Resources[font.Item1] = font.Item2);
        CommonSettingVM.GlobalFontSize = GlobalFontSizeEnum.General;
    }

    /// <summary>
    ///     增加全局字体大小
    /// </summary>
    [RelayCommand]
    private void IncreaseGlobalFontSize()
    {
        if (CommonSettingVM.GlobalFontSize == CommonSettingVM.GlobalFontSize.Max())
            return;
        CommonSettingVM.GlobalFontSize = CommonSettingVM.GlobalFontSize.Increment();
    }

    /// <summary>
    ///     降低全局字体大小
    /// </summary>
    [RelayCommand]
    private void ReduceGlobalFontSize()
    {
        if (CommonSettingVM.GlobalFontSize == CommonSettingVM.GlobalFontSize.Min())
            return;
        CommonSettingVM.GlobalFontSize = CommonSettingVM.GlobalFontSize.Decrement();
    }

    #endregion

    #region 显示图标

    [ObservableProperty] private bool _isShowClose;
    
    [ObservableProperty] private bool _showMinimalBtn;

    [ObservableProperty] private bool _isShowPreference;

    [ObservableProperty] private bool _isShowConfigureService;

    [ObservableProperty] private bool _isShowMousehook;

    [ObservableProperty] private bool _isShowAutoTranslate;

    [ObservableProperty] private bool _isShowIncrementalTranslation;

    [ObservableProperty] private bool _isShowOnlyShowRet;

    [ObservableProperty] private bool _isShowScreenshot;

    [ObservableProperty] private bool _isShowOCR;

    [ObservableProperty] private bool _isShowSilentOCR;

    [ObservableProperty] private bool _isShowClipboardMonitor;

    [ObservableProperty] private bool _isShowQRCode;

    [ObservableProperty] private bool _isShowHistory;

    [ObservableProperty] private bool _showMainOcrLang;

    #endregion 显示图标
}