using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.Translator;

namespace STranslate.ViewModels.Preference.OCR;

public partial class GeminiOCR : ObservableObject, IOCR
{
    #region Constructor

    public GeminiOCR()
        : this(Guid.NewGuid(), "https://generativelanguage.googleapis.com", "GeminiOCR", isEnabled: false)
    {
    }

    public GeminiOCR(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Gemini,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        OCRType type = OCRType.GeminiOCR
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

    [JsonIgnore][ObservableProperty] private OCRType _type = OCRType.BaiduOCR;

    [JsonIgnore][ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore][ObservableProperty] private string _name = string.Empty;

    [JsonIgnore][ObservableProperty] private IconType _icon = IconType.BaiduBce;

    [JsonIgnore] [ObservableProperty] public string _url = string.Empty;

    [JsonIgnore] [ObservableProperty] public string _appID = string.Empty;

    [JsonIgnore] [ObservableProperty] public string _appKey = string.Empty;

    [JsonIgnore] public Dictionary<IconType, string> Icons { get; private set; } = Constant.IconDict;

    #region Llm Profile

    [JsonIgnore][ObservableProperty] private double _temperature = 1.0;

    [JsonIgnore] [ObservableProperty] private string _model = "gemini-1.5-flash";

    #endregion

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
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

    #endregion Show/Hide Encrypt Info

    #endregion Properties

    #region Interface Implementation

    public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        var uriBuilder = new UriBuilder(Url);

        // 选择模型
        var a_model = Model.Trim();
        a_model = string.IsNullOrEmpty(a_model) ? "gemini-1.5-flash" : a_model;

        uriBuilder.Path = $"/v1beta/models/{a_model}:generateContent";
        uriBuilder.Query = $"?key={AppKey}";

        #region 构造请求数据
        // 温度限定
        var aTemperature = Math.Clamp(Temperature, 0, 2);
        var base64Str = Convert.ToBase64String(bytes);

        var data = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new
                        {
                            inline_data = new
                            {
                                mime_type = "image/png",
                                data = base64Str
                            }
                        },
                        new
                        {
                            text = "Please identify the text in the image, return the bounding boxes of all classes observed in the image, store each row in the word field as an array, and extract the coordinates of each row relative to the image (top, left, width, and height) from top left to bottom right."
                        }
                    }
                }
            },
            systemInstruction = new
            {
                role = "user",
                parts = new[]
                {
                    new
                    {
                        text = "You are a specialized OCR engine that accurately extracts each text from the image."
                    }
                }
            },
            generationConfig = new
            {
                temperature = aTemperature,
                response_mime_type = "application/json",
                response_schema = new
                {
                    type = "ARRAY",
                    items = new
                    {
                        type = "OBJECT",
                        properties = new
                        {
                            words = new
                            {
                                type = "STRING"
                            },
                            location = new
                            {
                                type = "OBJECT",
                                properties = new
                                {
                                    top = new
                                    {
                                        type = "NUMBER"
                                    },
                                    left = new
                                    {
                                        type = "NUMBER"
                                    },
                                    width = new
                                    {
                                        type = "NUMBER"
                                    },
                                    height = new
                                    {
                                        type = "NUMBER"
                                    }
                                }
                            }
                        }
                    }
                }
            },
            safetySettings = new object[]
            {
                new
                {
                    category = "HARM_CATEGORY_HARASSMENT",
                    threshold = "BLOCK_NONE"
                },
                new
                {
                    category = "HARM_CATEGORY_HATE_SPEECH",
                    threshold = "BLOCK_NONE"
                },
                new
                {
                    category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                    threshold = "BLOCK_NONE"
                },
                new
                {
                    category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                    threshold = "BLOCK_NONE"
                }
            }
        };

        #endregion

        var jsonData = JsonConvert.SerializeObject(data);
        var resp = await HttpUtil.PostAsync(uriBuilder.Uri.ToString(), jsonData, null, [], cancelToken);
        var parseData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception(resp);
        var jsonStr = parseData["candidates"]?.FirstOrDefault()?["content"]?["parts"]?.FirstOrDefault()?["text"]?.ToString() ?? throw new Exception($"缺失结果: {resp}");
        // 解析JSON数据
        var itemList = JsonConvert.DeserializeObject<List<DocumentItem>>(jsonStr) ?? throw new Exception($"反序列化失败: {resp}");

        // 提取content的值
        var ocrResult = new OcrResult();
        foreach (var item in itemList)
        {
            var content = new OcrContent(item.words);
            // TODO: 返回位置不精确，暂不添加标注
            //Converter(item.location).ForEach(pg =>
            //{
            //    //仅位置不全为0时添加
            //    if (!pg.X.Equals(pg.Y) || pg.X != 0)
            //        content.BoxPoints.Add(new BoxPoint(pg.X, pg.Y));
            //});
            ocrResult.OcrContents.Add(content);
        }

        return ocrResult;
    }

    public IOCR Clone()
    {
        return new GeminiOCR
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
            Model = Model,
            Temperature = Temperature,
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

    #region Support

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
        public int top { get; set; }
        public int left { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class DocumentItem
    {
        public string words { get; set; } = string.Empty;

        public Location location { get; set; } = new();
    }

    #endregion Support
}