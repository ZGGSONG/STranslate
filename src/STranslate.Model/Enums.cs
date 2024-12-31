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
    DeepLService,
    AzureOpenAIService,
    ClaudeService,
    DeepSeekService,
    KingSoftDictService,
    BingDictService,
}

public enum TTSType
{
    AzureTTS,
    OfflineTTS,
    LingvaTTS,
    EdgeTTS,
}

public enum VocabularyBookType
{
    /// <summary>
    ///     欧陆词典
    /// </summary>
    EuDictVocabularyBook,
}

public enum OCRType
{
    [Description("PaddleOCR")] PaddleOCR,
    [Description("百度OCR")] BaiduOCR,
    [Description("腾讯OCR")] TencentOCR,
    [Description("火山OCR")] VolcengineOCR,
    [Description("谷歌OCR")] GoogleOCR,
    [Description("OpenAIOCR")] OpenAIOCR,
    [Description("微信OCR")] WeChatOCR,
    [Description("GeminiOCR")] GeminiOCR,
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
    [Description("Kimi")] Kimi,
    [Description("Lingva")] Lingva,
    [Description("微信")] WeChat,
    [Description("Claude")] Claude,
    [Description("欧陆词典")] EuDict,
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
    Translator,
    Replace,
    OCR,
    TTS,
    VocabularyBook,
    Favorite,
    History,
    Backup,
    About,
    Service
}

/// <summary>
///     主题类型
/// </summary>
public enum ThemeType
{
    [Description("明亮主题")] Light,
    [Description("黑暗主题")] Dark,
    [Description("跟随系统")] FollowSystem,
    [Description("跟随应用")] FollowApp,
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
///     <see href="https://learn.microsoft.com/zh-cn/azure/ai-services/speech-service/language-support?tabs=tts#multilingual-voices"/>
/// </summary>
public enum AzureVoiceEnum
{
    [Description("晓彤(Xiaotong)-中文(吴语，简体)")] wuu_CN_XiaotongNeural,
    [Description("云哲(Yunzhe)-中文(吴语，简体)")] wuu_CN_YunzheNeural,
    [Description("晓敏(XiaoMin)-中文(粤语，简体)")] yue_CN_XiaoMinNeural,
    [Description("云松(YunSong)-中文(粤语，简体)")] yue_CN_YunSongNeural,
    [Description("晓晓(Xiaoxiao)-中文(普通话，简体)")] zh_CN_XiaoxiaoNeural,
    [Description("云希(Yunxi)-中文(普通话，简体)")] zh_CN_YunxiNeural,
    [Description("云健(Yunjian)-中文(普通话，简体)")] zh_CN_YunjianNeural,
    [Description("晓伊(Xiaoyi)-中文(普通话，简体)")] zh_CN_XiaoyiNeural,
    [Description("云扬(Yunyang)-中文(普通话，简体)")] zh_CN_YunyangNeural,
    [Description("晓辰(Xiaochen)-中文(普通话，简体)")] zh_CN_XiaochenNeural,
    [Description("晓辰 多语言(Xiaochen Multilingual)-中文(普通话，简体)")] zh_CN_XiaochenMultilingualNeural,
    [Description("晓涵(Xiaohan)-中文(普通话，简体)")] zh_CN_XiaohanNeural,
    [Description("晓梦(Xiaomeng)-中文(普通话，简体)")] zh_CN_XiaomengNeural,
    [Description("晓墨(Xiaomo)-中文(普通话，简体)")] zh_CN_XiaomoNeural,
    [Description("晓秋(Xiaoqiu)-中文(普通话，简体)")] zh_CN_XiaoqiuNeural,
    [Description("晓柔(Xiaorou)-中文(普通话，简体)")] zh_CN_XiaorouNeural,
    [Description("晓睿(Xiaorui)-中文(普通话，简体)")] zh_CN_XiaoruiNeural,
    [Description("晓双(Xiaoshuang)-中文(普通话，简体)")] zh_CN_XiaoshuangNeural,
    [Description("晓晓 方言(Xiaoxiao Dialects)-中文(普通话，简体)")] zh_CN_XiaoxiaoDialectsNeural,
    [Description("晓晓 多语言(Xiaoxiao Multilingual)-中文(普通话，简体)")] zh_CN_XiaoxiaoMultilingualNeural,
    [Description("晓颜(Xiaoyan)-中文(普通话，简体)")] zh_CN_XiaoyanNeural,
    [Description("晓悠(Xiaoyou)-中文(普通话，简体)")] zh_CN_XiaoyouNeural,
    [Description("晓宇 多语言(Xiaoyu Multilingual)-中文(普通话，简体)")] zh_CN_XiaoyuMultilingualNeural,
    [Description("晓甄(Xiaozhen)-中文(普通话，简体)")] zh_CN_XiaozhenNeural,
    [Description("云枫(Yunfeng)-中文(普通话，简体)")] zh_CN_YunfengNeural,
    [Description("云皓(Yunhao)-中文(普通话，简体)")] zh_CN_YunhaoNeural,
    [Description("云杰(Yunjie)-中文(普通话，简体)")] zh_CN_YunjieNeural,
    [Description("云夏(Yunxia)-中文(普通话，简体)")] zh_CN_YunxiaNeural,
    [Description("云野(Yunye)-中文(普通话，简体)")] zh_CN_YunyeNeural,
    [Description("云逸 多语言(Yunyi Multilingual)-中文(普通话，简体)")] zh_CN_YunyiMultilingualNeural,
    [Description("云泽(Yunze)-中文(普通话，简体)")] zh_CN_YunzeNeural,
    [Description("云帆 多语言(Yunfan Multilingual)-中文(普通话，简体)")] zh_CN_YunfanMultilingualNeural,
    [Description("云晓 多语言(Yunxiao Multilingual)-中文(普通话，简体)")] zh_CN_YunxiaoMultilingualNeural,
    [Description("云奇 广西(Yunqi)-中文(广西方言，简体)")] zh_CN_guangxi_YunqiNeural,
    [Description("云登(Yundeng)-中文(中原官话，河南，简体)")] zh_CN_henan_YundengNeural,
    [Description("晓北 辽宁(Xiaobei)-中文(东北官话，简体)")] zh_CN_liaoning_XiaobeiNeural,
    [Description("云彪 辽宁(Yunbiao)-中文(东北官话，简体)")] zh_CN_liaoning_YunbiaoNeural,
    [Description("晓妮(Xiaoni)-中文(中原官话，陕西，简体)")] zh_CN_shaanxi_XiaoniNeural,
    [Description("云翔(Yunxiang)-中文(吉鲁官话，简体)")] zh_CN_shandong_YunxiangNeural,
    [Description("云希 四川(Yunxi)-中文(西南官话，简体)")] zh_CN_sichuan_YunxiNeural,
    [Description("曉曼(HiuMaan)-中文(粤语，繁体)")] zh_HK_HiuMaanNeural,
    [Description("雲龍(WanLung)-中文(粤语，繁体)")] zh_HK_WanLungNeural,
    [Description("曉佳(HiuGaai)-中文(粤语，繁体)")] zh_HK_HiuGaaiNeural,
    [Description("曉臻(HsiaoChen)-中文(台湾官话，繁体)")] zh_TW_HsiaoChenNeural,
    [Description("雲哲(YunJhe)-中文(台湾官话，繁体)")] zh_TW_YunJheNeural,
    [Description("曉雨(HsiaoYu)-中文(台湾官话，繁体)")] zh_TW_HsiaoYuNeural,
    [Description("Natasha(Natasha)-英语(澳大利亚)")] en_AU_NatashaNeural,
    [Description("William(William)-英语(澳大利亚)")] en_AU_WilliamNeural,
    [Description("Annette(Annette)-英语(澳大利亚)")] en_AU_AnnetteNeural,
    [Description("Carly(Carly)-英语(澳大利亚)")] en_AU_CarlyNeural,
    [Description("Darren(Darren)-英语(澳大利亚)")] en_AU_DarrenNeural,
    [Description("Duncan(Duncan)-英语(澳大利亚)")] en_AU_DuncanNeural,
    [Description("Elsie(Elsie)-英语(澳大利亚)")] en_AU_ElsieNeural,
    [Description("Freya(Freya)-英语(澳大利亚)")] en_AU_FreyaNeural,
    [Description("Joanne(Joanne)-英语(澳大利亚)")] en_AU_JoanneNeural,
    [Description("Ken(Ken)-英语(澳大利亚)")] en_AU_KenNeural,
    [Description("Kim(Kim)-英语(澳大利亚)")] en_AU_KimNeural,
    [Description("Neil(Neil)-英语(澳大利亚)")] en_AU_NeilNeural,
    [Description("Tim(Tim)-英语(澳大利亚)")] en_AU_TimNeural,
    [Description("Tina(Tina)-英语(澳大利亚)")] en_AU_TinaNeural,
    [Description("Clara(Clara)-英语(加拿大)")] en_CA_ClaraNeural,
    [Description("Liam(Liam)-英语(加拿大)")] en_CA_LiamNeural,
    [Description("Sonia(Sonia)-英语(英国)")] en_GB_SoniaNeural,
    [Description("Ryan(Ryan)-英语(英国)")] en_GB_RyanNeural,
    [Description("Libby(Libby)-英语(英国)")] en_GB_LibbyNeural,
    [Description("Abbi(Abbi)-英语(英国)")] en_GB_AbbiNeural,
    [Description("Alfie(Alfie)-英语(英国)")] en_GB_AlfieNeural,
    [Description("Bella(Bella)-英语(英国)")] en_GB_BellaNeural,
    [Description("Elliot(Elliot)-英语(英国)")] en_GB_ElliotNeural,
    [Description("Ethan(Ethan)-英语(英国)")] en_GB_EthanNeural,
    [Description("Hollie(Hollie)-英语(英国)")] en_GB_HollieNeural,
    [Description("Maisie(Maisie)-英语(英国)")] en_GB_MaisieNeural,
    [Description("Noah(Noah)-英语(英国)")] en_GB_NoahNeural,
    [Description("Oliver(Oliver)-英语(英国)")] en_GB_OliverNeural,
    [Description("Olivia(Olivia)-英语(英国)")] en_GB_OliviaNeural,
    [Description("Thomas(Thomas)-英语(英国)")] en_GB_ThomasNeural,
    [Description("Ada Multilingual(Ada Multilingual)-英语(英国)")] en_GB_AdaMultilingualNeural,
    [Description("Ollie Multilingual(Ollie Multilingual)-英语(英国)")] en_GB_OllieMultilingualNeural,
    [Description("Mia(Mia)-英语(英国)")] en_GB_MiaNeural,
    [Description("Yan(Yan)-英语(香港特别行政区)")] en_HK_YanNeural,
    [Description("Sam(Sam)-英语(香港特别行政区)")] en_HK_SamNeural,
    [Description("Emily(Emily)-英语(爱尔兰)")] en_IE_EmilyNeural,
    [Description("Connor(Connor)-英语(爱尔兰)")] en_IE_ConnorNeural,
    [Description("Neerja(Neerja)-英语(印度)")] en_IN_NeerjaNeural,
    [Description("Prabhat(Prabhat)-英语(印度)")] en_IN_PrabhatNeural,
    [Description("Aarav(Aarav)-英语(印度)")] en_IN_AaravNeural,
    [Description("Aashi(Aashi)-英语(印度)")] en_IN_AashiNeural,
    [Description("Ananya(Ananya)-英语(印度)")] en_IN_AnanyaNeural,
    [Description("Kavya(Kavya)-英语(印度)")] en_IN_KavyaNeural,
    [Description("Kunal(Kunal)-英语(印度)")] en_IN_KunalNeural,
    [Description("Rehaan(Rehaan)-英语(印度)")] en_IN_RehaanNeural,
    [Description("Asilia(Asilia)-英语(肯尼亚)")] en_KE_AsiliaNeural,
    [Description("Chilemba(Chilemba)-英语(肯尼亚)")] en_KE_ChilembaNeural,
    [Description("Ezinne(Ezinne)-英语(尼日利亚)")] en_NG_EzinneNeural,
    [Description("Abeo(Abeo)-英语(尼日利亚)")] en_NG_AbeoNeural,
    [Description("Molly(Molly)-英语(新西兰)")] en_NZ_MollyNeural,
    [Description("Mitchell(Mitchell)-英语(新西兰)")] en_NZ_MitchellNeural,
    [Description("Rosa(Rosa)-英语(菲律宾)")] en_PH_RosaNeural,
    [Description("James(James)-英语(菲律宾)")] en_PH_JamesNeural,
    [Description("Luna(Luna)-英语(新加坡)")] en_SG_LunaNeural,
    [Description("Wayne(Wayne)-英语(新加坡)")] en_SG_WayneNeural,
    [Description("Imani(Imani)-英语(坦桑尼亚)")] en_TZ_ImaniNeural,
    [Description("Elimu(Elimu)-英语(坦桑尼亚)")] en_TZ_ElimuNeural,
    [Description("Ava Multilingual(Ava Multilingual)-英语(美国)")] en_US_AvaMultilingualNeural,
    [Description("Andrew Multilingual(Andrew Multilingual)-英语(美国)")] en_US_AndrewMultilingualNeural,
    [Description("Emma Multilingual(Emma Multilingual)-英语(美国)")] en_US_EmmaMultilingualNeural,
    [Description("Brian Multilingual(Brian Multilingual)-英语(美国)")] en_US_BrianMultilingualNeural,
    [Description("Ava(Ava)-英语(美国)")] en_US_AvaNeural,
    [Description("Andrew(Andrew)-英语(美国)")] en_US_AndrewNeural,
    [Description("Emma(Emma)-英语(美国)")] en_US_EmmaNeural,
    [Description("Brian(Brian)-英语(美国)")] en_US_BrianNeural,
    [Description("Jenny(Jenny)-英语(美国)")] en_US_JennyNeural,
    [Description("Guy(Guy)-英语(美国)")] en_US_GuyNeural,
    [Description("Aria(Aria)-英语(美国)")] en_US_AriaNeural,
    [Description("Davis(Davis)-英语(美国)")] en_US_DavisNeural,
    [Description("Jane(Jane)-英语(美国)")] en_US_JaneNeural,
    [Description("Jason(Jason)-英语(美国)")] en_US_JasonNeural,
    [Description("Sara(Sara)-英语(美国)")] en_US_SaraNeural,
    [Description("Tony(Tony)-英语(美国)")] en_US_TonyNeural,
    [Description("Nancy(Nancy)-英语(美国)")] en_US_NancyNeural,
    [Description("Amber(Amber)-英语(美国)")] en_US_AmberNeural,
    [Description("Ana(Ana)-英语(美国)")] en_US_AnaNeural,
    [Description("Ashley(Ashley)-英语(美国)")] en_US_AshleyNeural,
    [Description("Brandon(Brandon)-英语(美国)")] en_US_BrandonNeural,
    [Description("Christopher(Christopher)-英语(美国)")] en_US_ChristopherNeural,
    [Description("Cora(Cora)-英语(美国)")] en_US_CoraNeural,
    [Description("Elizabeth(Elizabeth)-英语(美国)")] en_US_ElizabethNeural,
    [Description("Eric(Eric)-英语(美国)")] en_US_EricNeural,
    [Description("Jacob(Jacob)-英语(美国)")] en_US_JacobNeural,
    [Description("Jenny Multilingual(Jenny Multilingual)-英语(美国)")] en_US_JennyMultilingualNeural,
    [Description("Michelle(Michelle)-英语(美国)")] en_US_MichelleNeural,
    [Description("Monica(Monica)-英语(美国)")] en_US_MonicaNeural,
    [Description("Roger(Roger)-英语(美国)")] en_US_RogerNeural,
    [Description("Ryan Multilingual(Ryan Multilingual)-英语(美国)")] en_US_RyanMultilingualNeural,
    [Description("Steffan(Steffan)-英语(美国)")] en_US_SteffanNeural,
    [Description("Adam Multilingual(Adam Multilingual)-英语(美国)")] en_US_AdamMultilingualNeural,
    [Description("AIGenerate1(AIGenerate1)-英语(美国)")] en_US_AIGenerate1Neural,
    [Description("AIGenerate2(AIGenerate2)-英语(美国)")] en_US_AIGenerate2Neural,
    [Description("AlloyTurboMultilingual(AlloyTurboMultilingual)-英语(美国)")] en_US_AlloyTurboMultilingualNeural,
    [Description("Amanda Multilingual(Amanda Multilingual)-英语(美国)")] en_US_AmandaMultilingualNeural,
    [Description("Blue(Blue)-英语(美国)")] en_US_BlueNeural,
    [Description("Davis Multilingual(Davis Multilingual)-英语(美国)")] en_US_DavisMultilingualNeural,
    [Description("Derek Multilingual(Derek Multilingual)-英语(美国)")] en_US_DerekMultilingualNeural,
    [Description("Dustin Multilingual(Dustin Multilingual)-英语(美国)")] en_US_DustinMultilingualNeural,
    [Description("Evelyn Multilingual(Evelyn Multilingual)-英语(美国)")] en_US_EvelynMultilingualNeural,
    [Description("Kai(Kai)-英语(美国)")] en_US_KaiNeural,
    [Description("Lewis Multilingual(Lewis Multilingual)-英语(美国)")] en_US_LewisMultilingualNeural,
    [Description("Lola Multilingual(Lola Multilingual)-英语(美国)")] en_US_LolaMultilingualNeural,
    [Description("Luna(Luna)-英语(美国)")] en_US_LunaNeural,
    [Description("Nancy Multilingual(Nancy Multilingual)-英语(美国)")] en_US_NancyMultilingualNeural,
    [Description("NovaTurboMultilingual(NovaTurboMultilingual)-英语(美国)")] en_US_NovaTurboMultilingualNeural,
    [Description("Phoebe Multilingual(Phoebe Multilingual)-英语(美国)")] en_US_PhoebeMultilingualNeural,
    [Description("Samuel Multilingual(Samuel Multilingual)-英语(美国)")] en_US_SamuelMultilingualNeural,
    [Description("Serena Multilingual(Serena Multilingual)-英语(美国)")] en_US_SerenaMultilingualNeural,
    [Description("Steffan Multilingual(Steffan Multilingual)-英语(美国)")] en_US_SteffanMultilingualNeural,
    [Description("Leah(Leah)-英语(南非)")] en_ZA_LeahNeural,
    [Description("Luke(Luke)-英语(南非)")] en_ZA_LukeNeural,
    [Description("Adri(Adri)-南非语(南非)")] af_ZA_AdriNeural,
    [Description("Willem(Willem)-南非语(南非)")] af_ZA_WillemNeural,
    [Description("መቅደስ(Mekdes)-阿姆哈拉语(埃塞俄比亚)")] am_ET_MekdesNeural,
    [Description("አምሀ(Ameha)-阿姆哈拉语(埃塞俄比亚)")] am_ET_AmehaNeural,
    [Description("فاطمة(Fatima)-阿拉伯语(阿拉伯联合酋长国)")] ar_AE_FatimaNeural,
    [Description("حمدان(Hamdan)-阿拉伯语(阿拉伯联合酋长国)")] ar_AE_HamdanNeural,
    [Description("ليلى(Laila)-阿拉伯语(巴林)")] ar_BH_LailaNeural,
    [Description("علي(Ali)-阿拉伯语(巴林)")] ar_BH_AliNeural,
    [Description("أمينة(Amina)-阿拉伯语(阿尔及利亚)")] ar_DZ_AminaNeural,
    [Description("إسماعيل(Ismael)-阿拉伯语(阿尔及利亚)")] ar_DZ_IsmaelNeural,
    [Description("سلمى(Salma)-阿拉伯语(埃及)")] ar_EG_SalmaNeural,
    [Description("شاكر(Shakir)-阿拉伯语(埃及)")] ar_EG_ShakirNeural,
    [Description("رنا(Rana)-阿拉伯语(伊拉克)")] ar_IQ_RanaNeural,
    [Description("باسل(Bassel)-阿拉伯语(伊拉克)")] ar_IQ_BasselNeural,
    [Description("سناء(Sana)-阿拉伯语(约旦)")] ar_JO_SanaNeural,
    [Description("تيم(Taim)-阿拉伯语(约旦)")] ar_JO_TaimNeural,
    [Description("نورا(Noura)-阿拉伯语(科威特)")] ar_KW_NouraNeural,
    [Description("فهد(Fahed)-阿拉伯语(科威特)")] ar_KW_FahedNeural,
    [Description("ليلى(Layla)-阿拉伯语(黎巴嫩)")] ar_LB_LaylaNeural,
    [Description("رامي(Rami)-阿拉伯语(黎巴嫩)")] ar_LB_RamiNeural,
    [Description("إيمان(Iman)-阿拉伯语(利比亚)")] ar_LY_ImanNeural,
    [Description("أحمد(Omar)-阿拉伯语(利比亚)")] ar_LY_OmarNeural,
    [Description("منى(Mouna)-阿拉伯语(摩洛哥)")] ar_MA_MounaNeural,
    [Description("جمال(Jamal)-阿拉伯语(摩洛哥)")] ar_MA_JamalNeural,
    [Description("عائشة(Aysha)-阿拉伯语(阿曼)")] ar_OM_AyshaNeural,
    [Description("عبدالله(Abdullah)-阿拉伯语(阿曼)")] ar_OM_AbdullahNeural,
    [Description("أمل(Amal)-阿拉伯语(卡塔尔)")] ar_QA_AmalNeural,
    [Description("معاذ(Moaz)-阿拉伯语(卡塔尔)")] ar_QA_MoazNeural,
    [Description("زارية(Zariyah)-阿拉伯语(沙特阿拉伯)")] ar_SA_ZariyahNeural,
    [Description("حامد(Hamed)-阿拉伯语(沙特阿拉伯)")] ar_SA_HamedNeural,
    [Description("أماني(Amany)-阿拉伯语(叙利亚)")] ar_SY_AmanyNeural,
    [Description("ليث(Laith)-阿拉伯语(叙利亚)")] ar_SY_LaithNeural,
    [Description("ريم(Reem)-阿拉伯语(突尼斯)")] ar_TN_ReemNeural,
    [Description("هادي(Hedi)-阿拉伯语(突尼斯)")] ar_TN_HediNeural,
    [Description("مريم(Maryam)-阿拉伯语(也门)")] ar_YE_MaryamNeural,
    [Description("صالح(Saleh)-阿拉伯语(也门)")] ar_YE_SalehNeural,
    [Description("প্ৰিয়ম(Priyom)-阿萨姆语(印度)")] as_IN_PriyomNeural,
    [Description("যাশিকা(Yashica)-阿萨姆语(印度)")] as_IN_YashicaNeural,
    [Description("Banu(Banu)-阿塞拜疆语(拉丁语，阿塞拜疆)")] az_AZ_BanuNeural,
    [Description("Babək(Babek)-阿塞拜疆语(拉丁语，阿塞拜疆)")] az_AZ_BabekNeural,
    [Description("Калина(Kalina)-保加利亚语(保加利亚)")] bg_BG_KalinaNeural,
    [Description("Борислав(Borislav)-保加利亚语(保加利亚)")] bg_BG_BorislavNeural,
    [Description("নবনীতা(Nabanita)-孟加拉语(孟加拉国)")] bn_BD_NabanitaNeural,
    [Description("প্রদ্বীপ(Pradeep)-孟加拉语(孟加拉国)")] bn_BD_PradeepNeural,
    [Description("তানিশা(Tanishaa)-孟加拉语(印度)")] bn_IN_TanishaaNeural,
    [Description("ভাস্কর(Bashkar)-孟加拉语(印度)")] bn_IN_BashkarNeural,
    [Description("Vesna(Vesna)-波斯尼亚语(波斯尼亚和黑塞哥维那)")] bs_BA_VesnaNeural,
    [Description("Goran(Goran)-波斯尼亚语(波斯尼亚和黑塞哥维那)")] bs_BA_GoranNeural,
    [Description("Joana(Joana)-加泰罗尼亚语(西班牙)")] ca_ES_JoanaNeural,
    [Description("Enric(Enric)-加泰罗尼亚语(西班牙)")] ca_ES_EnricNeural,
    [Description("Alba(Alba)-加泰罗尼亚语(西班牙)")] ca_ES_AlbaNeural,
    [Description("Vlasta(Vlasta)-捷克语(捷克语)")] cs_CZ_VlastaNeural,
    [Description("Antonín(Antonin)-捷克语(捷克语)")] cs_CZ_AntoninNeural,
    [Description("Nia(Nia)-威尔士语(英国)")] cy_GB_NiaNeural,
    [Description("Aled(Aled)-威尔士语(英国)")] cy_GB_AledNeural,
    [Description("Christel(Christel)-丹麦语(丹麦)")] da_DK_ChristelNeural,
    [Description("Jeppe(Jeppe)-丹麦语(丹麦)")] da_DK_JeppeNeural,
    [Description("Ingrid(Ingrid)-德语(奥地利)")] de_AT_IngridNeural,
    [Description("Jonas(Jonas)-德语(奥地利)")] de_AT_JonasNeural,
    [Description("Leni(Leni)-德语(瑞士)")] de_CH_LeniNeural,
    [Description("Jan(Jan)-德语(瑞士)")] de_CH_JanNeural,
    [Description("Katja(Katja)-德语(德国)")] de_DE_KatjaNeural,
    [Description("Conrad(Conrad)-德语(德国)")] de_DE_ConradNeural,
    [Description("Amala(Amala)-德语(德国)")] de_DE_AmalaNeural,
    [Description("Bernd(Bernd)-德语(德国)")] de_DE_BerndNeural,
    [Description("Christoph(Christoph)-德语(德国)")] de_DE_ChristophNeural,
    [Description("Elke(Elke)-德语(德国)")] de_DE_ElkeNeural,
    [Description("Florian Mehrsprachig(Florian Multilingual)-德语(德国)")] de_DE_FlorianMultilingualNeural,
    [Description("Gisela(Gisela)-德语(德国)")] de_DE_GiselaNeural,
    [Description("Kasper(Kasper)-德语(德国)")] de_DE_KasperNeural,
    [Description("Killian(Killian)-德语(德国)")] de_DE_KillianNeural,
    [Description("Klarissa(Klarissa)-德语(德国)")] de_DE_KlarissaNeural,
    [Description("Klaus(Klaus)-德语(德国)")] de_DE_KlausNeural,
    [Description("Louisa(Louisa)-德语(德国)")] de_DE_LouisaNeural,
    [Description("Maja(Maja)-德语(德国)")] de_DE_MajaNeural,
    [Description("Ralf(Ralf)-德语(德国)")] de_DE_RalfNeural,
    [Description("Seraphina Mehrsprachig(Seraphina Multilingual)-德语(德国)")] de_DE_SeraphinaMultilingualNeural,
    [Description("Tanja(Tanja)-德语(德国)")] de_DE_TanjaNeural,
    [Description("Αθηνά(Athina)-希腊语(希腊)")] el_GR_AthinaNeural,
    [Description("Νέστορας(Nestoras)-希腊语(希腊)")] el_GR_NestorasNeural,
    [Description("Elena(Elena)-西班牙语(阿根廷)")] es_AR_ElenaNeural,
    [Description("Tomas(Tomas)-西班牙语(阿根廷)")] es_AR_TomasNeural,
    [Description("Sofia(Sofia)-西班牙语(玻利维亚)")] es_BO_SofiaNeural,
    [Description("Marcelo(Marcelo)-西班牙语(玻利维亚)")] es_BO_MarceloNeural,
    [Description("Catalina(Catalina)-西班牙语(智利)")] es_CL_CatalinaNeural,
    [Description("Lorenzo(Lorenzo)-西班牙语(智利)")] es_CL_LorenzoNeural,
    [Description("Salome(Salome)-西班牙语(哥伦比亚)")] es_CO_SalomeNeural,
    [Description("Gonzalo(Gonzalo)-西班牙语(哥伦比亚)")] es_CO_GonzaloNeural,
    [Description("María(Maria)-西班牙语(哥斯达黎加)")] es_CR_MariaNeural,
    [Description("Juan(Juan)-西班牙语(哥斯达黎加)")] es_CR_JuanNeural,
    [Description("Belkys(Belkys)-西班牙语(古巴)")] es_CU_BelkysNeural,
    [Description("Manuel(Manuel)-西班牙语(古巴)")] es_CU_ManuelNeural,
    [Description("Ramona(Ramona)-西班牙语(多米尼加共和国)")] es_DO_RamonaNeural,
    [Description("Emilio(Emilio)-西班牙语(多米尼加共和国)")] es_DO_EmilioNeural,
    [Description("Andrea(Andrea)-西班牙语(厄瓜多尔)")] es_EC_AndreaNeural,
    [Description("Luis(Luis)-西班牙语(厄瓜多尔)")] es_EC_LuisNeural,
    [Description("Elvira(Elvira)-西班牙语(西班牙)")] es_ES_ElviraNeural,
    [Description("Álvaro(Alvaro)-西班牙语(西班牙)")] es_ES_AlvaroNeural,
    [Description("Abril(Abril)-西班牙语(西班牙)")] es_ES_AbrilNeural,
    [Description("Arnau(Arnau)-西班牙语(西班牙)")] es_ES_ArnauNeural,
    [Description("Dario(Dario)-西班牙语(西班牙)")] es_ES_DarioNeural,
    [Description("Elias(Elias)-西班牙语(西班牙)")] es_ES_EliasNeural,
    [Description("Estrella(Estrella)-西班牙语(西班牙)")] es_ES_EstrellaNeural,
    [Description("Irene(Irene)-西班牙语(西班牙)")] es_ES_IreneNeural,
    [Description("Laia(Laia)-西班牙语(西班牙)")] es_ES_LaiaNeural,
    [Description("Lia(Lia)-西班牙语(西班牙)")] es_ES_LiaNeural,
    [Description("Nil(Nil)-西班牙语(西班牙)")] es_ES_NilNeural,
    [Description("Saul(Saul)-西班牙语(西班牙)")] es_ES_SaulNeural,
    [Description("Teo(Teo)-西班牙语(西班牙)")] es_ES_TeoNeural,
    [Description("Triana(Triana)-西班牙语(西班牙)")] es_ES_TrianaNeural,
    [Description("Vera(Vera)-西班牙语(西班牙)")] es_ES_VeraNeural,
    [Description("Ximena(Ximena)-西班牙语(西班牙)")] es_ES_XimenaNeural,
    [Description("Arabella Multilingual(Arabella Multilingual)-西班牙语(西班牙)")] es_ES_ArabellaMultilingualNeural,
    [Description("Isidora Multilingual(Isidora Multilingual)-西班牙语(西班牙)")] es_ES_IsidoraMultilingualNeural,
    [Description("Tristan Multilingual(Tristan Multilingual)-西班牙语(西班牙)")] es_ES_TristanMultilingualNeural,
    [Description("Ximena Multilingual(Ximena Multilingual)-西班牙语(西班牙)")] es_ES_XimenaMultilingualNeural,
    [Description("Teresa(Teresa)-西班牙语(赤道几内亚)")] es_GQ_TeresaNeural,
    [Description("Javier(Javier)-西班牙语(赤道几内亚)")] es_GQ_JavierNeural,
    [Description("Marta(Marta)-西班牙语(危地马拉)")] es_GT_MartaNeural,
    [Description("Andrés(Andres)-西班牙语(危地马拉)")] es_GT_AndresNeural,
    [Description("Karla(Karla)-西班牙语(洪都拉斯)")] es_HN_KarlaNeural,
    [Description("Carlos(Carlos)-西班牙语(洪都拉斯)")] es_HN_CarlosNeural,
    [Description("Dalia(Dalia)-西班牙语(墨西哥)")] es_MX_DaliaNeural,
    [Description("Jorge(Jorge)-西班牙语(墨西哥)")] es_MX_JorgeNeural,
    [Description("Beatriz(Beatriz)-西班牙语(墨西哥)")] es_MX_BeatrizNeural,
    [Description("Candela(Candela)-西班牙语(墨西哥)")] es_MX_CandelaNeural,
    [Description("Carlota(Carlota)-西班牙语(墨西哥)")] es_MX_CarlotaNeural,
    [Description("Cecilio(Cecilio)-西班牙语(墨西哥)")] es_MX_CecilioNeural,
    [Description("Gerardo(Gerardo)-西班牙语(墨西哥)")] es_MX_GerardoNeural,
    [Description("Larissa(Larissa)-西班牙语(墨西哥)")] es_MX_LarissaNeural,
    [Description("Liberto(Liberto)-西班牙语(墨西哥)")] es_MX_LibertoNeural,
    [Description("Luciano(Luciano)-西班牙语(墨西哥)")] es_MX_LucianoNeural,
    [Description("Marina(Marina)-西班牙语(墨西哥)")] es_MX_MarinaNeural,
    [Description("Nuria(Nuria)-西班牙语(墨西哥)")] es_MX_NuriaNeural,
    [Description("Pelayo(Pelayo)-西班牙语(墨西哥)")] es_MX_PelayoNeural,
    [Description("Renata(Renata)-西班牙语(墨西哥)")] es_MX_RenataNeural,
    [Description("Yago(Yago)-西班牙语(墨西哥)")] es_MX_YagoNeural,
    [Description("Yolanda(Yolanda)-西班牙语(尼加拉瓜)")] es_NI_YolandaNeural,
    [Description("Federico(Federico)-西班牙语(尼加拉瓜)")] es_NI_FedericoNeural,
    [Description("Margarita(Margarita)-西班牙语(巴拿马)")] es_PA_MargaritaNeural,
    [Description("Roberto(Roberto)-西班牙语(巴拿马)")] es_PA_RobertoNeural,
    [Description("Camila(Camila)-西班牙语(秘鲁)")] es_PE_CamilaNeural,
    [Description("Alex(Alex)-西班牙语(秘鲁)")] es_PE_AlexNeural,
    [Description("Karina(Karina)-西班牙语(波多黎各)")] es_PR_KarinaNeural,
    [Description("Víctor(Victor)-西班牙语(波多黎各)")] es_PR_VictorNeural,
    [Description("Tania(Tania)-西班牙语(巴拉圭)")] es_PY_TaniaNeural,
    [Description("Mario(Mario)-西班牙语(巴拉圭)")] es_PY_MarioNeural,
    [Description("Lorena(Lorena)-西班牙语(萨尔瓦多)")] es_SV_LorenaNeural,
    [Description("Rodrigo(Rodrigo)-西班牙语(萨尔瓦多)")] es_SV_RodrigoNeural,
    [Description("Paloma(Paloma)-西班牙语(美国)")] es_US_PalomaNeural,
    [Description("Alonso(Alonso)-西班牙语(美国)")] es_US_AlonsoNeural,
    [Description("Valentina(Valentina)-西班牙语(乌拉圭)")] es_UY_ValentinaNeural,
    [Description("Mateo(Mateo)-西班牙语(乌拉圭)")] es_UY_MateoNeural,
    [Description("Paola(Paola)-西班牙语(委内瑞拉)")] es_VE_PaolaNeural,
    [Description("Sebastián(Sebastian)-西班牙语(委内瑞拉)")] es_VE_SebastianNeural,
    [Description("Anu(Anu)-爱沙尼亚语(爱沙尼亚)")] et_EE_AnuNeural,
    [Description("Kert(Kert)-爱沙尼亚语(爱沙尼亚)")] et_EE_KertNeural,
    [Description("Ainhoa(Ainhoa)-巴斯克语")] eu_ES_AinhoaNeural,
    [Description("Ander(Ander)-巴斯克语")] eu_ES_AnderNeural,
    [Description("دلارا(Dilara)-波斯语(伊朗)")] fa_IR_DilaraNeural,
    [Description("فرید(Farid)-波斯语(伊朗)")] fa_IR_FaridNeural,
    [Description("Selma(Selma)-芬兰语(芬兰)")] fi_FI_SelmaNeural,
    [Description("Harri(Harri)-芬兰语(芬兰)")] fi_FI_HarriNeural,
    [Description("Noora(Noora)-芬兰语(芬兰)")] fi_FI_NooraNeural,
    [Description("Blessica(Blessica)-菲律宾语(菲律宾)")] fil_PH_BlessicaNeural,
    [Description("Angelo(Angelo)-菲律宾语(菲律宾)")] fil_PH_AngeloNeural,
    [Description("Charline(Charline)-法语(比利时)")] fr_BE_CharlineNeural,
    [Description("Gerard(Gerard)-法语(比利时)")] fr_BE_GerardNeural,
    [Description("Sylvie(Sylvie)-法语(加拿大)")] fr_CA_SylvieNeural,
    [Description("Jean(Jean)-法语(加拿大)")] fr_CA_JeanNeural,
    [Description("Antoine(Antoine)-法语(加拿大)")] fr_CA_AntoineNeural,
    [Description("Thierry(Thierry)-法语(加拿大)")] fr_CA_ThierryNeural,
    [Description("Ariane(Ariane)-法语(瑞士)")] fr_CH_ArianeNeural,
    [Description("Fabrice(Fabrice)-法语(瑞士)")] fr_CH_FabriceNeural,
    [Description("Denise(Denise)-法语(法国)")] fr_FR_DeniseNeural,
    [Description("Henri(Henri)-法语(法国)")] fr_FR_HenriNeural,
    [Description("Alain(Alain)-法语(法国)")] fr_FR_AlainNeural,
    [Description("Brigitte(Brigitte)-法语(法国)")] fr_FR_BrigitteNeural,
    [Description("Celeste(Celeste)-法语(法国)")] fr_FR_CelesteNeural,
    [Description("Claude(Claude)-法语(法国)")] fr_FR_ClaudeNeural,
    [Description("Coralie(Coralie)-法语(法国)")] fr_FR_CoralieNeural,
    [Description("Eloise(Eloise)-法语(法国)")] fr_FR_EloiseNeural,
    [Description("Jacqueline(Jacqueline)-法语(法国)")] fr_FR_JacquelineNeural,
    [Description("Jerome(Jerome)-法语(法国)")] fr_FR_JeromeNeural,
    [Description("Josephine(Josephine)-法语(法国)")] fr_FR_JosephineNeural,
    [Description("Maurice(Maurice)-法语(法国)")] fr_FR_MauriceNeural,
    [Description("Rémy Multilingue(Remy Multilingual)-法语(法国)")] fr_FR_RemyMultilingualNeural,
    [Description("Vivienne Multilingue(Vivienne Multilingual)-法语(法国)")] fr_FR_VivienneMultilingualNeural,
    [Description("Yves(Yves)-法语(法国)")] fr_FR_YvesNeural,
    [Description("Yvette(Yvette)-法语(法国)")] fr_FR_YvetteNeural,
    [Description("Lucien Multilingual(Lucien Multilingual)-法语(法国)")] fr_FR_LucienMultilingualNeural,
    [Description("Orla(Orla)-爱尔兰语(爱尔兰)")] ga_IE_OrlaNeural,
    [Description("Colm(Colm)-爱尔兰语(爱尔兰)")] ga_IE_ColmNeural,
    [Description("Sabela(Sabela)-加利西亚语")] gl_ES_SabelaNeural,
    [Description("Roi(Roi)-加利西亚语")] gl_ES_RoiNeural,
    [Description("ધ્વની(Dhwani)-古吉拉特语(印度)")] gu_IN_DhwaniNeural,
    [Description("નિરંજન(Niranjan)-古吉拉特语(印度)")] gu_IN_NiranjanNeural,
    [Description("הילה(Hila)-希伯来语(以色列)")] he_IL_HilaNeural,
    [Description("אברי(Avri)-希伯来语(以色列)")] he_IL_AvriNeural,
    [Description("स्वरा(Swara)-印地语(印度)")] hi_IN_SwaraNeural,
    [Description("मधुर(Madhur)-印地语(印度)")] hi_IN_MadhurNeural,
    [Description("आरव (Aarav)-印地语(印度)")] hi_IN_AaravNeural,
    [Description("अनन्या(Ananya)-印地语(印度)")] hi_IN_AnanyaNeural,
    [Description("काव्या(Kavya)-印地语(印度)")] hi_IN_KavyaNeural,
    [Description("कुनाल (Kunal)-印地语(印度)")] hi_IN_KunalNeural,
    [Description("रेहान(Rehaan)-印地语(印度)")] hi_IN_RehaanNeural,
    [Description("Gabrijela(Gabrijela)-克罗地亚语(克罗地亚)")] hr_HR_GabrijelaNeural,
    [Description("Srećko(Srecko)-克罗地亚语(克罗地亚)")] hr_HR_SreckoNeural,
    [Description("Noémi(Noemi)-匈牙利语(匈牙利)")] hu_HU_NoemiNeural,
    [Description("Tamás(Tamas)-匈牙利语(匈牙利)")] hu_HU_TamasNeural,
    [Description("Անահիտ(Anahit)-亚美尼亚语(亚美尼亚)")] hy_AM_AnahitNeural,
    [Description("Հայկ(Hayk)-亚美尼亚语(亚美尼亚)")] hy_AM_HaykNeural,
    [Description("Gadis(Gadis)-印度尼西亚语(印度尼西亚)")] id_ID_GadisNeural,
    [Description("Ardi(Ardi)-印度尼西亚语(印度尼西亚)")] id_ID_ArdiNeural,
    [Description("Guðrún(Gudrun)-冰岛语(冰岛)")] is_IS_GudrunNeural,
    [Description("Gunnar(Gunnar)-冰岛语(冰岛)")] is_IS_GunnarNeural,
    [Description("Elsa(Elsa)-意大利语(意大利)")] it_IT_ElsaNeural,
    [Description("Isabella(Isabella)-意大利语(意大利)")] it_IT_IsabellaNeural,
    [Description("Diego(Diego)-意大利语(意大利)")] it_IT_DiegoNeural,
    [Description("Benigno(Benigno)-意大利语(意大利)")] it_IT_BenignoNeural,
    [Description("Calimero(Calimero)-意大利语(意大利)")] it_IT_CalimeroNeural,
    [Description("Cataldo(Cataldo)-意大利语(意大利)")] it_IT_CataldoNeural,
    [Description("Fabiola(Fabiola)-意大利语(意大利)")] it_IT_FabiolaNeural,
    [Description("Fiamma(Fiamma)-意大利语(意大利)")] it_IT_FiammaNeural,
    [Description("Gianni(Gianni)-意大利语(意大利)")] it_IT_GianniNeural,
    [Description("Giuseppe(Giuseppe)-意大利语(意大利)")] it_IT_GiuseppeNeural,
    [Description("Imelda(Imelda)-意大利语(意大利)")] it_IT_ImeldaNeural,
    [Description("Irma(Irma)-意大利语(意大利)")] it_IT_IrmaNeural,
    [Description("Lisandro(Lisandro)-意大利语(意大利)")] it_IT_LisandroNeural,
    [Description("Palmira(Palmira)-意大利语(意大利)")] it_IT_PalmiraNeural,
    [Description("Pierina(Pierina)-意大利语(意大利)")] it_IT_PierinaNeural,
    [Description("Rinaldo(Rinaldo)-意大利语(意大利)")] it_IT_RinaldoNeural,
    [Description("Alessio Multilingual(Alessio Multilingual)-意大利语(意大利)")] it_IT_AlessioMultilingualNeural,
    [Description("Giuseppe Multilingual(Giuseppe Multilingual)-意大利语(意大利)")] it_IT_GiuseppeMultilingualNeural,
    [Description("Isabella Multilingual(Isabella Multilingual)-意大利语(意大利)")] it_IT_IsabellaMultilingualNeural,
    [Description("Marcello Multilingual(Marcello Multilingual)-意大利语(意大利)")] it_IT_MarcelloMultilingualNeural,
    [Description("七海(Nanami)-日语(日本)")] ja_JP_NanamiNeural,
    [Description("圭太(Keita)-日语(日本)")] ja_JP_KeitaNeural,
    [Description("碧衣(Aoi)-日语(日本)")] ja_JP_AoiNeural,
    [Description("大智(Daichi)-日语(日本)")] ja_JP_DaichiNeural,
    [Description("真夕(Mayu)-日语(日本)")] ja_JP_MayuNeural,
    [Description("直紀(Naoki)-日语(日本)")] ja_JP_NaokiNeural,
    [Description("志織(Shiori)-日语(日本)")] ja_JP_ShioriNeural,
    [Description("勝 多言語(Masaru Multilingual)-日语(日本)")] ja_JP_MasaruMultilingualNeural,
    [Description("Siti(Siti)-爪哇语(拉丁语，印度尼西亚)")] jv_ID_SitiNeural,
    [Description("Dimas(Dimas)-爪哇语(拉丁语，印度尼西亚)")] jv_ID_DimasNeural,
    [Description("ეკა(Eka)-格鲁吉亚语(格鲁吉亚)")] ka_GE_EkaNeural,
    [Description("გიორგი(Giorgi)-格鲁吉亚语(格鲁吉亚)")] ka_GE_GiorgiNeural,
    [Description("Айгүл(Aigul)-哈萨克语(哈萨克斯坦)")] kk_KZ_AigulNeural,
    [Description("Дәулет(Daulet)-哈萨克语(哈萨克斯坦)")] kk_KZ_DauletNeural,
    [Description("ស្រីមុំ(Sreymom)-高棉语(柬埔寨)")] km_KH_SreymomNeural,
    [Description("ពិសិដ្ឋ(Piseth)-高棉语(柬埔寨)")] km_KH_PisethNeural,
    [Description("ಸಪ್ನಾ(Sapna)-卡纳达语(印度)")] kn_IN_SapnaNeural,
    [Description("ಗಗನ್(Gagan)-卡纳达语(印度)")] kn_IN_GaganNeural,
    [Description("선히(Sun-Hi)-韩语(韩国)")] ko_KR_SunHiNeural,
    [Description("인준(InJoon)-韩语(韩国)")] ko_KR_InJoonNeural,
    [Description("봉진(BongJin)-韩语(韩国)")] ko_KR_BongJinNeural,
    [Description("국민(GookMin)-韩语(韩国)")] ko_KR_GookMinNeural,
    [Description("현수(Hyunsu)-韩语(韩国)")] ko_KR_HyunsuNeural,
    [Description("지민(JiMin)-韩语(韩国)")] ko_KR_JiMinNeural,
    [Description("서현(SeoHyeon)-韩语(韩国)")] ko_KR_SeoHyeonNeural,
    [Description("순복(SoonBok)-韩语(韩国)")] ko_KR_SoonBokNeural,
    [Description("유진(YuJin)-韩语(韩国)")] ko_KR_YuJinNeural,
    [Description("Hyunsu Multilingual(Hyunsu Multilingual)-韩语(韩国)")] ko_KR_HyunsuMultilingualNeural,
    [Description("ແກ້ວມະນີ(Keomany)-老挝语(老挝)")] lo_LA_KeomanyNeural,
    [Description("ຈັນທະວົງ(Chanthavong)-老挝语(老挝)")] lo_LA_ChanthavongNeural,
    [Description("Ona(Ona)-立陶宛语(立陶宛)")] lt_LT_OnaNeural,
    [Description("Leonas(Leonas)-立陶宛语(立陶宛)")] lt_LT_LeonasNeural,
    [Description("Everita(Everita)-拉脱维亚语(拉脱维亚)")] lv_LV_EveritaNeural,
    [Description("Nils(Nils)-拉脱维亚语(拉脱维亚)")] lv_LV_NilsNeural,
    [Description("Марија(Marija)-马其顿语(北马其顿)")] mk_MK_MarijaNeural,
    [Description("Александар(Aleksandar)-马其顿语(北马其顿)")] mk_MK_AleksandarNeural,
    [Description("ശോഭന(Sobhana)-马拉雅拉姆语(印度)")] ml_IN_SobhanaNeural,
    [Description("മിഥുൻ(Midhun)-马拉雅拉姆语(印度)")] ml_IN_MidhunNeural,
    [Description("Есүй(Yesui)-蒙古语(蒙古)")] mn_MN_YesuiNeural,
    [Description("Батаа(Bataa)-蒙古语(蒙古)")] mn_MN_BataaNeural,
    [Description("आरोही(Aarohi)-马拉地语(印度)")] mr_IN_AarohiNeural,
    [Description("मनोहर(Manohar)-马拉地语(印度)")] mr_IN_ManoharNeural,
    [Description("Yasmin(Yasmin)-马来语(马来西亚)")] ms_MY_YasminNeural,
    [Description("Osman(Osman)-马来语(马来西亚)")] ms_MY_OsmanNeural,
    [Description("Grace(Grace)-马耳他语(马耳他)")] mt_MT_GraceNeural,
    [Description("Joseph(Joseph)-马耳他语(马耳他)")] mt_MT_JosephNeural,
    [Description("နီလာ(Nilar)-缅甸语(缅甸)")] my_MM_NilarNeural,
    [Description("သီဟ(Thiha)-缅甸语(缅甸)")] my_MM_ThihaNeural,
    [Description("Pernille(Pernille)-挪威博克马尔语(挪威)")] nb_NO_PernilleNeural,
    [Description("Finn(Finn)-挪威博克马尔语(挪威)")] nb_NO_FinnNeural,
    [Description("Iselin(Iselin)-挪威博克马尔语(挪威)")] nb_NO_IselinNeural,
    [Description("हेमकला(Hemkala)-尼泊尔语(尼泊尔)")] ne_NP_HemkalaNeural,
    [Description("सागर(Sagar)-尼泊尔语(尼泊尔)")] ne_NP_SagarNeural,
    [Description("Dena(Dena)-荷兰语(比利时)")] nl_BE_DenaNeural,
    [Description("Arnaud(Arnaud)-荷兰语(比利时)")] nl_BE_ArnaudNeural,
    [Description("Fenna(Fenna)-荷兰语(荷兰)")] nl_NL_FennaNeural,
    [Description("Maarten(Maarten)-荷兰语(荷兰)")] nl_NL_MaartenNeural,
    [Description("Colette(Colette)-荷兰语(荷兰)")] nl_NL_ColetteNeural,
    [Description("ସୁଭାସିନୀ(Subhasini)-奥里雅语(印度)")] or_IN_SubhasiniNeural,
    [Description("ସୁକାନ୍ତ(Sukant)-奥里雅语(印度)")] or_IN_SukantNeural,
    [Description("ਓਜਸ(Ojas)-旁遮普语(印度)")] pa_IN_OjasNeural,
    [Description("ਵਾਨੀ(Vaani)-旁遮普语(印度)")] pa_IN_VaaniNeural,
    [Description("Agnieszka(Agnieszka)-波兰语(波兰)")] pl_PL_AgnieszkaNeural,
    [Description("Marek(Marek)-波兰语(波兰)")] pl_PL_MarekNeural,
    [Description("Zofia(Zofia)-波兰语(波兰)")] pl_PL_ZofiaNeural,
    [Description("لطيفه(Latifa)-普什图语(阿富汗)")] ps_AF_LatifaNeural,
    [Description(" ګل نواز(Gul Nawaz)-普什图语(阿富汗)")] ps_AF_GulNawazNeural,
    [Description("Francisca(Francisca)-葡萄牙语(巴西)")] pt_BR_FranciscaNeural,
    [Description("Antônio(Antonio)-葡萄牙语(巴西)")] pt_BR_AntonioNeural,
    [Description("Brenda(Brenda)-葡萄牙语(巴西)")] pt_BR_BrendaNeural,
    [Description("Donato(Donato)-葡萄牙语(巴西)")] pt_BR_DonatoNeural,
    [Description("Elza(Elza)-葡萄牙语(巴西)")] pt_BR_ElzaNeural,
    [Description("Fabio(Fabio)-葡萄牙语(巴西)")] pt_BR_FabioNeural,
    [Description("Giovanna(Giovanna)-葡萄牙语(巴西)")] pt_BR_GiovannaNeural,
    [Description("Humberto(Humberto)-葡萄牙语(巴西)")] pt_BR_HumbertoNeural,
    [Description("Julio(Julio)-葡萄牙语(巴西)")] pt_BR_JulioNeural,
    [Description("Leila(Leila)-葡萄牙语(巴西)")] pt_BR_LeilaNeural,
    [Description("Leticia(Leticia)-葡萄牙语(巴西)")] pt_BR_LeticiaNeural,
    [Description("Manuela(Manuela)-葡萄牙语(巴西)")] pt_BR_ManuelaNeural,
    [Description("Nicolau(Nicolau)-葡萄牙语(巴西)")] pt_BR_NicolauNeural,
    [Description("Thalita(Thalita)-葡萄牙语(巴西)")] pt_BR_ThalitaNeural,
    [Description("Valerio(Valerio)-葡萄牙语(巴西)")] pt_BR_ValerioNeural,
    [Description("Yara(Yara)-葡萄牙语(巴西)")] pt_BR_YaraNeural,
    [Description("Macerio Multilingual(Macerio Multilingual)-葡萄牙语(巴西)")] pt_BR_MacerioMultilingualNeural,
    [Description("Thalita multilíngue(Thalita Multilingual)-葡萄牙语(巴西)")] pt_BR_ThalitaMultilingualNeural,
    [Description("Raquel(Raquel)-葡萄牙语(葡萄牙)")] pt_PT_RaquelNeural,
    [Description("Duarte(Duarte)-葡萄牙语(葡萄牙)")] pt_PT_DuarteNeural,
    [Description("Fernanda(Fernanda)-葡萄牙语(葡萄牙)")] pt_PT_FernandaNeural,
    [Description("Alina(Alina)-罗马尼亚语(罗马尼亚)")] ro_RO_AlinaNeural,
    [Description("Emil(Emil)-罗马尼亚语(罗马尼亚)")] ro_RO_EmilNeural,
    [Description("Светлана(Svetlana)-俄语(俄罗斯)")] ru_RU_SvetlanaNeural,
    [Description("Дмитрий(Dmitry)-俄语(俄罗斯)")] ru_RU_DmitryNeural,
    [Description("Дария(Dariya)-俄语(俄罗斯)")] ru_RU_DariyaNeural,
    [Description("තිළිණි(Thilini)-僧伽罗语(斯里兰卡)")] si_LK_ThiliniNeural,
    [Description("සමීර(Sameera)-僧伽罗语(斯里兰卡)")] si_LK_SameeraNeural,
    [Description("Viktória(Viktoria)-斯洛伐克语(斯洛伐克)")] sk_SK_ViktoriaNeural,
    [Description("Lukáš(Lukas)-斯洛伐克语(斯洛伐克)")] sk_SK_LukasNeural,
    [Description("Petra(Petra)-斯洛文尼亚语(斯洛文尼亚)")] sl_SI_PetraNeural,
    [Description("Rok(Rok)-斯洛文尼亚语(斯洛文尼亚)")] sl_SI_RokNeural,
    [Description("Ubax(Ubax)-索马里语(索马里)")] so_SO_UbaxNeural,
    [Description("Muuse(Muuse)-索马里语(索马里)")] so_SO_MuuseNeural,
    [Description("Anila(Anila)-阿尔巴尼亚语(阿尔巴尼亚)")] sq_AL_AnilaNeural,
    [Description("Ilir(Ilir)-阿尔巴尼亚语(阿尔巴尼亚)")] sq_AL_IlirNeural,
    [Description("Nicholas(Nicholas)-塞尔维亚语(拉丁字母，塞尔维亚)")] sr_Latn_RS_NicholasNeural,
    [Description("Sophie(Sophie)-塞尔维亚语(拉丁字母，塞尔维亚)")] sr_Latn_RS_SophieNeural,
    [Description("Софија(Sophie)-塞尔维亚语(西里尔字母，塞尔维亚)")] sr_RS_SophieNeural,
    [Description("Никола(Nicholas)-塞尔维亚语(西里尔字母，塞尔维亚)")] sr_RS_NicholasNeural,
    [Description("Tuti(Tuti)-巽他语(印度尼西亚)")] su_ID_TutiNeural,
    [Description("Jajang(Jajang)-巽他语(印度尼西亚)")] su_ID_JajangNeural,
    [Description("Sofie(Sofie)-瑞典语(瑞典)")] sv_SE_SofieNeural,
    [Description("Mattias(Mattias)-瑞典语(瑞典)")] sv_SE_MattiasNeural,
    [Description("Hillevi(Hillevi)-瑞典语(瑞典)")] sv_SE_HilleviNeural,
    [Description("Zuri(Zuri)-斯瓦希里语(肯尼亚)")] sw_KE_ZuriNeural,
    [Description("Rafiki(Rafiki)-斯瓦希里语(肯尼亚)")] sw_KE_RafikiNeural,
    [Description("Rehema(Rehema)-斯瓦希里语(坦桑尼亚)")] sw_TZ_RehemaNeural,
    [Description("Daudi(Daudi)-斯瓦希里语(坦桑尼亚)")] sw_TZ_DaudiNeural,
    [Description("பல்லவி(Pallavi)-泰米尔语(印度)")] ta_IN_PallaviNeural,
    [Description("வள்ளுவர்(Valluvar)-泰米尔语(印度)")] ta_IN_ValluvarNeural,
    [Description("சரண்யா(Saranya)-泰米尔语(斯里兰卡)")] ta_LK_SaranyaNeural,
    [Description("குமார்(Kumar)-泰米尔语(斯里兰卡)")] ta_LK_KumarNeural,
    [Description("கனி(Kani)-泰米尔语(马来西亚)")] ta_MY_KaniNeural,
    [Description("சூர்யா(Surya)-泰米尔语(马来西亚)")] ta_MY_SuryaNeural,
    [Description("வெண்பா(Venba)-泰米尔语(新加坡)")] ta_SG_VenbaNeural,
    [Description("அன்பு(Anbu)-泰米尔语(新加坡)")] ta_SG_AnbuNeural,
    [Description("శ్రుతి(Shruti)-泰卢固语(印度)")] te_IN_ShrutiNeural,
    [Description("మోహన్(Mohan)-泰卢固语(印度)")] te_IN_MohanNeural,
    [Description("เปรมวดี(Premwadee)-泰语(泰国)")] th_TH_PremwadeeNeural,
    [Description("นิวัฒน์(Niwat)-泰语(泰国)")] th_TH_NiwatNeural,
    [Description("อัจฉรา(Achara)-泰语(泰国)")] th_TH_AcharaNeural,
    [Description("Emel(Emel)-土耳其语(土耳其)")] tr_TR_EmelNeural,
    [Description("Ahmet(Ahmet)-土耳其语(土耳其)")] tr_TR_AhmetNeural,
    [Description("Поліна(Polina)-乌克兰语(乌克兰)")] uk_UA_PolinaNeural,
    [Description("Остап(Ostap)-乌克兰语(乌克兰)")] uk_UA_OstapNeural,
    [Description("گل(Gul)-乌尔都语(印度)")] ur_IN_GulNeural,
    [Description("سلمان(Salman)-乌尔都语(印度)")] ur_IN_SalmanNeural,
    [Description("عظمیٰ(Uzma)-乌尔都语(巴基斯坦)")] ur_PK_UzmaNeural,
    [Description("اسد(Asad)-乌尔都语(巴基斯坦)")] ur_PK_AsadNeural,
    [Description("Madina(Madina)-乌兹别克语(拉丁语，乌兹别克斯坦)")] uz_UZ_MadinaNeural,
    [Description("Sardor(Sardor)-乌兹别克语(拉丁语，乌兹别克斯坦)")] uz_UZ_SardorNeural,
    [Description("Hoài My(HoaiMy)-越南语(越南)")] vi_VN_HoaiMyNeural,
    [Description("Nam Minh(NamMinh)-越南语(越南)")] vi_VN_NamMinhNeural,
    [Description("Thando(Thando)-祖鲁语(南非)")] zu_ZA_ThandoNeural,
    [Description("Themba(Themba)-祖鲁语(南非)")] zu_ZA_ThembaNeural,
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
    SilentTtsHk,
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
    forbiddenhotkey,
    tts_silence
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
///     全局字体大小
/// </summary>
public enum GlobalFontSizeEnum
{
    [Description("特小(14px)")] ExtremelySmall = -4,
    [Description("超小(15px)")] UltraSmall,
    [Description("很小(16px)")] VerySmall,
    [Description("小(17px)")] Small,
    [Description("标准(18px)")] General = 0,
    [Description("大(19px)")] Big,
    [Description("很大(20px)")] VeryBig,
    [Description("超大(21px)")] UltraBig,
    [Description("特大(22px)")] ExtremelyBig,
}

/// <summary>
///     主界面动画速度
/// </summary>
public enum AnimationSpeedEnum
{
    [Description("较慢(300ms)")] Slow = 300,
    [Description("适中(200ms)")] Middle = 200,
    [Description("较快(150ms)")] Fast = 150,
}

/// <summary>
///     OCR 时图片质量
/// </summary>
public enum OcrImageQualityEnum
{
    [Description("低质量")] Low,
    [Description("中等质量")] Medium,
    [Description("高质量")] High
}

/// <summary>
///     Edge TTS 语音
///     <see href="https://github1s.com/Entity-Now/Edge_tts_sharp/blob/main/Edge_tts_sharp/Source/VoiceList.json"/>
/// </summary>
public enum EdgeVoiceEnum
{
    [Description("Adri - 南非荷兰语")] af_ZA8AdriNeural,
    [Description("Willem - 南非荷兰语")] af_ZA8WillemNeural,
    [Description("Anila - 阿尔巴尼亚语")] sq_AL8AnilaNeural,
    [Description("Ilir - 阿尔巴尼亚语")] sq_AL8IlirNeural,
    [Description("Ameha - 阿姆哈拉语")] am_ET8AmehaNeural,
    [Description("Mekdes - 阿姆哈拉语")] am_ET8MekdesNeural,
    [Description("Amina - 阿拉伯语(阿尔及利亚)")] ar_DZ8AminaNeural,
    [Description("Ismael - 阿拉伯语(阿尔及利亚)")] ar_DZ8IsmaelNeural,
    [Description("Ali - 阿拉伯语(巴林)")] ar_BH8AliNeural,
    [Description("Laila - 阿拉伯语(巴林)")] ar_BH8LailaNeural,
    [Description("Salma - 阿拉伯语(埃及)")] ar_EG8SalmaNeural,
    [Description("Shakir - 阿拉伯语(埃及)")] ar_EG8ShakirNeural,
    [Description("Bassel - 阿拉伯语(伊拉克)")] ar_IQ8BasselNeural,
    [Description("Rana - 阿拉伯语(伊拉克)")] ar_IQ8RanaNeural,
    [Description("Sana - 阿拉伯语(约旦)")] ar_JO8SanaNeural,
    [Description("Taim - 阿拉伯语(约旦)")] ar_JO8TaimNeural,
    [Description("Fahed - 阿拉伯语(科威特)")] ar_KW8FahedNeural,
    [Description("Noura - 阿拉伯语(科威特)")] ar_KW8NouraNeural,
    [Description("Layla - 阿拉伯语(黎巴嫩)")] ar_LB8LaylaNeural,
    [Description("Rami - 阿拉伯语(黎巴嫩)")] ar_LB8RamiNeural,
    [Description("Iman - 阿拉伯语(利比亚)")] ar_LY8ImanNeural,
    [Description("Omar - 阿拉伯语(利比亚)")] ar_LY8OmarNeural,
    [Description("Jamal - 阿拉伯语(摩洛哥)")] ar_MA8JamalNeural,
    [Description("Mouna - 阿拉伯语(摩洛哥)")] ar_MA8MounaNeural,
    [Description("Abdullah - 阿拉伯语(阿曼)")] ar_OM8AbdullahNeural,
    [Description("Aysha - 阿拉伯语(阿曼)")] ar_OM8AyshaNeural,
    [Description("Amal - 阿拉伯语(卡塔尔)")] ar_QA8AmalNeural,
    [Description("Moaz - 阿拉伯语(卡塔尔)")] ar_QA8MoazNeural,
    [Description("Hamed - 阿拉伯语(沙特阿拉伯)")] ar_SA8HamedNeural,
    [Description("Zariyah - 阿拉伯语(沙特阿拉伯)")] ar_SA8ZariyahNeural,
    [Description("Amany - 阿拉伯语(叙利亚)")] ar_SY8AmanyNeural,
    [Description("Laith - 阿拉伯语(叙利亚)")] ar_SY8LaithNeural,
    [Description("Hedi - 阿拉伯语(突尼斯)")] ar_TN8HediNeural,
    [Description("Reem - 阿拉伯语(突尼斯)")] ar_TN8ReemNeural,
    [Description("Fatima - 阿拉伯语(阿联酋)")] ar_AE8FatimaNeural,
    [Description("Hamdan - 阿拉伯语(阿联酋)")] ar_AE8HamdanNeural,
    [Description("Maryam - 阿拉伯语(也门)")] ar_YE8MaryamNeural,
    [Description("Saleh - 阿拉伯语(也门)")] ar_YE8SalehNeural,
    [Description("Babek - 阿塞拜疆语")] az_AZ8BabekNeural,
    [Description("Banu - 阿塞拜疆语")] az_AZ8BanuNeural,
    [Description("Nabanita - 孟加拉语(孟加拉国)")] bn_BD8NabanitaNeural,
    [Description("Pradeep - 孟加拉语(孟加拉国)")] bn_BD8PradeepNeural,
    [Description("Bashkar - 孟加拉语(印度)")] bn_IN8BashkarNeural,
    [Description("Tanishaa - 孟加拉语(印度)")] bn_IN8TanishaaNeural,
    [Description("Goran - 波斯尼亚语")] bs_BA8GoranNeural,
    [Description("Vesna - 波斯尼亚语")] bs_BA8VesnaNeural,
    [Description("Borislav - 保加利亚语")] bg_BG8BorislavNeural,
    [Description("Kalina - 保加利亚语")] bg_BG8KalinaNeural,
    [Description("Nilar - 缅甸语")] my_MM8NilarNeural,
    [Description("Thiha - 缅甸语")] my_MM8ThihaNeural,
    [Description("Enric - 加泰罗尼亚语")] ca_ES8EnricNeural,
    [Description("Joana - 加泰罗尼亚语")] ca_ES8JoanaNeural,
    [Description("HiuGaai - 中文(粤语, 香港)")] zh_HK8HiuGaaiNeural,
    [Description("HiuMaan - 中文(粤语, 香港)")] zh_HK8HiuMaanNeural,
    [Description("WanLung - 中文(粤语, 香港)")] zh_HK8WanLungNeural,
    [Description("Xiaoxiao - 中文(简体)")] zh_CN8XiaoxiaoNeural,
    [Description("Xiaoyi - 中文(简体)")] zh_CN8XiaoyiNeural,
    [Description("Yunjian - 中文(简体)")] zh_CN8YunjianNeural,
    [Description("Yunxi - 中文(简体)")] zh_CN8YunxiNeural,
    [Description("Yunxia - 中文(简体)")] zh_CN8YunxiaNeural,
    [Description("Yunyang - 中文(简体)")] zh_CN8YunyangNeural,
    [Description("Xiaobei - 中文(简体, 辽宁)")] zh_CN_liaoning8XiaobeiNeural,
    [Description("HsiaoChen - 中文(繁体, 台湾)")] zh_TW8HsiaoChenNeural,
    [Description("YunJhe - 中文(繁体, 台湾)")] zh_TW8YunJheNeural,
    [Description("HsiaoYu - 中文(繁体, 台湾)")] zh_TW8HsiaoYuNeural,
    [Description("Xiaoni - 中文(简体, 陕西)")] zh_CN_shaanxi8XiaoniNeural,
    [Description("Gabrijela - 克罗地亚语")] hr_HR8GabrijelaNeural,
    [Description("Srecko - 克罗地亚语")] hr_HR8SreckoNeural,
    [Description("Antonin - 捷克语")] cs_CZ8AntoninNeural,
    [Description("Vlasta - 捷克语")] cs_CZ8VlastaNeural,
    [Description("Christel - 丹麦语")] da_DK8ChristelNeural,
    [Description("Jeppe - 丹麦语")] da_DK8JeppeNeural,
    [Description("Arnaud - 荷兰语(比利时)")] nl_BE8ArnaudNeural,
    [Description("Dena - 荷兰语(比利时)")] nl_BE8DenaNeural,
    [Description("Colette - 荷兰语(荷兰)")] nl_NL8ColetteNeural,
    [Description("Fenna - 荷兰语(荷兰)")] nl_NL8FennaNeural,
    [Description("Maarten - 荷兰语(荷兰)")] nl_NL8MaartenNeural,
    [Description("Natasha - 英语(澳大利亚)")] en_AU8NatashaNeural,
    [Description("William - 英语(澳大利亚)")] en_AU8WilliamNeural,
    [Description("Clara - 英语(加拿大)")] en_CA8ClaraNeural,
    [Description("Liam - 英语(加拿大)")] en_CA8LiamNeural,
    [Description("Sam - 英语(香港)")] en_HK8SamNeural,
    [Description("Yan - 英语(香港)")] en_HK8YanNeural,
    [Description("NeerjaExpressive - 英语(印度)")] en_IN8NeerjaExpressiveNeural,
    [Description("Neerja - 英语(印度)")] en_IN8NeerjaNeural,
    [Description("Prabhat - 英语(印度)")] en_IN8PrabhatNeural,
    [Description("Connor - 英语(爱尔兰)")] en_IE8ConnorNeural,
    [Description("Emily - 英语(爱尔兰)")] en_IE8EmilyNeural,
    [Description("Asilia - 英语(肯尼亚)")] en_KE8AsiliaNeural,
    [Description("Chilemba - 英语(肯尼亚)")] en_KE8ChilembaNeural,
    [Description("Mitchell - 英语(新西兰)")] en_NZ8MitchellNeural,
    [Description("Molly - 英语(新西兰)")] en_NZ8MollyNeural,
    [Description("Abeo - 英语(尼日利亚)")] en_NG8AbeoNeural,
    [Description("Ezinne - 英语(尼日利亚)")] en_NG8EzinneNeural,
    [Description("James - 英语(菲律宾)")] en_PH8JamesNeural,
    [Description("Rosa - 英语(菲律宾)")] en_PH8RosaNeural,
    [Description("Luna - 英语(新加坡)")] en_SG8LunaNeural,
    [Description("Wayne - 英语(新加坡)")] en_SG8WayneNeural,
    [Description("Leah - 英语(南非)")] en_ZA8LeahNeural,
    [Description("Luke - 英语(南非)")] en_ZA8LukeNeural,
    [Description("Elimu - 英语(坦桑尼亚)")] en_TZ8ElimuNeural,
    [Description("Imani - 英语(坦桑尼亚)")] en_TZ8ImaniNeural,
    [Description("Libby - 英语(英国)")] en_GB8LibbyNeural,
    [Description("Maisie - 英语(英国)")] en_GB8MaisieNeural,
    [Description("Ryan - 英语(英国)")] en_GB8RyanNeural,
    [Description("Sonia - 英语(英国)")] en_GB8SoniaNeural,
    [Description("Thomas - 英语(英国)")] en_GB8ThomasNeural,
    [Description("Aria - 英语(美国)")] en_US8AriaNeural,
    [Description("Ana - 英语(美国)")] en_US8AnaNeural,
    [Description("Christopher - 英语(美国)")] en_US8ChristopherNeural,
    [Description("Eric - 英语(美国)")] en_US8EricNeural,
    [Description("Guy - 英语(美国)")] en_US8GuyNeural,
    [Description("Jenny - 英语(美国)")] en_US8JennyNeural,
    [Description("Michelle - 英语(美国)")] en_US8MichelleNeural,
    [Description("Roger - 英语(美国)")] en_US8RogerNeural,
    [Description("Steffan - 英语(美国)")] en_US8SteffanNeural,
    [Description("Anu - 爱沙尼亚语")] et_EE8AnuNeural,
    [Description("Kert - 爱沙尼亚语")] et_EE8KertNeural,
    [Description("Angelo - 菲律宾语")] fil_PH8AngeloNeural,
    [Description("Blessica - 菲律宾语")] fil_PH8BlessicaNeural,
    [Description("Harri - 芬兰语")] fi_FI8HarriNeural,
    [Description("Noora - 芬兰语")] fi_FI8NooraNeural,
    [Description("Charline - 法语(比利时)")] fr_BE8CharlineNeural,
    [Description("Gerard - 法语(比利时)")] fr_BE8GerardNeural,
    [Description("Antoine - 法语(加拿大)")] fr_CA8AntoineNeural,
    [Description("Jean - 法语(加拿大)")] fr_CA8JeanNeural,
    [Description("Sylvie - 法语(加拿大)")] fr_CA8SylvieNeural,
    [Description("Denise - 法语(法国)")] fr_FR8DeniseNeural,
    [Description("Eloise - 法语(法国)")] fr_FR8EloiseNeural,
    [Description("Henri - 法语(法国)")] fr_FR8HenriNeural,
    [Description("Ariane - 法语(瑞士)")] fr_CH8ArianeNeural,
    [Description("Fabrice - 法语(瑞士)")] fr_CH8FabriceNeural,
    [Description("Roi - 加利西亚语")] gl_ES8RoiNeural,
    [Description("Sabela - 加利西亚语")] gl_ES8SabelaNeural,
    [Description("Eka - 格鲁吉亚语")] ka_GE8EkaNeural,
    [Description("Giorgi - 格鲁吉亚语")] ka_GE8GiorgiNeural,
    [Description("Ingrid - 德语(奥地利)")] de_AT8IngridNeural,
    [Description("Jonas - 德语(奥地利)")] de_AT8JonasNeural,
    [Description("Amala - 德语(德国)")] de_DE8AmalaNeural,
    [Description("Conrad - 德语(德国)")] de_DE8ConradNeural,
    [Description("Katja - 德语(德国)")] de_DE8KatjaNeural,
    [Description("Killian - 德语(德国)")] de_DE8KillianNeural,
    [Description("Jan - 德语(瑞士)")] de_CH8JanNeural,
    [Description("Leni - 德语(瑞士)")] de_CH8LeniNeural,
    [Description("Athina - 希腊语")] el_GR8AthinaNeural,
    [Description("Nestoras - 希腊语")] el_GR8NestorasNeural,
    [Description("Dhwani - 古吉拉特语")] gu_IN8DhwaniNeural,
    [Description("Niranjan - 古吉拉特语")] gu_IN8NiranjanNeural,
    [Description("Avri - 希伯来语")] he_IL8AvriNeural,
    [Description("Hila - 希伯来语")] he_IL8HilaNeural,
    [Description("Madhur - 印地语")] hi_IN8MadhurNeural,
    [Description("Swara - 印地语")] hi_IN8SwaraNeural,
    [Description("Noemi - 匈牙利语")] hu_HU8NoemiNeural,
    [Description("Tamas - 匈牙利语")] hu_HU8TamasNeural,
    [Description("Gudrun - 冰岛语")] is_IS8GudrunNeural,
    [Description("Gunnar - 冰岛语")] is_IS8GunnarNeural,
    [Description("Ardi - 印度尼西亚语")] id_ID8ArdiNeural,
    [Description("Gadis - 印度尼西亚语")] id_ID8GadisNeural,
    [Description("Colm - 爱尔兰语")] ga_IE8ColmNeural,
    [Description("Orla - 爱尔兰语")] ga_IE8OrlaNeural,
    [Description("Diego - 意大利语")] it_IT8DiegoNeural,
    [Description("Elsa - 意大利语")] it_IT8ElsaNeural,
    [Description("Isabella - 意大利语")] it_IT8IsabellaNeural,
    [Description("Keita - 日语")] ja_JP8KeitaNeural,
    [Description("Nanami - 日语")] ja_JP8NanamiNeural,
    [Description("Dimas - 爪哇语")] jv_ID8DimasNeural,
    [Description("Siti - 爪哇语")] jv_ID8SitiNeural,
    [Description("Gagan - 卡纳达语")] kn_IN8GaganNeural,
    [Description("Sapna - 卡纳达语")] kn_IN8SapnaNeural,
    [Description("Aigul - 哈萨克语")] kk_KZ8AigulNeural,
    [Description("Daulet - 哈萨克语")] kk_KZ8DauletNeural,
    [Description("Piseth - 高棉语")] km_KH8PisethNeural,
    [Description("Sreymom - 高棉语")] km_KH8SreymomNeural,
    [Description("InJoon - 韩语")] ko_KR8InJoonNeural,
    [Description("SunHi - 韩语")] ko_KR8SunHiNeural,
    [Description("Chanthavong - 老挝语")] lo_LA8ChanthavongNeural,
    [Description("Keomany - 老挝语")] lo_LA8KeomanyNeural,
    [Description("Everita - 拉脱维亚语")] lv_LV8EveritaNeural,
    [Description("Nils - 拉脱维亚语")] lv_LV8NilsNeural,
    [Description("Leonas - 立陶宛语")] lt_LT8LeonasNeural,
    [Description("Ona - 立陶宛语")] lt_LT8OnaNeural,
    [Description("Aleksandar - 马其顿语")] mk_MK8AleksandarNeural,
    [Description("Marija - 马其顿语")] mk_MK8MarijaNeural,
    [Description("Osman - 马来语")] ms_MY8OsmanNeural,
    [Description("Yasmin - 马来语")] ms_MY8YasminNeural,
    [Description("Midhun - 马拉雅拉姆语")] ml_IN8MidhunNeural,
    [Description("Sobhana - 马拉雅拉姆语")] ml_IN8SobhanaNeural,
    [Description("Grace - 马耳他语")] mt_MT8GraceNeural,
    [Description("Joseph - 马耳他语")] mt_MT8JosephNeural,
    [Description("Aarohi - 马拉地语")] mr_IN8AarohiNeural,
    [Description("Manohar - 马拉地语")] mr_IN8ManoharNeural,
    [Description("Bataa - 蒙古语")] mn_MN8BataaNeural,
    [Description("Yesui - 蒙古语")] mn_MN8YesuiNeural,
    [Description("Hemkala - 尼泊尔语")] ne_NP8HemkalaNeural,
    [Description("Sagar - 尼泊尔语")] ne_NP8SagarNeural,
    [Description("Finn - 挪威语")] nb_NO8FinnNeural,
    [Description("Pernille - 挪威语")] nb_NO8PernilleNeural,
    [Description("GulNawaz - 普什图语")] ps_AF8GulNawazNeural,
    [Description("Latifa - 普什图语")] ps_AF8LatifaNeural,
    [Description("Dilara - 波斯语")] fa_IR8DilaraNeural,
    [Description("Farid - 波斯语")] fa_IR8FaridNeural,
    [Description("Marek - 波兰语")] pl_PL8MarekNeural,
    [Description("Zofia - 波兰语")] pl_PL8ZofiaNeural,
    [Description("Antonio - 葡萄牙语(巴西)")] pt_BR8AntonioNeural,
    [Description("Francisca - 葡萄牙语(巴西)")] pt_BR8FranciscaNeural,
    [Description("Duarte - 葡萄牙语(葡萄牙)")] pt_PT8DuarteNeural,
    [Description("Raquel - 葡萄牙语(葡萄牙)")] pt_PT8RaquelNeural,
    [Description("Alina - 罗马尼亚语")] ro_RO8AlinaNeural,
    [Description("Emil - 罗马尼亚语")] ro_RO8EmilNeural,
    [Description("Dmitry - 俄语")] ru_RU8DmitryNeural,
    [Description("Svetlana - 俄语")] ru_RU8SvetlanaNeural,
    [Description("Nicholas - 塞尔维亚语")] sr_RS8NicholasNeural,
    [Description("Sophie - 塞尔维亚语")] sr_RS8SophieNeural,
    [Description("Sameera - 僧伽罗语")] si_LK8SameeraNeural,
    [Description("Thilini - 僧伽罗语")] si_LK8ThiliniNeural,
    [Description("Lukas - 斯洛伐克语")] sk_SK8LukasNeural,
    [Description("Viktoria - 斯洛伐克语")] sk_SK8ViktoriaNeural,
    [Description("Petra - 斯洛文尼亚语")] sl_SI8PetraNeural,
    [Description("Rok - 斯洛文尼亚语")] sl_SI8RokNeural,
    [Description("Muuse - 索马里语")] so_SO8MuuseNeural,
    [Description("Ubax - 索马里语")] so_SO8UbaxNeural,
    [Description("Elena - 西班牙语(阿根廷)")] es_AR8ElenaNeural,
    [Description("Tomas - 西班牙语(阿根廷)")] es_AR8TomasNeural,
    [Description("Marcelo - 西班牙语(玻利维亚)")] es_BO8MarceloNeural,
    [Description("SofiaNeural-西班牙语(玻利维亚)")] es_BO8SofiaNeural,
    [Description("CatalinaNeural-西班牙语(智利)")] es_CL8CatalinaNeural,
    [Description("LorenzoNeural-西班牙语(智利)")] es_CL8LorenzoNeural,
    [Description("GonzaloNeural-西班牙语(哥伦比亚)")] es_CO8GonzaloNeural,
    [Description("SalomeNeural-西班牙语(哥伦比亚)")] es_CO8SalomeNeural,
    [Description("JuanNeural-西班牙语(哥斯达黎加)")] es_CR8JuanNeural,
    [Description("MariaNeural-西班牙语(哥斯达黎加)")] es_CR8MariaNeural,
    [Description("BelkysNeural-西班牙语(古巴)")] es_CU8BelkysNeural,
    [Description("ManuelNeural-西班牙语(古巴)")] es_CU8ManuelNeural,
    [Description("EmilioNeural-西班牙语(多米尼加共和国)")] es_DO8EmilioNeural,
    [Description("RamonaNeural-西班牙语(多米尼加共和国)")] es_DO8RamonaNeural,
    [Description("AndreaNeural-西班牙语(厄瓜多尔)")] es_EC8AndreaNeural,
    [Description("LuisNeural-西班牙语(厄瓜多尔)")] es_EC8LuisNeural,
    [Description("LorenaNeural-西班牙语(萨尔瓦多)")] es_SV8LorenaNeural,
    [Description("RodrigoNeural-西班牙语(萨尔瓦多)")] es_SV8RodrigoNeural,
    [Description("JavierNeural-西班牙语(赤道几内亚)")] es_GQ8JavierNeural,
    [Description("TeresaNeural-西班牙语(赤道几内亚)")] es_GQ8TeresaNeural,
    [Description("AndresNeural-西班牙语(危地马拉)")] es_GT8AndresNeural,
    [Description("MartaNeural-西班牙语(危地马拉)")] es_GT8MartaNeural,
    [Description("CarlosNeural-西班牙语(洪都拉斯)")] es_HN8CarlosNeural,
    [Description("KarlaNeural-西班牙语(洪都拉斯)")] es_HN8KarlaNeural,
    [Description("DaliaNeural-西班牙语(墨西哥)")] es_MX8DaliaNeural,
    [Description("JorgeNeural-西班牙语(墨西哥)")] es_MX8JorgeNeural,
    [Description("FedericoNeural-西班牙语(尼加拉瓜)")] es_NI8FedericoNeural,
    [Description("YolandaNeural-西班牙语(尼加拉瓜)")] es_NI8YolandaNeural,
    [Description("MargaritaNeural-西班牙语(巴拿马)")] es_PA8MargaritaNeural,
    [Description("RobertoNeural-西班牙语(巴拿马)")] es_PA8RobertoNeural,
    [Description("MarioNeural-西班牙语(巴拉圭)")] es_PY8MarioNeural,
    [Description("TaniaNeural-西班牙语(巴拉圭)")] es_PY8TaniaNeural,
    [Description("AlexNeural-西班牙语(秘鲁)")] es_PE8AlexNeural,
    [Description("CamilaNeural-西班牙语(秘鲁)")] es_PE8CamilaNeural,
    [Description("KarinaNeural-西班牙语(波多黎各)")] es_PR8KarinaNeural,
    [Description("VictorNeural-西班牙语(波多黎各)")] es_PR8VictorNeural,
    [Description("AlvaroNeural-西班牙语(西班牙)")] es_ES8AlvaroNeural,
    [Description("ElviraNeural-西班牙语(西班牙)")] es_ES8ElviraNeural,
    [Description("AlonsoNeural-西班牙语(美国)")] es_US8AlonsoNeural,
    [Description("PalomaNeural-西班牙语(美国)")] es_US8PalomaNeural,
    [Description("MateoNeural-西班牙语(乌拉圭)")] es_UY8MateoNeural,
    [Description("ValentinaNeural-西班牙语(乌拉圭)")] es_UY8ValentinaNeural,
    [Description("PaolaNeural-西班牙语(委内瑞拉)")] es_VE8PaolaNeural,
    [Description("SebastianNeural-西班牙语(委内瑞拉)")] es_VE8SebastianNeural,
    [Description("JajangNeural-巽他语(印度尼西亚)")] su_ID8JajangNeural,
    [Description("TutiNeural-巽他语(印度尼西亚)")] su_ID8TutiNeural,
    [Description("RafikiNeural-斯瓦希里语(肯尼亚)")] sw_KE8RafikiNeural,
    [Description("ZuriNeural-斯瓦希里语(肯尼亚)")] sw_KE8ZuriNeural,
    [Description("DaudiNeural-斯瓦希里语(坦桑尼亚)")] sw_TZ8DaudiNeural,
    [Description("RehemaNeural-斯瓦希里语(坦桑尼亚)")] sw_TZ8RehemaNeural,
    [Description("MattiasNeural-瑞典语(瑞典)")] sv_SE8MattiasNeural,
    [Description("SofieNeural-瑞典语(瑞典)")] sv_SE8SofieNeural,
    [Description("PallaviNeural-泰米尔语(印度)")] ta_IN8PallaviNeural,
    [Description("ValluvarNeural-泰米尔语(印度)")] ta_IN8ValluvarNeural,
    [Description("KaniNeural-泰米尔语(马来西亚)")] ta_MY8KaniNeural,
    [Description("SuryaNeural-泰米尔语(马来西亚)")] ta_MY8SuryaNeural,
    [Description("AnbuNeural-泰米尔语(新加坡)")] ta_SG8AnbuNeural,
    [Description("VenbaNeural-泰米尔语(新加坡)")] ta_SG8VenbaNeural,
    [Description("KumarNeural-泰米尔语(斯里兰卡)")] ta_LK8KumarNeural,
    [Description("SaranyaNeural-泰米尔语(斯里兰卡)")] ta_LK8SaranyaNeural,
    [Description("MohanNeural-泰卢固语(印度)")] te_IN8MohanNeural,
    [Description("ShrutiNeural-泰卢固语(印度)")] te_IN8ShrutiNeural,
    [Description("NiwatNeural-泰语(泰国)")] th_TH8NiwatNeural,
    [Description("PremwadeeNeural-泰语(泰国)")] th_TH8PremwadeeNeural,
    [Description("AhmetNeural-土耳其语(土耳其)")] tr_TR8AhmetNeural,
    [Description("EmelNeural-土耳其语(土耳其)")] tr_TR8EmelNeural,
    [Description("OstapNeural-乌克兰语(乌克兰)")] uk_UA8OstapNeural,
    [Description("PolinaNeural-乌克兰语(乌克兰)")] uk_UA8PolinaNeural,
    [Description("GulNeural-乌尔都语(印度)")] ur_IN8GulNeural,
    [Description("SalmanNeural-乌尔都语(印度)")] ur_IN8SalmanNeural,
    [Description("AsadNeural-乌尔都语(巴基斯坦)")] ur_PK8AsadNeural,
    [Description("UzmaNeural-乌尔都语(巴基斯坦)")] ur_PK8UzmaNeural,
    [Description("MadinaNeural-乌兹别克语(乌兹别克斯坦)")] uz_UZ8MadinaNeural,
    [Description("SardorNeural-乌兹别克语(乌兹别克斯坦)")] uz_UZ8SardorNeural,
    [Description("HoaiMyNeural-越南语(越南)")] vi_VN8HoaiMyNeural,
    [Description("NamMinhNeural-越南语(越南)")] vi_VN8NamMinhNeural,
    [Description("AledNeural-威尔士语(英国)")] cy_GB8AledNeural,
    [Description("NiaNeural-威尔士语(英国)")] cy_GB8NiaNeural,
    [Description("ThandoNeural-祖鲁语(南非)")] zu_ZA8ThandoNeural,
    [Description("ThembaNeural-祖鲁语(南非)")] zu_ZA8ThembaNeural
}

public enum STranslateMode
{
    [Description("模式一")] IOS,
    [Description("模式二")] Brower,
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
    private static T[] GetSortedEnumValues<T>() where T : Enum
    {
        var enumValues = (T[])Enum.GetValues(typeof(T));
        // 检查枚举的底层类型是否为 int
        if (Enum.GetUnderlyingType(typeof(T)) == typeof(int))
        {
            enumValues = enumValues
                .OrderBy(e => Convert.ToInt32(e))
                .ToArray();
        }
        return enumValues;
    }

    public static T Increment<T>(this T value) where T : Enum
    {
        var enumValues = GetSortedEnumValues<T>();
        var index = Array.IndexOf(enumValues, value);
        return index < enumValues.Length - 1 ? enumValues[index + 1] : value;
    }

    public static T Decrement<T>(this T value) where T : Enum
    {
        var enumValues = GetSortedEnumValues<T>();
        var index = Array.IndexOf(enumValues, value);
        return index > 0 ? enumValues[index - 1] : value;
    }

    /// <summary>
    ///     累加超出枚举范围则返回第一个
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enum"></param>
    /// <returns></returns>
    public static T Increase<T>(this T @enum) where T : Enum
    {
        var enumValues = GetSortedEnumValues<T>();
        var currentIndex = Array.IndexOf(enumValues, @enum);
        var nextIndex = (currentIndex + 1) % enumValues.Length;
        return enumValues[nextIndex];
    }

    public static T Max<T>(this T @enum) where T : Enum
    {
        return GetSortedEnumValues<T>().Max()!;
    }

    public static T Min<T>(this T @enum) where T : Enum
    {
        return GetSortedEnumValues<T>().Min()!;
    }
}