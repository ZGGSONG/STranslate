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

public partial class TranslatorYandexBuiltIn : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorYandexBuiltIn() : this(Guid.NewGuid(), "https://yandex.deno.dev/translate", "Yandex")
    {
    }

    public TranslatorYandexBuiltIn(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Yandex,
        string appId = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.YandexBuiltInService
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

    private const string ApiUrl = "https://translate.yandex.net/api/v1/tr.json";
    private const string DefaultUserAgent = "ru.yandex.translate/3.20.2024";
    private CachedObject<Guid>? _cachedUcid;

    /// <summary>
    ///     https://github.com/d4n3436/GTranslate/blob/master/src/GTranslate/Translators/YandexTranslator.cs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private class CachedObject<T>
    {
        public T Value { get; }
        private readonly DateTime _expirationTime;

        public CachedObject(T value, TimeSpan expirationPeriod)
        {
            Value = value;
            _expirationTime = DateTime.UtcNow.Add(expirationPeriod);
        }

        public bool IsExpired() => DateTime.UtcNow > _expirationTime;
    }

    private Guid GetOrUpdateUcid()
    {
        if (_cachedUcid == null || _cachedUcid.IsExpired())
        {
            _cachedUcid = new CachedObject<Guid>(Guid.NewGuid(), TimeSpan.FromSeconds(360));
        }
        return _cachedUcid.Value;
    }

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

        var convSource = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var convTarget = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

        string query = $"?ucid={GetOrUpdateUcid():N}&srv=android&format=text";

        var headers = new Dictionary<string, string>
        {
            { "User-Agent", DefaultUserAgent }
        };

        var reqData = new Dictionary<string, string>
        {
            { "text", req.Text },
            { "lang", convSource == null ? convTarget : $"{convSource}-{convTarget}" }
        };

        var url = $"{ApiUrl}/translate{query}";

        try
        {
            var resp = await HttpUtil.PostAsync(url, reqData, headers, canceltoken).ConfigureAwait(false) ?? throw new Exception("请求结果为空");
            var data = JsonConvert.DeserializeObject<JObject>(resp)?["text"]?.FirstOrDefault()?.ToString() ?? throw new Exception(resp);
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

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorYandexBuiltIn
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
    ///     https://zh.wikipedia.org/wiki/ISO_639-1
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh",
            LangEnum.zh_tw => null,
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
            LangEnum.mn_cy => "mn",
            LangEnum.mn_mo => "mn",
            LangEnum.km => "km",
            LangEnum.nb_no => "no",
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