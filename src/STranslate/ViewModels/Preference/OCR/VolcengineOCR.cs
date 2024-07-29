using System.ComponentModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.OCR;

public partial class VolcengineOCR : ObservableObject, IOCR
{
    #region Constructor

    public VolcengineOCR()
        : this(Guid.NewGuid(), "https://visual.volcengineapi.com", "火山OCR", isEnabled: false)
    {
    }

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

    [ObservableProperty] private Guid _identify = Guid.Empty;

    [JsonIgnore] [ObservableProperty] private OCRType _type = OCRType.VolcengineOCR;

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.Volcengine;

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

    [JsonIgnore] public Dictionary<IconType, string> Icons { get; private set; } = ConstStr.ICONDICT;

    /// <summary>
    ///     火山OCR版本(默认多语种OCR)
    /// </summary>
    [ObservableProperty] private VolcengineOCRAction _volcengineOcrAction = VolcengineOCRAction.MultiLanguageOCR;

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

    #endregion Properties

    #region Interface Implementation

    public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        //https://github.com/Baozisoftware/go-dll/wiki/C%23%E8%B0%83%E7%94%A8Go%E7%89%88DLL#%E5%85%B3%E4%BA%8Ego%E7%9A%84%E6%95%B0%E7%BB%84%E5%88%87%E7%89%87%E8%BF%94%E5%9B%9E%E9%97%AE%E9%A2%98
        //加入这个就不崩溃了
        Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=0");

        var accessKeyBytes = Encoding.UTF8.GetBytes(AppID);
        var secretKeyBytes = Encoding.UTF8.GetBytes(AppKey);
        var actionBytes = Encoding.UTF8.GetBytes(VolcengineOcrAction.ToString());
        var result = await Task
            .Run(
                () => GoUtil.VolcengineOcr(accessKeyBytes, secretKeyBytes, BitmapUtil.BytesToBase64StringBytes(bytes),
                    actionBytes), cancelToken).ConfigureAwait(false);
        var tuple = GoUtil.GoTupleToCSharpTuple(result);
        var resp = tuple.Item2 ?? throw new Exception("请求结果为空");
        object? parsedData;
        try
        {
            parsedData = VolcengineOcrAction switch
            {
                VolcengineOCRAction.OCRNormal => JsonConvert.DeserializeObject<Root>(resp),
                VolcengineOCRAction.MultiLanguageOCR => JsonConvert.DeserializeObject<RootMultiLang>(resp),
                _ => throw new Exception(resp)
            };
        }
        catch (Exception e)
        {
            throw new Exception($"{nameof(VolcengineOCR)} Error\n{resp}", e);
        }

        // 提取content的值
        var ocrResult = new OcrResult();
        switch (parsedData)
        {
            case Root root when root.code != 10000:
                return OcrResult.Fail(root?.ResponseMetadata?.Error?.Message ?? "空Error信息");
            case Root root when root?.data?.line_texts?.Count != root?.data?.line_rects?.Count:
                return OcrResult.Fail("识别和位置结果数量不匹配\n原始数据:" + resp);
            case Root root:
            {
                for (var i = 0; i < root?.data?.line_texts?.Count; i++)
                {
                    var content = new OcrContent(root.data.line_texts[i]);
                    Converter(root?.data?.line_rects?[i]).ForEach(pg =>
                    {
                        //仅位置不全为0时添加
                        if (pg.X != pg.Y || pg.X != 0)
                            content.BoxPoints.Add(new BoxPoint(pg.X, pg.Y));
                    });
                    ocrResult.OcrContents.Add(content);
                }

                break;
            }
            case RootMultiLang rootMultiLang when rootMultiLang.code != 10000:
                return OcrResult.Fail(rootMultiLang?.ResponseMetadata?.Error?.Message ?? "空Error信息");
            case RootMultiLang rootMultiLang:
            {
                foreach (var item in rootMultiLang?.data?.ocr_infos ?? [])
                {
                    var content = new OcrContent(item?.text ?? "");
                    Converter(item?.rect).ForEach(pg =>
                    {
                        //仅位置不全为0时添加
                        if (pg.X != pg.Y || pg.X != 0)
                            content.BoxPoints.Add(new BoxPoint(pg.X, pg.Y));
                    });
                    ocrResult.OcrContents.Add(content);
                }

                break;
            }
            default:
                throw new Exception($"反序列化失败: {resp}");
        }

        return ocrResult;
    }

    public IOCR Clone()
    {
        return new VolcengineOCR
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            AppID = AppID,
            AppKey = AppKey
        };
    }

    public string? LangConverter(LangEnum lang)
    {
        return "auto";
    }

    #endregion Interface Implementation

    #region Volcengine Offcial Support

    private List<BoxPoint> Converter(Line_rectsItem? rect)
    {
        if (rect is null)
            return [];
        return
        [
            //left top
            new BoxPoint(rect.x, rect.y),

            //right top
            new BoxPoint(rect.x + rect.width, rect.y),

            //right bottom
            new BoxPoint(rect.x + rect.width, rect.y + rect.height),

            //left bottom
            new BoxPoint(rect.x, rect.y + rect.height)
        ];
    }

    private List<BoxPoint> Converter(List<List<int>>? rect)
    {
        if (rect is null)
            return [];
        return
        [
            //left top
            new BoxPoint(rect[0][0], rect[0][1]),

            //right top
            new BoxPoint(rect[1][0], rect[1][1]),

            //right bottom
            new BoxPoint(rect[2][0], rect[2][1]),

            //left bottom
            new BoxPoint(rect[3][0], rect[3][1])
        ];
    }

    public class Error
    {
        /// <summary>
        /// </summary>
        public int CodeN { get; set; }

        /// <summary>
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// </summary>
        public string? Message { get; set; }
    }

    public class ResponseMetadata
    {
        /// <summary>
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// </summary>
        public string? Service { get; set; }

        /// <summary>
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// </summary>
        public Error? Error { get; set; }
    }

    public class Line_rectsItem
    {
        /// <summary>
        /// </summary>
        public int x { get; set; }

        /// <summary>
        /// </summary>
        public int y { get; set; }

        /// <summary>
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// </summary>
        public int height { get; set; }
    }

    public class CharsItemItem
    {
        /// <summary>
        /// </summary>
        public int x { get; set; }

        /// <summary>
        /// </summary>
        public int y { get; set; }

        /// <summary>
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// </summary>
        public int height { get; set; }

        /// <summary>
        /// </summary>
        public double score { get; set; }

        /// <summary>
        /// </summary>
        public string? @char { get; set; }
    }

    public class Data
    {
        /// <summary>
        /// </summary>
        public List<string>? line_texts { get; set; }

        /// <summary>
        /// </summary>
        public List<Line_rectsItem>? line_rects { get; set; }

        /// <summary>
        /// </summary>
        public List<List<CharsItemItem>>? chars { get; set; }

        /// <summary>
        /// </summary>
        public List<List<List<int>>>? polygons { get; set; }
    }

    public class Root
    {
        /// <summary>
        /// </summary>
        public ResponseMetadata? ResponseMetadata { get; set; }

        /// <summary>
        /// </summary>
        public string? request_id { get; set; }

        /// <summary>
        /// </summary>
        public string? time_elapsed { get; set; }

        /// <summary>
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// </summary>
        public string? message { get; set; }

        /// <summary>
        /// </summary>
        public Data? data { get; set; }
    }


    public class RootMultiLang
    {
        /// <summary>
        /// </summary>
        public ResponseMetadata? ResponseMetadata { get; set; }

        /// <summary>
        /// </summary>
        public string? request_id { get; set; }

        /// <summary>
        /// </summary>
        public string? time_elapsed { get; set; }

        /// <summary>
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// </summary>
        public string? message { get; set; }

        /// <summary>
        /// </summary>
        public DataMultiLang? data { get; set; }
    }

    public class Ocr_infosItem
    {
        /// <summary>
        /// </summary>
        public string? lang { get; set; }

        /// <summary>
        /// </summary>
        public double prob { get; set; }

        /// <summary>
        /// </summary>
        public List<List<int>>? rect { get; set; }

        /// <summary>
        /// </summary>
        public string? text { get; set; }
    }

    public class DataMultiLang
    {
        /// <summary>
        /// </summary>
        public List<Ocr_infosItem>? ocr_infos { get; set; }
    }

    #endregion Volcengine Offcial Support
}