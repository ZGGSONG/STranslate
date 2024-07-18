using System.ComponentModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorVolcengine : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorVolcengine()
        : this(Guid.NewGuid(), "https://translate.volcengineapi.com", "火山翻译")
    {
    }

    public TranslatorVolcengine(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Volcengine,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.VolcengineService
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

        //https://github.com/Baozisoftware/go-dll/wiki/C%23%E8%B0%83%E7%94%A8Go%E7%89%88DLL#%E5%85%B3%E4%BA%8Ego%E7%9A%84%E6%95%B0%E7%BB%84%E5%88%87%E7%89%87%E8%BF%94%E5%9B%9E%E9%97%AE%E9%A2%98
        //加入这个就不崩溃了
        Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=0");

        var accessKeyBytes = Encoding.UTF8.GetBytes(AppID);
        var secretKeyBytes = Encoding.UTF8.GetBytes(AppKey);
        var sourceBytes = Encoding.UTF8.GetBytes(source);
        var targetBytes = Encoding.UTF8.GetBytes(target);
        var contentBytes = Encoding.UTF8.GetBytes(req.Text);
        var result = await Task.Run(() =>
                GoUtil.VolcengineTranslator(accessKeyBytes, secretKeyBytes, sourceBytes, targetBytes, contentBytes))
            .ConfigureAwait(false);
        var tuple = GoUtil.GoTupleToCSharpTuple(result);
        var resp = tuple.Item2 ?? throw new Exception("请求结果为空");
        if (tuple.Item1 != 200)
            throw new Exception(resp);

        // 解析JSON数据
        var parsedData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception($"反序列化失败: {resp}");

        var data = "";
        if (string.IsNullOrEmpty(parsedData["TranslationList"]?.ToString()))
        {
            LogService.Logger.Error($"({Name})({Identify}) raw content:\n{resp}");
            data = parsedData["ResponseMetadata"]?["Error"]?["Message"]?.ToString() ??
                   parsedData["ResponseMetadata"]?.ToString();
            return string.IsNullOrEmpty(data) ? TranslationResult.Fail("获取错误信息为空") : TranslationResult.Fail(data);
        }
        // 提取content的值

        data = parsedData["TranslationList"]?.FirstOrDefault()?["Translation"]?.ToString();
        return string.IsNullOrEmpty(data) ? TranslationResult.Fail("获取结果为空") : TranslationResult.Success(data);
    }

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorVolcengine
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
    ///     https://www.volcengine.com/docs/4640/35107
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh",
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
            LangEnum.pt_pt => "pt",
            LangEnum.pt_br => "pt",
            LangEnum.vi => "vi",
            LangEnum.id => "id",
            LangEnum.th => "th",
            LangEnum.ms => "ms",
            LangEnum.ar => "ar",
            LangEnum.hi => "hi",
            LangEnum.mn_cy => "mn",
            LangEnum.mn_mo => "mn",
            LangEnum.km => "km",
            LangEnum.nb_no => "nb",
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