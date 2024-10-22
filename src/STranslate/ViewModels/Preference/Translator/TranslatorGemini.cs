using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
using STranslate.Views.Preference.Translator;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorGemini : TranslatorBase, ITranslatorLlm
{
    #region Constructor

    public TranslatorGemini()
        : this(Guid.NewGuid(), "https://generativelanguage.googleapis.com", "Gemini")
    {
    }

    public TranslatorGemini(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Gemini,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.GeminiService
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

    [JsonIgnore] [ObservableProperty] private ServiceType _type = 0;
    
    [JsonIgnore] [ObservableProperty] private double _temperature = 1.0;

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.Gemini;

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

    [JsonIgnore] [ObservableProperty] private bool _autoExecute = true;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    private string _model = "gemini-pro";

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    public TranslationResult _data = TranslationResult.Reset;

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _isExecuting;

    #region Show/Hide Encrypt Info

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _keyHide = true;

    private void ShowEncryptInfo()
    {
        KeyHide = !KeyHide;
    }

    private RelayCommand? showEncryptInfoCommand;

    [JsonIgnore]
    public IRelayCommand ShowEncryptInfoCommand => showEncryptInfoCommand ??= new RelayCommand(ShowEncryptInfo);

    #endregion Show/Hide Encrypt Info

    #region Prompt

    [JsonIgnore] [ObservableProperty] private BindingList<UserDefinePrompt> _userDefinePrompts =
    [
        new UserDefinePrompt(
            "翻译",
            [
                new Prompt(
                    "user",
                    "You are a professional translation engine, please translate the text into a colloquial, professional, elegant and fluent content, without the style of machine translation. You must only translate the text content, never interpret it."
                ),
                new Prompt("model", "Ok, I will only translate the text content, never interpret it"),
                new Prompt("user", "Translate the following text from en to zh: hello world"),
                new Prompt("model", "你好，世界"),
                new Prompt("user", "Translate the following text from $source to $target: $content")
            ],
            true
        ),
        new UserDefinePrompt(
            "润色",
            [
                new Prompt("user", "You are a text embellisher, you can only embellish the text, never interpret it."),
                new Prompt("model", "Ok, I will only embellish the text, never interpret it."),
                new Prompt("user", "Embellish the following text in $source: $content")
            ]
        ),
        new UserDefinePrompt(
            "总结",
            [
                new Prompt("user", "You are a text summarizer, you can only summarize the text, never interpret it."),
                new Prompt("model", "Ok, I will only summarize the text, never interpret it."),
                new Prompt("user", "Summarize the following text in $source: $content")
            ]
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
        var dialog = new PromptDialog(ServiceType.GeminiService, (UserDefinePrompt)userDefinePrompt.Clone());
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
        var dialog = new PromptDialog(ServiceType.GeminiService, userDefinePrompt);
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

    #region Translator Test

    [property: JsonIgnore] [ObservableProperty]
    private bool _isTesting;

    [property: JsonIgnore]
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task TestAsync(CancellationToken token)
    {
        var result = "";
        var isCancel = false;
        try
        {
            IsTesting = true;
            var reqModel = new RequestModel("你好", LangEnum.zh_cn, LangEnum.en);
            await TranslateAsync(reqModel, _ => result = "验证成功", token);
        }
        catch (OperationCanceledException)
        {
            isCancel = true;
        }
        catch (Exception)
        {
            result = "验证失败";
        }
        finally
        {
            IsTesting = false;
            if (!isCancel)
                ToastHelper.Show(result, WindowType.Preference);
        }
    }

    #endregion Translator Test

    #region Interface Implementation

    public async Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        if (string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(AppKey))
            throw new Exception("请先完善配置");

        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");

        //检查语种
        var source = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var target = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");
        var content = req.Text;

        UriBuilder uriBuilder = new(Url);

        // 选择模型
        var a_model = Model.Trim();
        a_model = string.IsNullOrEmpty(a_model) ? "gemini-pro" : a_model;

        if (!uriBuilder.Path.EndsWith($"/v1beta/models/{a_model}:streamGenerateContent"))
            uriBuilder.Path = $"/v1beta/models/{a_model}:streamGenerateContent";

        uriBuilder.Query = $"key={AppKey}";

        // 替换Prompt关键字
        var a_messages =
            (UserDefinePrompts.FirstOrDefault(x => x.Enabled)?.Prompts ?? throw new Exception("请先完善Propmpt配置")).Clone();
        a_messages.ToList().ForEach(item =>
            item.Content = item.Content.Replace("$source", source).Replace("$target", target)
                .Replace("$content", content));

        // 温度限定
        var a_temperature = Math.Clamp(Temperature, 0, 2);
        
        // 构建请求数据
        var reqData = new
        {
            contents = a_messages.Select(e => new { role = e.Role, parts = new[] { new { text = e.Content } } }),
            generationConfig = new { temperature = a_temperature },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE"},         //骚扰内容。
                new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE"},        //仇恨言论和内容。
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE"},  //露骨色情内容。
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE"},  //危险内容。
            }
        };

        // 为了流式输出与MVVM还是放这里吧
        var jsonData = JsonConvert.SerializeObject(reqData);

        try
        {
            await HttpUtil.PostAsync(
                uriBuilder.Uri,
                jsonData,
                null,
                msg =>
                {
                    // 使用正则表达式提取目标字符串
                    var pattern = "(?<=\"text\": \")[^\"]+(?=\")";

                    var match = Regex.Match(msg, pattern);

                    if (match.Success) onDataReceived?.Invoke(match.Value.Replace("\\n", "\n"));
                },
                token
            ).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == null)
        {
            var msg = $"请检查服务是否可以正常访问: {Name} ({Url}).";
            throw new HttpRequestException(msg);
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException is { } innEx)
            {
                var innMsg = JsonConvert.DeserializeObject<JArray>(innEx.Message);
                msg += $" {innMsg?.FirstOrDefault()?["error"]?["message"]}";
                LogService.Logger.Error($"({Name})({Identify}) raw content:\n{innEx.Message}");
            }

            msg = msg.Trim();

            throw new Exception(msg);
        }
    }

    public Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorGemini
        {
            Identify = Identify,
            Type = Type,
            Temperature = Temperature,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            Data = TranslationResult.Reset,
            AppID = AppID,
            AppKey = AppKey,
            Model = Model,
            UserDefinePrompts = UserDefinePrompts.Clone(),
            AutoExecute = AutoExecute,
            KeyHide = KeyHide,
            IsExecuting = IsExecuting,
            IsTranslateBackExecuting = IsTranslateBackExecuting,
            AutoExecuteTranslateBack = AutoExecuteTranslateBack,
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
}