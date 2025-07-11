using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using System.Net.Http;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorTransmartBuiltIn : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorTransmartBuiltIn()
        : this(Guid.NewGuid(), "https://transmart.qq.com/api/imt", "TranSmart")
    {
    }

    public TranslatorTransmartBuiltIn(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Transmart,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.TransmartBuiltInService
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
            if (!isCancel)
                ToastHelper.Show(result, WindowType.Preference);
        }
    }

    #endregion Translator Test

    #region Interface Implementation

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
    {
        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");

        //检查语种
        var convSource = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var convTarget = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

        var headers = new Dictionary<string, string>
        {
            { "User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.0.0 Safari/537.36" },
            { "Referer", "https://yi.qq.com/zh-CN/index" }
        };

        var reqData = new
        {
            header = new
            {
                fn = "auto_translation_block",
                client_key = "browser-chrome-110.0.0-Mac OS-df4bd4c5-a65d-44b2-a40f-42f34f3535f2-1677486696487"
            },
            type = "plain",
            model_category = "normal",
            source = new
            {
                lang = convSource,
                text_block = req.Text
            },
            target = new
            {
                lang = convTarget
            }
        };
        var reqStr = JsonConvert.SerializeObject(reqData);

        try
        {
            var resp = await HttpUtil.PostAsync(Url, reqStr, null, headers, token).ConfigureAwait(false) ?? throw new Exception("请求结果为空");
            var data = JsonConvert.DeserializeObject<JObject>(resp)?["auto_translation"]?.ToString() ?? throw new Exception(resp);
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
                msg += $" {innMsg?["message"]}";
                LogService.Logger.Error($"({Name})({Identify}) raw content:\n{innEx.Message}");
            }

            msg = msg.Trim();

            throw new Exception(msg);
        }
    }

    public ITranslator Clone()
    {
        return new TranslatorTransmartBuiltIn
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
            IsExecuting = IsExecuting,
            IsTranslateBackExecuting = IsTranslateBackExecuting,
            AutoExecuteTranslateBack = AutoExecuteTranslateBack,
        };
    }

    /// <summary>
    ///     https://docs.qq.com/doc/DY2NxUWpmdnB2RXV3
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh",
            LangEnum.zh_tw => "zh-TW",
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
            LangEnum.pt_pt => "pt",
            LangEnum.pt_br => "pt",
            LangEnum.vi => "vi",
            LangEnum.id => "id",
            LangEnum.th => "th",
            LangEnum.ms => "ms",
            LangEnum.ar => "ar",

            LangEnum.hi => null,
            LangEnum.mn_cy => null,
            LangEnum.mn_mo => null,
            LangEnum.km => null,
            LangEnum.nb_no => null,
            LangEnum.nn_no => null,
            LangEnum.fa => null,
            LangEnum.sv => null,
            LangEnum.pl => null,
            LangEnum.nl => null,
            LangEnum.uk => null,
            _ => "auto"
        };
    }

    #endregion Interface Implementation
}