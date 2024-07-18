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

public partial class TranslatorCaiyun : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorCaiyun()
        : this(Guid.NewGuid(), "http://api.interpreter.caiyunai.com/v1/translator", "彩云小译")
    {
    }

    public TranslatorCaiyun(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Caiyun,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.CaiyunService
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
        var convSource = LangConverter(req.SourceLang) ??
                         throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var convTarget = LangConverter(req.TargetLang) ??
                         throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

        var body = new
        {
            source = req.Text.Split(Environment.NewLine),
            trans_type = $"{convSource}2{convTarget}",
            request_id = "demo",
            detect = true
        };
        var reqStr = JsonConvert.SerializeObject(body);
        var headers = new Dictionary<string, string> { { "X-Authorization", $"token {AppKey}" } };

        try
        {
            var resp = await HttpUtil.PostAsync(Url, reqStr, null, headers, token).ConfigureAwait(false) ??
                       throw new Exception("请求结果为空");

            // 解析JSON数据
            var parsedData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception($"反序列化失败: {resp}");

            // 提取content的值
            var arrayData = parsedData["target"] as JArray ?? throw new Exception("未获取到结果");

            var data = string.Join(Environment.NewLine, arrayData.Select(item => item.ToString()));

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
        return new TranslatorCaiyun
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
    ///     https://docs.caiyunapp.com/blog/2018/09/03/lingocloud-api/#%E6%94%AF%E6%8C%81%E7%9A%84%E8%AF%AD%E8%A8%80
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh",
            LangEnum.zh_tw => "zh",
            LangEnum.yue => "zh",
            LangEnum.en => "en",
            LangEnum.ja => "ja",

            LangEnum.ko => null,
            LangEnum.fr => null,
            LangEnum.es => null,
            LangEnum.ru => null,
            LangEnum.de => null,
            LangEnum.it => null,
            LangEnum.tr => null,
            LangEnum.pt_pt => null,
            LangEnum.pt_br => null,
            LangEnum.vi => null,
            LangEnum.id => null,
            LangEnum.th => null,
            LangEnum.ms => null,
            LangEnum.ar => null,
            LangEnum.hi => null,
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