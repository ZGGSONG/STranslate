using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.Translator;

namespace STranslate.ViewModels.Preference.OCR;

public partial class GoogleOCR : ObservableObject, IOCR
{
    #region Constructor

    public GoogleOCR()
        : this(Guid.NewGuid(), "https://vision.googleapis.com", "谷歌OCR", isEnabled: false)
    {
    }

    public GoogleOCR(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Google,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        OCRType type = OCRType.GoogleOCR
    )
    {
        Identify = guid;
        Url = url;
        Name = name;
        Icon = icon;
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

    [JsonIgnore] public Dictionary<IconType, string> Icons { get; private set; } = Constant.IconDict;

    #region Show/Hide Encrypt Info

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _idHide = true;

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
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
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

    #endregion Show/Hide Encrypt Info

    /// <summary>
    ///     是否使用短语位置
    /// </summary>
    [ObservableProperty] private bool _useWordPosition;

    #endregion Properties

    #region Interface Implementation

    public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        const string path = "/v1/images:annotate";
        var query = $"?key={AppKey}";
        var uri = new Uri(Url);
        if (uri.AbsolutePath != path) uri = new Uri(uri, path);

        if (uri.Query != query) uri = new Uri(uri, query);

        var base64Str = Convert.ToBase64String(bytes);
        const string ocrType = "TEXT_DETECTION";
        var req = new
        {
            requests = new[]
            {
                new
                {
                    features = new[]
                    {
                        new { type = ocrType }
                    },
                    image = new
                    {
                        content = base64Str
                    }
                }
            }
        };
        var reqStr = JsonConvert.SerializeObject(req);
        var resp = await HttpUtil.PostAsync(uri.AbsoluteUri, reqStr, null, [], cancelToken);
        if (string.IsNullOrEmpty(resp))
            throw new Exception("请求结果为空");

        // 解析JSON数据
        var parsedData = JsonConvert.DeserializeObject<Root>(resp) ?? throw new Exception($"反序列化失败: {resp}");

        // 判断是否出错
        if (parsedData.Error != null) return OcrResult.Fail(parsedData.Error.Message);

        // 提取content的值
        var ocrResult = new OcrResult();

        if (UseWordPosition)
        {
            var getRets = parsedData.Responses?.FirstOrDefault()?.TextAnnotations;
            for (var i = 0; i < getRets?.Count; i++)
            {
                // 第一行为整体结果
                if (i == 0)
                {
                    continue;
                }

                var getRet = getRets[i];

                var content = new OcrContent(getRet.Description);
                Converter(getRet.BoundingPoly?.Vertices).ForEach(pg =>
                {
                    //仅位置不全为0时添加
                    if (!pg.X.Equals(pg.Y) || pg.X != 0)
                        content.BoxPoints.Add(new BoxPoint(pg.X, pg.Y));
                });
                ocrResult.OcrContents.Add(content);
            }
        }
        else
        {
            var getRet = parsedData.Responses?.FirstOrDefault()?.TextAnnotations?.FirstOrDefault();
            if (getRet == null) throw new Exception($"解析出错: {resp}");
            var content = new OcrContent(getRet.Description);
            Converter(getRet.BoundingPoly?.Vertices).ForEach(pg =>
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
        return new GoogleOCR
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
            UseWordPosition = UseWordPosition
        };
    }

    /// <summary>
    ///     from openai translator <see cref="TranslatorOpenAI.LangConverter" />
    /// </summary>
    /// <param name="lang"></param>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh-cn",
            LangEnum.zh_tw => "zh-tw",
            LangEnum.yue => "yue",
            LangEnum.ja => "ja",
            LangEnum.en => "en",
            LangEnum.ko => "ko",
            LangEnum.fr => "fr",
            LangEnum.es => "es",
            LangEnum.ru => "ru",
            LangEnum.de => "de",
            LangEnum.it => "it",
            LangEnum.tr => "tr",
            LangEnum.pt_pt => "pt_pt",
            LangEnum.pt_br => "pt_br",
            LangEnum.vi => "vi",
            LangEnum.id => "id",
            LangEnum.th => "th",
            LangEnum.ms => "ms",
            LangEnum.ar => "ar",
            LangEnum.hi => "hi",
            LangEnum.mn_cy => "mn_cy",
            LangEnum.mn_mo => "mn_mo",
            LangEnum.km => "km",
            LangEnum.nb_no => "nb_no",
            LangEnum.nn_no => "nn_no",
            LangEnum.fa => "fa",
            LangEnum.sv => "sv",
            LangEnum.pl => "pl",
            LangEnum.nl => "nl",
            LangEnum.uk => "uk",
            _ => "auto"
        };
    }

    #endregion Interface Implementation

    #region Google Offcial Support

    public List<BoxPoint> Converter(List<Vertex>? location)
    {
        return
            location == null
                ?
                [
                    new BoxPoint(0, 0),
                    new BoxPoint(0, 0),
                    new BoxPoint(0, 0),
                    new BoxPoint(0, 0)
                ]
                :
                [
                    //left top
                    new BoxPoint(location[0].X, location[0].Y),

                    //right top
                    new BoxPoint(location[1].X, location[1].Y),

                    //right bottom
                    new BoxPoint(location[2].X, location[2].Y),

                    //left bottom
                    new BoxPoint(location[3].X, location[3].Y)
                ];
    }

    public class Root
    {
        [JsonProperty("responses")] public List<Response>? Responses { get; set; }

        [JsonProperty("error")] public ErrorDetail? Error { get; set; }
    }

    public class Response
    {
        [JsonProperty("textAnnotations")] public List<TextAnnotation>? TextAnnotations { get; set; }

        // 目前用不上
        // [JsonProperty("fullTextAnnotation")] public FullTextAnnotation? FullTextAnnotation { get; set; }
    }

    public class TextAnnotation
    {
        [JsonProperty("locale")] public string Locale { get; set; } = "";

        [JsonProperty("description")] public string Description { get; set; } = "";

        [JsonProperty("boundingPoly")] public BoundingPoly? BoundingPoly { get; set; }
    }

    public class BoundingPoly
    {
        [JsonProperty("vertices")] public List<Vertex>? Vertices { get; set; }
    }

    public class Vertex
    {
        [JsonProperty("x")] public int X { get; set; }

        [JsonProperty("y")] public int Y { get; set; }
    }

    public class FullTextAnnotation
    {
        [JsonProperty("pages")] public List<Page>? Pages { get; set; }

        [JsonProperty("text")] public string Text { get; set; } = "";
    }

    public class Page
    {
        [JsonProperty("property")] public Property? Property { get; set; }

        [JsonProperty("width")] public int Width { get; set; }

        [JsonProperty("height")] public int Height { get; set; }

        [JsonProperty("blocks")] public List<Block>? Blocks { get; set; }
    }

    public class Property
    {
        [JsonProperty("detectedLanguages")] public List<DetectedLanguage>? DetectedLanguages { get; set; }
    }

    public class DetectedLanguage
    {
        [JsonProperty("languageCode")] public string LanguageCode { get; set; } = "";

        [JsonProperty("confidence")] public float? Confidence { get; set; }
    }

    public class Block
    {
        [JsonProperty("boundingBox")] public BoundingPoly? BoundingBox { get; set; }

        [JsonProperty("paragraphs")] public List<Paragraph>? Paragraphs { get; set; }

        [JsonProperty("blockType")] public string BlockType { get; set; } = "";
    }

    public class Paragraph
    {
        [JsonProperty("boundingBox")] public BoundingPoly? BoundingBox { get; set; }

        [JsonProperty("words")] public List<Word>? Words { get; set; }
    }

    public class Word
    {
        [JsonProperty("property")] public Property? Property { get; set; }

        [JsonProperty("boundingBox")] public BoundingPoly? BoundingBox { get; set; }

        [JsonProperty("symbols")] public List<Symbol>? Symbols { get; set; }
    }

    public class Symbol
    {
        [JsonProperty("boundingBox")] public BoundingPoly? BoundingBox { get; set; }

        [JsonProperty("text")] public string Text { get; set; } = "";

        [JsonProperty("property")] public SymbolProperty? Property { get; set; }
    }

    public class SymbolProperty
    {
        [JsonProperty("detectedBreak")] public DetectedBreak? DetectedBreak { get; set; }
    }

    public class DetectedBreak
    {
        [JsonProperty("type")] public string Type { get; set; } = "";
    }

    #region Error

    public class ErrorDetail
    {
        [JsonProperty("code")] public int Code { get; set; }

        [JsonProperty("message")] public string Message { get; set; } = "";

        [JsonProperty("status")] public string Status { get; set; } = "";

        [JsonProperty("details")] public List<Detail>? Details { get; set; }
    }

    public class Detail
    {
        [JsonProperty("@type")] public string Type { get; set; } = "";

        [JsonProperty("fieldViolations")] public List<FieldViolation>? FieldViolations { get; set; }
    }

    public class FieldViolation
    {
        [JsonProperty("field")] public string Field { get; set; } = "";

        [JsonProperty("description")] public string Description { get; set; } = "";
    }

    #endregion

    #endregion Google Offcial Support
}