using System.ComponentModel;
using System.Net.Http;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorChatglm : TranslatorLLMBase, ITranslatorLLM
{
    #region Constructor

    public TranslatorChatglm()
        : this(Guid.NewGuid(), "https://open.bigmodel.cn/api/paas/v4/chat/completions", "智谱AI")
    {
    }

    public TranslatorChatglm(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Chatglm,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.ChatglmService
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

    [JsonIgnore]
    private BindingList<UserDefinePrompt> _userDefinePrompts =
    [
        new UserDefinePrompt(
            "翻译",
            [
                new Prompt(
                    "user",
                    "You are a professional translation engine, please translate the text into a colloquial, professional, elegant and fluent content, without the style of machine translation. You must only translate the text content, never interpret it."
                ),
                new Prompt("assistant", "Ok, I will only translate the text content, never interpret it."),
                new Prompt("user", "Translate the following text from en to zh: hello world"),
                new Prompt("assistant", "你好，世界"),
                new Prompt("user", "Translate the following text from $source to $target: $content")
            ],
            true
        ),
        new UserDefinePrompt(
            "润色",
            [
                new Prompt("user", "You are a text embellisher, you can only embellish the text, never interpret it."),
                new Prompt("assistant", "Ok, I will only embellish the text, never interpret it."),
                new Prompt("user", "Embellish the following text in $source: $content")
            ]
        ),
        new UserDefinePrompt(
            "总结",
            [
                new Prompt("user", "You are a text summarizer, you can only summarize the text, never interpret it."),
                new Prompt("assistant", "Ok, I will only summarize the text, never interpret it."),
                new Prompt("user", "Summarize the following text in $source: $content")
            ]
        )
    ];
    public override BindingList<UserDefinePrompt> UserDefinePrompts
    {
        get => _userDefinePrompts;
        set => SetProperty(ref _userDefinePrompts, value);
    }

    #endregion Properties

    #region Translator Test

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
            await TranslateAsync(reqModel, _ => result = AppLanguageManager.GetString("Toast.VerifySuccess"), token);
        }
        catch (OperationCanceledException)
        {
            isCancel = true;
        }
        catch (Exception)
        {
            result = AppLanguageManager.GetString("Toast.VerifyFailed");
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

        // 兼容旧版API: https://open.bigmodel.cn/dev/api#glm-4
        if (uriBuilder.Path == "/")
            uriBuilder.Path = "/api/paas/v4/chat/completions";

        // 选择模型
        var a_model = Model.Trim();
        a_model = string.IsNullOrEmpty(a_model) ? "glm-4" : a_model;

        // 替换Prompt关键字
        var a_messages =
            (UserDefinePrompts.FirstOrDefault(x => x.Enabled)?.Prompts ?? throw new Exception("请先完善Propmpt配置")).Clone();
        a_messages.ToList().ForEach(item =>
            item.Content = item.Content.Replace("$source", source).Replace("$target", target)
                .Replace("$content", content));

        // 温度限定，文档中要求0-1之间
        var a_temperature = Math.Clamp(Temperature, 0, 1);

        // 构建请求数据
        var reqData = new
        {
            model = a_model,
            messages = a_messages,
            temperature = a_temperature,
            stream = true
        };

        var jsonData = JsonConvert.SerializeObject(reqData);

        var auth = ChatglmAuthenicationUtil.GenerateToken(AppKey, 60);

        try
        {
            await HttpUtil.PostAsync(
                uriBuilder.Uri,
                jsonData,
                auth,
                msg =>
                {
                    if (string.IsNullOrEmpty(msg?.Trim()))
                        return;

                    var preprocessString = msg.Replace("data:", "").Trim();

                    // 结束标记
                    if (preprocessString.Equals("[DONE]"))
                        return;

                    // 解析JSON数据
                    var parsedData = JsonConvert.DeserializeObject<JObject>(preprocessString);

                    if (parsedData is null)
                        return;

                    // 提取content的值
                    var contentValue = parsedData["choices"]?.FirstOrDefault()?["delta"]?["content"]?.ToString();

                    if (string.IsNullOrEmpty(contentValue))
                        return;

                    onDataReceived?.Invoke(contentValue);
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
                msg += $" {innMsg?["error"]?["message"]}";
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
        return new TranslatorChatglm
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