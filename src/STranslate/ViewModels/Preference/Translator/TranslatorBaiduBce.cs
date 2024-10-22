using System.ComponentModel;
using System.IO;
using System.Net.Http;
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
using STranslate.Views.Preference.Translator;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorBaiduBce : TranslatorBase, ITranslatorLlm
{
    #region Constructor

    public TranslatorBaiduBce()
        : this(Guid.NewGuid(), "https://aip.baidubce.com/rpc/2.0/ai_custom/v1", "百度千帆")
    {
    }

    public TranslatorBaiduBce(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.BaiduBce,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.BaiduBceService
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

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.Bing;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    private string _url = string.Empty;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    private string _appID = string.Empty;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    private string _appKey = string.Empty;

    [JsonIgnore] [ObservableProperty] private bool _autoExecute = true;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    private string _model = "ernie_speed";

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private TranslationResult _data = TranslationResult.Reset;

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _isExecuting;

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

    [JsonIgnore] [ObservableProperty] private BindingList<UserDefinePrompt> _userDefinePrompts =
    [
        new UserDefinePrompt(
            "翻译",
            [
                new Prompt(
                    "user",
                    "You are a professional, authentic translation engine. You only return the translated text, without any explanations.\nPlease translate  into zh-cn (avoid explaining the original text):\r\n\r\nhello world"
                ),
                new Prompt("assistant", "你好世界"),
                new Prompt("user",
                    "Please translate  into $target (avoid explaining the original text):\r\n\r\n$content")
            ],
            true
        ),
        new UserDefinePrompt(
            "润色",
            [
                new Prompt("user",
                    "You are a text embellisher, you can only embellish the text, never interpret it.Embellish the following text in $source: $content")
            ]
        ),
        new UserDefinePrompt(
            "总结",
            [
                new Prompt("user",
                    "You are a text summarizer, you can only summarize the text, never interpret it. Summarize the following text in $source: $content")
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

        // 选择模型
        var a_model = Model.Trim();
        a_model = string.IsNullOrEmpty(a_model) ? "ernie_speed" : a_model;

        // 替换Prompt关键字
        var a_messages =
            (UserDefinePrompts.FirstOrDefault(x => x.Enabled)?.Prompts ?? throw new Exception("请先完善Propmpt配置")).Clone();
        a_messages.ToList().ForEach(item =>
            item.Content = item.Content.Replace("$source", source).Replace("$target", target)
                .Replace("$content", content));

        // 温度限定
        var a_temperature = Math.Clamp(Temperature, 0, 2);

        #region 获取accesstoken

        var accessToken = string.Empty;
        try
        {
            var accessTokenUrl = "https://aip.baidubce.com/oauth/2.0/token";
            var formData = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", AppID },
                { "client_secret", AppKey }
            };
            var resp = await HttpUtil.PostAsync(accessTokenUrl, formData, token);
            accessToken = JObject.Parse(resp)?["access_token"]?.ToString() ??
                          throw new Exception("get accesstoken is null");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException is Exception innEx)
            {
                var innMsg = JsonConvert.DeserializeObject<JObject>(innEx.Message);
                msg += $" {innMsg?["error_description"]}";
                LogService.Logger.Error($"({Name})({Identify}) raw content:\n{innEx.Message}");
            }

            msg = msg.Trim();

            throw new Exception(msg);
        }

        #endregion 获取accesstoken

        // 构建请求数据
        var reqData = new
        {
            messages = a_messages,
            temperature = a_temperature,
            stream = true
        };

        var jsonData = JsonConvert.SerializeObject(reqData);

        var a_url = $"{Url}/wenxinworkshop/chat/{a_model}?access_token={accessToken}";

        try
        {
            await HttpUtil.PostAsync(
                new Uri(a_url),
                jsonData,
                null,
                msg =>
                {
                    if (string.IsNullOrEmpty(msg?.Trim()))
                        return;

                    var preprocessString = msg.Replace("data:", "").Trim();

                    // 解析JSON数据
                    var parsedData = JsonConvert.DeserializeObject<JObject>(preprocessString);

                    if (parsedData is null)
                        return;

                    if (!string.IsNullOrEmpty(parsedData["error_msg"]?.ToString()))
                        throw new Exception("", new Exception(parsedData?.ToString()));

                    // 提取content的值
                    var contentValue = parsedData["result"]?.ToString();

                    if (string.IsNullOrEmpty(contentValue))
                        return;

                    onDataReceived?.Invoke(contentValue);

                    // 结束
                    if (bool.TryParse(parsedData["is_end"]?.ToString(), out var isend) && isend)
                        return;
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
                var innMsg = JsonConvert.DeserializeObject<JObject>(innEx.Message);
                msg += $" {innMsg?["error_msg"]}";
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
        return new TranslatorBaiduBce
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
            UserDefinePrompts = UserDefinePrompts.Clone(),
            AutoExecute = AutoExecute,
            IdHide = IdHide,
            KeyHide = KeyHide,
            Model = Model,
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