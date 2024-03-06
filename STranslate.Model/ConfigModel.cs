using System.ComponentModel;
using System.Linq;

namespace STranslate.Model;

public class ConfigModel
{
    /// <summary>
    /// 开机自启动
    /// </summary>
    public bool IsStartup { get; set; }

    /// <summary>
    /// 是否管理员启动
    /// </summary>
    public bool NeedAdministrator { get; set; }

    /// <summary>
    /// 历史记录大小
    /// </summary>
    public long HistorySize { get; set; }

    /// <summary>
    /// 自动识别语种标度
    /// </summary>
    public double AutoScale { get; set; }

    /// <summary>
    /// 是否亮色模式
    /// </summary>
    public ThemeType ThemeType { get; set; }

    /// <summary>
    /// 是否跟随鼠标
    /// </summary>
    public bool IsFollowMouse { get; set; }

    /// <summary>
    /// OCR结果翻译关闭OCR界面
    /// </summary>
    public bool CloseUIOcrRetTranslate { get; set; }

    /// <summary>
    /// 截图出现问题尝试一下
    /// </summary>
    public bool UnconventionalScreen { get; set; }

    /// <summary>
    /// OCR时是否自动复制文本
    /// </summary>
    public bool IsOcrAutoCopyText { get; set; }

    /// <summary>
    /// 是否调整完语句后翻译
    /// </summary>
    public bool IsAdjustContentTranslate { get; set; }

    /// <summary>
    /// 取词时移除换行
    /// </summary>
    public bool IsRemoveLineBreakGettingWords { get; set; }

    /// <summary>
    /// 鼠标双击托盘程序功能
    /// </summary>
    public DoubleTapFuncEnum DoubleTapTrayFunc { get; set; } = DoubleTapFuncEnum.InputFunc;

    /// <summary>
    /// 原始语言
    /// </summary>
    public string SourceLanguage { get; set; } = string.Empty;

    /// <summary>
    /// 目标语言
    /// </summary>
    public string TargetLanguage { get; set; } = string.Empty;

    /// <summary>
    /// 退出时的位置
    /// </summary>
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// 自定义字体
    /// </summary>
    public string CustomFont { get; set; } = ConstStr.DEFAULTFONTNAME;

    /// <summary>
    /// 鼠标划词取消后是否保留置顶
    /// </summary>
    public bool IsKeepTopmostAfterMousehook { get; set; } = false;

    /// <summary>
    /// 是否显示设置图标
    /// </summary>
    public bool IsShowPreference { get; set; } = true;

    /// <summary>
    /// 是否显示打开鼠标划词图标
    /// </summary>
    public bool IsShowMousehook { get; set; } = false;

    /// <summary>
    /// 是否显示截图翻译图标
    /// </summary>
    public bool IsShowScreenshot { get; set; } = false;

    /// <summary>
    /// 是否显示OCR图标
    /// </summary>
    public bool IsShowOCR { get; set; } = false;

    /// <summary>
    /// 是否显示静默OCR图标
    /// </summary>
    public bool IsShowSilentOCR { get; set; } = false;

    /// <summary>
    /// 是否显示监听剪贴板
    /// </summary>
    public bool IsShowClipboardMonitor { get; set; } = false;

    /// <summary>
    /// 是否显示识别二维码图标
    /// </summary>
    public bool IsShowQRCode { get; set; } = false;

    /// <summary>
    /// 是否显示历史记录图标
    /// </summary>
    public bool IsShowHistory { get; set; } = false;

    /// <summary>
    /// 取词间隔
    /// </summary>
    /// <remarks>
    /// 默认200，体验区别不大，主要是避免了福昕阅读器，复制时弹窗导致取词时间变长最终无法取词成功
    /// https://github.com/zggsong/stranslate/issues/13
    /// </remarks>
    public int WordPickingInterval { get; set; } = 200;

    /// <summary>
    /// 启动时隐藏主界面
    /// </summary>
    public bool IsHideOnStart { get; set; } = false;

    /// <summary>
    /// 收缩框是否显示复制按钮
    /// </summary>
    public bool ShowCopyOnHeader { get; set; } = false;

    /// <summary>
    /// 激活窗口时光标移动至末尾
    /// </summary>
    public bool IsCaretLast { get; set; } = false;

    /// <summary>
    /// 最大高度
    /// </summary>
    public MaxHeight MaxHeight { get; set; } = MaxHeight.Maximum;

    /// <summary>
    /// 最大宽度
    /// </summary>
    public WidthEnum Width { get; set; } = WidthEnum.Minimum;

    /// <summary>
    /// 网络代理方式
    /// </summary>
    public ProxyMethodEnum ProxyMethod { get; set; } = ProxyMethodEnum.系统代理;

    /// <summary>
    /// 代理IP
    /// </summary>
    public string ProxyIp { get; set; } = string.Empty;

    /// <summary>
    /// 代理端口
    /// </summary>
    public int? ProxyPort { get; set; }

    /// <summary>
    /// 代理是否需要验证
    /// </summary>
    public bool IsProxyAuthentication { get; set; }

    /// <summary>
    /// 代理用户名
    /// </summary>
    public string ProxyUsername { get; set; } = string.Empty;

    /// <summary>
    /// 代理密码
    /// </summary>
    public string ProxyPassword { get; set; } = string.Empty;

    /// <summary>
    /// 热键
    /// </summary>
    public Hotkeys? Hotkeys { get; set; }

    /// <summary>
    /// 服务
    /// </summary>
    public BindingList<ITranslator>? Services { get; set; }

    /// <summary>
    /// TTS
    /// </summary>
    public TTSCollection<ITTS>? TTSList { get; set; }

    /// <summary>
    /// Copy
    /// </summary>
    /// <returns></returns>
    public ConfigModel Clone()
    {
        return new ConfigModel
        {
            IsStartup = IsStartup,
            NeedAdministrator = NeedAdministrator,
            HistorySize = HistorySize,
            AutoScale = AutoScale,
            ThemeType = ThemeType,
            IsFollowMouse = IsFollowMouse,
            CloseUIOcrRetTranslate = CloseUIOcrRetTranslate,
            UnconventionalScreen = UnconventionalScreen,
            IsOcrAutoCopyText = IsOcrAutoCopyText,
            IsAdjustContentTranslate = IsAdjustContentTranslate,
            IsRemoveLineBreakGettingWords = IsRemoveLineBreakGettingWords,
            DoubleTapTrayFunc = DoubleTapTrayFunc,
            SourceLanguage = SourceLanguage,
            TargetLanguage = TargetLanguage,
            Position = Position,
            CustomFont = CustomFont,
            IsKeepTopmostAfterMousehook = IsKeepTopmostAfterMousehook,
            IsShowPreference = IsShowPreference,
            IsShowMousehook = IsShowMousehook,
            IsShowScreenshot = IsShowScreenshot,
            IsShowOCR = IsShowOCR,
            IsShowSilentOCR = IsShowSilentOCR,
            IsShowClipboardMonitor = IsShowClipboardMonitor,
            IsShowQRCode = IsShowQRCode,
            IsShowHistory = IsShowHistory,
            WordPickingInterval = WordPickingInterval,
            IsHideOnStart = IsHideOnStart,
            ShowCopyOnHeader = ShowCopyOnHeader,
            IsCaretLast = IsCaretLast,
            MaxHeight = MaxHeight,
            Width = Width,
            ProxyMethod = ProxyMethod,
            ProxyIp = ProxyIp,
            ProxyPort = ProxyPort,
            IsProxyAuthentication = IsProxyAuthentication,
            ProxyUsername = ProxyUsername,
            ProxyPassword = ProxyPassword,
            Hotkeys = Hotkeys?.Clone(),
            Services = Services?.Clone(),
            TTSList = TTSList?.DeepCopy()
        };
    }
}

internal static class Extensions
{
    public static BindingList<T> Clone<T>(this BindingList<T> listToClone)
        where T : ITranslator
    {
        return new BindingList<T>(listToClone.Select(item => (T)item.Clone()).ToList());
    }
}
