using System.ComponentModel;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorMicrosoft : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorMicrosoft()
        : this(Guid.NewGuid(), "https://api.cognitive.microsofttranslator.com", "微软翻译")
    {
    }

    public TranslatorMicrosoft(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Microsoft,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.MicrosoftService
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

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.Bing;

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

    [JsonIgnore] public BindingList<UserDefinePrompt> UserDefinePrompts { get; set; } = [];

    [JsonIgnore] [ObservableProperty] private bool _autoExecute = true;

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    public TranslationResult _data = TranslationResult.Reset;

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

    [JsonIgnore]
    private Dictionary<string, string> ErrorDict =>
        new()
        {
            { "400000", "某个请求输入无效。" },
            { "400001", "“scope”参数无效。" },
            { "400002", "“category”参数无效。" },
            { "400003", "语言说明符缺失或无效。" },
            { "400004", "目标脚本说明符（“To script”）缺失或无效。" },
            { "400005", "输入文本缺失或无效。" },
            { "400006", "语言和脚本的组合无效。" },
            { "400018", "源脚本说明符（“From script”）缺失或无效。" },
            { "400019", "指定的某个语言不受支持。" },
            { "400020", "输入文本数组中的某个元素无效。" },
            { "400021", "API 版本参数缺失或无效。" },
            { "400023", "指定的某个语言对无效。" },
            { "400035", "源语言（“From”字段）无效。" },
            { "400036", "目标语言（“To”字段）缺失或无效。" },
            { "400042", "指定的某个选项（“Options”字段）无效。" },
            { "400043", "客户端跟踪 ID（ClientTraceId 字段或 X-ClientTranceId 标头）缺失或无效。" },
            { "400050", "输入文本过长。 查看请求限制。" },
            { "400064", "“translation”参数缺失或无效。" },
            { "400070", "目标脚本（ToScript 参数）的数目与目标语言（To 参数）的数目不匹配。" },
            { "400071", "TextType 的值无效。" },
            { "400072", "输入文本的数组包含过多的元素。" },
            { "400073", "脚本参数无效。" },
            { "400074", "请求正文是无效的 JSON。" },
            { "400075", "语言对和类别组合无效。" },
            { "400077", "超过了最大请求大小。 查看请求限制。" },
            { "400079", "请求用于在源语言与目标语言之间进行翻译的自定义系统不存在。" },
            { "400080", "语言或脚本不支持音译。" },
            { "401000", "由于凭据缺失或无效，请求未授权。" },
            { "401015", "“提供的凭据适用于语音 API。 此请求需要文本 API 的凭据。 请使用翻译器订阅。”" },
            { "403000", "不允许执行该操作。" },
            { "403001", "由于订阅已超过其免费配额，因此不允许该操作。" },
            { "405000", "请求的资源不支持该请求方法。" },
            { "408001", "正在准备所请求的翻译系统。 请在几分钟后重试。" },
            { "408002", "等待传入流时请求超时。 客户端没有在服务器准备等待的时间内生成请求。 客户端可以在以后的任何时间重复该请求，而不做任何修改。" },
            { "415000", "Content-Type 标头缺失或无效。" },
            { "429000", " 由于客户端已超出请求限制，服务器拒绝了请求。" },
            { "429001", " 由于客户端已超出请求限制，服务器拒绝了请求。" },
            { "429002", "由于客户端已超出请求限制，服务器拒绝了请求。" },
            { "500000", "发生了意外错误。 如果该错误持续出现，请报告发生错误的日期/时间、响应标头 X-RequestId 中的请求标识符，以及请求标头 X-ClientTraceId 中的客户端标识符。" },
            {
                "503000",
                "服务暂时不可用。 请重试。 如果该错误持续出现，请报告发生错误的日期/时间、响应标头 X-RequestId 中的请求标识符，以及请求标头 X-ClientTraceId 中的客户端标识符。"
            }
        };

    #endregion Properties

    #region Service Test

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
            var ret = await TranslateAsync(reqModel, token);

            result = ret.IsSuccess ? "验证成功" : "验证失败";
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

    #endregion Service Test

    #region Interface Implementation

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
    {
        if (!Url.EndsWith("translate")) Url = Url.TrimEnd('/') + "/translate";

        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");

        //检查语种
        var source = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var target = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

        var query = new Dictionary<string, string> { { "api-version", "3.0" }, { "to", target } };

        if (!string.Equals(source, "auto", StringComparison.CurrentCultureIgnoreCase)) query.Add("from", source);

        var headers = new Dictionary<string, string>
            { { "Ocp-Apim-Subscription-Key", AppKey }, { "Ocp-Apim-Subscription-Region", AppID } };
        var body = new[] { new { text = req.Text } };
        var reqStr = JsonConvert.SerializeObject(body);

        try
        {
            var resp = await HttpUtil.PostAsync(Url, reqStr, query, headers, token).ConfigureAwait(false) ??
                       throw new Exception("请求结果为空");
            var parseData = JsonConvert.DeserializeObject<JArray>(resp) ?? throw new Exception(resp);
            var data = parseData.First()["translations"]?.FirstOrDefault()?["text"]?.ToString() ??
                       throw new Exception("请求结果为空");

            return TranslationResult.Success(data);
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
                var innErrCode = innMsg?["error"]?["code"]?.ToString();
                if (innErrCode != null && ErrorDict.TryGetValue(innErrCode, out var value))
                    msg += $" {value}";
                else
                    msg += $" {innMsg?["error"]?["message"]}";
                LogService.Logger.Error($"({Name})({Identify}) raw content:\n{innEx.Message}");
            }

            msg = msg.Trim();

            throw new Exception(msg);
        }
    }

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorMicrosoft
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            Data = TranslationResult.Reset,
            AppID = AppID,
            AppKey = AppKey,
            AutoExecute = AutoExecute,
            IdHide = IdHide,
            KeyHide = KeyHide,
            IsExecuting = IsExecuting
        };
    }

    /// <summary>
    ///     https://learn.microsoft.com/zh-cn/azure/ai-services/translator/language-support
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh-Hans",
            LangEnum.zh_tw => "zh-Hant",
            LangEnum.yue => "yue",
            LangEnum.en => "en",
            LangEnum.ja => "ja",
            LangEnum.ko => "ko",
            LangEnum.fr => "fr",
            LangEnum.es => "es",
            LangEnum.ru => "ru",
            LangEnum.de => "de",
            LangEnum.it => "it",
            LangEnum.tr => "tr",
            LangEnum.pt_pt => "pt-pt",
            LangEnum.pt_br => "pt",
            LangEnum.vi => "vi",
            LangEnum.id => "id",
            LangEnum.th => "th",
            LangEnum.ms => "ms",
            LangEnum.ar => "ar",
            LangEnum.hi => "hi",
            LangEnum.mn_cy => "mn-Cyrl",
            LangEnum.mn_mo => "mn-Mong",
            LangEnum.km => "km",
            LangEnum.nb_no => "nb",
            LangEnum.nn_no => "nb",
            LangEnum.fa => "fa",
            LangEnum.sv => "sv",
            LangEnum.pl => "pl",
            LangEnum.nl => "nl",
            LangEnum.uk => "uk",
            _ => "auto"
        };
    }

    #endregion Interface Implementation
}