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

public partial class TranslatorDeepL : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorDeepL()
        : this(Guid.NewGuid(), "https://api-free.deepl.com/v2/translate", "DeepL")
    {
    }

    public TranslatorDeepL(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.DeepL,
        string appId = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.DeepLService
    )
    {
        Identify = guid;
        Url = url;
        Name = name;
        Icon = icon;
        AppID = appId;
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

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.DeepL;

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

    private RelayCommand<string>? _showEncryptInfoCommand;

    [JsonIgnore]
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        _showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

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
            if (!isCancel) ToastHelper.Show(result, WindowType.Preference);
        }
    }

    #endregion Service Test

    #region Interface Implementation

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken canceltoken)
    {
        if (request is not RequestModel reqModel)
            throw new Exception($"请求数据出错: {request}");

        var convSource = LangConverter(reqModel.SourceLang) ??
                         throw new Exception($"该服务不支持{reqModel.SourceLang.GetDescription()}");
        var convTarget = LangConverter(reqModel.TargetLang) ??
                         throw new Exception($"该服务不支持{reqModel.TargetLang.GetDescription()}");

        object preReq;
        if (convSource.Equals("auto"))
            preReq = new { text = new[] { reqModel.Text }, target_lang = convTarget };
        else
            preReq = new
            {
                text = new[] { reqModel.Text },
                target_lang = convTarget,
                source_lang = convSource
            };

        var req = JsonConvert.SerializeObject(preReq);

        var authToken = string.IsNullOrEmpty(AppKey)
            ? []
            : new Dictionary<string, string> { { "Authorization", $"DeepL-Auth-Key {AppKey}" } };

        try
        {
            var resp = await HttpUtil.PostAsync(Url, req, null, authToken, canceltoken).ConfigureAwait(false) ??
                       throw new Exception("请求结果为空");
            var parseData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception(resp);
            var data = parseData["translations"]?.FirstOrDefault()?["text"]?.ToString();

            return string.IsNullOrEmpty(data) ? TranslationResult.Fail("获取结果为空") : TranslationResult.Success(data);
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
                msg += $" {innMsg?["message"]}";
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
        return new TranslatorDeepL
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
            IsExecuting = IsExecuting
        };
    }

    /// <summary>
    ///     https://developers.deepl.com/docs/v/zh/resources/supported-languages#source-languages
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "ZH",
            LangEnum.zh_tw => "ZH",
            LangEnum.yue => "ZH",
            LangEnum.en => "EN",
            LangEnum.ja => "JA",
            LangEnum.ko => "KO",
            LangEnum.fr => "FR",
            LangEnum.es => "ES",
            LangEnum.ru => "RU",
            LangEnum.de => "DE",
            LangEnum.it => "IT",
            LangEnum.tr => "TR",
            LangEnum.pt_pt => "PT-PT",
            LangEnum.pt_br => "PT-BR",
            LangEnum.vi => null,
            LangEnum.id => "ID",
            LangEnum.th => null,
            LangEnum.ms => null,
            LangEnum.ar => "AR",
            LangEnum.hi => null,
            LangEnum.mn_cy => null,
            LangEnum.mn_mo => null,
            LangEnum.km => null,
            LangEnum.nb_no => "NB",
            LangEnum.nn_no => "NB",
            LangEnum.fa => null,
            LangEnum.sv => "SV",
            LangEnum.pl => "PL",
            LangEnum.nl => "NL",
            LangEnum.uk => null,
            _ => "auto"
        };
    }

    #endregion Interface Implementation
}