using AlibabaCloud.OpenApiClient.Models;
using AlibabaCloud.SDK.Alimt20181012;
using AlibabaCloud.SDK.Alimt20181012.Models;
using AlibabaCloud.TeaUtil.Models;
using CommunityToolkit.Mvvm.Input;
using STranslate.Helper;
using STranslate.Model;
using Task = System.Threading.Tasks.Task;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorAli : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorAli()
        : this(Guid.NewGuid(), "https://mt.cn-hangzhou.aliyuncs.com", "阿里翻译")
    {
    }

    public TranslatorAli(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Ali,
        string appId = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.AliService
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

    #region Translator Test

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

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
    {
        if (request is not RequestModel reqModel)
            throw new Exception($"请求数据出错: {request}");

        //检查语种
        var convSource = LangConverter(reqModel.SourceLang) ??
                         throw new Exception($"该服务不支持{reqModel.SourceLang.GetDescription()}");
        var convTarget = LangConverter(reqModel.TargetLang) ??
                         throw new Exception($"该服务不支持{reqModel.TargetLang.GetDescription()}");

        // 请确保代码运行环境设置了环境变量 ALIBABA_CLOUD_ACCESS_KEY_ID 和 ALIBABA_CLOUD_ACCESS_KEY_SECRET。
        // 工程代码泄露可能会导致 AccessKey 泄露，并威胁账号下所有资源的安全性。以下代码示例使用环境变量获取 AccessKey 的方式进行调用，仅供参考，建议使用更安全的 STS 方式，更多鉴权访问方式请参见：https://help.aliyun.com/document_detail/378671.html
        var client = CreateClient(AppID, AppKey, Url);
        TranslateGeneralRequest translateGeneralRequest =
            new()
            {
                FormatType = "text",
                SourceLanguage = convSource,
                TargetLanguage = convTarget,
                SourceText = reqModel.Text,
                Scene = "general"
            };
        RuntimeOptions runtime = new();

        var resp = await client.TranslateGeneralWithOptionsAsync(translateGeneralRequest, runtime)
            .ConfigureAwait(false);

        var data = resp.Body.Data.Translated;
        data = data.Length == 0 ? throw new Exception("请求结果为空") : data;

        return TranslationResult.Success(data);
    }

    public ITranslator Clone()
    {
        return new TranslatorAli
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
    ///     https://help.aliyun.com/zh/machine-translation/support/supported-languages-and-codes?spm=a2c4g.158269.0.0.ddfc4f62vEpa38
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh",
            LangEnum.zh_tw => "zh-tw",
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
            LangEnum.nb_no => "no",
            LangEnum.nn_no => "no",
            LangEnum.fa => "fa",
            LangEnum.sv => "sv",
            LangEnum.pl => "pl",
            LangEnum.nl => "nl",
            LangEnum.uk => null,
            _ => "auto"
        };
    }

    #endregion Interface Implementation

    #region Support

    /**
     * 使用AK & SK初始化账号Client
     * @param accessKeyId
     * @param accessKeySecret
     * @param url
     * @return Client
     * @throws Exception
     */
    private static Client CreateClient(string accessKeyId, string accessKeySecret, string url)
    {
        if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            // 删除 "https://"
            url = new string(url.Skip("https://".Length).ToArray());

        Config config =
            new()
            {
                // 必填，您的 AccessKey ID
                AccessKeyId = accessKeyId,
                // 必填，您的 AccessKey Secret
                AccessKeySecret = accessKeySecret,
                // Endpoint 请参考 https://api.aliyun.com/product/alimt
                Endpoint = url
            };
        return new Client(config);
    }

    #endregion
}