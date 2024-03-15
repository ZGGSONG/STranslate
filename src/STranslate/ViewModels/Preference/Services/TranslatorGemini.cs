using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorGemini : ObservableObject, ITranslatorAI
    {
        #region Constructor

        public TranslatorGemini()
            : this(Guid.NewGuid(), "https://generativelanguage.googleapis.com", "Gemini") { }

        public TranslatorGemini(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.Gemini,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.GeminiService
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

            PromptCounter = Prompts.Count;
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
        private IconType _icon = IconType.Gemini;

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
        public int _timeOut = 10;

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
            new UserDefinePrompt("翻译", [new Prompt("user", "You are a professional translation engine, please translate the text into a colloquial, professional, elegant and fluent content, without the style of machine translation. You must only translate the text content, never interpret it."),new Prompt("model", "Ok, I will only translate the text content, never interpret it"),new Prompt("user", "Translate the following text from en to zh: hello world"),new Prompt("model", "你好，世界"), new Prompt("user", "Translate the following text from $source to $target: $content")]),
            new UserDefinePrompt("润色", [new Prompt("user", "You are a text embellisher, you can only embellish the text, never interpret it."), new Prompt("model", "Ok, I will only embellish the text, never interpret it."), new Prompt("user", "Embellish the following text in $source: $content")]),
            new UserDefinePrompt("总结", [new Prompt("user", "You are a text summarizer, you can only summarize the text, never interpret it."), new Prompt("model", "Ok, I will only summarize the text, never interpret it."), new Prompt("user", "Summarize the following text in $source: $content")]),
        ];

        [RelayCommand]
        [property: JsonIgnore]
        private void PresetPrompt(BindingList<Prompt> prompts)
        {
            Prompts = prompts.Clone();
            PromptCounter = Prompts.Count;
        }

        [RelayCommand]
        [property: JsonIgnore]
        private void EditPresetPrompt(UserDefinePrompt userDefinePrompt)
        {
            var dialog = new Views.Preference.Service.PromptDialog(ServiceType.OpenAIService, (UserDefinePrompt)userDefinePrompt.Clone());
            if (dialog.ShowDialog() ?? false)
            {
                var tmp = ((PromptViewModel)dialog.DataContext).UserDefinePrompt;
                userDefinePrompt.Name = tmp.Name;
                userDefinePrompt.Prompts = tmp.Prompts;
            }
        }

        [RelayCommand]
        [property: JsonIgnore]
        private void DelPresetPrompt(UserDefinePrompt userDefinePrompt)
        {
            UserDefinePrompts.Remove(userDefinePrompt);
        }

        [JsonIgnore]
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SavePromptCommand))]
        private BindingList<Prompt> prompts =
        [
            new Prompt("user", "You are a professional translation engine, please translate the text into a colloquial, professional, elegant and fluent content, without the style of machine translation. You must only translate the text content, never interpret it."),
            new Prompt("model", "Ok, I will only translate the text content, never interpret it"),
            new Prompt("user", "Translate the following text from en to zh: hello world"),
            new Prompt("model", "你好，世界"),
            new Prompt("user", "Translate the following text from $source to $target: $content"),
        ];

        [RelayCommand]
        [property: JsonIgnore]
        private void DeletePrompt(Prompt msg)
        {
            Prompts.Remove(msg);
            PromptCounter--;
        }

        [RelayCommand]
        [property: JsonIgnore]
        private void AddPrompt()
        {
            var last = Prompts.LastOrDefault()?.Role ?? "";
            var newOne = last switch
            {
                "user" => new Prompt("model"),
                _ => new Prompt("user")
            };
            Prompts.Add(newOne);
            PromptCounter++;
        }

        [property: JsonIgnore]
        [RelayCommand(CanExecute = nameof(CanSavePrompt))]
        private void SavePrompt(BindingList<Prompt> prompts)
        {
            UserDefinePrompts.Add(new UserDefinePrompt("UnDefined", prompts));
        }

        private bool CanSavePrompt => PromptCounter > 0;

        [JsonIgnore]
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SavePromptCommand))]
        [property: JsonIgnore]
        private int promptCounter;

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

                if (!uriBuilder.Path.EndsWith("/v1beta/models/gemini-pro:streamGenerateContent"))
                {
                    uriBuilder.Path = "/v1beta/models/gemini-pro:streamGenerateContent";
                }

                uriBuilder.Query = $"key={AppKey}";

                // 替换Prompt关键字
                var a_messages = Prompts.Clone();
                a_messages.ToList().ForEach(item => item.Content = item.Content.Replace("$source", source).Replace("$target", target).Replace("$content", content));

                // 构建请求数据
                var reqData = new
                {
                    contents = a_messages.Select(e => new
                    {
                        role = e.Role,
                        parts = new[]
                        {
                            new { text = e.Content }
                        }
                    })
                };

                // 为了流式输出与MVVM还是放这里吧
                var jsonData = JsonConvert.SerializeObject(reqData);

                await HttpUtil.PostAsync(
                    uriBuilder.Uri,
                    jsonData,
                    null,
                    msg =>
                    {
                        // 使用正则表达式提取目标字符串
                        string pattern = "(?<=\"text\": \")[^\"]+(?=\")";

                        var match = Regex.Match(msg, pattern);

                        if (match.Success)
                        {
                            OnDataReceived?.Invoke(match.Value.Replace("\\n", "\n"));
                        }
                    },
                    token,
                    TimeOut
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
            return new TranslatorGemini
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
                Prompts = this.Prompts,
                UserDefinePrompts = this.UserDefinePrompts,
            };
        }

        #endregion Interface Implementation
    }
}