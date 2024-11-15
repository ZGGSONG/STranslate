using System.ComponentModel;
using System.Speech.Synthesis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Log;
using STranslate.Model;
using STranslate.Style.Controls;

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

    /// <summary>
    ///     设置 SpeechSynthesizer 对象的语速。
    /// </summary>
    [JsonIgnore] [ObservableProperty] private int _rate = 1;
    
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

    [JsonIgnore] public Dictionary<IconType, string> Icons { get; private set; } = Constant.IconDict;

    [RelayCommand]
    private void GetSystemInfo()
    {
        var installedVoices = new SpeechSynthesizer().GetInstalledVoices();
        LogService.Logger.Info($"TTS|TTSOffline|SystemSupportLanguage:{string.Join(",", installedVoices.Select(x => x.VoiceInfo.Culture.DisplayName))}");
        MessageBox_S.Show("系统支持的语音：" + string.Join(",", installedVoices.Select(x => x.VoiceInfo.Culture.DisplayName)));
    }

    public async Task SpeakTextAsync(string text, CancellationToken token)
    {
        var hasDone = false;
        // 避免有人瞎改配置文件
        if (Rate is < -10 or > 10)
        {
            Rate = 1;
            LogService.Logger.Warn("TTS|TTSOffline|Speech rate is out of range, set to default value 1.");
        }
        using var synth = new SpeechSynthesizer
        {
            Volume = 100,
            Rate = Rate,
        };
        var cancellationTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested && !hasDone)
            {
                // 使用小睡眠来减少CPU使用，这里的时间可以根据需要调整
                await Task.Delay(100);
            }
            if (token.IsCancellationRequested)
            {
                synth.SpeakAsyncCancelAll();
            }
        });
        // Configure the audio output.
        synth.SetOutputToDefaultAudioDevice();
        try
        {
            await Task.Run(() => synth.Speak(text));
            // 手动跳出循环
            hasDone = true;
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("TTS|TTSOffline|Error during speech synthesis.", ex);
        }
        finally
        {
            await cancellationTask;
        }
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
            Icons = Icons,
            Rate = Rate
        };
    }
}