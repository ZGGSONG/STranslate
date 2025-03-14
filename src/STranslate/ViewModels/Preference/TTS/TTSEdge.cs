using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EdgeTTS.Net;
using EdgeTTS.Net.Models;
using Newtonsoft.Json;
using STranslate.Log;
using STranslate.Model;

namespace STranslate.ViewModels.Preference.TTS;

public partial class TTSEdge : ObservableObject, ITTS
{
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

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.Bing;

    [ObservableProperty] private Guid _identify = Guid.Empty;

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private TTSType _type = TTSType.EdgeTTS;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string _url = string.Empty;

    /// <summary>
    ///     声音名称
    /// </summary>
    [JsonIgnore] [ObservableProperty] private EdgeVoiceEnum _voice = EdgeVoiceEnum.zh_CN8XiaoxiaoNeural;

    public TTSEdge()
        : this(Guid.NewGuid(), "https://stranslate.zggsong.com", "EdgeTTS", isEnabled: false)
    {
    }

    public TTSEdge(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Bing,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        TTSType type = TTSType.EdgeTTS
    )
    {
        Identify = guid;
        Url = url;
        Name = name;
        Icon = icon;
        AppID = appID;
        AppKey = appKey;
        IsEnabled = isEnabled;
        Type = type;
    }

    [JsonIgnore] public Dictionary<IconType, string> Icons { get; private set; } = Constant.IconDict;

    public async Task SpeakTextAsync(string text, CancellationToken token)
    {
        try
        {
            var voiceName = $"Microsoft Server Speech Text to Speech Voice ({Voice.ToString().Replace("_", "-").Replace("8", ", ")})";
            var voice = EdgeTts.GetVoice().FirstOrDefault(x => x.Name == voiceName) ?? EdgeTts.GetVoice().First();
            var option = new PlayOption
            {
                Rate = 0,
                Text = text
            };
            await EdgeTts.PlayTextAsync(option, voice, token);
        }
        catch (OperationCanceledException)
        {
            LogService.Logger.Debug("TTS|Edge TTS|Operation was canceled.");
        }
        catch (Exception ex)
        {
            LogService.Logger.Error($"TTS|Edge TTS|Error Occured: {ex.Message}");
        }
    }

    public ITTS Clone()
    {
        return new TTSEdge
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            AppID = AppID,
            AppKey = AppKey,
            IdHide = IdHide,
            KeyHide = KeyHide,
            Voice = Voice
        };
    }

    #region Show/Hide Encrypt Info

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _idHide = true;

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _keyHide = true;

    private void ShowEncryptInfo(string? obj)
    {
        if (obj == null)
            return;

        if (obj.Equals(nameof(AppID)))
            IdHide = !IdHide;
        else if (obj.Equals(nameof(AppKey))) KeyHide = !KeyHide;
    }

    private RelayCommand<string>? showEncryptInfoCommand;

    [JsonIgnore]
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

    #endregion Show/Hide Encrypt Info
}