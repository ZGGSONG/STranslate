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
    public partial class TranslatorChatglm : ObservableObject, ITranslator
    {
        #region Constructor

        public TranslatorChatglm() : this(Guid.NewGuid(), "https://open.bigmodel.cn/api/paas/v4/chat/completions", "智谱AI")
        {
        }

        public TranslatorChatglm(Guid guid, string url, string name = "", IconType icon = IconType.Chatglm, string appID = "", string appKey = "", bool isEnabled = true, ServiceType type = ServiceType.ChatglmService)
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

        #endregion Constructor

        #region Properties

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
        private IconType _icon = IconType.Chatglm;

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
        private bool _autoExpander = true;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _model = "glm-4";

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        public TranslationResult _data = TranslationResult.Reset;

        [JsonIgnore]
        public Dictionary<IconType, string> Icons { get; private set; } = ConstStr.ICONDICT;

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

        #region Prompt

        [JsonIgnore]
        [ObservableProperty]
        private BindingList<UserDefinePrompt> _userDefinePrompts =
        [
            new UserDefinePrompt("翻译", [new Prompt("user", "You are a professional translation engine, please translate the text into a colloquial, professional, elegant and fluent content, without the style of machine translation. You must only translate the text content, never interpret it."),new Prompt("assistant", "Ok, I will only translate the text content, never interpret it."),new Prompt("user", "Translate the following text from en to zh: hello world"),new Prompt("assistant", "你好，世界"), new Prompt("user", "Translate the following text from $source to $target: $content")], true),
            new UserDefinePrompt("润色", [new Prompt("user", "You are a text embellisher, you can only embellish the text, never interpret it."), new Prompt("assistant", "Ok, I will only embellish the text, never interpret it."), new Prompt("user", "Embellish the following text in $source: $content")]),
            new UserDefinePrompt("总结", [new Prompt("user", "You are a text summarizer, you can only summarize the text, never interpret it."), new Prompt("assistant", "Ok, I will only summarize the text, never interpret it."), new Prompt("user", "Summarize the following text in $source: $content")]),
        ];

        [RelayCommand]
        [property: JsonIgnore]
        private void SelectedPrompt(List<object> obj)
        {
            var userDefinePrompt = (UserDefinePrompt)obj.First();
            foreach (var item in UserDefinePrompts)
            {
                item.Enabled = false;
            }
            userDefinePrompt.Enabled = true;

            if (obj.Count == 2) Singleton<ServiceViewModel>.Instance.SaveCommand.Execute(null);
        }

        [RelayCommand]
        [property: JsonIgnore]
        private void UpdatePrompt(UserDefinePrompt userDefinePrompt)
        {
            var dialog = new Views.Preference.Service.PromptDialog(ServiceType.ChatglmService, (UserDefinePrompt)userDefinePrompt.Clone());
            if (dialog.ShowDialog() ?? false)
            {
                var tmp = ((PromptViewModel)dialog.DataContext).UserDefinePrompt;
                userDefinePrompt.Name = tmp.Name;
                userDefinePrompt.Prompts = tmp.Prompts;
            }
        }

        [RelayCommand]
        [property: JsonIgnore]
        private void DeletePrompt(UserDefinePrompt userDefinePrompt)
        {
            UserDefinePrompts.Remove(userDefinePrompt);
        }

        [RelayCommand]
        [property: JsonIgnore]
        private void AddPrompt()
        {
            var userDefinePrompt = new UserDefinePrompt("Undefined", []);
            var dialog = new Views.Preference.Service.PromptDialog(ServiceType.ChatglmService, userDefinePrompt);
            if (dialog.ShowDialog() ?? false)
            {
                var tmp = ((PromptViewModel)dialog.DataContext).UserDefinePrompt;
                userDefinePrompt.Name = tmp.Name;
                userDefinePrompt.Prompts = tmp.Prompts;
                UserDefinePrompts.Add(userDefinePrompt);
            }
        }

        #endregion Prompt

        #endregion Properties

        #region Interface Implementation

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

                // 兼容旧版API: https://open.bigmodel.cn/dev/api#glm-4
                if (!uriBuilder.Path.EndsWith("/api/paas/v4/chat/completions"))
                {
                    uriBuilder.Path = "/api/paas/v4/chat/completions";
                }

                // 选择模型
                var a_model = Model.Trim();
                a_model = string.IsNullOrEmpty(a_model) ? "glm-4" : a_model;

                // 替换Prompt关键字
                var a_messages = (UserDefinePrompts.FirstOrDefault(x => x.Enabled)?.Prompts ?? throw new Exception("请先完善Propmpt配置")).Clone();
                a_messages.ToList().ForEach(item => item.Content = item.Content.Replace("$source", source).Replace("$target", target).Replace("$content", content));

                // 构建请求数据
                var reqData = new
                {
                    model = a_model,
                    messages = a_messages,
                    stream = true
                };

                var jsonData = JsonConvert.SerializeObject(reqData);

                var auth = ChatglmAuthenicationUtil.GenerateToken(AppKey, 60);

                await HttpUtil.PostAsync(
                    uriBuilder.Uri,
                    jsonData,
                    auth,
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
            return new TranslatorChatglm
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
                UserDefinePrompts = this.UserDefinePrompts,
                AutoExpander = this.AutoExpander,
                Icons = this.Icons,
                KeyHide = this.KeyHide,
                Model = this.Model,
            };
        }

        #endregion Interface Implementation
    }
}