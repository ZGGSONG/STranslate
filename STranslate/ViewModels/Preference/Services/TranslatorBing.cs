using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public partial class TranslatorBing : ObservableObject, ITranslator
    {
        public TranslatorBing()
            : this(Guid.NewGuid(), "https://api.cognitive.microsofttranslator.com", "微软翻译") { }

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
        private IconType _icon = IconType.Bing;

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

        public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
        {
            if (!Url.EndsWith("translate"))
            {
                Url = Url.TrimEnd('/') + "/translate";
            }

            if (request is RequestModel req)
            {
                var query = new Dictionary<string, string> { { "api-version", "3.0" }, { "to", req.TargetLang.ToLower() } };

                if (!string.Equals(req.SourceLang, "auto", StringComparison.CurrentCultureIgnoreCase))
                {
                    query.Add("from", req.SourceLang.ToLower());
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
    }
}
