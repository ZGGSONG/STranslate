using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Model;
using STranslate.Util;
using System.ComponentModel;

namespace STranslate.ViewModels.Preference.OCR;

public partial class OpenAIOCR : OCRLLMBase, IOCRLLM
{
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

    public OpenAIOCR()
        : this(Guid.NewGuid(), "https://api.openai.com", "OpenAIOCR", isEnabled: false)
    {
    }

    public OpenAIOCR(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.OpenAI,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        OCRType type = OCRType.OpenAIOCR
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

    #region Interface Implementation

    public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        var target = LangConverter(lang) ?? throw new Exception($"该服务不支持{lang.GetDescription()}");

        UriBuilder uriBuilder = new(Url);

        // 兼容旧版API: https://platform.openai.com/docs/guides/text-generation
        // 如果路径不是有效的API路径结尾，使用默认路径
        if (uriBuilder.Path == "/")
            uriBuilder.Path = "/v1/chat/completions";

        #region 构造请求数据
        // 温度限定
        var aTemperature = Math.Clamp(Temperature, 0, 2);

        var openAiModel = Model.Trim();
        var base64Str = Convert.ToBase64String(bytes);
        // https://www.volcengine.com/docs/82379/1362931#%E5%9B%BE%E7%89%87%E6%A0%BC%E5%BC%8F%E8%AF%B4%E6%98%8E
        var formatStr = (Singleton<ConfigHelper>.Instance.CurrentConfig?.OcrImageQuality ?? OcrImageQualityEnum.Medium) switch
        {
            OcrImageQualityEnum.Low => "image/jpeg",
            OcrImageQualityEnum.Medium => "image/png",
            OcrImageQualityEnum.High => "image/bmp",
            _ => "image/png"
        };

        // 选择模型
        var a_model = Model.Trim();
        a_model = string.IsNullOrEmpty(a_model) ? "gpt-4o-2024-08-06" : a_model;

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
                content = item.Content
            });
        }
        messages.Add(new
        {
            role = "user",
            content = new object[]
            {
                new
                {
                    type = "text",
                    text = userPrompt.Content
                },
                new
                {
                    type = "image_url",
                    image_url = new
                    {
                        url = $"data:{formatStr};base64,{base64Str}"
                    }
                }
            }
        });
        
        var data = new
        {
            model = openAiModel,
            messages = messages.ToArray(),
            temperature = aTemperature,
#if false
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "ocr_response",
                    strict = true,
                    schema = new
                    {
                        type = "object",
                        properties = new
                        {
                            words_result = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        words = new
                                        {
                                            type = "string"
                                        }
                                    },
                                    required = new[] { "words" },
                                    additionalProperties = false
                                }
                            }
                        },
                        required = new[] { "words_result" },
                        additionalProperties = false
                    }
                }
            } 
#endif
        };

        #endregion
        
        var jsonData = JsonConvert.SerializeObject(data);

        var headers = new Dictionary<string, string> { { "Authorization", $"Bearer {AppKey}" } };

        // 提取content的值
        var ocrResult = new OcrResult();
        var resp = await HttpUtil.PostAsync(uriBuilder.Uri.AbsoluteUri, jsonData, null, headers, cancelToken)
            .ConfigureAwait(false);
        if (string.IsNullOrEmpty(resp))
            throw new Exception("请求结果为空");

        // 解析JSON数据
        var rawData = JsonConvert.DeserializeObject<JObject>(resp)?["choices"]?[0]?["message"]?["content"] ??
                        throw new Exception($"反序列化失败: {resp}");

        foreach (var content in rawData.ToString().Split("\n").ToList().Select(item => new OcrContent(item)))
        {
            ocrResult.OcrContents.Add(content);
        }
        
        return ocrResult;
    }

    public IOCR Clone()
    {
        return new OpenAIOCR
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
            UserDefinePrompts = UserDefinePrompts.Clone(),
        };
    }

    /// <summary>
    ///     https://zh.wikipedia.org/wiki/ISO_639-1%E4%BB%A3%E7%A0%81%E5%88%97%E8%A1%A8
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
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

    #endregion

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
    }

    #endregion Support
}