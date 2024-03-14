using CommunityToolkit.Mvvm.ComponentModel;
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
    public partial class TranslatorApi : ObservableObject, ITranslator
    {
        public TranslatorApi()
            : this(Guid.NewGuid(), "https://deeplx.deno.dev/translate", "自建服务") { }

        public TranslatorApi(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.DeepL,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.ApiService
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
        private IconType _icon = IconType.DeepL;

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
        [property: JsonIgnore]
        public TranslationResult _data = TranslationResult.Reset;

        [JsonIgnore]
        public List<IconType> Icons { get; private set; } = Enum.GetValues(typeof(IconType)).OfType<IconType>().ToList();

        [JsonIgnore]
        public string Tips { get; set; } =
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

        public async Task<TranslationResult> TranslateAsync(object request, CancellationToken canceltoken)
        {
            if (request is RequestModel)
            {
                var req = JsonConvert.SerializeObject(request);

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

        public Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public ITranslator Clone()
        {
            return new TranslatorApi
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
                Tips = this.Tips,
                Token = this.Token,
            };
        }
    }
}