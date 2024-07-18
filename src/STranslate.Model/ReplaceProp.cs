using CommunityToolkit.Mvvm.ComponentModel;

namespace STranslate.Model;

public partial class ReplaceProp : ObservableObject, ICloneable
{
    [ObservableProperty] private ITranslator? _activeService;

    [ObservableProperty] private double _autoScale = 0.8;

    [ObservableProperty] private LangDetectType _detectType;

    [ObservableProperty] private LangEnum _targetLang;

    public object Clone()
    {
        return new ReplaceProp
        {
            ActiveService = ActiveService?.Clone(),
            AutoScale = AutoScale,
            DetectType = DetectType,
            TargetLang = TargetLang
        };
    }
}