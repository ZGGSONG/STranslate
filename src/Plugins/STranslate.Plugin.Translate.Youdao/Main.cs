using STranslate.Plugin.Translate.Youdao.View;
using STranslate.Plugin.Translate.Youdao.ViewModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;

namespace STranslate.Plugin.Translate.Youdao;

public class Main : TranslatePluginBase
{
    private Control? _settingUi;
    private SettingsViewModel? _viewModel;
    private Settings Settings { get; set; } = null!;
    private IPluginContext Context { get; set; } = null!;

    private const string Url = "https://openapi.youdao.com/api";

    public override Control GetSettingUI()
    {
        _viewModel ??= new SettingsViewModel(Context, Settings);
        _settingUi ??= new SettingsView { DataContext = _viewModel };
        return _settingUi;
    }

    /// <summary>
    ///     https://ai.youdao.com/DOCSIRMA/html/trans/api/wbfy/index.html
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public override string? GetSourceLanguage(LangEnum langEnum) => langEnum switch
    {
        LangEnum.Auto => "auto",
        LangEnum.ChineseSimplified => "zh-CHS",
        LangEnum.ChineseTraditional => "zh-CHT",
        LangEnum.Cantonese => "yue",
        LangEnum.English => "en",
        LangEnum.Japanese => "jp",
        LangEnum.Korean => "ko",
        LangEnum.French => "fr",
        LangEnum.Spanish => "es",
        LangEnum.Russian => "ru",
        LangEnum.German => "de",
        LangEnum.Italian => "it",
        LangEnum.Turkish => "tr",
        LangEnum.PortuguesePortugal => "pt",
        LangEnum.PortugueseBrazil => "pt",
        LangEnum.Vietnamese => "vie",
        LangEnum.Indonesian => "id",
        LangEnum.Thai => "th",
        LangEnum.Malay => "ms",
        LangEnum.Arabic => "ar",
        LangEnum.Hindi => "hi",
        LangEnum.MongolianCyrillic => "mn",
        LangEnum.MongolianTraditional => "mn",
        LangEnum.Khmer => "km",
        LangEnum.NorwegianBokmal => "no",
        LangEnum.NorwegianNynorsk => "no",
        LangEnum.Persian => "fa",
        LangEnum.Swedish => "sv",
        LangEnum.Polish => "pl",
        LangEnum.Dutch => "nl",
        LangEnum.Ukrainian => "uk",
        LangEnum.Uzbek => "uz",
        LangEnum.Uyghur => null,
        _ => "auto"
    };

    /// <summary>
    ///     https://ai.youdao.com/DOCSIRMA/html/trans/api/wbfy/index.html
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public override string? GetTargetLanguage(LangEnum langEnum) => langEnum switch
    {
        LangEnum.Auto => "auto",
        LangEnum.ChineseSimplified => "zh-CHS",
        LangEnum.ChineseTraditional => "zh-CHT",
        LangEnum.Cantonese => "yue",
        LangEnum.English => "en",
        LangEnum.Japanese => "jp",
        LangEnum.Korean => "ko",
        LangEnum.French => "fr",
        LangEnum.Spanish => "es",
        LangEnum.Russian => "ru",
        LangEnum.German => "de",
        LangEnum.Italian => "it",
        LangEnum.Turkish => "tr",
        LangEnum.PortuguesePortugal => "pt",
        LangEnum.PortugueseBrazil => "pt",
        LangEnum.Vietnamese => "vie",
        LangEnum.Indonesian => "id",
        LangEnum.Thai => "th",
        LangEnum.Malay => "ms",
        LangEnum.Arabic => "ar",
        LangEnum.Hindi => "hi",
        LangEnum.MongolianCyrillic => "mn",
        LangEnum.MongolianTraditional => "mn",
        LangEnum.Khmer => "km",
        LangEnum.NorwegianBokmal => "no",
        LangEnum.NorwegianNynorsk => "no",
        LangEnum.Persian => "fa",
        LangEnum.Swedish => "sv",
        LangEnum.Polish => "pl",
        LangEnum.Dutch => "nl",
        LangEnum.Ukrainian => "uk",
        LangEnum.Uzbek => "uz",
        LangEnum.Uyghur => null,
        _ => "auto"
    };

    public override void Init(IPluginContext context)
    {
        Context = context;
        Settings = context.LoadSettingStorage<Settings>();
    }

    public override void Dispose() => _viewModel?.Dispose();

    public override async Task TranslateAsync(TranslateRequest request, TranslateResult result, CancellationToken cancellationToken = default)
    {
        if (GetSourceLanguage(request.SourceLang) is not string sourceStr)
        {
            result.Fail(Context.GetTranslation("UnsupportedSourceLang"));
            return;
        }
        if (GetTargetLanguage(request.TargetLang) is not string targetStr)
        {
            result.Fail(Context.GetTranslation("UnsupportedTargetLang"));
            return;
        }

        var formData = new Dictionary<string, string>
        {
            { "q", request.Text },
            { "from", sourceStr },
            { "to", targetStr }
        };
        AddAuthParams(Settings.AppKey, Settings.AppSecret, formData);

        var response = await Context.HttpService.PostFormAsync(Url, formData, cancellationToken: cancellationToken);
        result.Success(TranslationResponse.Parse(response, Context.GetTranslation));
    }

    /*
        添加鉴权相关参数 -
        appKey : 应用ID
        salt : 随机值
        curtime : 当前时间戳(秒)
        signType : 签名版本
        sign : 请求签名

        @param appKey    您的应用ID
        @param appSecret 您的应用密钥
        @param paramsMap 请求参数表
    */
    private static void AddAuthParams(string appKey, string appSecret, Dictionary<string, string> paramsMap)
    {
        var q = paramsMap.TryGetValue("q", out string? value) ? value : paramsMap["img"];
        var salt = Guid.NewGuid().ToString();
        var curtime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "";
        var sign = CalculateSign(appKey, appSecret, q, salt, curtime);
        paramsMap.Add("appKey", appKey);
        paramsMap.Add("salt", salt);
        paramsMap.Add("curtime", curtime);
        paramsMap.Add("signType", "v3");
        paramsMap.Add("sign", sign);
    }

    /*
        计算鉴权签名 -
        计算方式 : sign = sha256(appKey + input(q) + salt + curtime + appSecret)

        @param appKey    您的应用ID
        @param appSecret 您的应用密钥
        @param q         请求内容
        @param salt      随机值
        @param curtime   当前时间戳(秒)
        @return 鉴权签名sign
    */
    private static string CalculateSign(string appKey, string appSecret, string q, string salt, string curtime)
    {
        var strSrc = appKey + GetInput(q) + salt + curtime + appSecret;
        return Encrypt(strSrc);
    }

    private static string Encrypt(string strSrc)
    {
        var inputBytes = Encoding.UTF8.GetBytes(strSrc);
        var hashedBytes = SHA256.HashData(inputBytes);
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToUpperInvariant();
    }

    private static string GetInput(string q)
    {
        if (q == null) return "";
        var len = q.Length;
        return len <= 20 ? q : q[..10] + len + q.Substring(len - 10, 10);
    }
}
