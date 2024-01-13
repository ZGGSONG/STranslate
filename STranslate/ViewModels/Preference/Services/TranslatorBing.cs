using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorBing : ObservableObject, ITranslator
    {
        public TranslatorBing()
            : this(Guid.NewGuid(), "https://api.cognitive.microsofttranslator.com", "Bing") { }

        public TranslatorBing(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.Bing,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.BingService
        )
        {
            Identify = guid;
            Url = url;
            Name = name;
            Icon = icon;
            AppIDRegion = appID;
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
        public string _appIDRegion = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _appKey = string.Empty;

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

        public async Task<object> TranslateAsync(object request, CancellationToken token)
        {
            Url += Url.EndsWith("translate") ? "" : "/translate";

            if (request is RequestBing req)
            {
                var query = new Dictionary<string, string>
                {
                    { "api-version", "3.0" },
                    { "to", req.To.ToLower() }
                };

                if (!string.Equals(req.From, "auto", StringComparison.CurrentCultureIgnoreCase))
                {
                    query.Add("from", req.From.ToLower());
                }

                var headers = new Dictionary<string, string>
                {
                    { "Ocp-Apim-Subscription-Key", AppKey },
                    { "Ocp-Apim-Subscription-Region", AppIDRegion },
                };

                string resp = await HttpUtil.PostAsync(Url, JsonConvert.SerializeObject(req.Req), query, headers, token);
                if (string.IsNullOrEmpty(resp))
                    throw new Exception("请求结果为空");

                //TODO: 有问题
                var ret = JsonConvert.DeserializeObject<ResponseBing[]>(resp ?? "");

                //如果出错就将整个返回信息写入取值处
                if (ret is null || string.IsNullOrEmpty(ret?.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text))
                {
                    ret = [new ResponseBing { Translations = [new Translation { Text = resp! }] }];
                }
                return Task.FromResult<object>(ret);
            }

            return Task.FromResult<object>(new ResponseBing[] { new() { Translations = [new Translation { Text = "请求数据出错..." }] } });
        }
    }
}
