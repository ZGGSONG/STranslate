using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.OCR;

public partial class TencentOCR : ObservableObject, IOCR
{
    #region Constructor

    public TencentOCR()
        : this(Guid.NewGuid(), "https://ocr.tencentcloudapi.com", "腾讯OCR", isEnabled: false)
    {
    }

    public TencentOCR(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.TencentOCR,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        OCRType type = OCRType.TencentOCR
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

    [ObservableProperty] private Guid _identify = Guid.Empty;

    [JsonIgnore] [ObservableProperty] private OCRType _type = OCRType.TencentOCR;

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.Tencent;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string _url = string.Empty;

    /// <summary>
    ///     腾讯OCR版本(默认通用印刷体识别)
    /// </summary>
    [ObservableProperty] private TencentOCRAction _tencentOcrAction = TencentOCRAction.GeneralBasicOCR;

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

    [JsonIgnore] public Dictionary<IconType, string> Icons { get; private set; } = ConstStr.ICONDICT;

    #region Show/Hide Encrypt Info

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _idHide = true;

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _keyHide = true;

    private void ShowEncryptInfo(string? obj)
    {
        if (obj == null)
            return;

        if (obj.Equals(nameof(AppID)))
            IdHide = !IdHide;
        else if (obj.Equals(nameof(AppKey))) KeyHide = !KeyHide;
    }

    private RelayCommand<string>? showEncryptInfoCommand;

    [JsonIgnore]
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

    #endregion Show/Hide Encrypt Info

    #endregion Properties

    #region Interface Implementation

    public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        var secretId = AppID;
        var secretKey = AppKey;
        var token = "";
        var version = "2018-11-19";
        var action = TencentOcrAction.ToString();
        var region = TencentRegionEnum.ap_shanghai.ToString().Replace("_", "-");

        var base64Str = Convert.ToBase64String(bytes);
        var body = "";
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
        var auth = GetAuth(secretId, secretKey, host, contentType, timestamp, body);
        var headers = new Dictionary<string, string>
        {
            { "Host", host },
            { "X-TC-Timestamp", timestamp },
            { "X-TC-Version", version },
            { "X-TC-Action", action },
            { "X-TC-Region", region },
            { "X-TC-Token", token },
            { "X-TC-RequestClient", "SDK_NET_BAREBONE" },
            { "Authorization", auth }
        };
        var resp = await HttpUtil.PostAsync(url, body, null, headers, cancelToken);
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
    }

    public IOCR Clone()
    {
        return new TencentOCR
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            AppID = AppID,
            AppKey = AppKey,
            TencentOcrAction = TencentOcrAction
        };
    }

    /// <summary>
    ///     https://cloud.tencent.com/document/product/866/33526
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh",
            LangEnum.zh_tw => "zh_rare",
            LangEnum.yue => "zh_rare",
            LangEnum.en => "auto",
            LangEnum.ja => "jap",
            LangEnum.ko => "kor",
            LangEnum.fr => "fre",
            LangEnum.es => "spa",
            LangEnum.ru => "rus",
            LangEnum.de => "ger",
            LangEnum.it => "ita",
            LangEnum.tr => null,
            LangEnum.pt_pt => "por",
            LangEnum.pt_br => "por",
            LangEnum.vi => "vie",
            LangEnum.th => "tha",
            LangEnum.ms => "may",
            LangEnum.ar => "ara",
            LangEnum.hi => "hi",
            LangEnum.id => null,
            LangEnum.mn_cy => null,
            LangEnum.mn_mo => null,
            LangEnum.km => null,
            LangEnum.nb_no => "nor",
            LangEnum.nn_no => "nor",
            LangEnum.fa => null,
            LangEnum.sv => "swe",
            LangEnum.pl => null,
            LangEnum.nl => "hol",
            LangEnum.uk => null,
            _ => "auto"
        };
    }

    #endregion Interface Implementation

    #region Tencent Offcial Support

    protected class ItemPolygon
    {
        /// <summary>
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// </summary>
        public int Y { get; set; }
    }

    protected class PolygonItem
    {
        /// <summary>
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// </summary>
        public int Y { get; set; }
    }

    protected class TextDetectionsItem
    {
        /// <summary>
        /// </summary>
        public string AdvancedInfo { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        public int Confidence { get; set; }

        /// <summary>
        /// </summary>
        public string DetectedText { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        public ItemPolygon ItemPolygon { get; set; } = new();

        /// <summary>
        /// </summary>
        public List<PolygonItem> Polygon { get; set; } = [];

        /// <summary>
        /// </summary>
        public List<string> WordCoordPoint { get; set; } = [];

        /// <summary>
        /// </summary>
        public List<string> Words { get; set; } = [];
    }

    protected class Response
    {
        /// <summary>
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        public int PdfPageSize { get; set; }

        /// <summary>
        /// </summary>
        public string RequestId { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        public List<TextDetectionsItem> TextDetections { get; set; } = [];

        public Error? Error { get; set; }
    }

    public class Error
    {
        public string Code { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }

    protected class Root
    {
        /// <summary>
        /// </summary>
        public Response Response { get; set; } = new();
    }

    protected static string GetAuth(
        string secretId, string secretKey, string host, string contentType,
        string timestamp, string body
    )
    {
        var canonicalURI = "/";
        var canonicalHeaders = "content-type:" + contentType + "\nhost:" + host + "\n";
        var signedHeaders = "content-type;host";
        var hashedRequestPayload = Sha256Hex(body);
        var canonicalRequest = "POST" + "\n"
                                      + canonicalURI + "\n"
                                      + "\n"
                                      + canonicalHeaders + "\n"
                                      + signedHeaders + "\n"
                                      + hashedRequestPayload;

        var algorithm = "TC3-HMAC-SHA256";
        var date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(int.Parse(timestamp))
            .ToString("yyyy-MM-dd");
        var service = host.Split(".")[0];
        var credentialScope = date + "/" + service + "/" + "tc3_request";
        var hashedCanonicalRequest = Sha256Hex(canonicalRequest);
        var stringToSign = algorithm + "\n"
                                     + timestamp + "\n"
                                     + credentialScope + "\n"
                                     + hashedCanonicalRequest;

        var tc3SecretKey = Encoding.UTF8.GetBytes("TC3" + secretKey);
        var secretDate = HmacSha256(tc3SecretKey, Encoding.UTF8.GetBytes(date));
        var secretService = HmacSha256(secretDate, Encoding.UTF8.GetBytes(service));
        var secretSigning = HmacSha256(secretService, Encoding.UTF8.GetBytes("tc3_request"));
        var signatureBytes = HmacSha256(secretSigning, Encoding.UTF8.GetBytes(stringToSign));
        var signature = BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

        return algorithm + " "
                         + "Credential=" + secretId + "/" + credentialScope + ", "
                         + "SignedHeaders=" + signedHeaders + ", "
                         + "Signature=" + signature;
    }

    protected static string Sha256Hex(string s)
    {
        using var algo = SHA256.Create();
        var hashbytes = algo.ComputeHash(Encoding.UTF8.GetBytes(s));
        var builder = new StringBuilder();
        for (var i = 0; i < hashbytes.Length; ++i) builder.Append(hashbytes[i].ToString("x2"));

        return builder.ToString();
    }

    private static byte[] HmacSha256(byte[] key, byte[] msg)
    {
        using var mac = new HMACSHA256(key);
        return mac.ComputeHash(msg);
    }

    #endregion Tencent Offcial Support
}