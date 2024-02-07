using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
        Tencent,
        Ali,
        Niutrans,
        Caiyun,
        Microsoft,
        Volcengine,
        Ecdict,
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
    /// 腾讯地区
    /// </summary>
    public enum TencentRegionEnum
    {
        [Description("ap-bangkok")]
        亚太东南_曼谷,

        [Description("ap-beijing")]
        华北地区_北京,

        [Description("ap-chengdu")]
        西南地区_成都,

        [Description("ap-chongqing")]
        西南地区_重庆,

        [Description("ap-guangzhou")]
        华南地区_广州,

        [Description("ap-hongkong")]
        港澳台地区_中国香港,

        [Description("ap-mumbai")]
        亚太南部_孟买,

        [Description("ap-seoul")]
        亚太东北_首尔,

        [Description("ap-shanghai")]
        华东地区_上海,

        [Description("ap-shanghai-fsi")]
        华东地区_上海金融,

        [Description("ap-shenzhen-fsi")]
        华南地区_深圳金融,

        [Description("ap-singapore")]
        亚太东南_新加坡,

        [Description("ap-tokyo")]
        亚太东北_东京,

        [Description("eu-frankfurt")]
        欧洲地区_法兰克福,

        [Description("na-ashburn")]
        美国东部_弗吉尼亚,

        [Description("na-siliconvalley")]
        美国西部_硅谷,

        [Description("na-toronto")]
        北美地区_多伦多,
    }

    /// <summary>
    /// 主界面最大高度
    /// </summary>
    public enum MaxHeight : int
    {
        [Description("最小高度")]
        Minimum = 328,

        [Description("中等高度")]
        Medium = 496,

        [Description("最大高度")]
        Maximum = 800,

        [Description("工作区高度")]
        WorkAreaMaximum = 9999
    }

    /// <summary>
    /// 设置页面导航
    /// </summary>
    public enum PerferenceType
    {
        Common,
        Hotkey,
        Service,
        Favorite,
        History,
        About
    }

    /// <summary>
    /// 主题类型
    /// </summary>
    public enum ThemeType
    {
        Light,
        Dark,
        Auto,
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

        /// <summary>
        /// https://blog.csdn.net/lzdidiv/article/details/71170528
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static int ToInt(this System.Enum e)
        {
            return e.GetHashCode();
        }

        /// <summary>
        /// 通过枚举对象获取枚举列表
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

                list.AddRange(from object tp in tps where ((int)Convert.ToInt32((T)Enum.Parse(typeof(T), tp.ToString() ?? "")) & valData) == valData select (T)tp);
            }

            return list;
        }

        /// <summary>
        /// 通过枚举类型获取枚举列表;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<T> GetEnumList<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).OfType<T>().ToList();
        }
    }
}
