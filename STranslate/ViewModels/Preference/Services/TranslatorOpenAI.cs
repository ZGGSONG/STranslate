using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorOpenAI : ObservableObject, ITranslator
    {
        public TranslatorOpenAI()
            : this(Guid.NewGuid(), "https://api.openai.com", "OpenAI") { }

        public TranslatorOpenAI(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.OpenAI,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.OpenAIService
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
        private ServiceType _type = 0;

        [JsonIgnore]
        [ObservableProperty]
        public bool _isEnabled = true;

        [JsonIgnore]
        [ObservableProperty]
        private string _name = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private IconType _icon = IconType.Bing;

        [JsonIgnore]
        [ObservableProperty]
        public string _url = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _AppID = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _appKey = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private string _model = "gpt-3.5-turbo";

        [JsonIgnore]
        public object _data = string.Empty;

        [JsonIgnore]
        public object Data
        {
            get => _data;
            set
            {
                if (_data != value)
                {
                    OnPropertyChanging(nameof(Data));
                    _data = value;
                    OnPropertyChanged(nameof(Data));
                }
            }
        }

        [JsonIgnore]
        public List<IconType> Icons { get; private set; } = Enum.GetValues(typeof(IconType)).OfType<IconType>().ToList();

        #region Show/Hide Encrypt Info

        [JsonIgnore]
        private bool _keyHide = true;

        [JsonIgnore]
        public bool KeyHide
        {
            get => _keyHide;
            set
            {
                if (_keyHide != value)
                {
                    OnPropertyChanging(nameof(KeyHide));
                    _keyHide = value;
                    OnPropertyChanged(nameof(KeyHide));
                }
            }
        }

        private void ShowEncryptInfo() => KeyHide = !KeyHide;

        private RelayCommand? showEncryptInfoCommand;

        [JsonIgnore]
        public IRelayCommand ShowEncryptInfoCommand => showEncryptInfoCommand ??= new RelayCommand(new Action(ShowEncryptInfo));

        #endregion Show/Hide Encrypt Info

        public async Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            if (string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(AppKey))
                throw new Exception("请先完善配置");

            if (request is string[] strs)
            {
                var source = strs[0];
                var target = strs[1];
                var content = strs[2];

                UriBuilder uriBuilder = new(Url);

                // 兼容旧版API: https://platform.openai.com/docs/guides/text-generation
                if (!uriBuilder.Path.EndsWith("/v1/chat/completions") && !uriBuilder.Path.EndsWith("/v1/completions"))
                {
                    uriBuilder.Path = "/v1/chat/completions";
                }

                // 选择模型
                var a_model = Model;
                a_model = string.IsNullOrEmpty(a_model) ? "gpt-3.5-turbo" : a_model;

                // 组织语言
                var a_content = source.Equals("auto", StringComparison.CurrentCultureIgnoreCase)
                    ? $"Translate the following text to {target}: {content}"
                    : $"Translate the following text from {source} to {target}: {content}";

                // 构建请求数据
                var reqData = new
                {
                    model = a_model,
                    messages = new[] { new { role = "user", content = a_content } },
                    temperature = 1.0,
                    stream = true
                };

                var jsonData = JsonConvert.SerializeObject(reqData);

                await HttpUtil.PostAsync(uriBuilder.Uri, jsonData, AppKey, msg => OnDataReceived?.Invoke(msg), token);
            }
        }

        public Task<object> TranslateAsync(object request, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
