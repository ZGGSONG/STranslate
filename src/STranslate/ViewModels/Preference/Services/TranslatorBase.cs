using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using STranslate.Model;

namespace STranslate.ViewModels.Preference.Services;

public class TranslatorBase : ObservableObject
{
    [JsonIgnore]
    public Dictionary<IconType, string> Icons => ConstStr.ICONDICT;
}
