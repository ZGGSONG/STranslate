using CommunityToolkit.Mvvm.ComponentModel;

namespace STranslate.Model;

public partial class ReplaceProp : ObservableObject
{
    [ObservableProperty] private ITranslator? _activeService;

    [ObservableProperty] private double _autoScale = 0.8;

    [ObservableProperty] private LangDetectType _detectType;

    [ObservableProperty] private LangEnum _sourceLang;

    [ObservableProperty] private LangEnum _targetLang;
    /// <summary>
    ///     原始语言识别为自动时使用该配置
    ///     * 使用在线识别服务出错时使用
    /// </summary>
    [ObservableProperty] private LangEnum _sourceLangIfAuto = LangEnum.en;

    /// <summary>
    ///     目标语种为自动时
    ///     * 原始语种识别为中文/中文繁体/中文粤语
    ///     * 目标语种使用该配置
    /// </summary>
    [ObservableProperty] private LangEnum _targetLangIfSourceZh = LangEnum.en;

    /// <summary>
    ///     目标语种为自动时
    ///     * 原始语种识别为非中文
    ///     * 目标语种使用该配置
    /// </summary>
    [ObservableProperty] private LangEnum _targetLangIfSourceNotZh = LangEnum.zh_cn;

    public ReplaceProp Clone()
    {
        return new ReplaceProp
        {
            ActiveService = ActiveService?.Clone(),
            AutoScale = AutoScale,
            DetectType = DetectType,
            SourceLang = SourceLang,
            TargetLang = TargetLang,
            SourceLangIfAuto = SourceLangIfAuto,
            TargetLangIfSourceZh = TargetLangIfSourceZh,
            TargetLangIfSourceNotZh = TargetLangIfSourceNotZh,
        };
    }
}