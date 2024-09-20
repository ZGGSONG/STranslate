using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using STranslate.Model;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorBase : ObservableObject
{
    [JsonIgnore]
    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isTranslateBackExecuting;

    [JsonIgnore] public Dictionary<IconType, string> Icons => Constant.IconDict;

    public void ManualPropChanged(params string[] array)
    {
        foreach (var str in array) OnPropertyChanged(str);
    }
}