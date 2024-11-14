using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.Views;
using STranslate.Views.Preference;

namespace STranslate.ViewModels.Preference;

public partial class CommonViewModel : ObservableObject
{
    #region 属性 & 字段
    
    /// <summary>
    ///     ConfigHelper单例
    /// </summary>
    private static readonly ConfigHelper ConfigHelper = Singleton<ConfigHelper>.Instance;

    [ObservableProperty] private double _autoScale = ConfigHelper.CurrentConfig?.AutoScale ?? 0.8;

    /// <summary>
    ///     修改语言后立即翻译
    /// </summary>
    [ObservableProperty] private bool _changedLang2Execute = ConfigHelper.CurrentConfig?.ChangedLang2Execute ?? true;

    [ObservableProperty] private bool _closeUIOcrRetTranslate = ConfigHelper.CurrentConfig?.CloseUIOcrRetTranslate ?? false;

    /// <summary>
    ///     翻译后执行自动复制动作(Ctrl+1...9)
    /// </summary>
    [ObservableProperty] private int _copyResultAfterTranslateIndex = ConfigHelper.CurrentConfig?.CopyResultAfterTranslateIndex ?? 0;

    [ObservableProperty] private string _customFont = ConfigHelper.CurrentConfig?.CustomFont ?? Constant.DefaultFontName;

    /// <summary>
    ///     语种识别类型
    /// </summary>
    [ObservableProperty] private LangDetectType _detectType = ConfigHelper.CurrentConfig?.DetectType ?? LangDetectType.Local;

    /// <summary>
    ///     禁用全局热键
    /// </summary>
    [ObservableProperty] private bool _disableGlobalHotkeys = ConfigHelper.CurrentConfig?.DisableGlobalHotkeys ?? false;

    [ObservableProperty]
    private DoubleTapFuncEnum _doubleTapTrayFunc = ConfigHelper.CurrentConfig?.DoubleTapTrayFunc ?? DoubleTapFuncEnum.InputFunc;

    /// <summary>
    ///     是否启用外部调用服务
    /// </summary>
    [ObservableProperty] private bool _externalCall = ConfigHelper.CurrentConfig?.ExternalCall ?? false;

    /// <summary>
    ///     外部调用服务端口
    /// </summary>
    [ObservableProperty] private int? _externalCallPort = ConfigHelper.CurrentConfig?.ExternalCallPort ?? 50020;

    [ObservableProperty] private List<string> _getFontFamilys;

    /// <summary>
    ///     全局字体大小
    /// </summary>
    [ObservableProperty]
    private GlobalFontSizeEnum _globalFontSize = ConfigHelper.CurrentConfig?.GlobalFontSize ?? GlobalFontSizeEnum.General;

    /// <summary>
    ///     历史记录大小
    /// </summary>
    [ObservableProperty] private long _historySizeType = 1;


    /// <summary>
    ///     热键触发复制后是否显示成功提示
    /// </summary>
    [ObservableProperty] private bool _hotkeyCopySuccessToast = ConfigHelper.CurrentConfig?.HotkeyCopySuccessToast ?? true;

    /// <summary>
    ///     全屏模式下忽略热键
    /// </summary>
    [ObservableProperty] private bool _ignoreHotkeysOnFullscreen = ConfigHelper.CurrentConfig?.IgnoreHotkeysOnFullscreen ?? false;

    /// <summary>
    ///     是否开启增量翻译
    /// </summary>
    [ObservableProperty] private bool _incrementalTranslation = ConfigHelper.CurrentConfig?.IncrementalTranslation ?? false;

    [ObservableProperty] private bool _isAdjustContentTranslate = ConfigHelper.CurrentConfig?.IsAdjustContentTranslate ?? false;

    /// <summary>
    ///     激活窗口时光标移动至末尾
    /// </summary>
    [ObservableProperty] private bool _isCaretLast = ConfigHelper.CurrentConfig?.IsCaretLast ?? false;

    [ObservableProperty] private bool _isFollowMouse = ConfigHelper.CurrentConfig?.IsFollowMouse ?? false;

    /// <summary>
    ///     启动时隐藏主界面
    /// </summary>
    [ObservableProperty] private bool _isHideOnStart = ConfigHelper.CurrentConfig?.IsHideOnStart ?? false;

    /// <summary>
    ///     启动时不显示通知
    /// </summary>
    [ObservableProperty] private bool _isDisableNoticeOnStart = ConfigHelper.CurrentConfig?.IsDisableNoticeOnStart ?? false;

    /// <summary>
    ///     是否在关闭鼠标划词后保持最前
    /// </summary>
    [ObservableProperty] private bool _isKeepTopmostAfterMousehook = ConfigHelper.CurrentConfig?.IsKeepTopmostAfterMousehook ?? false;

    [ObservableProperty] private bool _isOcrAutoCopyText = ConfigHelper.CurrentConfig?.IsOcrAutoCopyText ?? false;

    /// <summary>
    ///     输出界面是否显示Prompt切换
    /// </summary>
    [ObservableProperty] private bool _isPromptToggleVisible = ConfigHelper.CurrentConfig?.IsPromptToggleVisible ?? true;

    /// <summary>
    ///     是否启用代理认证
    /// </summary>
    [ObservableProperty] private bool _isProxyAuthentication = ConfigHelper.CurrentConfig?.IsProxyAuthentication ?? false;

    /// <summary>
    ///     显示/隐藏密码
    /// </summary>
    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _isProxyPasswordHide = true;

    [ObservableProperty]
    private bool _isRemoveLineBreakGettingWords = ConfigHelper.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false;

    [ObservableProperty]
    private bool _isRemoveLineBreakGettingWordsOCR = ConfigHelper.CurrentConfig?.IsRemoveLineBreakGettingWordsOCR ?? false;

    /// <summary>
    ///     是否显示监听剪贴板
    /// </summary>
    [ObservableProperty] private bool _isShowClipboardMonitor = ConfigHelper.CurrentConfig?.IsShowClipboardMonitor ?? false;

    /// <summary>
    ///     是否显示关闭图标
    /// </summary>
    [ObservableProperty] private bool _isShowClose = ConfigHelper.CurrentConfig?.IsShowClose ?? false;

    /// <summary>
    ///     是否显示快速配置服务
    /// </summary>
    [ObservableProperty] private bool _isShowConfigureService = ConfigHelper.CurrentConfig?.IsShowConfigureService ?? false;

    /// <summary>
    ///     是否显示历史记录图标
    /// </summary>
    [ObservableProperty] private bool _isShowHistory = ConfigHelper.CurrentConfig?.IsShowHistory ?? false;

    /// <summary>
    ///     是否显示打开增量翻译图标
    /// </summary>
    [ObservableProperty] private bool _isShowIncrementalTranslation = ConfigHelper.CurrentConfig?.IsShowIncrementalTranslation ?? false;

    [ObservableProperty] private bool _isShowOnlyShowRet = ConfigHelper.CurrentConfig?.IsShowOnlyShowRet ?? false;

    [ObservableProperty] private bool _isShowLargeHumpCopyBtn = ConfigHelper.CurrentConfig?.IsShowLargeHumpCopyBtn ?? false;

    /// <summary>
    ///     是否显示主窗口提示词
    /// </summary>
    [ObservableProperty] private bool _isShowMainPlaceholder = ConfigHelper.CurrentConfig?.IsShowMainPlaceholder ?? true;

    /// <summary>
    ///     是否显示打开鼠标划词图标
    /// </summary>
    [ObservableProperty] private bool _isShowMousehook = ConfigHelper.CurrentConfig?.IsShowMousehook ?? false;

    /// <summary>
    ///     是否显示OCR图标
    /// </summary>
    [ObservableProperty] private bool _isShowOCR = ConfigHelper.CurrentConfig?.IsShowOCR ?? false;

    /// <summary>
    ///     是否显示设置图标
    /// </summary>
    [ObservableProperty] private bool _isShowPreference = ConfigHelper.CurrentConfig?.IsShowPreference ?? false;

    /// <summary>
    ///     是否显示识别二维码图标
    /// </summary>
    [ObservableProperty] private bool _isShowQRCode = ConfigHelper.CurrentConfig?.IsShowQRCode ?? false;

    /// <summary>
    ///     是否显示截图翻译图标
    /// </summary>
    [ObservableProperty] private bool _isShowScreenshot = ConfigHelper.CurrentConfig?.IsShowScreenshot ?? false;

    /// <summary>
    ///     是否显示静默OCR图标
    /// </summary>
    [ObservableProperty] private bool _isShowSilentOCR = ConfigHelper.CurrentConfig?.IsShowSilentOCR ?? false;

    [ObservableProperty] private bool _isShowSmallHumpCopyBtn = ConfigHelper.CurrentConfig?.IsShowSmallHumpCopyBtn ?? false;

    [ObservableProperty] private bool _isShowSnakeCopyBtn = ConfigHelper.CurrentConfig?.IsShowSnakeCopyBtn ?? false;

    [ObservableProperty] private bool _isShowTranslateBackBtn = ConfigHelper.CurrentConfig?.IsShowTranslateBackBtn ?? false;

    /// <summary>
    ///     是否开机启动
    /// </summary>
    [ObservableProperty] private bool _isStartup = ConfigHelper.CurrentConfig?.IsStartup ?? false;

    /// <summary>
    ///     是否开启重复触发显示界面为显示/隐藏
    /// </summary>
    [ObservableProperty] private bool _isTriggerShowHide = ConfigHelper.CurrentConfig?.IsTriggerShowHide ?? false;

    /// <summary>
    ///     主界面截图识别语种
    /// </summary>
    [ObservableProperty] private LangEnum _mainOcrLang = ConfigHelper.CurrentConfig?.MainOcrLang ?? LangEnum.auto;

    /// <summary>
    ///     主窗口阴影
    ///     * 比较损耗性能 实测多占用30MB内存
    /// </summary>
    [ObservableProperty] private bool _mainViewShadow = ConfigHelper.CurrentConfig?.MainViewShadow ?? false;

    /// <summary>
    ///     是否默认管理员启动
    /// </summary>
    [ObservableProperty] private bool _needAdmin = ConfigHelper.CurrentConfig?.NeedAdministrator ?? false;

    /// <summary>
    ///     OCR修改语言后立即翻译
    /// </summary>
    [ObservableProperty] private bool _ocrChangedLang2Execute = ConfigHelper.CurrentConfig?.OcrChangedLang2Execute ?? true;

    /// <summary>
    ///     常用语言
    /// </summary>
    [ObservableProperty] private string _oftenUsedLang = ConfigHelper.CurrentConfig?.OftenUsedLang ?? string.Empty;

    /// <summary>
    ///     代理服务器IP
    /// </summary>
    [ObservableProperty] private string _proxyIp = ConfigHelper.CurrentConfig?.ProxyIp ?? string.Empty;

    /// <summary>
    ///     所选代理方式
    /// </summary>
    [ObservableProperty] private ProxyMethodEnum _proxyMethod = ConfigHelper.CurrentConfig?.ProxyMethod ?? ProxyMethodEnum.系统代理;

    /// <summary>
    ///     代理认证密码
    /// </summary>
    [ObservableProperty] private string _proxyPassword = ConfigHelper.CurrentConfig?.ProxyPassword ?? string.Empty;

    /// <summary>
    ///     代理服务器端口
    /// </summary>
    [ObservableProperty] private int? _proxyPort = ConfigHelper.CurrentConfig?.ProxyPort ?? 8089;

    /// <summary>
    ///     代理认证用户名
    /// </summary>
    [ObservableProperty] private string _proxyUsername = ConfigHelper.CurrentConfig?.ProxyUsername ?? string.Empty;

    /// <summary>
    ///     截图是否显示辅助线
    /// </summary>
    [ObservableProperty] private bool _showAuxiliaryLine = ConfigHelper.CurrentConfig?.ShowAuxiliaryLine ?? true;

    /// <summary>
    ///     收缩框是否显示复制按钮
    /// </summary>
    [ObservableProperty] private bool _showCopyOnHeader = ConfigHelper.CurrentConfig?.ShowCopyOnHeader ?? false;

    private RelayCommand<string>? _showEncryptInfoCommand;

    /// <summary>
    ///     显示主界面截图翻译语种选择图标
    /// </summary>
    [ObservableProperty] private bool _showMainOcrLang = ConfigHelper.CurrentConfig?.ShowMainOcrLang ?? false;

    /// <summary>
    ///     是否显示主界面最小化按钮
    ///     * 仅在开启丢失焦点不隐藏项时有效 <see cref="StayMainViewWhenLoseFocus" />
    /// </summary>
    [ObservableProperty] private bool _showMinimalBtn = ConfigHelper.CurrentConfig?.ShowMinimalBtn ?? false;

    /// <summary>
    ///     丢失焦点时主界面不隐藏
    /// </summary>
    [ObservableProperty] private bool _stayMainViewWhenLoseFocus = ConfigHelper.CurrentConfig?.StayMainViewWhenLoseFocus ?? false;

    /// <summary>
    ///     主题类型
    /// </summary>
    [ObservableProperty] private ThemeType _themeType = ConfigHelper.CurrentConfig?.ThemeType ?? ThemeType.Light;

    /// <summary>
    ///     是否缓存位置
    /// </summary>
    [ObservableProperty] private bool _useCacheLocation = ConfigHelper.CurrentConfig?.UseCacheLocation ?? false;

    /// <summary>
    ///     使用windows forms库中的Clipboard尝试解决剪贴板占用问题
    /// </summary>
    [ObservableProperty] private bool _useFormsCopy = ConfigHelper.CurrentConfig?.UseFormsCopy ?? true;

    /// <summary>
    ///     取词时间间隔
    /// </summary>
    [ObservableProperty] private int _wordPickingInterval = ConfigHelper.CurrentConfig?.WordPickingInterval ?? 100;

    /// <summary>
    ///     自动执行翻译
    /// </summary>
    [ObservableProperty] private bool _autoTranslate = ConfigHelper.CurrentConfig?.AutoTranslate ?? false;

    /// <summary>
    ///     是否显示自动执行翻译
    /// </summary>
    [ObservableProperty] private bool _isShowAutoTranslate = ConfigHelper.CurrentConfig?.IsShowAutoTranslate ?? false;

    /// <summary>
    ///     动画速度
    /// </summary>
    [ObservableProperty] private AnimationSpeedEnum _animationSpeed = ConfigHelper.CurrentConfig?.AnimationSpeed ?? AnimationSpeedEnum.Middle;

    /// <summary>
    ///     主界面是否仅显示输出结果
    /// </summary>
    [ObservableProperty] private bool _isOnlyShowRet = ConfigHelper.CurrentConfig?.IsOnlyShowRet ?? false;

    /// <summary>
    ///     仅显示输出结果时是否隐藏语言界面
    /// </summary>
    [ObservableProperty] private bool _isHideLangWhenOnlyShowOutput = ConfigHelper.CurrentConfig?.IsHideLangWhenOnlyShowOutput ?? false;
    
    /// <summary>
    ///     是否净化内容格式
    /// </summary>
    [ObservableProperty] private bool _isPurify = ConfigHelper.CurrentConfig?.IsPurify ?? true;
    
    /// <summary>
    ///     OCR时图片质量
    /// </summary>
    [ObservableProperty] private OcrImageQualityEnum _ocrImageQuality = ConfigHelper.CurrentConfig?.OcrImageQuality ?? OcrImageQualityEnum.Medium;
    
    /// <summary>
    ///     原始语言识别为自动时使用该配置
    ///     * 使用在线识别服务出错时使用
    /// </summary>
    [ObservableProperty] private LangEnum _sourceLangIfAuto = ConfigHelper.CurrentConfig?.SourceLangIfAuto ?? LangEnum.en;

    /// <summary>
    ///     目标语种为自动时
    ///     * 原始语种识别为中文/中文繁体/中文粤语
    ///     * 目标语种使用该配置
    /// </summary>
    [ObservableProperty] private LangEnum _targetLangIfSourceZh = ConfigHelper.CurrentConfig?.TargetLangIfSourceZh ?? LangEnum.en;

    /// <summary>
    ///     目标语种为自动时
    ///     * 原始语种识别为非中文
    ///     * 目标语种使用该配置
    /// </summary>
    [ObservableProperty] private LangEnum _targetLangIfSourceNotZh = ConfigHelper.CurrentConfig?.TargetLangIfSourceNotZh ?? LangEnum.zh_cn;

    public long HistorySize = ConfigHelper.CurrentConfig?.HistorySize ?? 100;
    public Action? OnOftenUsedLang;

    #endregion
    
    public CommonViewModel()
    {
        // 获取系统已安装字体
        GetFontFamilys = Fonts.SystemFontFamilies.Select(font => font.Source).ToList();
        // 判断是否已安装软件字体，没有则插入到列表中
        if (!GetFontFamilys.Contains(Constant.DefaultFontName))
            GetFontFamilys.Insert(0, Constant.DefaultFontName);
        if (!GetFontFamilys.Contains(Constant.PingFangFontName))
            GetFontFamilys.Insert(0, Constant.PingFangFontName);

        // 加载历史记录类型
        LoadHistorySizeType();
    }

    public InputViewModel InputVm => Singleton<InputViewModel>.Instance;

    /// <summary>
    ///     显示/隐藏密码Command
    /// </summary>
    [JsonIgnore]
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        _showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);


    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(CustomFont):
            {
                // 切换字体
                var isAppFont = new List<string> { Constant.DefaultFontName, Constant.PingFangFontName }.Contains(CustomFont);
                Application.Current.Resources[Constant.UserDefineFontKey] = isAppFont
                    ? Application.Current.Resources[CustomFont]
                    : new FontFamily(CustomFont);
                break;
            }
            case nameof(HistorySizeType):
            {
                HistorySize = HistorySizeType switch
                {
                    0 => 50,
                    1 => 100,
                    2 => 200,
                    3 => 500,
                    4 => 1000,
                    5 => long.MaxValue,
                    6 => 0,
                    _ => 100
                };
                break;
            }
            case nameof(GlobalFontSize):
            {
                Constant.GlobalFontSizeList.ForEach(font
                    => Application.Current.Resources[font.Item1] = font.Item2 + GlobalFontSize.ToInt());
                break;
            }
        }

        base.OnPropertyChanged(e);
    }

    /// <summary>
    ///     通知增量翻译、自动翻译、仅显示输出界面配置到主界面
    /// </summary>
    public event Action<bool, bool, bool>? OnBooleansChanged;

    [RelayCommand]
    private Task SaveAsync()
    {
        // 保存时如果未开启丢失焦点不隐藏则关闭最小化按钮配置
        if (!StayMainViewWhenLoseFocus && ShowMinimalBtn)
        {
            var mainView = Application.Current.MainWindow;
            if (mainView is { WindowState: WindowState.Minimized })
                mainView.WindowState = WindowState.Normal;
            ShowMinimalBtn = false;
            LogService.Logger.Info("关闭丢失焦点不隐藏取消显示最小化按钮");
        }

        if (ConfigHelper.WriteConfig(this))
        {
            OnBooleansChanged?.Invoke(IncrementalTranslation, AutoTranslate, IsOnlyShowRet);
            ToastHelper.Show("保存常规配置成功", WindowType.Preference);
        }
        else
        {
            LogService.Logger.Debug($"保存常规配置失败，{JsonConvert.SerializeObject(this)}");
            ToastHelper.Show("保存常规配置失败", WindowType.Preference);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private void Reset()
    {
        IsStartup = ConfigHelper.CurrentConfig?.IsStartup ?? false;
        NeedAdmin = ConfigHelper.CurrentConfig?.NeedAdministrator ?? false;
        HistorySize = ConfigHelper.CurrentConfig?.HistorySize ?? 100;
        AutoScale = ConfigHelper.CurrentConfig?.AutoScale ?? 0.8;
        ThemeType = ConfigHelper.CurrentConfig?.ThemeType ?? ThemeType.Light;
        IsFollowMouse = ConfigHelper.CurrentConfig?.IsFollowMouse ?? false;
        CloseUIOcrRetTranslate = ConfigHelper.CurrentConfig?.CloseUIOcrRetTranslate ?? false;
        IsOcrAutoCopyText = ConfigHelper.CurrentConfig?.IsOcrAutoCopyText ?? false;
        IsAdjustContentTranslate = ConfigHelper.CurrentConfig?.IsAdjustContentTranslate ?? false;
        IsRemoveLineBreakGettingWords = ConfigHelper.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false;
        IsRemoveLineBreakGettingWordsOCR = ConfigHelper.CurrentConfig?.IsRemoveLineBreakGettingWordsOCR ?? false;
        DoubleTapTrayFunc = ConfigHelper.CurrentConfig?.DoubleTapTrayFunc ?? DoubleTapFuncEnum.InputFunc;
        CustomFont = ConfigHelper.CurrentConfig?.CustomFont ?? Constant.DefaultFontName;
        IsKeepTopmostAfterMousehook = ConfigHelper.CurrentConfig?.IsKeepTopmostAfterMousehook ?? false;
        IsShowClose = ConfigHelper.CurrentConfig?.IsShowClose ?? false;
        IsShowPreference = ConfigHelper.CurrentConfig?.IsShowPreference ?? false;
        IsShowMousehook = ConfigHelper.CurrentConfig?.IsShowMousehook ?? false;
        IsShowIncrementalTranslation = ConfigHelper.CurrentConfig?.IsShowIncrementalTranslation ?? false;
        IsShowOnlyShowRet = ConfigHelper.CurrentConfig?.IsShowOnlyShowRet ?? false;
        IsShowScreenshot = ConfigHelper.CurrentConfig?.IsShowScreenshot ?? false;
        IsShowOCR = ConfigHelper.CurrentConfig?.IsShowOCR ?? false;
        IsShowSilentOCR = ConfigHelper.CurrentConfig?.IsShowSilentOCR ?? false;
        IsShowClipboardMonitor = ConfigHelper.CurrentConfig?.IsShowClipboardMonitor ?? false;
        IsShowQRCode = ConfigHelper.CurrentConfig?.IsShowQRCode ?? false;
        IsShowHistory = ConfigHelper.CurrentConfig?.IsShowHistory ?? false;
        IsShowConfigureService = ConfigHelper.CurrentConfig?.IsShowConfigureService ?? false;
        WordPickingInterval = ConfigHelper.CurrentConfig?.WordPickingInterval ?? 200;
        IsHideOnStart = ConfigHelper.CurrentConfig?.IsHideOnStart ?? false;
        IsDisableNoticeOnStart = ConfigHelper.CurrentConfig?.IsDisableNoticeOnStart ?? false;
        ShowCopyOnHeader = ConfigHelper.CurrentConfig?.ShowCopyOnHeader ?? false;
        IsCaretLast = ConfigHelper.CurrentConfig?.IsCaretLast ?? false;
        ProxyMethod = ConfigHelper.CurrentConfig?.ProxyMethod ?? ProxyMethodEnum.系统代理;
        ProxyIp = ConfigHelper.CurrentConfig?.ProxyIp ?? string.Empty;
        ProxyPort = ConfigHelper.CurrentConfig?.ProxyPort ?? 0;
        ProxyUsername = ConfigHelper.CurrentConfig?.ProxyUsername ?? string.Empty;
        ProxyPassword = ConfigHelper.CurrentConfig?.ProxyPassword ?? string.Empty;
        CopyResultAfterTranslateIndex = ConfigHelper.CurrentConfig?.CopyResultAfterTranslateIndex ?? 0;
        IncrementalTranslation = ConfigHelper.CurrentConfig?.IncrementalTranslation ?? false;
        IsTriggerShowHide = ConfigHelper.CurrentConfig?.IsTriggerShowHide ?? false;
        IsShowMainPlaceholder = ConfigHelper.CurrentConfig?.IsShowMainPlaceholder ?? true;
        ShowAuxiliaryLine = ConfigHelper.CurrentConfig?.ShowAuxiliaryLine ?? true;
        ChangedLang2Execute = ConfigHelper.CurrentConfig?.ChangedLang2Execute ?? false;
        OcrChangedLang2Execute = ConfigHelper.CurrentConfig?.OcrChangedLang2Execute ?? false;
        UseFormsCopy = ConfigHelper.CurrentConfig?.UseFormsCopy ?? false;
        ExternalCallPort = ConfigHelper.CurrentConfig?.ExternalCallPort ?? 50020;
        ExternalCall = ConfigHelper.CurrentConfig?.ExternalCall ?? false;
        DetectType = ConfigHelper.CurrentConfig?.DetectType ?? LangDetectType.Local;
        DisableGlobalHotkeys = ConfigHelper.CurrentConfig?.DisableGlobalHotkeys ?? false;
        MainViewMaxHeight = ConfigHelper.CurrentConfig?.MainViewMaxHeight ?? 840;
        MainViewWidth = ConfigHelper.CurrentConfig?.MainViewWidth ?? 460;
        MainViewShadow = ConfigHelper.CurrentConfig?.MainViewShadow ?? false;
        IsPromptToggleVisible = ConfigHelper.CurrentConfig?.IsPromptToggleVisible ?? true;
        IsShowSnakeCopyBtn = ConfigHelper.CurrentConfig?.IsShowSnakeCopyBtn ?? true;
        IsShowSmallHumpCopyBtn = ConfigHelper.CurrentConfig?.IsShowSmallHumpCopyBtn ?? true;
        IsShowLargeHumpCopyBtn = ConfigHelper.CurrentConfig?.IsShowLargeHumpCopyBtn ?? true;
        IsShowTranslateBackBtn = ConfigHelper.CurrentConfig?.IsShowTranslateBackBtn ?? true;
        IgnoreHotkeysOnFullscreen = ConfigHelper.CurrentConfig?.IgnoreHotkeysOnFullscreen ?? false;
        StayMainViewWhenLoseFocus = ConfigHelper.CurrentConfig?.StayMainViewWhenLoseFocus ?? false;
        MainOcrLang = ConfigHelper.CurrentConfig?.MainOcrLang ?? LangEnum.auto;
        ShowMainOcrLang = ConfigHelper.CurrentConfig?.ShowMainOcrLang ?? false;
        HotkeyCopySuccessToast = ConfigHelper.CurrentConfig?.HotkeyCopySuccessToast ?? true;
        OftenUsedLang = ConfigHelper.CurrentConfig?.OftenUsedLang ?? string.Empty;
        UseCacheLocation = ConfigHelper.CurrentConfig?.UseCacheLocation ?? false;
        ShowMinimalBtn = ConfigHelper.CurrentConfig?.ShowMinimalBtn ?? false;
        GlobalFontSize = ConfigHelper.CurrentConfig?.GlobalFontSize ?? GlobalFontSizeEnum.General;
        AutoTranslate = ConfigHelper.CurrentConfig?.AutoTranslate ?? false;
        IsShowAutoTranslate = ConfigHelper.CurrentConfig?.IsShowAutoTranslate ?? false;
        AnimationSpeed = ConfigHelper.CurrentConfig?.AnimationSpeed ?? AnimationSpeedEnum.Middle;
        IsHideLangWhenOnlyShowOutput = ConfigHelper.CurrentConfig?.IsHideLangWhenOnlyShowOutput ?? false;
        IsPurify = ConfigHelper.CurrentConfig?.IsPurify ?? true;
        IsOnlyShowRet = ConfigHelper.CurrentConfig?.IsOnlyShowRet ?? true;
        OcrImageQuality = ConfigHelper.CurrentConfig?.OcrImageQuality ?? OcrImageQualityEnum.Medium;
        SourceLangIfAuto = ConfigHelper.CurrentConfig?.SourceLangIfAuto ?? LangEnum.en;
        TargetLangIfSourceZh = ConfigHelper.CurrentConfig?.TargetLangIfSourceZh ?? LangEnum.en;
        TargetLangIfSourceNotZh = ConfigHelper.CurrentConfig?.TargetLangIfSourceNotZh ?? LangEnum.zh_cn;

        LoadHistorySizeType();
        ToastHelper.Show("重置配置", WindowType.Preference);
    }

    private void LoadHistorySizeType()
    {
        HistorySizeType = HistorySize switch
        {
            50 => 0,
            100 => 1,
            200 => 2,
            500 => 3,
            1000 => 4,
            long.MaxValue => 5,
            0 => 6,
            _ => 1
        };
    }

    private void ShowEncryptInfo(string? obj)
    {
        if (obj is nameof(ProxyPassword)) IsProxyPasswordHide = !IsProxyPasswordHide;
    }

    [RelayCommand]
    private async Task OftenUsedLangChangeAsync()
    {
        var view = new LangSettingView(OftenUsedLang)
        {
            Owner = Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault(),
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        if (view.ShowDialog() == false) return;
        OftenUsedLang = view.LangResult;
        OnOftenUsedLang?.Invoke();

        // 不等待上面回调可能会导致绑定出错
        await Task.Delay(1000);

        await SaveAsync();
    }

    #region 主界面调整

    /// <summary>
    ///     主界面最大高度
    /// </summary>
    [ObservableProperty] private double _mainViewMaxHeight = ConfigHelper.CurrentConfig?.MainViewMaxHeight ?? 840;

    /// <summary>
    ///     主界面宽度
    /// </summary>
    [ObservableProperty] private double _mainViewWidth = ConfigHelper.CurrentConfig?.MainViewWidth ?? 460;

    private const double ChangeValue = 40;

    [RelayCommand]
    private void MainViewChange(object? obj = null)
    {
        MainViewMaxHeightChange(obj);
        MainViewWidthChange(obj);
    }

    [RelayCommand]
    private void ResetMainView()
    {
        MainViewMaxHeight = ConfigHelper.CurrentConfig?.MainViewMaxHeight ?? 840;
        MainViewWidth = ConfigHelper.CurrentConfig?.MainViewWidth ?? 460;
    }

    /// <summary>
    /// </summary>
    /// <param name="obj">null is plus</param>
    [RelayCommand]
    private void MainViewMaxHeightChange(object? obj = null)
    {
        double newValue;
        if (obj == null)
        {
            newValue = MainViewMaxHeight + ChangeValue;
            if (newValue >= 1080) newValue = 1080;
        }
        else
        {
            newValue = MainViewMaxHeight - ChangeValue;
            if (newValue <= 400) newValue = 400;
        }

        MainViewMaxHeight = newValue;
    }

    [RelayCommand]
    private void MainViewWidthChange(object? obj = null)
    {
        double newValue;
        if (obj == null)
        {
            newValue = MainViewWidth + ChangeValue;
            if (newValue >= 1920) newValue = 1920;
        }
        else
        {
            newValue = MainViewWidth - ChangeValue;
            if (newValue <= 400) newValue = 400;
        }

        MainViewWidth = newValue;
    }

    #endregion 主界面调整
}