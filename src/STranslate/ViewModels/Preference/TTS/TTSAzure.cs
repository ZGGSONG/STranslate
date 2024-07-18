using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;

namespace STranslate.ViewModels.Preference.TTS;

public partial class TTSAzure : ObservableObject, ITTS
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

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.Azure;

    [ObservableProperty] private Guid _identify = Guid.Empty;

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private TTSType _type = TTSType.AzureTTS;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string _url = string.Empty;

    /// <summary>
    ///     声音名称
    /// </summary>
    [JsonIgnore] [ObservableProperty] private AzureVoiceEnum _voice = AzureVoiceEnum.zh_CN_liaoning_XiaobeiNeural;

    public TTSAzure()
        : this(Guid.NewGuid(), "https://eastasia.api.cognitive.microsoft.com/", "微软TTS", isEnabled: false)
    {
    }

    public TTSAzure(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Azure,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        TTSType type = TTSType.AzureTTS
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

        var speechConfig = SpeechConfig.FromSubscription(AppKey, AppID);

        // The language of the voice that speaks.
        speechConfig.SpeechSynthesisVoiceName = Voice.ToString().Replace("_", "-");

        using var speechSynthesizer = new SpeechSynthesizer(speechConfig);
        _ = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested && !hasDone)
                // 使用小睡眠来减少CPU使用，这里的时间可以根据需要调整
                Task.Delay(100).Wait();
            await speechSynthesizer.StopSpeakingAsync();
        });
        var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
        OutputSpeechSynthesisResult(speechSynthesisResult, text);

        // 手动跳出循环
        hasDone = true;
    }

    public ITTS Clone()
    {
        return new TTSAzure
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

    private static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
    {
        switch (speechSynthesisResult.Reason)
        {
            case ResultReason.SynthesizingAudioCompleted:
                LogService.Logger.Debug($"Speech synthesized for text: [{text}]");
                break;

            case ResultReason.Canceled:
                //界面显示
                ToastHelper.Show("文本转语音失败");

                var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                LogService.Logger.Warn($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    LogService.Logger.Error($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    LogService.Logger.Error($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                    LogService.Logger.Error("CANCELED: Did you set the speech resource key and region values?");
                }

                break;
        }
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