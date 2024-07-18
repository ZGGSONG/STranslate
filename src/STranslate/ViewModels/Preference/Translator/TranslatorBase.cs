using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using STranslate.Model;

namespace STranslate.ViewModels.Preference.Translator;

public class TranslatorBase : ObservableObject
{
    [JsonIgnore] public Dictionary<IconType, string> Icons => ConstStr.ICONDICT;

    public void ManualPropChanged(params string[] array)
    {
        foreach (var str in array) OnPropertyChanged(str);
    }
}