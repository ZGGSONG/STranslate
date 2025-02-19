using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.Translator;
using STranslate.Views.Preference.Translator;

namespace STranslate.ViewModels.Preference.OCR;

public partial class OpenAIOCR : OCRBase, IOCR
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

    [JsonIgnore][ObservableProperty] private double _temperature = 1.0;

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

    #region Prompt

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    private string _model = "gpt-4o-2024-08-06";

    [JsonIgnore]
    [ObservableProperty]
    private BindingList<UserDefinePrompt> _userDefinePrompts =
    [
        new(
            "文本识别",
            [
                new Prompt("system", "You are a specialized OCR engine that accurately extracts each text from the image."),
                new Prompt("user", "Please recognize the text in the picture, the language in the picture is $target")
            ],
            true
        )
    ];

    [RelayCommand]
    [property: JsonIgnore]
    private void SelectedPrompt(List<object> obj)
    {
        var userDefinePrompt = (UserDefinePrompt)obj.First();
        foreach (var item in UserDefinePrompts) item.Enabled = false;
        userDefinePrompt.Enabled = true;
        ManualPropChanged(nameof(UserDefinePrompts));

        if (obj.Count == 2)
            Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void UpdatePrompt(UserDefinePrompt userDefinePrompt)
    {
        var dialog = new PromptDialog(ServiceType.OpenAIService, (UserDefinePrompt)userDefinePrompt.Clone());
        if (!(dialog.ShowDialog() ?? false)) return;
        var tmp = ((PromptViewModel)dialog.DataContext).UserDefinePrompt;
        userDefinePrompt.Name = tmp.Name;
        userDefinePrompt.Prompts = tmp.Prompts;
        ManualPropChanged(nameof(UserDefinePrompts));
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void DeletePrompt(UserDefinePrompt userDefinePrompt)
    {
        UserDefinePrompts.Remove(userDefinePrompt);
        ManualPropChanged(nameof(UserDefinePrompts));
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void AddPrompt()
    {
        var userDefinePrompt = new UserDefinePrompt("Undefined", []);
        var dialog = new PromptDialog(ServiceType.OpenAIService, userDefinePrompt);
        if (!(dialog.ShowDialog() ?? false)) return;
        var tmp = ((PromptViewModel)dialog.DataContext).UserDefinePrompt;
        userDefinePrompt.Name = tmp.Name;
        userDefinePrompt.Prompts = tmp.Prompts;
        UserDefinePrompts.Add(userDefinePrompt);
        ManualPropChanged(nameof(UserDefinePrompts));
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void AddPromptFromDrop(DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;
        // 取第一个文件
        var filePath = files[0];

        if (Path.GetExtension(filePath).Equals(".json", StringComparison.CurrentCultureIgnoreCase))
        {
            PromptFileHandle(filePath);
            ToastHelper.Show("导入成功", WindowType.Preference);
        }
        else
            ToastHelper.Show("请拖入Prompt文件", WindowType.Preference);
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void AddPromptFromFile()
    {
        var openFileDialog = new OpenFileDialog { Filter = "json(*.json)|*.json" };
        if (openFileDialog.ShowDialog() != true)
            return;
        PromptFileHandle(openFileDialog.FileName);
    }

    private void PromptFileHandle(string path)
    {
        var jsonStr = File.ReadAllText(path);
        try
        {
            var prompt = JsonConvert.DeserializeObject<UserDefinePrompt>(jsonStr);
            if (prompt is { Name: not null, Prompts: not null })
            {
                prompt.Enabled = false;
                UserDefinePrompts.Add(prompt);
                ManualPropChanged(nameof(UserDefinePrompts));
            }
            else
            {
                ToastHelper.Show("导入内容为空", WindowType.Preference);
            }
        }
        catch
        {
            try
            {
                var prompt = JsonConvert.DeserializeObject<List<UserDefinePrompt>>(jsonStr);
                if (prompt != null)
                {
                    foreach (var item in prompt)
                    {
                        item.Enabled = false;
                        UserDefinePrompts.Add(item);
                    }
                    ManualPropChanged(nameof(UserDefinePrompts));
                }
                else
                {
                    ToastHelper.Show("导入内容为空", WindowType.Preference);
                }
            }
            catch (Exception e)
            {
                LogService.Logger.Error($"导入Prompt失败: {e.Message}", e);
                ToastHelper.Show("导入失败", WindowType.Preference);
            }
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    private void Export()
    {
        string jsonStr;
        StringBuilder sb = new($"{Name}_Prompt_");
        if ((Keyboard.Modifiers & ModifierKeys.Control) <= 0)
        {
            var selectedPrompt = UserDefinePrompts.FirstOrDefault(x => x.Enabled);
            if (selectedPrompt == null)
            {
                ToastHelper.Show("未选择Prompt", WindowType.Preference);
                return;
            }
            jsonStr = JsonConvert.SerializeObject(selectedPrompt, Formatting.Indented);
            sb.Append(selectedPrompt.Name);
        }
        else
        {
            jsonStr = JsonConvert.SerializeObject(UserDefinePrompts, Formatting.Indented);
            sb.Append("All");
        }
        sb.Append($"_{DateTime.Now:yyyyMMddHHmmss}");
        var saveFileDialog = new SaveFileDialog { Filter = "json(*.json)|*.json", FileName = sb.ToString() };

        if (saveFileDialog.ShowDialog() != true) return;
        File.WriteAllText(saveFileDialog.FileName, jsonStr);
        ToastHelper.Show("导出成功", WindowType.Preference);
    }

    #endregion Prompt

    #endregion Properties

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
                        url = $"data:image/png;base64,{base64Str}"
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
            LangEnum.auto => "auto",
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