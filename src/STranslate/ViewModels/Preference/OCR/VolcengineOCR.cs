using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.OCR
{
    public partial class VolcengineOCR : ObservableObject, IOCR
    {
        #region Constructor

        public VolcengineOCR()
            : this(Guid.NewGuid(), "https://visual.volcengineapi.com", "火山OCR") { }

        public VolcengineOCR(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.Volcengine,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            OCRType type = OCRType.VolcengineOCR
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
        private OCRType _type = OCRType.VolcengineOCR;

        [JsonIgnore]
        [ObservableProperty]
        public bool _isEnabled = true;

        [JsonIgnore]
        [ObservableProperty]
        private string _name = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private IconType _icon = IconType.Volcengine;

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
        public Dictionary<IconType, string> Icons { get; private set; } = ConstStr.ICONDICT;

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
            switch (obj)
            {
                case null:
                    return;
                case nameof(AppID):
                    IdHide = !IdHide;
                    break;
                case nameof(AppKey):
                    KeyHide = !KeyHide;
                    break;
            }
        }

        private RelayCommand<string>? showEncryptInfoCommand;

        [JsonIgnore]
        public IRelayCommand<string> ShowEncryptInfoCommand => showEncryptInfoCommand ??= new RelayCommand<string>(new Action<string?>(ShowEncryptInfo));

        #endregion Show/Hide Encrypt Info

        #endregion Properties

        #region Interface Implementation

        public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
        {
            var action = "OCRNormal";
            var version = "2020-08-26";

#if false
var secretId = AppID;
            var secretKey = AppKey;
            var token = "";
            var region = TencentRegionEnum.ap_shanghai.ToString().Replace("_", "-");

            var base64Str = Convert.ToBase64String(bytes);
            string body = "";
            if (TencentOcrAction == TencentOCRAction.GeneralBasicOCR)
            {
                var target = LangConverter(lang) ?? throw new Exception($"该服务不支持{lang.GetDescription()}");
                body = "{\"ImageBase64\":\"" + base64Str + "\",\"LanguageType\":\"" + target + "\"}";
            }
            else
            {
                body = "{\"ImageBase64\":\"" + base64Str + "\"}";
            }
            
            var url = Url;
            var host = url.Replace("https://", "");
            var contentType = "application/json; charset=utf-8";
            var timestamp = ((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString();
            var headers = new Dictionary<string, string>
            {
                { "Host", host },
                { "X-TC-Timestamp", timestamp },
                { "X-TC-Version", version },
                { "X-TC-Region", region },
                { "X-TC-Token", token },
                { "X-TC-RequestClient", "SDK_NET_BAREBONE" },
            };
            string resp = await HttpUtil.PostAsync(url, body, null, headers, cancelToken);
            if (string.IsNullOrEmpty(resp))
                throw new Exception("请求结果为空");

            // 解析JSON数据
            var parsedData = JsonConvert.DeserializeObject<Root>(resp) ?? throw new Exception($"反序列化失败: {resp}");

            // 判断是否出错
            if (parsedData.Response.Error != null) return OcrResult.Fail(parsedData.Response.Error.Message);
            // 提取content的值
            var ocrResult = new OcrResult();
            foreach (var item in parsedData.Response.TextDetections)
            {
                var content = new OcrContent(item.DetectedText);
                item.Polygon.ForEach(pg => content.BoxPoints.Add(new BoxPoint(pg.X, pg.Y)));
                ocrResult.OcrContents.Add(content);
            }
            return ocrResult;
#endif
            return OcrResult.Empty;
        }

        public IOCR Clone()
        {
            return new VolcengineOCR
            {
                Identify = this.Identify,
                Type = this.Type,
                IsEnabled = this.IsEnabled,
                Icon = this.Icon,
                Name = this.Name,
                Url = this.Url,
                AppID = this.AppID,
                AppKey = this.AppKey,
            };
        }

        public string? LangConverter(LangEnum lang) => "auto";

#endregion Interface Implementation
    }
}