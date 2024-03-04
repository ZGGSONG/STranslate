using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.TTS
{
    public partial class TTSOffline : ObservableObject, ITTS
    {
        public TTSOffline()
            : this(Guid.NewGuid(), "", "离线TTS") { }

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

        [ObservableProperty]
        private Guid _identify = Guid.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private TTSType _type = TTSType.OfflineTTS;

        [JsonIgnore]
        [ObservableProperty]
        public bool _isEnabled = true;

        [JsonIgnore]
        [ObservableProperty]
        private string _name = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private IconType _icon = IconType.STranslate;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string _url = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string _AppID = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string _appKey = string.Empty;

        [JsonIgnore]
        public List<IconType> Icons { get; private set; } = Enum.GetValues(typeof(IconType)).OfType<IconType>().ToList();

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

        public async Task SpeakTextAsync(string text, CancellationToken token)
        {
            var hasDone = false;
            var synth = new SpeechSynthesizer
            {
                Volume = 100,
                //Rate = 2,
            };
            _ = Task.Run(() =>
            {
                while (!token.IsCancellationRequested && !hasDone)
                {
                    // 使用小睡眠来减少CPU使用，这里的时间可以根据需要调整
                    Task.Delay(100).Wait();
                }
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
                catch (Exception) { }
            });
        }
    }
}
