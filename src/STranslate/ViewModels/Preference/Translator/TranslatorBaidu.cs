using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorBaidu : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorBaidu()
        : this(Guid.NewGuid(), "https://fanyi-api.baidu.com/api/trans/vip/translate", "百度翻译")
    {
    }

    public TranslatorBaidu(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Baidu,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.BaiduService
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

    [JsonIgnore] public BindingList<UserDefinePrompt> UserDefinePrompts { get; set; } = [];

    [JsonIgnore] [ObservableProperty] private bool _autoExecute = true;

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
        if (obj == null)
            return;

        if (obj.Equals(nameof(AppID)))
            IdHide = !IdHide;
        else if (obj.Equals(nameof(AppKey))) KeyHide = !KeyHide;
    }

    private RelayCommand<string>? showEncryptInfoCommand;

    [JsonIgnore]
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

    #endregion Show/Hide Encrypt Info

    /// <summary>
    ///     错误代码: https://fanyi-api.baidu.com/product/113
    /// </summary>
    [JsonIgnore]
    private Dictionary<string, string> ErrorDict =>
        new()
        {
            { "52000", "成功" },
            { "52001", "请求超时" },
            { "52002", "系统错误" },
            { "52003", "未授权用户" },
            { "54000", "必填参数为空" },
            { "54001", "签名错误" },
            { "54003", "访问频率受限" },
            { "54004", "账户余额不足" },
            { "54005", "长query请求频繁" },
            { "58000", "客户端IP非法" },
            { "58001", "译文语言方向不支持" },
            { "58002", "服务当前已关闭" },
            { "90107", "认证未通过或未生效" }
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
            if (!isCancel) ToastHelper.Show(result, WindowType.Preference);
        }
    }

    #endregion Service Test

    #region Interface Implementation

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
    {
        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");

        //检查语种
        var convSource = LangConverter(req.SourceLang) ??
                         throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var convTarget = LangConverter(req.TargetLang) ??
                         throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

        var salt = new Random().Next(100000).ToString();
        var sign = StringUtil.EncryptString(AppID + req.Text + salt + AppKey);

        var queryparams = new Dictionary<string, string>
        {
            { "q", req.Text },
            { "from", convSource },
            { "to", convTarget },
            { "appid", AppID },
            { "salt", salt },
            { "sign", sign }
        };

        var resp = await HttpUtil.GetAsync(Url, queryparams, token).ConfigureAwait(false) ??
                   throw new Exception("请求结果为空");

        var parseData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception(resp);
        var errorCode = parseData["error_code"]?.ToString();
        if (errorCode != null)
        {
            if (ErrorDict.TryGetValue(errorCode, out var value)) throw new Exception(value);
            throw new Exception(parseData["error_msg"]?.ToString());
        }

        var dsts = parseData["trans_result"]?.Select(x => x["dst"]?.ToString()) ?? throw new Exception("未获取到结果");
        var data = string.Join(Environment.NewLine, dsts);

        return TranslationResult.Success(data);
    }

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorBaidu
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
    ///     https://fanyi-api.baidu.com/product/113
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
            LangEnum.ja => "jp",
            LangEnum.ko => "kor",
            LangEnum.fr => "fra",
            LangEnum.es => "spa",
            LangEnum.ru => "ru",
            LangEnum.de => "de",
            LangEnum.it => "it",
            LangEnum.tr => "tr",
            LangEnum.pt_pt => "pt",
            LangEnum.pt_br => "pot",
            LangEnum.vi => "vie",
            LangEnum.id => "id",
            LangEnum.th => "th",
            LangEnum.ms => "may",
            LangEnum.ar => "ar",
            LangEnum.hi => "hi",
            LangEnum.mn_cy => null,
            LangEnum.mn_mo => null,
            LangEnum.km => "hkm",
            LangEnum.nb_no => "nob",
            LangEnum.nn_no => "nno",
            LangEnum.fa => "per",
            LangEnum.sv => "swe",
            LangEnum.pl => "pl",
            LangEnum.nl => "nl",
            LangEnum.uk => "ukr",
            _ => "auto"
        };
    }

    #endregion Interface Implementation
}