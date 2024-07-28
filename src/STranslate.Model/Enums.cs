using System.ComponentModel;

namespace STranslate.Model;

/// <summary>
///     支持的语种 https://zh.wikipedia.org/wiki/ISO_639-1%E4%BB%A3%E7%A0%81%E5%88%97%E8%A1%A8
/// </summary>
public enum LangEnum
{
    [Description("自动选择")] auto,

    [Description("中文")] zh_cn,

    [Description("中文繁体")] zh_tw,

    [Description("中文粤语")] yue,

    [Description("英语")] en,

    [Description("日语")] ja,

    [Description("韩语")] ko,

    [Description("法语")] fr,

    [Description("西班牙语")] es,

    [Description("俄语")] ru,

    [Description("德语")] de,

    [Description("意大利语")] it,

    [Description("土耳其语")] tr,

    [Description("葡萄牙语")] pt_pt,

    [Description("布列塔尼语")] pt_br,

    [Description("越南语")] vi,

    [Description("印度尼西亚语")] id,

    [Description("泰语")] th,

    [Description("马来语")] ms,

    [Description("阿拉伯语")] ar,

    [Description("印地语")] hi,

    [Description("蒙古语(西里尔)")] mn_cy,

    [Description("蒙古语")] mn_mo,

    [Description("高棉语")] km,

    [Description("书面挪威语")] nb_no,

    [Description("新挪威语")] nn_no,

    [Description("波斯语")] fa,

    [Description("瑞典语")] sv,

    [Description("波兰语")] pl,

    [Description("荷兰语")] nl,

    [Description("乌克兰语")] uk
}

[Obsolete("过期")]
public enum LanguageEnum
{
    [Description("自动选择")] AUTO, //自动

    [Description("中文")] ZH, //中文

    [Description("英语")] EN, //英语

    [Description("德语")] DE, //德语

    [Description("西班牙语")] ES, //西班牙语

    [Description("法语")] FR, //法语

    [Description("意大利语")] IT, //意大利语

    [Description("日语")] JA, //日语

    [Description("荷兰语")] NL, //荷兰语

    [Description("波兰语")] PL, //波兰语

    [Description("葡萄牙语")] PT, //葡萄牙语

    [Description("俄语")] RU, //俄语

    [Description("保加利亚语")] BG, //保加利亚语

    [Description("捷克语")] CS, //捷克语

    [Description("丹麦语")] DA, //丹麦语

    [Description("希腊语")] EL, //希腊语

    [Description("爱沙尼亚语")] ET, //爱沙尼亚语

    [Description("芬兰语")] FI, //芬兰语

    [Description("匈牙利语")] HU, //匈牙利语

    [Description("立陶宛语")] LT, //立陶宛语

    [Description("拉脱维亚语")] LV, //拉脱维亚语

    [Description("罗马尼亚语")] RO, //罗马尼亚语

    [Description("斯洛伐克语")] SK, //斯洛伐克语

    [Description("斯洛文尼亚语")] SL, //斯洛文尼亚语

    [Description("瑞典语")] SV, //瑞典语

    [Description("土耳其语")] TR //土耳其语
}

/// <summary>
///     ServiceView 重置选中项类型
/// </summary>
public enum ActionType
{
    Initialize,
    Delete,
    Add
}

/// <summary>
///     服务类型
/// </summary>
public enum ServiceType
{
    ApiService = 0,
    BaiduService,
    MicrosoftService,
    OpenAIService,
    GeminiService,
    TencentService,
    AliService,
    YoudaoService,
    NiutransService,
    CaiyunService,
    VolcengineService,
    STranslateService,
    EcdictService,
    ChatglmService,
    OllamaService,
    BaiduBceService,
    DeepLService
}

public enum TTSType
{
    AzureTTS,
    OfflineTTS
}

public enum OCRType
{
    [Description("PaddleOCR")] PaddleOCR,
    [Description("百度OCR")] BaiduOCR,
    [Description("腾讯OCR")] TencentOCR,
    [Description("火山OCR")] VolcengineOCR
}

public enum BaiduOCRAction
{
    [Description("高精度含位置版")] accurate,
    [Description("高精度版")] accurate_basic,
    [Description("标准含位置版")] general,
    [Description("标准版")] general_basic
}

public enum TencentOCRAction
{
    [Description("通用印刷体识别")] GeneralBasicOCR,
    [Description("通用印刷体识别(高精度版)")] GeneralAccurateOCR
}

public enum VolcengineOCRAction
{
    [Description("通用文字识别")] OCRNormal,
    [Description("多语种OCR")] MultiLanguageOCR
}

/// <summary>
///     图标类型
/// </summary>
public enum IconType
{
    [Description("本地")] STranslate,

    [Description("DeepL")] DeepL,

    [Description("百度")] Baidu,

    [Description("谷歌")] Google,

    [Description("爱词霸")] Iciba,

    [Description("有道")] Youdao,

    [Description("必应")] Bing,

    [Description("OpenAI")] OpenAI,

    [Description("Gemini")] Gemini,

    [Description("腾讯")] Tencent,

    [Description("阿里")] Ali,

    [Description("小牛")] Niutrans,

    [Description("彩云")] Caiyun,

    [Description("微软")] Microsoft,

    [Description("火山")] Volcengine,

    [Description("简明汉字词典")] Ecdict,

    [Description("Azure")] Azure,

    [Description("智谱清言")] Chatglm,

    [Description("零一万物")] Linyi,

    [Description("DeepSeek")] DeepSeek,

    [Description("Groq")] Groq,

    [Description("PaddleOCR")] PaddleOCR,

    [Description("百度云平台")] BaiduBce,

    [Description("腾讯OCR")] TencentOCR,

    [Description("Ollama")] Ollama,

    [Description("Kimi")] Kimi
}

/// <summary>
///     快捷键修饰键
/// </summary>
[Flags]
public enum KeyModifiers : byte
{
    MOD_NONE = 0x0,
    MOD_ALT = 0x1,
    MOD_CTRL = 0x2,
    MOD_SHIFT = 0x4,
    MOD_WIN = 0x8
}

public enum KeyCodes
{
    None = 0,
    A = 65,
    B,
    C,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K,
    L,
    M,
    N,
    O,
    P,
    Q,
    R,
    S,
    T,
    U,
    V,
    W,
    X,
    Y,
    Z
}

/// <summary>
///     窗体类型-用于通知窗口
/// </summary>
public enum WindowType
{
    Main,
    Preference,
    OCR
}

/// <summary>
///     托盘功能枚举
/// </summary>
public enum DoubleTapFuncEnum
{
    [Description("输入翻译")] InputFunc,

    [Description("截图翻译")] ScreenFunc,

    [Description("鼠标划词")] MouseHookFunc,

    [Description("文字识别")] OCRFunc,

    [Description("显示界面")] ShowViewFunc,

    [Description("偏好设置")] PreferenceFunc,

    [Description("禁用热键")] ForbidShortcutFunc,

    [Description("退出程序")] ExitFunc
}

/// <summary>
///     腾讯地区
/// </summary>
public enum TencentRegionEnum
{
    [Description("亚太东南_曼谷")] ap_bangkok,

    [Description("华北地区_北京")] ap_beijing,

    [Description("西南地区_成都")] ap_chengdu,

    [Description("西南地区_重庆")] ap_chongqing,

    [Description("华南地区_广州")] ap_guangzhou,

    [Description("港澳台地区_中国香港")] ap_hongkong,

    [Description("亚太南部_孟买")] ap_mumbai,

    [Description("亚太东北_首尔")] ap_seoul,

    [Description("华东地区_上海")] ap_shanghai,

    [Description("华东地区_上海金融")] ap_shanghai_fsi,

    [Description("华南地区_深圳金融")] ap_shenzhen_fsi,

    [Description("亚太东南_新加坡")] ap_singapore,

    [Description("亚太东北_东京")] ap_tokyo,

    [Description("欧洲地区_法兰克福")] eu_frankfurt,

    [Description("美国东部_弗吉尼亚")] na_ashburn,

    [Description("美国西部_硅谷")] na_siliconvalley,

    [Description("北美地区_多伦多")] na_toronto
}

/// <summary>
///     设置页面导航
/// </summary>
public enum PerferenceType
{
    Common,
    Hotkey,
    Service,
    Replace,
    OCR,
    TTS,
    Favorite,
    History,
    Backup,
    About
}

/// <summary>
///     主题类型
/// </summary>
public enum ThemeType
{
    [Description("明亮主题")] Light,

    [Description("黑暗主题")] Dark,

    [Description("跟随系统")] Auto
}

/// <summary>
///     网络代理方式
/// </summary>
public enum ProxyMethodEnum
{
    不使用代理,
    系统代理,
    HTTP,
    SOCKS5
}

/// <summary>
///     Azure TTS 语音
/// </summary>
public enum AzureVoiceEnum
{
    [Description("晓北-女-东北官话")] zh_CN_liaoning_XiaobeiNeural,

    [Description("云登-男-中原官话河南")] zh_CN_henan_YundengNeural,

    [Description("晓妮-女-中原官话陕西")] zh_CN_shaanxi_XiaoniNeural,

    [Description("云翔-男-冀鲁官话")] zh_CN_shandong_YunxiangNeural,

    [Description("晓晓-女-普通话")] zh_CN_XiaoxiaoNeural,

    [Description("云希-男-普通话")] zh_CN_YunxiNeural,

    [Description("云建-男-普通话")] zh_CN_YunjianNeural,

    [Description("晓伊-女-普通话")] zh_CN_XiaoyiNeural,

    [Description("云扬-男-普通话")] zh_CN_YunyangNeural,

    [Description("晓辰-男-普通话")] zh_CN_XiaochenNeural,

    [Description("晓涵-男-普通话")] zh_CN_XiaohanNeural,

    [Description("晓梦-女-普通话")] zh_CN_XiaomengNeural,

    [Description("晓墨-女-普通话")] zh_CN_XiaomoNeural,

    [Description("晓秋-女-普通话")] zh_CN_XiaoqiuNeural,

    [Description("晓睿-女-普通话")] zh_CN_XiaoruiNeural,

    [Description("晓双-女-普通话")] zh_CN_XiaoshuangNeural,

    [Description("晓颜-女-普通话")] zh_CN_XiaoyanNeural,

    [Description("晓悠-女-普通话")] zh_CN_XiaoyouNeural,

    [Description("晓甄-女-普通话")] zh_CN_XiaozhenNeural,

    [Description("云枫-男-普通话")] zh_CN_YunfengNeural,

    [Description("云皓-男-普通话")] zh_CN_YunhaoNeural,

    [Description("云夏-男-普通话")] zh_CN_YunxiaNeural,

    [Description("云野-男-普通话")] zh_CN_YunyeNeural,

    [Description("云泽-男-普通话")] zh_CN_YunzeNeural,

    [Description("小晨-女-普通话")] zh_CN_XiaochenMultilingualNeural,

    [Description("晓柔-女-普通话")] zh_CN_XiaorouNeural,

    [Description("晓晓方言-女-普通话")] zh_CN_XiaoxiaoDialectsNeural,

    [Description("云杰-男-普通话")] zh_CN_YunjieNeural,

    [Description("晓萱-女-普通话")] zh_CN_XiaoxuanNeural,

    [Description("云希-男-西南官话")] zh_CN_sichuan_YunxiNeural
}

/// <summary>
///     备份方式
/// </summary>
public enum BackupType
{
    [Description("本地")] Local,

    [Description("WebDav")] WebDav
}

/// <summary>
///     全局热键类型
/// </summary>
public enum HotkeyEnum
{
    InputHk,
    CrosswordHk,
    ScreenshotHk,
    OpenHk,
    ReplaceHk,
    MousehookHk,
    OcrHk,
    SilentOcrHk,
    ClipboardMonitorHk
}

/// <summary>
///     外部调用功能
/// </summary>
public enum ExternalCallAction
{
    translate = 1,
    translate_force,
    translate_input,
    translate_ocr,
    translate_crossword,
    translate_mousehook,
    listenclipboard,
    ocr,
    ocr_silence,
    ocr_qrcode,
    open_window,
    open_preference,
    open_history,
    forbiddenhotkey
}

/// <summary>
///     语种识别方式
/// </summary>
public enum LangDetectType
{
    [Description("本地识别")] Local,
    [Description("百度识别")] Baidu,
    [Description("腾讯识别")] Tencent,
    [Description("小牛识别")] Niutrans,
    [Description("必应识别")] Bing,
    [Description("Yandex")] Yandex,
    [Description("谷歌识别")] Google
}

/// <summary>
///     Github代理
/// </summary>
public enum GithubProxy
{
    [Description("")] None,

    [Description("https://mirror.ghproxy.com/")]
    GHProxy
}

/// <summary>
///     获取Description
/// </summary>
public static class EnumExtensions
{
    public static string GetDescription(this Enum val)
    {
        var field = val.GetType().GetField(val.ToString());
        var customAttribute = Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
        return customAttribute == null ? val.ToString() : ((DescriptionAttribute)customAttribute).Description;
    }

    /// <summary>
    ///     https://blog.csdn.net/lzdidiv/article/details/71170528
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static int ToInt(this Enum e)
    {
        return e.GetHashCode();
    }

    /// <summary>
    ///     通过枚举对象获取枚举列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static List<T> GetEnumList<T>(this T value)
    {
        var list = new List<T>();
        if (value is Enum)
        {
            var valData = Convert.ToInt32((T)Enum.Parse(typeof(T), value.ToString() ?? ""));
            var tps = Enum.GetValues(typeof(T));

            list.AddRange(from object tp in tps
                where (Convert.ToInt32((T)Enum.Parse(typeof(T), tp.ToString() ?? "")) & valData) == valData
                select (T)tp);
        }

        return list;
    }

    /// <summary>
	///     获取枚举类型的所有枚举值集合
	/// </summary>
	/// <typeparam name="T">枚举类型</typeparam>
	/// <returns>枚举值的集合</returns>
	public static T[] GetEnumArray<T>() where T : Enum
    {
        return (T[])Enum.GetValues(typeof(T));
    }

    /// <summary>
    ///     通过枚举类型获取枚举列表;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static List<T> GetEnumList<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).OfType<T>().ToList();
    }

    public static T Increment<T>(this T value)
        where T : Enum
    {
        var enumValues = (T[])Enum.GetValues(typeof(T));
        var index = Array.IndexOf(enumValues, value);
        if (index < enumValues.Length - 1)
            return enumValues[index + 1];
        return value;
    }

    public static T Decrement<T>(this T value)
        where T : Enum
    {
        var enumValues = (T[])Enum.GetValues(typeof(T));
        var index = Array.IndexOf(enumValues, value);
        if (index > 0)
            return enumValues[index - 1];
        return value;
    }

    /// <summary>
    ///     累加超出枚举范围则返回第一个
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enum"></param>
    /// <returns></returns>
    public static T Increase<T>(this T @enum) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        var currentIndex = Array.IndexOf(values, @enum);
        var nextIndex = (currentIndex + 1) % values.Length;
        return values[nextIndex];
    }
}