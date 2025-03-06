using Newtonsoft.Json.Linq;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.OCR;

public partial class YoudaoOCR : OCRBase, IOCR
{
    #region Constructor

    public YoudaoOCR()
        : this(Guid.NewGuid(), "https://openapi.youdao.com/ocrapi", "有道OCR", isEnabled: false)
    {
    }

    public YoudaoOCR(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Youdao,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        OCRType type = OCRType.YoudaoOCR
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

    #region Interface Implementation

    public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        //检查语种
        var target = LangConverter(lang) ?? throw new Exception($"该服务不支持{lang.GetDescription()}");
        var base64Str = Convert.ToBase64String(bytes);
        var paramsMap = new Dictionary<string, string[]>
        {
            { "img", [base64Str] },
            { "langType", [target] },
            { "detectType", ["10012"] },
            { "imageType", ["1"] },
            { "docType", ["json"] },
        };

        YoudaoAuthV3Util.AddAuthParams(AppID, AppKey, paramsMap);
        var headers = new Dictionary<string, string[]>
            { { "Content-Type", ["application/x-www-form-urlencoded"] } };
        var resp = await HttpUtil.PostAsync(Url, headers, paramsMap, cancelToken);
        if (string.IsNullOrEmpty(resp))
            throw new Exception("请求结果为空");

        // 解析JSON数据
        var parsedData = JObject.Parse(resp) ?? throw new Exception($"反序列化失败: {resp}");

        if (parsedData["errorCode"]?.ToString() != "0")
            return OcrResult.Fail(parsedData["msg"]?.ToString() ?? resp);

        // 提取content的值
        var ocrResult = new OcrResult();

        var list = parsedData?["Result"]?["regions"]?.ToList();
        for (int i = 0; i < list?.Count; i++)
        {
            var item = list[i];

            // 处理文本信息
            var text = item?["lines"]?.FirstOrDefault()?["text"]?.ToString();
            if (string.IsNullOrEmpty(text))
                continue;
            var content = new OcrContent(text);
            ocrResult.OcrContents.Add(content);

            // 处理区域信息
            var location = item?["lines"]?.FirstOrDefault()?["boundingBox"]?.ToString();
            if (string.IsNullOrEmpty(location))
                continue;
            var array = location.Split(',').Select(int.Parse).ToArray();
            content.BoxPoints.Add(new BoxPoint(array[0], array[1]));
            content.BoxPoints.Add(new BoxPoint(array[2], array[3]));
            content.BoxPoints.Add(new BoxPoint(array[4], array[5]));
            content.BoxPoints.Add(new BoxPoint(array[6], array[7]));
        }

        return ocrResult;
    }

    public IOCR Clone()
    {
        return new YoudaoOCR
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
        };
    }

    /// <summary>
    ///     https://ai.youdao.com/DOCSIRMA/html/ocr/api/tyocr/index.html
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh-CHS",
            LangEnum.zh_tw => "zh-CHT",
            LangEnum.yue => null,
            LangEnum.en => "en",
            LangEnum.ja => "jp",
            LangEnum.ko => "ko",
            LangEnum.fr => "fr",
            LangEnum.es => "es",
            LangEnum.ru => "ru",
            LangEnum.de => "de",
            LangEnum.it => "it",
            LangEnum.tr => "tr",
            LangEnum.pt_pt => "pt",
            LangEnum.pt_br => "pt",
            LangEnum.vi => null,
            LangEnum.id => "id",
            LangEnum.th => "th",
            LangEnum.ms => "ms",
            LangEnum.ar => "ar",
            LangEnum.hi => "hi",
            LangEnum.mn_cy => "mn",
            LangEnum.mn_mo => "mn",
            LangEnum.km => "km",
            LangEnum.nb_no => "no",
            LangEnum.nn_no => "no",
            LangEnum.fa => "bs",
            LangEnum.sv => "sv",
            LangEnum.pl => "pl",
            LangEnum.nl => "nl",
            LangEnum.uk => "uk",
            _ => "auto"
        };
    }

    #endregion Interface Implementation

    #region Youdao Offcial Support

    //public List<BoxPoint> Converter(Location location)
    //{
    //    return
    //    [
    //        //left top
    //        new BoxPoint(location.Left, location.Top),

    //        //right top
    //        new BoxPoint(location.Left + location.Width, location.Top),

    //        //right bottom
    //        new BoxPoint(location.Left + location.Width, location.Top + location.Height),

    //        //left bottom
    //        new BoxPoint(location.Left, location.Top + location.Height)
    //    ];
    //}

    #endregion
}