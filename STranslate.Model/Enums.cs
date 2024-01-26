using System;
using System.ComponentModel;

namespace STranslate.Model
{
    public enum LanguageEnum
    {
        [Description("自动选择")]
        AUTO, //自动

        [Description("中文")]
        ZH, //中文

        [Description("英语")]
        EN, //英语

        [Description("德语")]
        DE, //德语

        [Description("西班牙语")]
        ES, //西班牙语

        [Description("法语")]
        FR, //法语

        [Description("意大利语")]
        IT, //意大利语

        [Description("日语")]
        JA, //日语

        [Description("荷兰语")]
        NL, //荷兰语

        [Description("波兰语")]
        PL, //波兰语

        [Description("葡萄牙语")]
        PT, //葡萄牙语

        [Description("俄语")]
        RU, //俄语

        [Description("保加利亚语")]
        BG, //保加利亚语

        [Description("捷克语")]
        CS, //捷克语

        [Description("丹麦语")]
        DA, //丹麦语

        [Description("希腊语")]
        EL, //希腊语

        [Description("爱沙尼亚语")]
        ET, //爱沙尼亚语

        [Description("芬兰语")]
        FI, //芬兰语

        [Description("匈牙利语")]
        HU, //匈牙利语

        [Description("立陶宛语")]
        LT, //立陶宛语

        [Description("拉脱维亚语")]
        LV, //拉脱维亚语

        [Description("罗马尼亚语")]
        RO, //罗马尼亚语

        [Description("斯洛伐克语")]
        SK, //斯洛伐克语

        [Description("斯洛文尼亚语")]
        SL, //斯洛文尼亚语

        [Description("瑞典语")]
        SV, //瑞典语

        [Description("土耳其语")]
        TR, //土耳其语
    }

    /// <summary>
    /// ServiceView 重置选中项类型
    /// </summary>
    public enum ActionType
    {
        Initialize,
        Delete,
        Add
    }

    /// <summary>
    /// 服务类型
    /// </summary>
    public enum ServiceType
    {
        ApiService = 0,
        BaiduService,
        BingService,
        OpenAIService,
        GeminiService,
    }

    /// <summary>
    /// 图标类型
    /// </summary>
    public enum IconType
    {
        STranslate,
        DeepL,
        Baidu,
        Google,
        Iciba,
        Youdao,
        Bing,
        OpenAI,
        Gemini,
    }

    /// <summary>
    /// 快捷键修饰键
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

    public enum OCRType
    {
        Chinese,
        English
    }

    /// <summary>
    /// 窗体类型-用于通知窗口
    /// </summary>
    public enum WindowType
    {
        Main,
        Preference,
        OCR
    }

    /// <summary>
    /// 托盘功能枚举
    /// </summary>
    public enum DoubleTapFuncEnum
    {
        [Description("输入翻译")]
        InputFunc,
        [Description("截图翻译")]
        ScreenFunc,
        [Description("鼠标划词")]
        MouseHookFunc,
        [Description("文字识别")]
        OCRFunc,
        [Description("显示界面")]
        ShowViewFunc,
        [Description("偏好设置")]
        PreferenceFunc,
        [Description("禁用热键")]
        ForbidShortcutFunc,
        [Description("退出程序")]
        ExitFunc
    }

    /// <summary>
    /// 获取Description
    /// </summary>
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum val)
        {
            var field = val.GetType().GetField(val.ToString());
            var customAttribute = Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute));
            return customAttribute == null ? val.ToString() : ((DescriptionAttribute)customAttribute).Description;
        }
    }
}