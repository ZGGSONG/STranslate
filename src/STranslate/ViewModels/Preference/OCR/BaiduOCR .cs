using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.OCR;

public partial class BaiduOCR : ObservableObject, IOCR
{
    #region Constructor

    public BaiduOCR()
        : this(Guid.NewGuid(), "https://aip.baidubce.com", "百度OCR", isEnabled: false)
    {
    }

    public BaiduOCR(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.BaiduBce,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        OCRType type = OCRType.BaiduOCR
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

    [JsonIgnore] [ObservableProperty] private OCRType _type = OCRType.BaiduOCR;

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.BaiduBce;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string _url = string.Empty;

    /// <summary>
    ///     百度OCR版本(默认高精度含位置版)
    /// </summary>
    [ObservableProperty] private BaiduOCRAction _baiduOcrAction = BaiduOCRAction.accurate;

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
        var token = await GetAccessTokenAsync(AppID, AppKey, cancelToken);
        var suffix = $"/rest/2.0/ocr/v1/{BaiduOcrAction}?access_token={token}";
        var url = Url.TrimEnd('/') + suffix;
        var headers = new Dictionary<string, string[]>
        {
            { "Content-Type", ["application/x-www-form-urlencoded"] },
            { "Accept", ["application/json"] }
        };
        var base64Str = Convert.ToBase64String(bytes);
        var target = LangConverter(lang) ?? throw new Exception($"该服务不支持{lang.GetDescription()}");
        var queryParams = new Dictionary<string, string[]>
        {
            { "image", [base64Str] },
            { "language_type", [target] },
            { "detect_direction", ["false"] },
            { "detect_language", ["false"] },
            { "vertexes_location", ["false"] },
            { "paragraph", ["false"] },
            { "probability", ["false"] }
        };
        var resp = await HttpUtil.PostAsync(url, headers, queryParams, cancelToken);
        if (string.IsNullOrEmpty(resp))
            throw new Exception("请求结果为空");

        // 解析JSON数据
        var parsedData = JsonConvert.DeserializeObject<Root>(resp) ?? throw new Exception($"反序列化失败: {resp}");

        // 判断是否出错
        if (parsedData.error_code != 0) return OcrResult.Fail(parsedData.error_msg);

        // 提取content的值
        var ocrResult = new OcrResult();
        foreach (var item in parsedData.words_result)
        {
            var content = new OcrContent(item.words);
            Converter(item.location).ForEach(pg =>
            {
                //仅位置不全为0时添加
                if (pg.X != pg.Y || pg.X != 0)
                    content.BoxPoints.Add(new BoxPoint(pg.X, pg.Y));
            });
            ocrResult.OcrContents.Add(content);
        }

        return ocrResult;
    }

    public IOCR Clone()
    {
        return new BaiduOCR
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            AppID = AppID,
            AppKey = AppKey,
            Icons = Icons,
            BaiduOcrAction = BaiduOcrAction
        };
    }

    /// <summary>
    ///     https://ai.baidu.com/ai-doc/OCR/1k3h7y3db
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return BaiduOcrAction switch
        {
            BaiduOCRAction.accurate => AccurateType(lang),
            BaiduOCRAction.accurate_basic => AccurateType(lang),
            BaiduOCRAction.general => GeneralType(lang),
            BaiduOCRAction.general_basic => GeneralType(lang),
            _ => null
        };
    }

    /// <summary>
    ///     高精度版支持语言
    /// </summary>
    /// <param name="lang"></param>
    private string? AccurateType(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto_detect",
            LangEnum.zh_cn => "CHN_ENG",
            LangEnum.zh_tw => "CHN_ENG",
            LangEnum.yue => "CHN_ENG",
            LangEnum.en => "ENG",
            LangEnum.ja => "JAP",
            LangEnum.ko => "KOR",
            LangEnum.fr => "FRE",
            LangEnum.es => "SPA",
            LangEnum.ru => "RUS",
            LangEnum.de => "GER",
            LangEnum.it => "ITA",
            LangEnum.tr => "TUR",
            LangEnum.pt_pt => "POR",
            LangEnum.pt_br => "POR",
            LangEnum.vi => "VIE",
            LangEnum.id => "IND",
            LangEnum.th => "THA",
            LangEnum.ms => "MAL",
            LangEnum.ar => "ARA",
            LangEnum.hi => "HIN",
            LangEnum.mn_cy => null,
            LangEnum.mn_mo => null,
            LangEnum.km => null,
            LangEnum.nb_no => null,
            LangEnum.nn_no => null,
            LangEnum.fa => null,
            LangEnum.sv => "SWE",
            LangEnum.pl => "POL",
            LangEnum.nl => "DUT",
            LangEnum.uk => null,
            _ => "auto_detect"
        };
    }

    /// <summary>
    ///     标准版支持语言
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    private string? GeneralType(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "CHN_ENG",
            LangEnum.zh_cn => "CHN_ENG",
            LangEnum.zh_tw => "CHN_ENG",
            LangEnum.yue => "CHN_ENG",
            LangEnum.en => "ENG",
            LangEnum.ja => "JAP",
            LangEnum.ko => "KOR",
            LangEnum.fr => "FRE",
            LangEnum.es => "SPA",
            LangEnum.ru => "RUS",
            LangEnum.de => "GER",
            LangEnum.it => "ITA",
            LangEnum.tr => null,
            LangEnum.pt_pt => "POR",
            LangEnum.pt_br => "POR",
            LangEnum.vi => null,
            LangEnum.id => null,
            LangEnum.th => null,
            LangEnum.ms => null,
            LangEnum.ar => null,
            LangEnum.hi => null,
            LangEnum.mn_cy => null,
            LangEnum.mn_mo => null,
            LangEnum.km => null,
            LangEnum.nb_no => null,
            LangEnum.nn_no => null,
            LangEnum.fa => null,
            LangEnum.sv => null,
            LangEnum.pl => null,
            LangEnum.nl => null,
            LangEnum.uk => null,
            _ => "CHN_ENG"
        };
    }

    #endregion Interface Implementation

    #region Baidu Offcial Support

    public List<BoxPoint> Converter(Location location)
    {
        return
        [
            //left top
            new BoxPoint(location.left, location.top),

            //right top
            new BoxPoint(location.left + location.width, location.top),

            //right bottom
            new BoxPoint(location.left + location.width, location.top + location.height),

            //left bottom
            new BoxPoint(location.left, location.top + location.height)
        ];
    }

    public class Location
    {
        /// <summary>
        /// </summary>
        public int top { get; set; }

        /// <summary>
        /// </summary>
        public int left { get; set; }

        /// <summary>
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// </summary>
        public int height { get; set; }
    }

    public class Words_resultItem
    {
        /// <summary>
        /// </summary>
        public string words { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        public Location location { get; set; } = new();
    }

    public class Root
    {
        /// <summary>
        /// </summary>
        public List<Words_resultItem> words_result { get; set; } = [];

        /// <summary>
        /// </summary>
        public int words_result_num { get; set; }

        /// <summary>
        /// </summary>
        public string log_id { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        public string error_msg { get; set; } = string.Empty;

        /// <summary>
        /// </summary>
        public int error_code { get; set; }
    }

    /**
    * 使用 AK，SK 生成鉴权签名（Access Token）
    * @return 鉴权签名信息（Access Token）
    */
    public async Task<string> GetAccessTokenAsync(string API_KEY, string SECRET_KEY, CancellationToken token)
    {
        var url = "https://aip.baidubce.com/oauth/2.0/token";
        var param = new Dictionary<string, string[]>
        {
            { "grant_type", ["client_credentials"] },
            { "client_id", [API_KEY] },
            { "client_secret", [SECRET_KEY] }
        };
        var resp = await HttpUtil.PostAsync(url, null, param, token);
        var access_token = JsonConvert.DeserializeObject<JObject>(resp)?["access_token"]?.ToString() ?? "";
        return access_token;
    }

    #endregion Baidu Offcial Support
}