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
    /// <summary>
    ///     ConfigHelper单例
    /// </summary>
    private static readonly ConfigHelper ConfigHelper = Singleton<ConfigHelper>.Instance;

    /// <summary>
    ///     当前配置实例
    /// </summary>
    private static readonly ConfigModel? CurConfig = ConfigHelper.CurrentConfig;

    [ObservableProperty] private double _autoScale = CurConfig?.AutoScale ?? 0.8;

    /// <summary>
    ///     修改语言后立即翻译
    /// </summary>
    [ObservableProperty] private bool _changedLang2Execute = CurConfig?.ChangedLang2Execute ?? true;

    [ObservableProperty] private bool _closeUIOcrRetTranslate = CurConfig?.CloseUIOcrRetTranslate ?? false;

    /// <summary>
    ///     翻译后执行自动复制动作(Ctrl+1...9)
    /// </summary>
    [ObservableProperty] private int _copyResultAfterTranslateIndex = CurConfig?.CopyResultAfterTranslateIndex ?? 0;

    private string _customFont = CurConfig?.CustomFont ?? ConstStr.DEFAULTFONTNAME;

    /// <summary>
    ///     语种识别类型
    /// </summary>
    [ObservableProperty] private LangDetectType _detectType = CurConfig?.DetectType ?? LangDetectType.Local;

    /// <summary>
    ///     禁用全局热键
    /// </summary>
    [ObservableProperty] private bool _disableGlobalHotkeys = CurConfig?.DisableGlobalHotkeys ?? false;

    [ObservableProperty]
    private DoubleTapFuncEnum _doubleTapTrayFunc = CurConfig?.DoubleTapTrayFunc ?? DoubleTapFuncEnum.InputFunc;

    /// <summary>
    ///     是否启用外部调用服务
    /// </summary>
    [ObservableProperty] private bool _externalCall = CurConfig?.ExternalCall ?? false;

    /// <summary>
    ///     外部调用服务端口
    /// </summary>
    [ObservableProperty] private int? _externalCallPort = CurConfig?.ExternalCallPort ?? 50020;

    [ObservableProperty] private List<string> _getFontFamilys;

    /// <summary>
    ///     历史记录大小
    /// </summary>
    private long _historySizeType = 1;


    /// <summary>
    ///     热键触发复制后是否显示成功提示
    /// </summary>
    [ObservableProperty] private bool _hotkeyCopySuccessToast = CurConfig?.HotkeyCopySuccessToast ?? true;

    /// <summary>
    ///     全屏模式下忽略热键
    /// </summary>
    [ObservableProperty] private bool _ignoreHotkeysOnFullscreen = CurConfig?.IgnoreHotkeysOnFullscreen ?? false;

    /// <summary>
    ///     是否开启增量翻译
    /// </summary>
    [ObservableProperty] private bool _incrementalTranslation = CurConfig?.IncrementalTranslation ?? false;

    [ObservableProperty] private bool _isAdjustContentTranslate = CurConfig?.IsAdjustContentTranslate ?? false;

    /// <summary>
    ///     激活窗口时光标移动至末尾
    /// </summary>
    [ObservableProperty] private bool _isCaretLast = CurConfig?.IsCaretLast ?? false;

    [ObservableProperty] private bool _isFollowMouse = CurConfig?.IsFollowMouse ?? false;

    /// <summary>
    ///     启动时隐藏主界面
    /// </summary>
    [ObservableProperty] private bool _isHideOnStart = CurConfig?.IsHideOnStart ?? false;

    /// <summary>
    ///     是否在关闭鼠标划词后保持最前
    /// </summary>
    [ObservableProperty] private bool _isKeepTopmostAfterMousehook = CurConfig?.IsKeepTopmostAfterMousehook ?? false;

    [ObservableProperty] private bool _isOcrAutoCopyText = CurConfig?.IsOcrAutoCopyText ?? false;

    /// <summary>
    ///     输出界面是否显示Prompt切换
    /// </summary>
    [ObservableProperty] private bool _isPromptToggleVisible = CurConfig?.IsPromptToggleVisible ?? true;

    /// <summary>
    ///     是否启用代理认证
    /// </summary>
    [ObservableProperty] private bool _isProxyAuthentication = CurConfig?.IsProxyAuthentication ?? false;

    /// <summary>
    ///     显示/隐藏密码
    /// </summary>
    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _isProxyPasswordHide = true;

    [ObservableProperty]
    private bool _isRemoveLineBreakGettingWords = CurConfig?.IsRemoveLineBreakGettingWords ?? false;

    [ObservableProperty]
    private bool _isRemoveLineBreakGettingWordsOCR = CurConfig?.IsRemoveLineBreakGettingWordsOCR ?? false;

    /// <summary>
    ///     是否显示监听剪贴板
    /// </summary>
    [ObservableProperty] private bool _isShowClipboardMonitor = CurConfig?.IsShowClipboardMonitor ?? false;

    /// <summary>
    ///     是否显示关闭图标
    /// </summary>
    [ObservableProperty] private bool _isShowClose = CurConfig?.IsShowClose ?? false;

    /// <summary>
    ///     是否显示快速配置服务
    /// </summary>
    [ObservableProperty] private bool _isShowConfigureService = CurConfig?.IsShowConfigureService ?? false;

    /// <summary>
    ///     是否显示历史记录图标
    /// </summary>
    [ObservableProperty] private bool _isShowHistory = CurConfig?.IsShowHistory ?? false;

    /// <summary>
    ///     是否显示打开增量翻译图标
    /// </summary>
    [ObservableProperty] private bool _isShowIncrementalTranslation = CurConfig?.IsShowIncrementalTranslation ?? false;

    [ObservableProperty] private bool _isShowLargeHumpCopyBtn = CurConfig?.IsShowLargeHumpCopyBtn ?? false;
    
    [ObservableProperty] private bool _isShowTranslateBackBtn = CurConfig?.IsShowTranslateBackBtn ?? false;

    /// <summary>
    ///     是否显示主窗口提示词
    /// </summary>
    [ObservableProperty] private bool _isShowMainPlaceholder = CurConfig?.IsShowMainPlaceholder ?? true;

    /// <summary>
    ///     是否显示打开鼠标划词图标
    /// </summary>
    [ObservableProperty] private bool _isShowMousehook = CurConfig?.IsShowMousehook ?? false;

    /// <summary>
    ///     是否显示OCR图标
    /// </summary>
    [ObservableProperty] private bool _isShowOCR = CurConfig?.IsShowOCR ?? false;

    /// <summary>
    ///     是否显示设置图标
    /// </summary>
    [ObservableProperty] private bool _isShowPreference = CurConfig?.IsShowPreference ?? false;

    /// <summary>
    ///     是否显示识别二维码图标
    /// </summary>
    [ObservableProperty] private bool _isShowQRCode = CurConfig?.IsShowQRCode ?? false;

    /// <summary>
    ///     是否显示截图翻译图标
    /// </summary>
    [ObservableProperty] private bool _isShowScreenshot = CurConfig?.IsShowScreenshot ?? false;

    /// <summary>
    ///     是否显示静默OCR图标
    /// </summary>
    [ObservableProperty] private bool _isShowSilentOCR = CurConfig?.IsShowSilentOCR ?? false;

    [ObservableProperty] private bool _isShowSmallHumpCopyBtn = CurConfig?.IsShowSmallHumpCopyBtn ?? false;

    [ObservableProperty] private bool _isShowSnakeCopyBtn = CurConfig?.IsShowSnakeCopyBtn ?? false;

    /// <summary>
    ///     是否开机启动
    /// </summary>
    [ObservableProperty] private bool _isStartup = CurConfig?.IsStartup ?? false;

    /// <summary>
    ///     是否开启重复触发显示界面为显示/隐藏
    /// </summary>
    [ObservableProperty] private bool _isTriggerShowHide = CurConfig?.IsTriggerShowHide ?? false;

    /// <summary>
    ///     主界面截图识别语种
    /// </summary>
    [ObservableProperty] private LangEnum _mainOcrLang = CurConfig?.MainOcrLang ?? LangEnum.auto;

    /// <summary>
    ///     主窗口阴影
    ///     * 比较损耗性能 实测多占用30MB内存
    /// </summary>
    [ObservableProperty] private bool _mainViewShadow = CurConfig?.MainViewShadow ?? false;

    /// <summary>
    ///     是否默认管理员启动
    /// </summary>
    [ObservableProperty] private bool _needAdmin = CurConfig?.NeedAdministrator ?? false;

    /// <summary>
    ///     OCR修改语言后立即翻译
    /// </summary>
    [ObservableProperty] private bool _ocrChangedLang2Execute = CurConfig?.OcrChangedLang2Execute ?? true;

    /// <summary>
    ///     常用语言
    /// </summary>
    [ObservableProperty] private string _oftenUsedLang = CurConfig?.OftenUsedLang ?? string.Empty;

    /// <summary>
    ///     代理服务器IP
    /// </summary>
    [ObservableProperty] private string _proxyIp = CurConfig?.ProxyIp ?? string.Empty;

    /// <summary>
    ///     所选代理方式
    /// </summary>
    [ObservableProperty] private ProxyMethodEnum _proxyMethod = CurConfig?.ProxyMethod ?? ProxyMethodEnum.系统代理;

    /// <summary>
    ///     代理认证密码
    /// </summary>
    [ObservableProperty] private string _proxyPassword = CurConfig?.ProxyPassword ?? string.Empty;

    /// <summary>
    ///     代理服务器端口
    /// </summary>
    [ObservableProperty] private int? _proxyPort = CurConfig?.ProxyPort ?? 8089;

    /// <summary>
    ///     代理认证用户名
    /// </summary>
    [ObservableProperty] private string _proxyUsername = CurConfig?.ProxyUsername ?? string.Empty;

    /// <summary>
    ///     截图是否显示辅助线
    /// </summary>
    [ObservableProperty] private bool _showAuxiliaryLine = CurConfig?.ShowAuxiliaryLine ?? true;

    /// <summary>
    ///     收缩框是否显示复制按钮
    /// </summary>
    [ObservableProperty] private bool _showCopyOnHeader = CurConfig?.ShowCopyOnHeader ?? false;

    private RelayCommand<string>? _showEncryptInfoCommand;

    /// <summary>
    ///     显示主界面截图翻译语种选择图标
    /// </summary>
    [ObservableProperty] private bool _showMainOcrLang = CurConfig?.ShowMainOcrLang ?? false;

    /// <summary>
    ///     丢失焦点时主界面不隐藏
    /// </summary>
    [ObservableProperty] private bool _stayMainViewWhenLoseFocus = CurConfig?.StayMainViewWhenLoseFocus ?? false;

    /// <summary>
    ///     主题类型
    /// </summary>
    [ObservableProperty] private ThemeType _themeType = CurConfig?.ThemeType ?? ThemeType.Light;

    /// <summary>
    ///     使用windows forms库中的Clipboard尝试解决剪贴板占用问题
    /// </summary>
    [ObservableProperty] private bool _useFormsCopy = CurConfig?.UseFormsCopy ?? true;

    /// <summary>
    ///     取词时间间隔
    /// </summary>
    [ObservableProperty] private int _wordPickingInterval = CurConfig?.WordPickingInterval ?? 100;

    /// <summary>
    ///     是否缓存位置
    /// </summary>
    [ObservableProperty] private bool _useCacheLocation = CurConfig?.UseCacheLocation ?? false;

    public long HistorySize = CurConfig?.HistorySize ?? 100;
    public Action? OnOftenUsedLang;

    public CommonViewModel()
    {
        // 获取系统已安装字体
        GetFontFamilys = Fonts.SystemFontFamilies.Select(font => font.Source).ToList();
        // 判断是否已安装软件字体，没有则插入到列表中
        if (!GetFontFamilys.Contains(ConstStr.DEFAULTFONTNAME)) GetFontFamilys.Insert(0, ConstStr.DEFAULTFONTNAME);

        // 加载历史记录类型
        LoadHistorySizeType();
    }

    public InputViewModel InputVm => Singleton<InputViewModel>.Instance;

    public long HistorySizeType
    {
        get => _historySizeType;
        set
        {
            if (_historySizeType == value)
                return;
            OnPropertyChanging();
            _historySizeType = value;

            HistorySize = value switch
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

            OnPropertyChanged();
        }
    }

    public string CustomFont
    {
        get => _customFont;
        set
        {
            if (_customFont == value)
                return;
            OnPropertyChanging();

            try
            {
                // 切换字体
                Application.Current.Resources[ConstStr.USERDEFINEFONTKEY] = value.Equals(ConstStr.DEFAULTFONTNAME)
                    ? Application.Current.Resources[ConstStr.DEFAULTFONTNAME]
                    : new FontFamily(value);
                _customFont = value;
            }
            catch (Exception)
            {
                Application.Current.Resources[ConstStr.USERDEFINEFONTKEY] =
                    Application.Current.Resources[ConstStr.DEFAULTFONTNAME];
                _customFont = ConstStr.DEFAULTFONTNAME;
            }

            OnPropertyChanged();
        }
    }

    /// <summary>
    ///     显示/隐藏密码Command
    /// </summary>
    [JsonIgnore]
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        _showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

    public event Action<bool>? OnIncrementalChanged;

    [RelayCommand]
    private void Save()
    {
        if (ConfigHelper.WriteConfig(this))
        {
            //通知增量翻译配置到主界面
            OnIncrementalChanged?.Invoke(IncrementalTranslation);
            ToastHelper.Show("保存常规配置成功", WindowType.Preference);
        }
        else
        {
            LogService.Logger.Debug($"保存常规配置失败，{JsonConvert.SerializeObject(this)}");
            ToastHelper.Show("保存常规配置失败", WindowType.Preference);
        }
    }

    [RelayCommand]
    private void Reset()
    {
        IsStartup = CurConfig?.IsStartup ?? false;
        NeedAdmin = CurConfig?.NeedAdministrator ?? false;
        HistorySize = CurConfig?.HistorySize ?? 100;
        AutoScale = CurConfig?.AutoScale ?? 0.8;
        ThemeType = CurConfig?.ThemeType ?? ThemeType.Light;
        IsFollowMouse = CurConfig?.IsFollowMouse ?? false;
        CloseUIOcrRetTranslate = CurConfig?.CloseUIOcrRetTranslate ?? false;
        IsOcrAutoCopyText = CurConfig?.IsOcrAutoCopyText ?? false;
        IsAdjustContentTranslate = CurConfig?.IsAdjustContentTranslate ?? false;
        IsRemoveLineBreakGettingWords = CurConfig?.IsRemoveLineBreakGettingWords ?? false;
        IsRemoveLineBreakGettingWordsOCR = CurConfig?.IsRemoveLineBreakGettingWordsOCR ?? false;
        DoubleTapTrayFunc = CurConfig?.DoubleTapTrayFunc ?? DoubleTapFuncEnum.InputFunc;
        CustomFont = CurConfig?.CustomFont ?? ConstStr.DEFAULTFONTNAME;
        IsKeepTopmostAfterMousehook = CurConfig?.IsKeepTopmostAfterMousehook ?? false;
        IsShowClose = CurConfig?.IsShowClose ?? false;
        IsShowPreference = CurConfig?.IsShowPreference ?? false;
        IsShowMousehook = CurConfig?.IsShowMousehook ?? false;
        IsShowIncrementalTranslation = CurConfig?.IsShowIncrementalTranslation ?? false;
        IsShowScreenshot = CurConfig?.IsShowScreenshot ?? false;
        IsShowOCR = CurConfig?.IsShowOCR ?? false;
        IsShowSilentOCR = CurConfig?.IsShowSilentOCR ?? false;
        IsShowClipboardMonitor = CurConfig?.IsShowClipboardMonitor ?? false;
        IsShowQRCode = CurConfig?.IsShowQRCode ?? false;
        IsShowHistory = CurConfig?.IsShowHistory ?? false;
        IsShowConfigureService = CurConfig?.IsShowConfigureService ?? false;
        WordPickingInterval = CurConfig?.WordPickingInterval ?? 200;
        IsHideOnStart = CurConfig?.IsHideOnStart ?? false;
        ShowCopyOnHeader = CurConfig?.ShowCopyOnHeader ?? false;
        IsCaretLast = CurConfig?.IsCaretLast ?? false;
        ProxyMethod = CurConfig?.ProxyMethod ?? ProxyMethodEnum.系统代理;
        ProxyIp = CurConfig?.ProxyIp ?? string.Empty;
        ProxyPort = CurConfig?.ProxyPort ?? 0;
        ProxyUsername = CurConfig?.ProxyUsername ?? string.Empty;
        ProxyPassword = CurConfig?.ProxyPassword ?? string.Empty;
        CopyResultAfterTranslateIndex = CurConfig?.CopyResultAfterTranslateIndex ?? 0;
        IncrementalTranslation = CurConfig?.IncrementalTranslation ?? false;
        IsTriggerShowHide = CurConfig?.IsTriggerShowHide ?? false;
        IsShowMainPlaceholder = CurConfig?.IsShowMainPlaceholder ?? true;
        ShowAuxiliaryLine = CurConfig?.ShowAuxiliaryLine ?? true;
        ChangedLang2Execute = CurConfig?.ChangedLang2Execute ?? false;
        OcrChangedLang2Execute = CurConfig?.OcrChangedLang2Execute ?? false;
        UseFormsCopy = CurConfig?.UseFormsCopy ?? false;
        ExternalCallPort = CurConfig?.ExternalCallPort ?? 50020;
        ExternalCall = CurConfig?.ExternalCall ?? false;
        DetectType = CurConfig?.DetectType ?? LangDetectType.Local;
        DisableGlobalHotkeys = CurConfig?.DisableGlobalHotkeys ?? false;
        MainViewMaxHeight = CurConfig?.MainViewMaxHeight ?? 840;
        MainViewWidth = CurConfig?.MainViewWidth ?? 460;
        MainViewShadow = CurConfig?.MainViewShadow ?? false;
        IsPromptToggleVisible = CurConfig?.IsPromptToggleVisible ?? true;
        IsShowSnakeCopyBtn = CurConfig?.IsShowSnakeCopyBtn ?? true;
        IsShowSmallHumpCopyBtn = CurConfig?.IsShowSmallHumpCopyBtn ?? true;
        IsShowLargeHumpCopyBtn = CurConfig?.IsShowLargeHumpCopyBtn ?? true;
        IsShowTranslateBackBtn = CurConfig?.IsShowTranslateBackBtn ?? true;
        IgnoreHotkeysOnFullscreen = CurConfig?.IgnoreHotkeysOnFullscreen ?? false;
        StayMainViewWhenLoseFocus = CurConfig?.StayMainViewWhenLoseFocus ?? false;
        MainOcrLang = CurConfig?.MainOcrLang ?? LangEnum.auto;
        ShowMainOcrLang = CurConfig?.ShowMainOcrLang ?? false;
        HotkeyCopySuccessToast = CurConfig?.HotkeyCopySuccessToast ?? true;
        OftenUsedLang = CurConfig?.OftenUsedLang ?? string.Empty;
        UseCacheLocation = CurConfig?.UseCacheLocation ?? false;

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

        Save();
    }

    #region 主界面调整

    /// <summary>
    ///     主界面最大高度
    /// </summary>
    [ObservableProperty] private double _mainViewMaxHeight = CurConfig?.MainViewMaxHeight ?? 840;

    /// <summary>
    ///     主界面宽度
    /// </summary>
    [ObservableProperty] private double _mainViewWidth = CurConfig?.MainViewWidth ?? 460;

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
        MainViewMaxHeight = CurConfig?.MainViewMaxHeight ?? 840;
        MainViewWidth = CurConfig?.MainViewWidth ?? 460;
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