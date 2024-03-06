using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorOpenAI : ObservableObject, ITranslator
    {
        public TranslatorOpenAI() : this(Guid.NewGuid(), "https://api.openai.com", "OpenAI")
        {
        }

        public TranslatorOpenAI(Guid guid, string url, string name = "", IconType icon = IconType.OpenAI, string appID = "", string appKey = "", bool isEnabled = true, ServiceType type = ServiceType.OpenAIService)
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
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _model = "gpt-3.5-turbo";

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        public TranslationResult _data = TranslationResult.Reset;

        [JsonIgnore]
        public List<IconType> Icons { get; private set; } = Enum.GetValues(typeof(IconType)).OfType<IconType>().ToList();

        #region Show/Hide Encrypt Info

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        private bool _keyHide = true;

        private void ShowEncryptInfo() => KeyHide = !KeyHide;

        private RelayCommand? showEncryptInfoCommand;

        [JsonIgnore]
        public IRelayCommand ShowEncryptInfoCommand => showEncryptInfoCommand ??= new RelayCommand(new Action(ShowEncryptInfo));

        #endregion Show/Hide Encrypt Info

        [JsonIgnore]
        [ObservableProperty]
        private BindingList<OpenaiMessage> openaiMessages =
        [
            new OpenaiMessage("system", "You are a professional translation engine, please translate the text into a colloquial, professional, elegant and fluent content, without the style of machine translation. You must only translate the text content, never interpret it."),
            new OpenaiMessage("user", "Translate the following text from $source to $target: $content")
        ];

        [RelayCommand]
        [property: JsonIgnore]
        private void DeletePrompt(OpenaiMessage openaiMsg)
        {
            OpenaiMessages.Remove(openaiMsg);
        }

        [RelayCommand]
        [property: JsonIgnore]
        private void AddPrompt()
        {
            var last = OpenaiMessages.LastOrDefault()?.Role ?? "";
            var newOne = last switch
            {
                "" => new OpenaiMessage("system"),
                "user" => new OpenaiMessage("assistant"),
                _ => new OpenaiMessage("user")
            };
            OpenaiMessages.Add(newOne);
        }

        public async Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            if (string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(AppKey))
                throw new Exception("请先完善配置");

            if (request is RequestModel req)
            {
                var source = req.SourceLang.ToLower();
                var target = req.TargetLang.ToLower();
                var content = req.Text;

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
                var a_messages = OpenaiMessages.Clone();
                a_messages.ToList().ForEach(item => item.Content = item.Content.Replace("$source", source).Replace("$target", target).Replace("$content", content));

                // 构建请求数据
                var reqData = new
                {
                    model = a_model,
                    messages = a_messages,
                    //temperature = 1.0,
                    stream = true
                };

                var jsonData = JsonConvert.SerializeObject(reqData);

                await HttpUtil.PostAsync(
                    uriBuilder.Uri,
                    jsonData,
                    AppKey,
                    msg =>
                    {
                        if (string.IsNullOrEmpty(msg?.Trim()))
                            return;

                        var preprocessString = msg.Replace("data:", "").Trim();

                        // 结束标记
                        if (preprocessString.Equals("[DONE]"))
                            return;

                        // 解析JSON数据
                        var parsedData = JsonConvert.DeserializeObject<JObject>(preprocessString);

                        if (parsedData is null)
                            return;

                        // 提取content的值
                        var contentValue = parsedData["choices"]?.FirstOrDefault()?["delta"]?["content"]?.ToString();

                        if (string.IsNullOrEmpty(contentValue))
                            return;

                        OnDataReceived?.Invoke(contentValue);
                    },
                    token
                );

                return;
            }

            throw new Exception($"请求数据出错: {request}");
        }

        public Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public ITranslator Clone()
        {
            return new TranslatorOpenAI
            {
                Identify = this.Identify,
                Type = this.Type,
                IsEnabled = this.IsEnabled,
                Icon = this.Icon,
                Name = this.Name,
                Url = this.Url,
                Data = TranslationResult.Reset,
                AppID = this.AppID,
                AppKey = this.AppKey,
                Icons = this.Icons,
                KeyHide = this.KeyHide,
                OpenaiMessages = this.OpenaiMessages,
            };
        }
    }
}