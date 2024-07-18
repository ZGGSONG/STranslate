using System.ComponentModel;
using System.Speech.Synthesis;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using STranslate.Model;

namespace STranslate.ViewModels.Preference.TTS;

public partial class TTSOffline : ObservableObject, ITTS
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

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.STranslate;

    [ObservableProperty] private Guid _identify = Guid.Empty;

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private TTSType _type = TTSType.OfflineTTS;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string _url = string.Empty;

    public TTSOffline()
        : this(Guid.NewGuid(), "", "离线TTS", isEnabled: false)
    {
    }

    public TTSOffline(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.STranslate,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        TTSType type = TTSType.OfflineTTS
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

    [JsonIgnore] public Dictionary<IconType, string> Icons { get; private set; } = ConstStr.ICONDICT;

    public async Task SpeakTextAsync(string text, CancellationToken token)
    {
        var hasDone = false;
        var synth = new SpeechSynthesizer
        {
            Volume = 100
            //Rate = 2,
        };
        _ = Task.Run(() =>
        {
            while (!token.IsCancellationRequested && !hasDone)
                // 使用小睡眠来减少CPU使用，这里的时间可以根据需要调整
                Task.Delay(100).Wait();
            synth.SpeakAsyncCancelAll();
        });
        // Configure the audio output.
        synth.SetOutputToDefaultAudioDevice();
        await Task.Run(() =>
        {
            try
            {
                synth.Speak(text);
                // 手动跳出循环
                hasDone = true;
            }
            catch (Exception)
            {
            }
        });
    }

    public ITTS Clone()
    {
        return new TTSOffline
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            AppID = AppID,
            AppKey = AppKey,
            Icons = Icons
        };
    }
}