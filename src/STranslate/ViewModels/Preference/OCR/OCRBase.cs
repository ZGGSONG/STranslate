using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;
using System.ComponentModel;

namespace STranslate.ViewModels.Preference.OCR;

public partial class OCRBase : ObservableObject
{
    #region Properties

    [ObservableProperty] private Guid _identify = Guid.Empty;

    [JsonIgnore][ObservableProperty] private OCRType _type = OCRType.OpenAIOCR;

    [JsonIgnore][ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore][ObservableProperty] private string _name = string.Empty;

    [JsonIgnore][ObservableProperty] private IconType _icon = IconType.OpenAI;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string _url = string.Empty;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string _appID = string.Empty;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string _appKey = string.Empty;

    [JsonIgnore] public Dictionary<IconType, string> Icons { get; protected set; } = Constant.IconDict;

    #endregion

    #region Show/Hide Encrypt Info

    [JsonIgnore]
    [ObservableProperty]
    [property: JsonIgnore]
    private bool _idHide = true;

    [JsonIgnore]
    [ObservableProperty]
    [property: JsonIgnore]
    private bool _keyHide = true;

    private void ShowEncryptInfo(string? obj)
    {
        switch (obj)
        {
            case null:
                return;
            case nameof(AppID):
                IdHide = !IdHide;
                break;
            case nameof(AppKey):
                KeyHide = !KeyHide;
                break;
        }
    }

    private RelayCommand<string>? showEncryptInfoCommand;

    [JsonIgnore]
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

    #endregion Show/Hide Encrypt Info
}
