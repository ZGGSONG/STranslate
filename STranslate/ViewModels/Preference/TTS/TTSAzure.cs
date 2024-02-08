using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.TTS
{
    public partial class TTSAzure : ObservableObject, ITTS
    {
        public TTSAzure()
            : this(Guid.NewGuid(), "https://eastasia.api.cognitive.microsoft.com/", "微软TTS") { }

        public TTSAzure(Guid guid, string url, string name = "", IconType icon = IconType.Azure, string appID = "", string appKey = "", TTSType type = TTSType.AzureTTS)
        {
            Identify = guid;
            Url = url;
            Name = name;
            Icon = icon;
            AppID = appID;
            AppKey = appKey;
            Type = type;
        }

        [ObservableProperty]
        private Guid _identify = Guid.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private TTSType _type = TTSType.AzureTTS;

        [JsonIgnore]
        [ObservableProperty]
        private string _name = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private IconType _icon = IconType.Azure;

        [JsonIgnore]
        [ObservableProperty]
        public string _url = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _AppID = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _appKey = string.Empty;

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
            if (obj == null)
                return;

            if (obj.Equals(nameof(AppID)))
            {
                IdHide = !IdHide;
            }
            else if (obj.Equals(nameof(AppKey)))
            {
                KeyHide = !KeyHide;
            }
        }

        private RelayCommand<string>? showEncryptInfoCommand;

        [JsonIgnore]
        public IRelayCommand<string> ShowEncryptInfoCommand => showEncryptInfoCommand ??= new RelayCommand<string>(new Action<string?>(ShowEncryptInfo));

        #endregion Show/Hide Encrypt Info

        [ObservableProperty]
        private string _voiceName = "zh-CN-liaoning-XiaobeiNeural";

        public async Task SpeakTextAsync(string text, CancellationToken token)
        {
            var speechConfig = SpeechConfig.FromSubscription(AppKey, AppID);

            // The language of the voice that speaks.
            speechConfig.SpeechSynthesisVoiceName = VoiceName;

            using var speechSynthesizer = new SpeechSynthesizer(speechConfig);
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    // 使用小睡眠来减少CPU使用，这里的时间可以根据需要调整
                    Task.Delay(100).Wait();
                }
                await speechSynthesizer.StopSpeakingAsync();
            });
            var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
            OutputSpeechSynthesisResult(speechSynthesisResult, text);
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
                    ToastHelper.Show("文本转语音失败", WindowType.Main);

                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                    LogService.Logger.Warn($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        LogService.Logger.Error($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        LogService.Logger.Error($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                        LogService.Logger.Error($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
