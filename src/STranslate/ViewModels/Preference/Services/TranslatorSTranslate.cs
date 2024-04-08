using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorSTranslate : ObservableObject, ITranslator
    {
        #region Constructor

        public TranslatorSTranslate()
            : this(Guid.NewGuid(), "", "STranslate") { }

        public TranslatorSTranslate(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.STranslate,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.STranslateService
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
        public string Tips { get; set; } = "本地服务，无需配置";

        #endregion Properties

        #region Interface Implementation

        public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
        {
            if (request is RequestModel req)
            {
                //检查语种
                var source = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
                var target = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

                //https://github.com/Baozisoftware/go-dll/wiki/C%23%E8%B0%83%E7%94%A8Go%E7%89%88DLL#%E5%85%B3%E4%BA%8Ego%E7%9A%84%E6%95%B0%E7%BB%84%E5%88%87%E7%89%87%E8%BF%94%E5%9B%9E%E9%97%AE%E9%A2%98
                //加入这个就不崩溃了
                Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=0");
                var sourceBytes = Encoding.UTF8.GetBytes(source);
                var targetBytes = Encoding.UTF8.GetBytes(target);
                var contentBytes = Encoding.UTF8.GetBytes(req.Text);
                var result = await Task.Run(() => GoUtil.Execute(sourceBytes, targetBytes, contentBytes));
                var resp = GoUtil.GoStringToCSharpString(result);

                // 解析JSON数据
                var parsedData = JsonConvert.DeserializeObject<JObject>(resp ?? throw new Exception("请求结果为空")) ?? throw new Exception($"反序列化失败: {resp}");

                // 提取content的值
                var data = parsedData["data"]?.ToString() ?? throw new Exception("未获取到结果");

                return string.IsNullOrEmpty(data) ? TranslationResult.Fail("获取结果为空") : TranslationResult.Success(data);
            }

            throw new Exception($"请求数据出错: {request}");
        }

        public Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public ITranslator Clone()
        {
            return new TranslatorSTranslate
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
                Tips = this.Tips,
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