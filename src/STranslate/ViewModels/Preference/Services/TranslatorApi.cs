using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorApi : ObservableObject, ITranslator
    {
        #region Constructor

        public TranslatorApi()
            : this(Guid.NewGuid(), "https://deeplx.deno.dev/translate", "自建服务") { }

        public TranslatorApi(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.DeepL,
            string appId = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.ApiService
        )
        {
            Identify = guid;
            Url = url;
            Name = name;
            Icon = icon;
            AppID = appId;
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
        private bool _isEnabled = true;

        [JsonIgnore]
        [ObservableProperty]
        private string _name = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private IconType _icon = IconType.DeepL;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _url = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _appID = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _appKey = string.Empty;

        [JsonIgnore]
        public BindingList<UserDefinePrompt> UserDefinePrompts { get; set; } = [];

        [JsonIgnore]
        [ObservableProperty]
        private bool _autoExpander = true;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        private TranslationResult _data = TranslationResult.Reset;

        [JsonIgnore]
        public Dictionary<IconType, string> Icons { get; private set; } = ConstStr.ICONDICT;

        [JsonIgnore]
        public string Tips { get; private init; } =
            @"请求:
{
    ""text"": ""test"",
    ""source_lang"": ""auto"",
    ""target_lang"": ""zh""
}
回复:
{
    ""code"": 200,
    ""data"": ""测试""
}";

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _token = string.Empty;

        #endregion Properties

        #region Interface Implementation

        public async Task<TranslationResult> TranslateAsync(object request, CancellationToken canceltoken)
        {
            if (request is RequestModel reqModel)
            {
                var convSource = LangConverter(reqModel.SourceLang) ?? throw new Exception($"该服务不支持{reqModel.SourceLang.GetDescription()}");
                var convTarget = LangConverter(reqModel.TargetLang) ?? throw new Exception($"该服务不支持{reqModel.TargetLang.GetDescription()}");

                var preReq = new
                {
                    text = reqModel.Text,
                    source_lang = convSource,
                    target_lang = convTarget,
                };

                var req = JsonConvert.SerializeObject(preReq);

                var authToken = string.IsNullOrEmpty(Token) ? [] : new Dictionary<string, string> { { "Authorization", $"Bearer {Token}" } };

                string resp = await HttpUtil.PostAsync(Url, req, null, authToken, canceltoken);
                if (string.IsNullOrEmpty(resp))
                    throw new Exception("请求结果为空");

                var ret = JsonConvert.DeserializeObject<ResponseApi>(resp ?? "");

                if (ret is null || string.IsNullOrEmpty(ret.Data.ToString()))
                {
                    throw new Exception(resp);
                }

                return TranslationResult.Success(ret.Data.ToString() ?? "");
            }

            throw new Exception($"请求数据出错: {request}");
        }

        public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public ITranslator Clone()
        {
            return new TranslatorApi
            {
                Identify = Identify,
                Type = Type,
                IsEnabled = IsEnabled,
                Icon = Icon,
                Name = Name,
                Url = Url,
                Data = TranslationResult.Reset,
                AppID = AppID,
                AppKey = AppKey,
                AutoExpander = AutoExpander,
                Icons = Icons,
                Tips = Tips,
                Token = Token
            };
        }

        /// <summary>
        /// https://github.com/ZGGSONG/deepl-api#Languages
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public string? LangConverter(LangEnum lang)
        {
            return lang switch
            {
                LangEnum.auto => "auto",
                LangEnum.zh_cn => "ZH",
                LangEnum.zh_tw => "ZH",
                LangEnum.yue => "ZH",
                LangEnum.en => "EN",
                LangEnum.ja => "JA",
                LangEnum.ko => "KO",
                LangEnum.fr => "FR",
                LangEnum.es => "ES",
                LangEnum.ru => "RU",
                LangEnum.de => "DE",
                LangEnum.it => "IT",
                LangEnum.tr => "TR",
                LangEnum.pt_pt => "PT-PT",
                LangEnum.pt_br => "PT-BR",
                LangEnum.vi => null,
                LangEnum.id => "ID",
                LangEnum.th => null,
                LangEnum.ms => null,
                LangEnum.ar => "AR",
                LangEnum.hi => null,
                LangEnum.mn_cy => null,
                LangEnum.mn_mo => null,
                LangEnum.km => null,
                LangEnum.nb_no => "NB",
                LangEnum.nn_no => "NB",
                LangEnum.fa => null,
                LangEnum.sv => "SV",
                LangEnum.pl => "PL",
                LangEnum.nl => "NL",
                LangEnum.uk => null,
                _ => "auto"
            };
        }

        #endregion Interface Implementation
    }
}