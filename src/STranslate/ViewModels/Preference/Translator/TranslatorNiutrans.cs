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

public partial class TranslatorNiutrans : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorNiutrans()
        : this(Guid.NewGuid(), "http://api.niutrans.com/NiuTransServer/translation", "小牛翻译")
    {
    }

    public TranslatorNiutrans(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Niutrans,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.NiutransService
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

    [JsonIgnore]
    private Dictionary<string, string> ErrorDict =>
        new()
        {
            { "10000", "输入为空" },
            { "10001", "请求频繁，超出QPS限制" },
            { "10003", "请求字符串长度超过限制" },
            { "10005", "源语编码有问题，非UTF-8" },
            { "13001", "字符流量不足或者没有访问权限" },
            { "13002", "'apikey'参数不可以是空" },
            { "13003", "内容过滤异常" },
            { "13007", "语言不支持" },
            { "13008", "请求处理超时" },
            { "14001", "分句异常" },
            { "14002", "分词异常" },
            { "14003", "后处理异常" },
            { "14004", "对齐失败，不能够返回正确的对应关系" },
            { "000000", "请求参数有误，请检查参数" },
            { "000001", "Content-Type不支持【multipart/form-data】" }
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
        var queryparams = new Dictionary<string, string>
        {
            { "from", source },
            { "to", target },
            { "src_text", req.Text },
            { "apikey", AppKey }
        };

        var resp = await HttpUtil.GetAsync(Url, queryparams, token).ConfigureAwait(false) ??
                   throw new Exception("请求结果为空");

        // 解析JSON数据
        var parsedData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception($"反序列化失败: {resp}");

        //{ "error_msg":"apikey error OR apikey unauthorized OR service package running out","apikey":"xx","src_text":"xx","error_code":"13001","from":"auto","to":"en"}
        var errorCode = parsedData["error_code"]?.ToString();
        if (errorCode != null)
        {
            LogService.Logger.Error($"({Name})({Identify}) raw content:\n{resp}");
            if (ErrorDict.TryGetValue(errorCode, out var value))
                throw new Exception(value);
            throw new Exception(parsedData["error_msg"]?.ToString());
        }

        // 提取content的值
        var data = parsedData["tgt_text"]?.ToString() ?? throw new Exception("未获取到结果");

        return TranslationResult.Success(data);
    }

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorNiutrans
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
    ///     https://niutrans.com/documents/contents/trans_text#languageList
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh",
            LangEnum.zh_tw => "cht",
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
            LangEnum.pt_pt => "pt",
            LangEnum.pt_br => "pt",
            LangEnum.vi => "vi",
            LangEnum.id => "id",
            LangEnum.th => "th",
            LangEnum.ms => "ms",
            LangEnum.ar => "ar",
            LangEnum.hi => "hi",
            LangEnum.mn_cy => "mn",
            LangEnum.mn_mo => "mo",
            LangEnum.km => "km",
            LangEnum.nb_no => "nb",
            LangEnum.nn_no => "nn",
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