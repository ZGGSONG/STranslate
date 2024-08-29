using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CognitiveServices.Speech;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.TTS;

public partial class TTSLingva : ObservableObject, ITTS
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

    public TTSLingva()
        : this(Guid.NewGuid(), "http://localhost:3000", "Lingva", isEnabled: false)
    {
    }

    public TTSLingva(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Lingva,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        TTSType type = TTSType.LingvaTTS
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
            var detectType = Singleton<ConfigHelper>.Instance.CurrentConfig?.DetectType ?? LangDetectType.Local;
            var rate = Singleton<ConfigHelper>.Instance.CurrentConfig?.AutoScale ?? 0.8;
            var lang = LangConverter(await LangDetectHelper.DetectAsync(text, detectType, rate, token));
            LogService.Logger.Info($"TTS|Lingva TTS|Detected Lang: {lang} DetectType: {detectType.GetDescription()}");
            UriBuilder uriBuilder = new(Url.Trim('/'));

            if (!uriBuilder.Path.EndsWith("/api/v1/audio"))
                uriBuilder.Path = "/api/v1/audio";

            uriBuilder.Path += $"/{lang}/{text}";

            var response = await HttpUtil.GetAsync(uriBuilder.Uri.ToString(), token);
            var resp = JsonConvert.DeserializeObject<LingvaResponse>(response);
            if (resp?.Error != null)
            {
                throw new Exception(resp.Error);
            }

            var audio = resp?.Audio ?? throw new Exception("Audio is null");
            using var ms = new MemoryStream(audio);
            await using var media = new StreamMediaFoundationReader(ms);
            using var waveOut = new WaveOutEvent();
            waveOut.Init(media);
            waveOut.Play();
            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(100);
                if (!token.IsCancellationRequested) continue;
                try
                {
                    waveOut.Stop();
                }
                catch
                {
                    // ignored
                }
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException)
        {
            LogService.Logger.Info("TTS|Lingva TTS|Request Canceled");
        }
        catch (Exception ex) when (ex is ApplicationException)
        {
            LogService.Logger.Error("TTS|Lingva TTS|Please check the account password...");
        }
        catch (Exception ex)
        {
            LogService.Logger.Error($"TTS|Lingva TTS|Error Occured: {ex.Message}");
        }
    }

    public class LingvaResponse
    {
        [JsonProperty("audio")] public byte[]? Audio { get; set; }

        [JsonProperty("error")] public string? Error { get; set; }
    }

    private string LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh",
            LangEnum.zh_tw => "zh_HANT",
            LangEnum.yue => "zh",
            LangEnum.en => "en",
            LangEnum.ja => "ja",
            LangEnum.ko => "ko",
            LangEnum.fr => "fr",
            LangEnum.es => "es",
            LangEnum.ru => "ru",
            LangEnum.de => "de",
            LangEnum.it => "it",
            LangEnum.tr => "tr",
            LangEnum.pt_pt => "pt",
            LangEnum.pt_br => "pt",
            LangEnum.vi => "vi",
            LangEnum.id => "id",
            LangEnum.th => "th",
            LangEnum.ms => "ms",
            LangEnum.ar => "ar",
            LangEnum.hi => "hi",
            LangEnum.mn_cy => "mn",
            LangEnum.mn_mo => "mn",
            LangEnum.km => "km",
            LangEnum.nb_no => "no",
            LangEnum.nn_no => "no",
            LangEnum.fa => "fa",
            LangEnum.sv => "sv",
            LangEnum.pl => "pl",
            LangEnum.nl => "nl",
            LangEnum.uk => "uk",
            _ => "en"
        };
    }

    public ITTS Clone()
    {
        return new TTSLingva
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