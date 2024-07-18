using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorYoudao : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorYoudao()
        : this(Guid.NewGuid(), "https://openapi.youdao.com/api", "有道翻译")
    {
    }

    public TranslatorYoudao(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Youdao,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.YoudaoService
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

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.Baidu;

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

    /// <summary>
    ///     错误代码: https://fanyi.youdao.com/openapi/
    /// </summary>
    [JsonIgnore]
    private Dictionary<string, string> ErrorDict =>
        new()
        {
            { "101", "缺少必填的参数,首先确保必填参数齐全，然后确认参数书写是否正确。" },
            { "102", "不支持的语言类型" },
            { "103", "翻译文本过长" },
            { "104", "不支持的API类型" },
            { "105", "不支持的签名类型" },
            { "106", "不支持的响应类型" },
            { "107", "不支持的传输加密类型" },
            { "108", "应用ID无效，注册账号，登录后台创建应用并完成绑定，可获得应用ID和应用密钥等信息" },
            { "109", "batchLog格式不正确" },
            { "110", "无相关服务的有效应用,应用没有绑定服务应用，可以新建服务应用。注：某些服务的翻译结果发音需要tts服务，需要在控制台创建语音合成服务绑定应用后方能使用。" },
            { "111", "开发者账号无效" },
            { "112", "请求服务无效" },
            { "113", "q不能为空" },
            { "114", "不支持的图片传输方式" },
            { "116", "strict字段取值无效，请参考文档填写正确参数值" },
            { "201", "解密失败，可能为DES,BASE64,URLDecode的错误" },
            { "202", "签名检验失败,如果确认应用ID和应用密钥的正确性，仍返回202，一般是编码问题。请确保翻译文本 q 为UTF-8编码." },
            { "203", "访问IP地址不在可访问IP列表" },
            { "205", "请求的接口与应用的平台类型不一致，确保接入方式（Android SDK、IOS SDK、API）与创建的应用平台类型一致。如有疑问请参考入门指南" },
            { "206", "因为时间戳无效导致签名校验失败" },
            { "207", "重放请求" },
            { "301", "辞典查询失败" },
            { "302", "翻译查询失败" },
            { "303", "服务端的其它异常" },
            { "304", "翻译失败，请联系技术同学" },
            { "308", "rejectFallback参数错误" },
            { "309", "domain参数错误" },
            { "310", "未开通领域翻译服务" },
            { "401", "账户已经欠费，请进行账户充值" },
            { "402", "offlinesdk不可用" },
            { "411", "访问频率受限,请稍后访问" },
            { "412", "长请求过于频繁，请稍后访问" }
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
        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");

        //检查语种
        var source = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var target = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");
        var paramsMap = new Dictionary<string, string[]>
        {
            { "q", new[] { req.Text } },
            { "from", new[] { source } },
            { "to", new[] { target } }
        };
        // 添加鉴权相关参数
        YoudaoAuthV3Util.AddAuthParams(AppID, AppKey, paramsMap);
        var headers = new Dictionary<string, string[]>
            { { "Content-Type", new[] { "application/x-www-form-urlencoded" } } };

        var resp = await HttpUtil.PostAsync(Url, headers, paramsMap, token).ConfigureAwait(false) ??
                   throw new Exception("请求结果为空");

        // 解析JSON数据
        var parsedData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception($"反序列化失败: {resp}");

        var errorCode = parsedData["errorCode"]?.ToString();
        if (errorCode != null && errorCode != "0")
        {
            LogService.Logger.Error($"({Name})({Identify}) raw content:\n{resp}");
            if (ErrorDict.TryGetValue(errorCode, out var value))
                throw new Exception(value);
            throw new Exception($"错误代码({errorCode})");
        }

        // 提取content的值
        var data = parsedData["translation"]?.FirstOrDefault()?.ToString() ?? throw new Exception("未获取到结果");

        return TranslationResult.Success(data);
    }

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorYoudao
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
    ///     https://ai.youdao.com/DOCSIRMA/html/trans/api/wbfy/index.html
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
            LangEnum.yue => "yue",
            LangEnum.en => "en",
            LangEnum.ja => "jp",
            LangEnum.ko => "kor",
            LangEnum.fr => "fra",
            LangEnum.es => "spa",
            LangEnum.ru => "ru",
            LangEnum.de => "de",
            LangEnum.it => "it",
            LangEnum.tr => "tr",
            LangEnum.pt_pt => "pt",
            LangEnum.pt_br => "pt",
            LangEnum.vi => "vie",
            LangEnum.id => "id",
            LangEnum.th => "th",
            LangEnum.ms => "may",
            LangEnum.ar => "ar",
            LangEnum.hi => "hi",
            LangEnum.mn_cy => "mn",
            LangEnum.mn_mo => "mn",
            LangEnum.km => "km",
            LangEnum.nb_no => "no",
            LangEnum.nn_no => "no",
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