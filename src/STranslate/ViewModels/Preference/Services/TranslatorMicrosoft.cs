using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
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
    public partial class TranslatorMicrosoft : ObservableObject, ITranslator
    {
        #region Constructor

        public TranslatorMicrosoft()
            : this(Guid.NewGuid(), "https://api.cognitive.microsofttranslator.com", "微软翻译") { }

        public TranslatorMicrosoft(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.Microsoft,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.MicrosoftService
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
        public BindingList<UserDefinePrompt> UserDefinePrompts { get; set; } = [];

        [JsonIgnore]
        [ObservableProperty]
        private bool _autoExpander = true;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        public TranslationResult _data = TranslationResult.Reset;

        [JsonIgnore]
        public Dictionary<IconType, string> Icons { get; private set; } = ConstStr.ICONDICT;

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

        #endregion Properties

        #region Interface Implementation

        public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
        {
            if (!Url.EndsWith("translate"))
            {
                Url = Url.TrimEnd('/') + "/translate";
            }

            if (request is RequestModel req)
            {
                //检查语种
                var source = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
                var target = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

                var query = new Dictionary<string, string> { { "api-version", "3.0" }, { "to", target } };

                if (!string.Equals(source, "auto", StringComparison.CurrentCultureIgnoreCase))
                {
                    query.Add("from", source);
                }

                var headers = new Dictionary<string, string> { { "Ocp-Apim-Subscription-Key", AppKey }, { "Ocp-Apim-Subscription-Region", AppID }, };
                var body = new[] { new { text = req.Text } };

                string resp = await HttpUtil.PostAsync(Url, JsonConvert.SerializeObject(body), query, headers, token);
                if (string.IsNullOrEmpty(resp))
                    throw new Exception("请求结果为空");

                var ret = JsonConvert.DeserializeObject<ResponseBing[]>(resp ?? "");

                //如果出错就将整个返回信息写入取值处
                if (ret is null || string.IsNullOrEmpty(ret?.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text))
                {
                    throw new Exception(resp);
                }

                var data = ret.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text ?? throw new Exception("请求结果为空");

                return TranslationResult.Success(data);
            }

            throw new Exception($"请求数据出错: {request}");
        }

        public Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public ITranslator Clone()
        {
            return new TranslatorMicrosoft
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
                AutoExpander = this.AutoExpander,
                Icons = this.Icons,
                IdHide = this.IdHide,
                KeyHide = this.KeyHide,
                IsExecuting = IsExecuting,
            };
        }

        /// <summary>
        /// https://learn.microsoft.com/zh-cn/azure/ai-services/translator/language-support
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public string? LangConverter(LangEnum lang)
        {
            return lang switch
            {
                LangEnum.auto => "auto",
                LangEnum.zh_cn => "zh-Hans",
                LangEnum.zh_tw => "zh-Hant",
                LangEnum.yue => "yue",
                LangEnum.en => "en",
                LangEnum.ja => "ja",
                LangEnum.ko => "ko",
                LangEnum.fr => "fr",
                LangEnum.es => "es",
                LangEnum.ru => "ru",
                LangEnum.de => "de",
                LangEnum.it => "it",
                LangEnum.tr => "tr",
                LangEnum.pt_pt => "pt-pt",
                LangEnum.pt_br => "pt",
                LangEnum.vi => "vi",
                LangEnum.id => "id",
                LangEnum.th => "th",
                LangEnum.ms => "ms",
                LangEnum.ar => "ar",
                LangEnum.hi => "hi",
                LangEnum.mn_cy => "mn-Cyrl",
                LangEnum.mn_mo => "mn-Mong",
                LangEnum.km => "km",
                LangEnum.nb_no => "nb",
                LangEnum.nn_no => "nb",
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