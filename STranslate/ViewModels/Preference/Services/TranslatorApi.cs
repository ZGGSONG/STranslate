using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorApi : ObservableObject, ITranslator
    {
        public TranslatorApi()
            : this(Guid.NewGuid(), "https://deeplx.deno.dev/translate", "DeepL") { }

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
        public string _url = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _AppID = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _appKey = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        public TranslationResult _data = TranslationResult.Reset;

        [JsonIgnore]
        public List<IconType> Icons { get; private set; } = Enum.GetValues(typeof(IconType)).OfType<IconType>().ToList();

        public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
        {
            if (request is RequestModel)
            {
                var req = JsonConvert.SerializeObject(request);

                string resp = await HttpUtil.PostAsync(Url, req, token);
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
    }
}
