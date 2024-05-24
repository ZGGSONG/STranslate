using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorBaiduBce : TranslatorBase, ITranslator
    {
        #region Constructor

        public TranslatorBaiduBce() : this(Guid.NewGuid(), "https://aip.baidubce.com/rpc/2.0/ai_custom/v1", "百度千帆")
        {
        }

        public TranslatorBaiduBce(Guid guid, string url, string name = "", IconType icon = IconType.Baidu, string appID = "", string appKey = "", bool isEnabled = true, ServiceType type = ServiceType.BaiduBceService)
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
        private bool _autoExpander = true;

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
        [ObservableProperty]
        [property: JsonIgnore]
        private bool _isExecuting = false;

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
            if (obj == null) return;

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

        #region Prompt

        [JsonIgnore]
        [ObservableProperty]
        private BindingList<UserDefinePrompt> _userDefinePrompts =
        [
            new UserDefinePrompt("翻译", [new Prompt("system", "You are a professional, authentic translation engine. You only return the translated text, without any explanations."), new Prompt("user", "Please translate  into $target (avoid explaining the original text):\r\n\r\n$content")], true),
            new UserDefinePrompt("润色", [new Prompt("system", "You are a text embellisher, you can only embellish the text, never interpret it."), new Prompt("user", "Embellish the following text in $source: $content")]),
            new UserDefinePrompt("总结", [new Prompt("system", "You are a text summarizer, you can only summarize the text, never interpret it."), new Prompt("user", "Summarize the following text in $source: $content")]),
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
        private void DeletePrompt(UserDefinePrompt userDefinePrompt)
        {
            UserDefinePrompts.Remove(userDefinePrompt);
        }

        [RelayCommand]
        [property: JsonIgnore]
        private void AddPrompt()
        {
            var userDefinePrompt = new UserDefinePrompt("Undefined", []);
            var dialog = new Views.Preference.Service.PromptDialog(ServiceType.OpenAIService, userDefinePrompt);
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

            if (request is not RequestModel req)
                throw new Exception($"请求数据出错: {request}");

            //检查语种
            var source = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
            var target = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");
            var content = req.Text;

            // 选择模型
            var a_model = Model.Trim();
            a_model = string.IsNullOrEmpty(a_model) ? "gpt-3.5-turbo" : a_model;

            // 替换Prompt关键字
            var a_messages = (UserDefinePrompts.FirstOrDefault(x => x.Enabled)?.Prompts ?? throw new Exception("请先完善Propmpt配置")).Clone();
            a_messages.ToList().ForEach(item => item.Content = item.Content.Replace("$source", source).Replace("$target", target).Replace("$content", content));

            // 构建请求数据
            var reqData = new
            {
                messages = a_messages,
                //temperature = 1.0,
                stream = true
            };

            var jsonData = JsonConvert.SerializeObject(reqData);

            // 获取accesstoken
            var accessTokenUrl = $"https://aip.baidubce.com/oauth/2.0/token?client_id={AppID}G&client_secret={AppKey}&grant_type=client_credentials";
            var accessToken = string.Empty;
            try
            {
                var resp = await HttpUtil.PostAsync(accessTokenUrl, string.Empty);
                accessToken = JObject.Parse(resp)?["session_key"]?.ToString();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (ex.InnerException is Exception innEx)
                {
                    var innMsg = JsonConvert.DeserializeObject<JObject>(innEx.Message);
                    msg += $" {innMsg?["error_description"]?.ToString()}";
                    LogService.Logger.Error($"({Name})({Identify}) raw content:\n{innEx.Message}");
                }
                msg = msg.Trim();

                throw new Exception(msg);
            }

            var a_url = $"{Url}/wenxinworkshop/chat/{a_model}?access_token={accessToken}";

            try
            {
                await HttpUtil.PostAsync(
                        new Uri(a_url),
                        jsonData,
                        null,
                        msg =>
                        {
                            if (string.IsNullOrEmpty(msg?.Trim()))
                                return;

                            var preprocessString = msg.Replace("data:", "").Trim();

                            // 解析JSON数据
                            var parsedData = JsonConvert.DeserializeObject<JObject>(preprocessString);

                            if (parsedData is null)
                                return;

                            // 结束
                            if (bool.TryParse(parsedData["is_end"]?.ToString(), out var isend) && isend)
                                return;

                            // 提取content的值
                            var contentValue = parsedData["result"]?.ToString();

                            if (string.IsNullOrEmpty(contentValue))
                                return;

                            OnDataReceived?.Invoke(contentValue);
                        },
                        token
                    );
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (ex.InnerException is Exception innEx)
                {
                    var innMsg = JsonConvert.DeserializeObject<JObject>(innEx.Message);
                    msg += $" {innMsg?["error"]?["message"]?.ToString()}";
                    LogService.Logger.Error($"({Name})({Identify}) raw content:\n{innEx.Message}");
                }
                msg = msg.Trim();

                throw new Exception(msg);
            }
        }

        public Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public ITranslator Clone()
        {
            return new TranslatorBaiduBce
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
                UserDefinePrompts = this.UserDefinePrompts.Clone(),
                AutoExpander = this.AutoExpander,
                IdHide = this.IdHide,
                KeyHide = this.KeyHide,
                Model = this.Model,
                IsExecuting = IsExecuting,
            };
        }

        /// <summary>
        /// https://zh.wikipedia.org/wiki/ISO_639-1%E4%BB%A3%E7%A0%81%E5%88%97%E8%A1%A8
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public string? LangConverter(LangEnum lang)
        {
            return lang switch
            {
                LangEnum.auto => "auto",
                LangEnum.zh_cn => "zh-cn",
                LangEnum.zh_tw => "zh-tw",
                LangEnum.yue => "yue",
                LangEnum.ja => "ja",
                LangEnum.en => "en",
                LangEnum.ko => "ko",
                LangEnum.fr => "fr",
                LangEnum.es => "es",
                LangEnum.ru => "ru",
                LangEnum.de => "de",
                LangEnum.it => "it",
                LangEnum.tr => "tr",
                LangEnum.pt_pt => "pt_pt",
                LangEnum.pt_br => "pt_br",
                LangEnum.vi => "vi",
                LangEnum.id => "id",
                LangEnum.th => "th",
                LangEnum.ms => "ms",
                LangEnum.ar => "ar",
                LangEnum.hi => "hi",
                LangEnum.mn_cy => "mn_cy",
                LangEnum.mn_mo => "mn_mo",
                LangEnum.km => "km",
                LangEnum.nb_no => "nb_no",
                LangEnum.nn_no => "nn_no",
                LangEnum.fa => "fa",
                LangEnum.sv => "sv",
                LangEnum.pl => "pl",
                LangEnum.nl => "nl",
                LangEnum.uk => "uk",
                _ => "auto"
            };
        }

        #endregion Interface Implementation
    }
}