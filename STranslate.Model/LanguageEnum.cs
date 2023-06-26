using System;
using System.ComponentModel;

namespace STranslate.Model
{
    public enum LanguageEnum
    {
        [Description("自动")]
        AUTO,   //自动

        [Description("德语")]
        DE,    //德语

        [Description("英语")]
        EN,    //英语

        [Description("西班牙语")]
        ES,    //西班牙语

        [Description("法语")]
        FR,    //法语

        [Description("意大利语")]
        IT,    //意大利语

        [Description("日语")]
        JA,    //日语

        [Description("荷兰语")]
        NL,    //荷兰语

        [Description("波兰语")]
        PL,    //波兰语

        [Description("葡萄牙语")]
        PT,    //葡萄牙语

        [Description("俄语")]
        RU,    //俄语

        [Description("中文")]
        ZH,    //中文

        [Description("保加利亚语")]
        BG,    //保加利亚语

        [Description("捷克语")]
        CS,    //捷克语

        [Description("丹麦语")]
        DA,    //丹麦语

        [Description("希腊语")]
        EL,    //希腊语

        [Description("爱沙尼亚语")]
        ET,    //爱沙尼亚语

        [Description("芬兰语")]
        FI,    //芬兰语

        [Description("匈牙利语")]
        HU,    //匈牙利语

        [Description("立陶宛语")]
        LT,    //立陶宛语

        [Description("拉脱维亚语")]
        LV,    //拉脱维亚语

        [Description("罗马尼亚语")]
        RO,    //罗马尼亚语

        [Description("斯洛伐克语")]
        SK,    //斯洛伐克语

        [Description("斯洛文尼亚语")]
        SL,    //斯洛文尼亚语

        [Description("瑞典语")]
        SV,    //瑞典语
    }

    /// <summary>
    /// 获取Description
    /// </summary>
    internal static class EnumExtensions
    {
        public static string GetDescription(this Enum val)
        {
            var field = val.GetType().GetField(val.ToString());
            var customAttribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return customAttribute == null ? val.ToString() : ((DescriptionAttribute)customAttribute).Description;
        }
    }
}