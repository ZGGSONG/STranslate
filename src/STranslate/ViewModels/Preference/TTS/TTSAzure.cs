using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CognitiveServices.Speech;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;

namespace STranslate.ViewModels.Preference.TTS
{
    public partial class TTSAzure : ObservableObject, ITTS
    {
        public TTSAzure()
            : this(Guid.NewGuid(), "https://eastasia.api.cognitive.microsoft.com/", "微软TTS", isEnabled: false) { }

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

        [ObservableProperty]
        private Guid _identify = Guid.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private TTSType _type = TTSType.AzureTTS;

        [JsonIgnore]
        [ObservableProperty]
        private bool _isEnabled = true;

        [JsonIgnore]
        [ObservableProperty]
        private string _name = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private IconType _icon = IconType.Azure;

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

        [JsonIgnore]
        public Dictionary<IconType, string> Icons { get; private set; } = ConstStr.ICONDICT;

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

        /// <summary>
        /// 声音名称
        /// </summary>
        [JsonIgnore]
        [ObservableProperty]
        private AzureVoiceEnum _voice = AzureVoiceEnum.zh_CN_liaoning_XiaobeiNeural;

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
                {
                    // 使用小睡眠来减少CPU使用，这里的时间可以根据需要调整
                    Task.Delay(100).Wait();
                }
                await speechSynthesizer.StopSpeakingAsync();
            });
            var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
            OutputSpeechSynthesisResult(speechSynthesisResult, text);

            // 手动跳出循环
            hasDone = true;
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

        public ITTS Clone()
        {
            return new TTSAzure
            {
                Identify = this.Identify,
                Type = this.Type,
                IsEnabled = this.IsEnabled,
                Icon = this.Icon,
                Name = this.Name,
                Url = this.Url,
                AppID = this.AppID,
                AppKey = this.AppKey,
                IdHide = this.IdHide,
                KeyHide = this.KeyHide,
                Voice = this.Voice,
            };
        }
    }
}
