using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Model;
using STranslateDLL;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorSTranslate : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorSTranslate()
        : this(Guid.NewGuid(), "", "STranslate")
    {
    }

    public TranslatorSTranslate(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.STranslate,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.STranslateService
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

    [JsonIgnore] [ObservableProperty] private STranslateMode sTranslateMode = STranslateMode.IOS;

    [JsonIgnore] public string Tips { get; set; } = "本地服务，无需配置";

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
        var source = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var target = TargetLangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

        var resp = STranslateMode switch
        {
            STranslateMode.IOS => await LocalModeIOS.ExecuteAsync(req.Text, source, target, token).ConfigureAwait(false),
            _ => await LocalMode.ExecuteAsync(req.Text, source, target, token).ConfigureAwait(false)
        } ?? throw new Exception("请求结果为空");

        // 解析JSON数据
        var parsedData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception($"反序列化失败: {resp}");

        // 提取content的值
        var data = parsedData["Data"]?.ToString() ?? throw new Exception("未获取到结果");

        return TranslationResult.Success(data);
    }

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorSTranslate
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
            Tips = Tips,
            IsExecuting = IsExecuting,
            IsTranslateBackExecuting = IsTranslateBackExecuting,
            AutoExecuteTranslateBack = AutoExecuteTranslateBack,
            STranslateMode = STranslateMode,
        };
    }

    /// <summary>
    ///     https://developers.deepl.com/docs/zh/resources/supported-languages#source-languages
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