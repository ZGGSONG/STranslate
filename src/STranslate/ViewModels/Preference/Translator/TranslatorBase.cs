using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorBase : ObservableObject
{
    /// <summary>
    ///     只是为了方便绑定
    ///     * 非LLM Translator无用
    /// </summary>
    [JsonIgnore] public virtual BindingList<UserDefinePrompt> UserDefinePrompts { get; set; } = [];

    [ObservableProperty] private Guid _identify = Guid.Empty;

    [JsonIgnore][ObservableProperty] private ServiceType _type = 0;

    [JsonIgnore][ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore][ObservableProperty] private string _name = string.Empty;

    [JsonIgnore][ObservableProperty] private IconType _icon = IconType.Ali;

    [JsonIgnore] public Dictionary<IconType, string> Icons => Constant.IconDict;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    private string _url = string.Empty;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    private string _appID = string.Empty;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    private string _appKey = string.Empty;

    [JsonIgnore][ObservableProperty] private bool _autoExecute = true;

    [JsonIgnore]
    [ObservableProperty]
    [property: JsonIgnore]
    private TranslationResult _data = TranslationResult.Reset;

    [JsonIgnore]
    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isExecuting;

    [JsonIgnore]
    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isTranslateBackExecuting;

    [JsonIgnore]
    [ObservableProperty]
    private bool _autoExecuteTranslateBack;

    [JsonIgnore]
    [property: JsonIgnore]
    [ObservableProperty]
    private bool _isTesting;

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

    private RelayCommand<string>? _showEncryptInfoCommand;

    [JsonIgnore]
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        _showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

    #endregion
}