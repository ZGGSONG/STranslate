using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.OCR;

public partial class OpenAIOCR : ObservableObject, IOCR
{
    #region Constructor

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

    #endregion Properties

    #region Interface Implementation

    public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        UriBuilder uriBuilder = new(Url);

        // 兼容旧版API: https://platform.openai.com/docs/guides/text-generation
        if (!uriBuilder.Path.EndsWith("/v1/chat/completions"))
            uriBuilder.Path = "/v1/chat/completions";
        // 选择模型
        const string openAiModel = "gpt-4o-2024-08-06";
        var base64Str = Convert.ToBase64String(bytes);

        var jsonData = """
                       {
                          "model": "$model",
                          "messages": [
                              {
                                  "role": "system",
                                  "content": "You are a specialized OCR engine that accurately extracts each text from the image and gets its position."
                              },
                              {
                                  "role": "user",
                                  "content": [
                                      {
                                          "type": "text",
                                          "text": "Please recognize the text in the image and return its location, return by paragraph, save each paragraph as a word and each immediately following position as the top, left, width and height in pixels in the image."
                                      },
                                      {
                                          "type": "image_url",
                                          "image_url":
                                          {
                                               "url": "data:image/png;base64,$image"
                                          }
                                      }
                                  ]
                              }
                          ],
                          "response_format": {
                              "type": "json_schema",
                              "json_schema": {
                                  "name": "ocr_response",
                                  "strict": true,
                                  "schema": {
                                      "type": "object",
                                      "properties": {
                                          "words_result": {
                       "type": "array",
                       "items": {
                           "type": "object",
                           "properties": {
                               "words": {
                                   "type": "string"
                               },
                               "location": {
                                   "type": "object",
                                   "properties": {
                                       "top": {
                                           "type": "number"
                                       },
                                       "left": {
                                           "type": "number"
                                       },
                                       "width": {
                                           "type": "number"
                                       },
                                       "height": {
                                           "type": "number"
                                       }
                                   },
                                   "required": [
                                       "top",
                                       "left",
                                       "width",
                                       "height"
                                   ],
                                   "additionalProperties": false
                               }
                           },
                           "required": [
                               "words",
                               "location"
                           ],
                           "additionalProperties": false
                       }
                                          }
                                      },
                                      "required": [
                                          "words_result"
                                      ],
                                      "additionalProperties": false
                                  }
                              }
                          }
                       }
                       """;
        jsonData = jsonData.Replace("$model", openAiModel).Replace("$image", base64Str);

        var headers = new Dictionary<string, string> { { "Authorization", $"Bearer {AppKey}" } };

        //TODO: 暂不使用
        var target = LangConverter(lang) ?? throw new Exception($"该服务不支持{lang.GetDescription()}");

        // 提取content的值
        var ocrResult = new OcrResult();
        try
        {
            var resp = await HttpUtil.PostAsync(uriBuilder.Uri.AbsoluteUri, jsonData, null, headers, cancelToken);
            if (string.IsNullOrEmpty(resp))
                throw new Exception("请求结果为空");

            // 解析JSON数据
            var rawData = JsonConvert.DeserializeObject<JObject>(resp)?["choices"]?[0]?["message"]?["content"] ??
                          throw new Exception($"反序列化失败: {resp}");
            var parsedData = JsonConvert.DeserializeObject<Root>(rawData.ToString()) ??
                             throw new Exception($"反序列化失败: {resp}");

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
        }
        catch (OperationCanceledException)
        {
            ocrResult.Success = false;
            ocrResult.ErrorMsg = "请求取消...";
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException is { } innEx)
            {
                var innMsg = JsonConvert.DeserializeObject<JObject>(innEx.Message);
                msg += $" {innMsg?["error"]?["message"]}";
                LogService.Logger.Error($"({Name})({Identify}) raw content:\n{innEx.Message}");
            }

            ocrResult.Success = false;
            ocrResult.ErrorMsg = msg.Trim();
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
            Icons = Icons
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
    }

    #endregion Baidu Offcial Support
}