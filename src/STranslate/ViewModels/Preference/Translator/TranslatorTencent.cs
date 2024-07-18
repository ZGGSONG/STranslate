using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Model;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Tmt.V20180321;
using TencentCloud.Tmt.V20180321.Models;
using Task = System.Threading.Tasks.Task;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorTencent : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorTencent()
        : this(Guid.NewGuid(), "https://tmt.tencentcloudapi.com", "腾讯翻译君")
    {
    }

    public TranslatorTencent(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Tencent,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.TencentService
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

    [JsonIgnore] [ObservableProperty] private TencentRegionEnum _region = TencentRegionEnum.ap_shanghai;

    [JsonIgnore] [ObservableProperty] private string? _projectId = "0";

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

    public Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
    {
        if (request is not RequestModel reqModel)
            throw new Exception($"请求数据出错: {request}");

        //检查语种
        var source = LangConverter(reqModel.SourceLang) ??
                     throw new Exception($"该服务不支持{reqModel.SourceLang.GetDescription()}");
        var target = LangConverter(reqModel.TargetLang) ??
                     throw new Exception($"该服务不支持{reqModel.TargetLang.GetDescription()}");

        // 实例化一个认证对象，入参需要传入腾讯云账户 SecretId 和 SecretKey，此处还需注意密钥对的保密
        // 代码泄露可能会导致 SecretId 和 SecretKey 泄露，并威胁账号下所有资源的安全性。以下代码示例仅供参考，建议采用更安全的方式来使用密钥，请参见：https://cloud.tencent.com/document/product/1278/85305
        // 密钥可前往官网控制台 https://console.cloud.tencent.com/cam/capi 进行获取
        Credential cred = new() { SecretId = AppID, SecretKey = AppKey };
        // 实例化一个client选项，可选的，没有特殊需求可以跳过
        ClientProfile clientProfile = new();
        // 实例化一个http选项，可选的，没有特殊需求可以跳过
        var url = Url.Replace("https://", "");
        HttpProfile httpProfile = new() { Endpoint = url };
        clientProfile.HttpProfile = httpProfile;

        //Region
        var region = Region.ToString().Replace("_", "-");
        // 实例化要请求产品的client对象,clientProfile是可选的
        TmtClient client = new(cred, region, clientProfile);
        // 实例化一个请求对象,每个接口都会对应一个request对象
        TextTranslateRequest req =
            new()
            {
                SourceText = reqModel.Text,
                Source = source,
                Target = target,
                ProjectId = Convert.ToInt64(ProjectId)
            };
        // 返回的resp是一个TextTranslateResponse的实例，与请求对象对应
        var resp = client.TextTranslateSync(req);

        var data = resp.TargetText.Length == 0 ? throw new Exception("请求结果为空") : resp.TargetText;

        return Task.FromResult(TranslationResult.Success(data));
    }

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorTencent
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
            Region = Region,
            ProjectId = ProjectId,
            IdHide = IdHide,
            KeyHide = KeyHide,
            IsExecuting = IsExecuting
        };
    }

    /// <summary>
    ///     https://cloud.tencent.com/document/product/551/15619
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
            LangEnum.hi => "hi",

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