using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.Translator;
using System.ComponentModel;

namespace STranslate.ViewModels.Preference.OCR;

public partial class GeminiOCR : OCRLLMBase, IOCRLLM
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

    [JsonIgnore]
    private BindingList<UserDefinePrompt> _userDefinePrompts =
    [
        new UserDefinePrompt(
            "文本识别",
            [
                // https://github.com/skitsanos/gemini-ocr/blob/main/ocr.sh
                new Prompt("user", "Act like a text scanner. Extract text as it is without analyzing it and without summarizing it. Treat all images as a whole document and analyze them accordingly. Think of it as a document with multiple pages, each image being a page. Understand page-to-page flow logically and semantically.")
            ],
            true
        )
    ];

    public override BindingList<UserDefinePrompt> UserDefinePrompts
    {
        get => _userDefinePrompts;
        set => SetProperty(ref _userDefinePrompts, value);
    }

    #region Interface Implementation

    public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        var target = LangConverter(lang) ?? throw new Exception($"该服务不支持{lang.GetDescription()}");
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

        // https://ai.google.dev/gemini-api/docs/image-understanding?hl=zh-cn#supported-formats
        var formatStr = (Singleton<ConfigHelper>.Instance.CurrentConfig?.OcrImageQuality ?? OcrImageQualityEnum.Medium) switch
        {
            OcrImageQualityEnum.Low => "image/jpeg",
            OcrImageQualityEnum.Medium => "image/png",
            _ => "image/png"//即便是bmp 使用 png 标签 gemini 也能正常识别（gemini-2.0-flash-exp）
        };

        // 替换Prompt关键字
        var a_messages =
            (UserDefinePrompts.FirstOrDefault(x => x.Enabled)?.Prompts ?? throw new Exception("请先完善Propmpt配置")).Clone();
        a_messages.ToList().ForEach(item =>
            item.Content = item.Content.Replace("$target", target));
        var userPrompt = a_messages.LastOrDefault() ?? throw new Exception("Prompt配置为空");
        a_messages.Remove(userPrompt);
        var messages = new List<object>();
        foreach (var item in a_messages)
        {
            messages.Add(new
            {
                role = item.Role,
                parts = new[]
                {
                    new { text = item.Content }
                }
            });
        }
        messages.Add(new
        {
            role = "user",
            parts = new object[]
            {
                new
                {
                    inline_data = new
                    {
                        mime_type = formatStr,
                        data = base64Str
                    }
                },
                new
                {
                    text = userPrompt.Content
                }
            }
        });

        var data = new
        {
            contents = messages,
#if false
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
#endif
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
        var result = parseData["candidates"]?.FirstOrDefault()?["content"]?["parts"]?.FirstOrDefault()?["text"]?.ToString() ?? throw new Exception($"缺失结果: {resp}");

        // 提取content的值
        var ocrResult = new OcrResult();
        foreach (var item in result.Split("\n").ToList())
        {
            var content = new OcrContent(item);
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
            UserDefinePrompts = UserDefinePrompts.Clone()
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
            LangEnum.auto => "Requires you to identify automatically",
            LangEnum.zh_cn => "Simplified Chinese",
            LangEnum.zh_tw => "Traditional Chinese",
            LangEnum.yue => "Cantonese",
            LangEnum.ja => "Japanese",
            LangEnum.en => "English",
            LangEnum.ko => "Korean",
            LangEnum.fr => "French",
            LangEnum.es => "Spanish",
            LangEnum.ru => "Russian",
            LangEnum.de => "German",
            LangEnum.it => "Italian",
            LangEnum.tr => "Turkish",
            LangEnum.pt_pt => "Portuguese",
            LangEnum.pt_br => "Portuguese",
            LangEnum.vi => "Vietnamese",
            LangEnum.id => "Indonesian",
            LangEnum.th => "Thai",
            LangEnum.ms => "Malay",
            LangEnum.ar => "Arabic",
            LangEnum.hi => "Hindi",
            LangEnum.mn_cy => "Mongolian",
            LangEnum.mn_mo => "Mongolian",
            LangEnum.km => "Central Khmer",
            LangEnum.nb_no => "Norwegian Bokmål",
            LangEnum.nn_no => "Norwegian Nynorsk",
            LangEnum.fa => "Persian",
            LangEnum.sv => "Swedish",
            LangEnum.pl => "Polish",
            LangEnum.nl => "Dutch",
            LangEnum.uk => "Ukrainian",
            _ => "Requires you to identify automatically"
        };
    }

    #endregion Interface Implementation

    #region Support - Obsolete - LLM 不使用位置坐标

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