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
using Exception = System.Exception;

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
    
    [JsonIgnore] [ObservableProperty] private double _usage = 0;
    
    [JsonIgnore] [ObservableProperty] private string _usageStr = string.Empty;

    #endregion Properties

    #region Commands

    [property: JsonIgnore]
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task TestAsync(CancellationToken token)
    {
        var result = "";
        var isCancel = false;
        try
        {
            var reqModel = new RequestModel("你好", LangEnum.zh_cn, LangEnum.en);
            var ret = await TranslateAsync(reqModel, token);

            result = ret.IsSuccess ? AppLanguageManager.GetString("Toast.VerifySuccess") : AppLanguageManager.GetString("Toast.VerifyFailed");
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
            if (!isCancel) ToastHelper.Show(result, WindowType.Preference);
        }
    }

    [property: JsonIgnore]
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task UsageAsync(CancellationToken token)
    {
        var result = "";
        const string path = "/v2/usage";
        var uriBuilder = new UriBuilder(Url);
        uriBuilder.Path = path;
        var authToken = string.IsNullOrEmpty(AppKey)
            ? []
            : new Dictionary<string, string> { { "Authorization", $"DeepL-Auth-Key {AppKey}" } };
        try
        {
            var resp =
                await HttpUtil.GetAsync(uriBuilder.Uri.ToString(), null, token, authToken) ??
                throw new Exception("请求结果为空");
            var parseData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception(resp);
            var count = parseData["character_count"]?.ToString() ?? throw new Exception("用量为空");
            var limit = parseData["character_limit"]?.ToString() ?? throw new Exception("用量上限为空");
            UsageStr = $"{count}/{limit}";
            Usage = double.Parse(count) / double.Parse(limit) * 100;
            result = AppLanguageManager.GetString("Toast.QuerySuccess");
        }
        catch (OperationCanceledException)
        {
            // ignored
            result = AppLanguageManager.GetString("Toast.QueryCanceled");
        }
        catch (Exception ex)
        {
            if (ex.InnerException is { } innEx) ex = innEx;
            LogService.Logger.Error($"TranslatorDeepL|UsageAsync: {ex.Message}");
            result = AppLanguageManager.GetString("Toast.QueryFailed");
        }
        finally
        {
            ToastHelper.Show(result, WindowType.Preference);
        }
    }

    #endregion

    #region Interface Implementation

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken canceltoken)
    {
        if (request is not RequestModel reqModel)
            throw new Exception($"请求数据出错: {request}");

        var convSource = LangConverter(reqModel.SourceLang) ??
                         throw new Exception($"该服务不支持{reqModel.SourceLang.GetDescription()}");
        var convTarget = TargetLangConverter(reqModel.TargetLang) ??
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
            IsExecuting = IsExecuting,
            Usage = Usage,
            UsageStr = UsageStr,
            IsTranslateBackExecuting = IsTranslateBackExecuting,
            AutoExecuteTranslateBack = AutoExecuteTranslateBack,
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

    public string? TargetLangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "ZH-HANS",
            LangEnum.zh_tw => "ZH-HANT",
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