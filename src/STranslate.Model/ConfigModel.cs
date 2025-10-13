﻿using System.ComponentModel;

namespace STranslate.Model;

public class ConfigModel
{
    /// <summary>
    ///     应用程序语言
    /// </summary>
    public AppLanguageKind AppLanguage { get; set; } = AppLanguageKind.zh_Hans_CN;

    /// <summary>
    ///     开机自启动
    /// </summary>
    public bool IsStartup { get; set; }

    /// <summary>
    ///     启动方式
    /// </summary>
    public StartModeKind StartMode { get; set; }

    /// <summary>
    ///     是否自动检查更新
    /// </summary>
    public bool AutoCheckUpdate { get; set; } = true;

    /// <summary>
    ///     Github 资源下载代理
    /// </summary>
    public DownloadProxyKind DownloadProxy { get; set; } = DownloadProxyKind.GhProxy;

    /// <summary>
    ///     历史记录大小
    /// </summary>
    public long HistorySize { get; set; }

    /// <summary>
    ///     自动识别语种标度
    /// </summary>
    public double AutoScale { get; set; }

    /// <summary>
    ///     是否亮色模式
    /// </summary>
    public ThemeType ThemeType { get; set; }

    /// <summary>
    ///     是否跟随鼠标
    /// </summary>
    public bool IsFollowMouse { get; set; }

    /// <summary>
    ///     OCR结果翻译关闭OCR界面
    /// </summary>
    public bool CloseUIOcrRetTranslate { get; set; }

    /// <summary>
    ///     OCR时是否自动复制文本
    /// </summary>
    public bool IsOcrAutoCopyText { get; set; }

    /// <summary>
    ///     截图OCR时是否自动复制文本
    /// </summary>
    public bool IsScreenshotOcrAutoCopyText { get; set; }

    /// <summary>
    ///     是否调整完语句后翻译
    /// </summary>
    public bool IsAdjustContentTranslate { get; set; }

    /// <summary>
    ///     主界面取词换行处理
    /// </summary>
    public LineBreakHandlingMode LineBreakHandler { get; set; } = LineBreakHandlingMode.None;

    /// <summary>
    ///     OCR取词换行处理
    /// </summary>
    public LineBreakHandlingMode LineBreakOCRHandler { get; set; } = LineBreakHandlingMode.None;

    /// <summary>
    ///     鼠标双击托盘程序功能
    /// </summary>
    public DoubleTapFuncEnum DoubleTapTrayFunc { get; set; } = DoubleTapFuncEnum.InputFunc;

    /// <summary>
    ///     原始语言
    /// </summary>
    public LangEnum SourceLang { get; set; } = LangEnum.auto;

    /// <summary>
    ///     目标语言
    /// </summary>
    public LangEnum TargetLang { get; set; } = LangEnum.auto;

    /// <summary>
    ///     退出时的位置
    /// </summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>
    ///     自定义字体
    /// </summary>
    public string CustomFont { get; set; } = Constant.DefaultFontName;

    /// <summary>
    ///     鼠标划词取消后是否保留置顶
    /// </summary>
    public bool IsKeepTopmostAfterMousehook { get; set; }

    /// <summary>
    ///     是否显示设置图标
    /// </summary>
    public bool IsShowPreference { get; set; } = true;

    /// <summary>
    ///     是否显示配置服务图标
    /// </summary>
    public bool IsShowConfigureService { get; set; }

    /// <summary>
    ///     是否显示打开鼠标划词图标
    /// </summary>
    public bool IsShowMousehook { get; set; }

    /// <summary>
    ///     是否显示打开增量翻译图标
    /// </summary>
    public bool IsShowIncrementalTranslation { get; set; }

    /// <summary>
    ///     是否显示截图翻译图标
    /// </summary>
    public bool IsShowScreenshot { get; set; }

    /// <summary>
    ///     是否显示OCR图标
    /// </summary>
    public bool IsShowOCR { get; set; }

    /// <summary>
    ///     是否显示静默OCR图标
    /// </summary>
    public bool IsShowSilentOCR { get; set; }

    /// <summary>
    ///     是否显示监听剪贴板
    /// </summary>
    public bool IsShowClipboardMonitor { get; set; }

    /// <summary>
    ///     是否显示识别二维码图标
    /// </summary>
    public bool IsShowQRCode { get; set; }

    /// <summary>
    ///     是否显示历史记录图标
    /// </summary>
    public bool IsShowHistory { get; set; }

    /// <summary>
    ///     取词间隔
    /// </summary>
    /// <remarks>
    ///     默认200，体验区别不大，主要是避免了福昕阅读器，复制时弹窗导致取词时间变长最终无法取词成功
    ///     https://github.com/zggsong/stranslate/issues/13
    /// </remarks>
    public int WordPickingInterval { get; set; } = 200;

    /// <summary>
    ///     启动时隐藏主界面
    /// </summary>
    public bool IsHideOnStart { get; set; }

    /// <summary>
    ///     启动时不显示通知
    /// </summary>
    public bool IsDisableNoticeOnStart { get; set; }

    /// <summary>
    ///     收缩框是否显示复制按钮
    /// </summary>
    public bool ShowCopyOnHeader { get; set; }

    /// <summary>
    ///     激活窗口时光标移动至末尾
    /// </summary>
    public bool IsCaretLast { get; set; }

    /// <summary>
    ///     网络代理方式
    /// </summary>
    public ProxyMethodEnum ProxyMethod { get; set; } = ProxyMethodEnum.SystemProxy;

    /// <summary>
    ///     代理IP
    /// </summary>
    public string ProxyIp { get; set; } = string.Empty;

    /// <summary>
    ///     代理端口
    /// </summary>
    public int? ProxyPort { get; set; }

    /// <summary>
    ///     代理是否需要验证
    /// </summary>
    public bool IsProxyAuthentication { get; set; }

    /// <summary>
    ///     代理用户名
    /// </summary>
    public string ProxyUsername { get; set; } = string.Empty;

    /// <summary>
    ///     代理密码
    /// </summary>
    public string ProxyPassword { get; set; } = string.Empty;

    /// <summary>
    ///     翻译后自动复制结果
    /// </summary>
    public int CopyResultAfterTranslateIndex { get; set; }

    /// <summary>
    ///     是否增量翻译
    /// </summary>
    public bool IncrementalTranslation { get; set; }

    /// <summary>
    ///     是否开启重复触发显示界面为显示/隐藏
    /// </summary>
    public bool IsTriggerShowHide { get; set; }

    /// <summary>
    ///     是否显示主窗口提示词
    /// </summary>
    public bool IsShowMainPlaceholder { get; set; } = true;

    /// <summary>
    ///     截图是否显示辅助线
    /// </summary>
    public bool ShowAuxiliaryLine { get; set; } = true;

    /// <summary>
    ///     WebDav Type
    /// </summary>
    public BackupType BackupType { get; set; } = BackupType.Local;

    /// <summary>
    ///     WebDavUrl
    /// </summary>
    public string WebDavUrl { get; set; } = string.Empty;

    /// <summary>
    ///     WebDav用户名
    /// </summary>
    public string WebDavUsername { get; set; } = string.Empty;

    /// <summary>
    ///     WebDav密码
    /// </summary>
    public string WebDavPassword { get; set; } = string.Empty;

    /// <summary>
    ///     切换语言时自动翻译
    /// </summary>
    public bool ChangedLang2Execute { get; set; }

    /// <summary>
    ///     Ocr切换语言时自动翻译
    /// </summary>
    public bool OcrChangedLang2Execute { get; set; }

    /// <summary>
    ///     如果出现OpenClipboard失败(0x800401D0(CLIPBRD E CANT OPEN))尝试解决
    ///     https://stackoverflow.com/questions/68666/clipbrd-e-cant-open-error-when-setting-the-clipboard-from-net
    /// </summary>
    public bool UseFormsCopy { get; set; }

    /// <summary>
    ///     开启外部调用服务
    /// </summary>
    public bool ExternalCall { get; set; }

    /// <summary>
    ///     外部调用端口
    /// </summary>
    public int? ExternalCallPort { get; set; }

    /// <summary>
    ///     OCR页面高度
    /// </summary>
    public double? OcrViewHeight { get; set; }

    /// <summary>
    ///     OCR页面宽度
    /// </summary>
    public double? OcrViewWidth { get; set; }

    /// <summary>
    ///     语种识别服务
    /// </summary>
    public LangDetectType DetectType { get; set; } = LangDetectType.Local;

    /// <summary>
    ///     禁用热键
    /// </summary>
    public bool DisableGlobalHotkeys { get; set; }

    /// <summary>
    ///     主界面最大高度
    /// </summary>
    public double MainViewMaxHeight { get; set; } = 840;

    /// <summary>
    ///     主界面宽度
    /// </summary>
    public double MainViewWidth { get; set; } = 460;

    /// <summary>
    ///     输入框高度
    /// </summary>
    public double InputViewMaxHeight { get; set; } = 200;
    
    public double InputViewMinHeight { get; set; } = 70;

    /// <summary>
    ///     主窗口阴影
    ///     * 比较损耗性能 实测多占用30MB内存
    /// </summary>
    public bool MainViewShadow { get; set; }

    /// <summary>
    ///     输出界面是否显示Prompt切换
    /// </summary>
    public bool IsPromptToggleVisible { get; set; } = true;

    public bool IsShowSnakeCopyBtn { get; set; }

    public bool IsShowSmallHumpCopyBtn { get; set; }

    public bool IsShowLargeHumpCopyBtn { get; set; }
    
    public bool IsShowTranslateBackBtn { get; set; }

    /// <summary>
    ///     全屏模式下忽略热键
    /// </summary>
    public bool IgnoreHotkeysOnFullscreen { get; set; }

    /// <summary>
    ///     丢失焦点时主界面不隐藏
    /// </summary>
    public bool StayMainViewWhenLoseFocus { get; set; }

    /// <summary>
    ///     是否显示关闭图标
    /// </summary>
    public bool IsShowClose { get; set; }

    /// <summary>
    ///     主界面截图翻译语种
    /// </summary>
    public LangEnum MainOcrLang { get; set; } = LangEnum.auto;

    /// <summary>
    ///     显示主界面截图翻译语种
    /// </summary>
    public bool ShowMainOcrLang { get; set; }

    /// <summary>
    ///     热键触发复制后是否显示成功提示
    /// </summary>
    public bool HotkeyCopySuccessToast { get; set; } = true;

    /// <summary>
    ///    常用语种
    /// </summary>
    public string OftenUsedLang { get; set; } = string.Empty;
    
    /// <summary>
    ///     是否缓存位置
    /// </summary>
    public bool UseCacheLocation { get; set; }
    
    /// <summary>
    ///     是否显示主界面最小化按钮
    ///     * 仅在开启丢失焦点不隐藏项时有效 <see cref="StayMainViewWhenLoseFocus"/>
    /// </summary>
    public bool ShowMinimalBtn { get; set; }

    /// <summary>
    ///     全局字体大小
    /// </summary>
    public GlobalFontSizeEnum GlobalFontSize { get; set; } = GlobalFontSizeEnum.General;

    /// <summary>
    ///     自动执行翻译
    /// </summary>
    public bool AutoTranslate { get; set; } = false;

    /// <summary>
    ///     主界面是否显示自动执行翻译
    /// </summary>
    public bool IsShowAutoTranslate { get; set; } = false;

    /// <summary>
    ///     动画速度
    /// </summary>
    public AnimationSpeedEnum AnimationSpeed { get; set; } = AnimationSpeedEnum.Middle;

    /// <summary>
    ///     显示结果时是否隐藏语言选择界面
    /// </summary>
    public bool IsHideLangWhenOnlyShowOutput { get; set; } = true;

    /// <summary>
    ///     主界面头部显示仅显示输出结果
    /// </summary>
    public bool IsShowOnlyShowRet { get; set; } = false;

    /// <summary>
    ///     主界面是否仅显示输出结果
    /// </summary>
    public bool IsOnlyShowRet { get; set; } = false;

    /// <summary>
    ///     OCR时图片质量
    /// </summary>
    public OcrImageQualityEnum OcrImageQuality { get; set; } = OcrImageQualityEnum.Medium;

    /// <summary>
    ///     原始语言识别为自动时使用该配置
    ///     * 使用在线识别服务出错时使用
    /// </summary>
    public LangEnum SourceLangIfAuto { get; set; } = LangEnum.en;

    /// <summary>
    ///     目标语种为自动时
    ///     * 原始语种识别为中文/中文繁体/中文粤语
    ///     * 目标语种使用该配置
    /// </summary>
    public LangEnum TargetLangIfSourceZh { get; set; } = LangEnum.en;

    /// <summary>
    ///     目标语种为自动时
    ///     * 原始语种识别为非中文
    ///     * 目标语种使用该配置
    /// </summary>
    public LangEnum TargetLangIfSourceNotZh { get; set; } = LangEnum.zh_cn;

    /// <summary>
    ///     调用系统剪贴板来插入结果
    /// </summary>
    public bool UsePasteOutput { get; set; }

    /// <summary>
    ///     Http 请求超时时间
    ///     * s
    /// </summary>
    public int HttpTimeout { get; set; } = 10;

    /// <summary>
    ///     服务标题最大宽度
    /// </summary>
    public double TitleMaxWidth { get; set; } = 120;

    /// <summary>
    ///     服务提示词最大宽度
    /// </summary>
    public double PromptMaxWidth { get; set; } = 100;

    /// <summary>
    ///     替换翻译
    /// </summary>
    public ReplaceProp ReplaceProp { get; set; } = new();

    /// <summary>
    ///     热键
    /// </summary>
    public Hotkeys? Hotkeys { get; set; }

    /// <summary>
    ///     服务
    /// </summary>
    public BindingList<ITranslator>? Services { get; set; }

    /// <summary>
    ///     OCR
    /// </summary>
    public OCRCollection<IOCR>? OCRList { get; set; }

    /// <summary>
    ///     TTS
    /// </summary>
    public TTSCollection<ITTS>? TTSList { get; set; }

    /// <summary>
    ///     生词本
    /// </summary>
    public VocabularyBookCollection<IVocabularyBook>? VocabularyBookList { get; set; }

    /// <summary>
    ///     Copy
    /// </summary>
    /// <returns></returns>
    public ConfigModel Clone()
    {
        return new ConfigModel
        {
            IsStartup = IsStartup,
            StartMode = StartMode,
            AutoCheckUpdate = AutoCheckUpdate,
            DownloadProxy = DownloadProxy,
            HistorySize = HistorySize,
            AutoScale = AutoScale,
            ThemeType = ThemeType,
            IsFollowMouse = IsFollowMouse,
            CloseUIOcrRetTranslate = CloseUIOcrRetTranslate,
            IsOcrAutoCopyText = IsOcrAutoCopyText,
            IsScreenshotOcrAutoCopyText = IsScreenshotOcrAutoCopyText,
            IsAdjustContentTranslate = IsAdjustContentTranslate,
            LineBreakHandler = LineBreakHandler,
            LineBreakOCRHandler = LineBreakOCRHandler,
            DoubleTapTrayFunc = DoubleTapTrayFunc,
            SourceLang = SourceLang,
            TargetLang = TargetLang,
            Position = Position,
            CustomFont = CustomFont,
            IsKeepTopmostAfterMousehook = IsKeepTopmostAfterMousehook,
            IsShowClose = IsShowClose,
            IsShowPreference = IsShowPreference,
            IsShowConfigureService = IsShowConfigureService,
            IsShowMousehook = IsShowMousehook,
            IsShowIncrementalTranslation = IsShowIncrementalTranslation,
            IsShowOnlyShowRet = IsShowOnlyShowRet,
            IsShowScreenshot = IsShowScreenshot,
            IsShowOCR = IsShowOCR,
            IsShowSilentOCR = IsShowSilentOCR,
            IsShowClipboardMonitor = IsShowClipboardMonitor,
            IsShowQRCode = IsShowQRCode,
            IsShowHistory = IsShowHistory,
            WordPickingInterval = WordPickingInterval,
            IsHideOnStart = IsHideOnStart,
            IsDisableNoticeOnStart = IsDisableNoticeOnStart,
            ShowCopyOnHeader = ShowCopyOnHeader,
            IsCaretLast = IsCaretLast,
            ProxyMethod = ProxyMethod,
            ProxyIp = ProxyIp,
            ProxyPort = ProxyPort,
            IsProxyAuthentication = IsProxyAuthentication,
            ProxyUsername = ProxyUsername,
            ProxyPassword = ProxyPassword,
            CopyResultAfterTranslateIndex = CopyResultAfterTranslateIndex,
            IncrementalTranslation = IncrementalTranslation,
            IsTriggerShowHide = IsTriggerShowHide,
            IsShowMainPlaceholder = IsShowMainPlaceholder,
            ShowAuxiliaryLine = ShowAuxiliaryLine,
            BackupType = BackupType,
            WebDavUrl = WebDavUrl,
            WebDavUsername = WebDavUsername,
            WebDavPassword = WebDavPassword,
            ChangedLang2Execute = ChangedLang2Execute,
            OcrChangedLang2Execute = OcrChangedLang2Execute,
            UseFormsCopy = UseFormsCopy,
            ExternalCall = ExternalCall,
            ExternalCallPort = ExternalCallPort,
            OcrViewHeight = OcrViewHeight,
            OcrViewWidth = OcrViewWidth,
            DetectType = DetectType,
            DisableGlobalHotkeys = DisableGlobalHotkeys,
            MainViewMaxHeight = MainViewMaxHeight,
            MainViewWidth = MainViewWidth,
            InputViewMaxHeight = InputViewMaxHeight,
            InputViewMinHeight = InputViewMinHeight,
            MainViewShadow = MainViewShadow,
            IsPromptToggleVisible = IsPromptToggleVisible,
            IsShowSnakeCopyBtn = IsShowSnakeCopyBtn,
            IsShowSmallHumpCopyBtn = IsShowSmallHumpCopyBtn,
            IsShowLargeHumpCopyBtn = IsShowLargeHumpCopyBtn,
            IsShowTranslateBackBtn = IsShowTranslateBackBtn,
            IgnoreHotkeysOnFullscreen = IgnoreHotkeysOnFullscreen,
            StayMainViewWhenLoseFocus = StayMainViewWhenLoseFocus,
            MainOcrLang = MainOcrLang,
            ShowMainOcrLang = ShowMainOcrLang,
            HotkeyCopySuccessToast = HotkeyCopySuccessToast,
            OftenUsedLang = OftenUsedLang,
            UseCacheLocation = UseCacheLocation,
            ShowMinimalBtn = ShowMinimalBtn,
            GlobalFontSize = GlobalFontSize,
            AutoTranslate = AutoTranslate,
            IsShowAutoTranslate = IsShowAutoTranslate,
            AnimationSpeed = AnimationSpeed,
            IsHideLangWhenOnlyShowOutput = IsHideLangWhenOnlyShowOutput,
            IsOnlyShowRet = IsOnlyShowRet,
            OcrImageQuality = OcrImageQuality,
            SourceLangIfAuto = SourceLangIfAuto,
            TargetLangIfSourceZh = TargetLangIfSourceZh,
            TargetLangIfSourceNotZh = TargetLangIfSourceNotZh,
            UsePasteOutput = UsePasteOutput,
            HttpTimeout = HttpTimeout,
            AppLanguage = AppLanguage,
            TitleMaxWidth = TitleMaxWidth,
            PromptMaxWidth = PromptMaxWidth,
            ReplaceProp = ReplaceProp.Clone(),
            Hotkeys = Hotkeys?.Clone(),
            Services = Services?.Clone(),
            OCRList = OCRList?.DeepCopy(),
            TTSList = TTSList?.DeepCopy(),
            VocabularyBookList = VocabularyBookList?.DeepCopy()
        };
    }
}

public static class Extensions
{
    public static BindingList<T> Clone<T>(this BindingList<T> listToClone)
        where T : ITranslator
    {
        return new BindingList<T>(listToClone.Select(item => (T)item.Clone()).ToList());
    }
}