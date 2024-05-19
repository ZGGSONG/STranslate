using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorVolcengine : ObservableObject, ITranslator
    {
        #region Constructor

        public TranslatorVolcengine()
            : this(Guid.NewGuid(), "https://translate.volcengineapi.com", "火山翻译") { }

        public TranslatorVolcengine(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.Volcengine,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.VolcengineService
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
        private IconType _icon = IconType.Baidu;

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
            if (request is not RequestModel req)
                throw new Exception($"请求数据出错: {request}");

            //检查语种
            var source = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
            var target = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

            //https://github.com/Baozisoftware/go-dll/wiki/C%23%E8%B0%83%E7%94%A8Go%E7%89%88DLL#%E5%85%B3%E4%BA%8Ego%E7%9A%84%E6%95%B0%E7%BB%84%E5%88%87%E7%89%87%E8%BF%94%E5%9B%9E%E9%97%AE%E9%A2%98
            //加入这个就不崩溃了
            Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=0");

            var accessKeyBytes = Encoding.UTF8.GetBytes(AppID);
            var secretKeyBytes = Encoding.UTF8.GetBytes(AppKey);
            var sourceBytes = Encoding.UTF8.GetBytes(source);
            var targetBytes = Encoding.UTF8.GetBytes(target);
            var contentBytes = Encoding.UTF8.GetBytes(req.Text);
            var result = await Task.Run(() => GoUtil.Execute(accessKeyBytes, secretKeyBytes, sourceBytes, targetBytes, contentBytes));
            var tuple = GoUtil.GoTupleToCSharpTuple(result);
            var resp = tuple.Item2 ?? throw new Exception("请求结果为空");
            if (tuple.Item1 != 200)
                throw new Exception(resp);

            // 解析JSON数据
            var parsedData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception($"反序列化失败: {resp}");

            var data = "";
            if (string.IsNullOrEmpty(parsedData["TranslationList"]?.ToString()))
            {
                LogService.Logger.Error($"({Name})({Identify}) raw content:\n{resp}");
                data = parsedData["ResponseMetadata"]?["Error"]?["Message"]?.ToString() ?? parsedData["ResponseMetadata"]?.ToString();
                return string.IsNullOrEmpty(data) ? TranslationResult.Fail("获取错误信息为空") : TranslationResult.Fail(data);
            }
            // 提取content的值
            else
            {
                data = parsedData["TranslationList"]?.FirstOrDefault()?["Translation"]?.ToString();
                return string.IsNullOrEmpty(data) ? TranslationResult.Fail("获取结果为空") : TranslationResult.Success(data);
            }

        }

        public Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public ITranslator Clone()
        {
            return new TranslatorVolcengine
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
        /// https://www.volcengine.com/docs/4640/35107
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public string? LangConverter(LangEnum lang)
        {
            return lang switch
            {
                LangEnum.auto => "auto",
                LangEnum.zh_cn => "zh",
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
                LangEnum.pt_pt => "pt",
                LangEnum.pt_br => "pt",
                LangEnum.vi => "vi",
                LangEnum.id => "id",
                LangEnum.th => "th",
                LangEnum.ms => "ms",
                LangEnum.ar => "ar",
                LangEnum.hi => "hi",
                LangEnum.mn_cy => "mn",
                LangEnum.mn_mo => "mn",
                LangEnum.km => "km",
                LangEnum.nb_no => "nb",
                LangEnum.nn_no => "no",
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