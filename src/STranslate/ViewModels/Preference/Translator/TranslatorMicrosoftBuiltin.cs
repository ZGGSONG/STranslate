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

public partial class TranslatorMicrosoftBuiltin : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorMicrosoftBuiltin() : this(Guid.NewGuid(), "https://st-ms.deno.dev/translate", "Microsoft")
    {
    }

    public TranslatorMicrosoftBuiltin(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Microsoft,
        string appId = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.MicrosoftBuiltinService
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

    private const string ApiEndpoint = "api.cognitive.microsofttranslator.com";
    private const string ApiVersion = "3.0";
    private const int MaxTextLength = 1000;

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
            IsTesting = false;
            if (!isCancel) ToastHelper.Show(result, WindowType.Preference);
        }
    }

    #endregion Translator Test

    #region Interface Implementation

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken canceltoken)
    {
        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");
        if (req.Text.Length > MaxTextLength)
            throw new ArgumentException($"文本长度不能超过{MaxTextLength}个字符");

        var convSource = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var convTarget = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

        string url = $"{ApiEndpoint}/translate?api-version={ApiVersion}&to={convTarget}";
        if (!string.IsNullOrEmpty(convSource))
        {
            url += $"&from={convSource}";
        }

        var headers = new Dictionary<string, string>
        {
            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36" },
            { "X-MT-Signature", MicrosoftAuthUtil.GetSignature(url) }
        };

        var reqStr = JsonConvert.SerializeObject(new[] { new { req.Text } });

        try
        {
            var resp = await HttpUtil.PostAsync($"https://{url}", reqStr, null, headers, canceltoken).ConfigureAwait(false) ?? throw new Exception("请求结果为空");
            var data = JsonConvert.DeserializeObject<JArray>(resp)?.FirstOrDefault()?["translations"]?.FirstOrDefault()?["text"]?.ToString() ?? throw new Exception(resp);
            return TranslationResult.Success(data);
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
                msg += $" {innMsg?["error"]}";
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
        return new TranslatorGoogleBuiltin
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
            IsTranslateBackExecuting = IsTranslateBackExecuting,
            AutoExecuteTranslateBack = AutoExecuteTranslateBack,
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
            LangEnum.yue => null,
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
            LangEnum.hi => null,
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