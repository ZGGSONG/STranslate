using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorKingSoftDict : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorKingSoftDict()
        : this(Guid.NewGuid(), "http://dict-co.iciba.com/api/dictionary.php", "金山词霸")
    {
    }

    public TranslatorKingSoftDict(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Iciba,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.KingSoftDictService
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

    #region Translator Test

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

    #endregion Translator Test

    #region Interface Implementation

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
    {
        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");

        //英文似乎只接收小写
        var content = req.Text.ToLower();

        //金山词霸可查中文无需检查是否为英文单词
        //var isWord = StringUtil.IsWord(content);
        //if (!isWord)
        //    goto Empty;

        var uriBuilder = new UriBuilder(Url);
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["type"] = "json";
        query["w"] = content;
        query["key"] = "54A9DE969E911BC5294B70DA8ED5C9C4";
        uriBuilder.Query = query.ToString();

        var resp = "";
        JObject parseData;
        try
        {
            resp = await HttpUtil.GetAsync(uriBuilder.Uri.AbsoluteUri, token).ConfigureAwait(false);
            parseData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception($"反序列化失败: {resp}");
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
        catch
        {
            LogService.Logger.Error($"金山词霸服务失效,访问结果: {resp}");
            return TranslationResult.Success("");
        }

        var wordName = parseData["word_name"]?.ToString();
        if (string.IsNullOrEmpty(wordName)) goto Empty;

        StringBuilder sb = new();
        sb.AppendLine(wordName);

        var isChinese = StringUtil.IsChinese(content);
        if (isChinese)
        {
            var symbol = parseData["symbols"]?.FirstOrDefault()?["word_symbol"]?.ToString();
            if (!string.IsNullOrEmpty(symbol))
                sb.AppendLine($"\r\nzh · [{symbol}]");
        }
        else
        {
            var symbolEn = parseData["symbols"]?.FirstOrDefault()?["ph_en"]?.ToString();
            if (!string.IsNullOrEmpty(symbolEn))
                sb.Append($"\r\nuk · [{symbolEn}]\n");

            var symbolAm = parseData["symbols"]?.FirstOrDefault()?["ph_am"]?.ToString();
            if (!string.IsNullOrEmpty(symbolAm))
                sb.AppendLine($"us · [{symbolAm}]");
        }

        var means = parseData["symbols"]?.FirstOrDefault()?["parts"];
        if (means == null)
            return TranslationResult.Success(sb.ToString());

        foreach (var meanItem in means)
        {
            var meansContent = meanItem["means"];
            if (meansContent == null) continue;

            var meanList = meansContent.Select(item => isChinese ? item["word_mean"]?.ToString() : item?.ToString())
                .Where(wordMean => !string.IsNullOrEmpty(wordMean)).ToList();
            var meanType = meanItem[isChinese ? "part_name" : "part"]?.ToString();
            if (!string.IsNullOrEmpty(meanType))
                sb.Append($"\n[{meanType}] ");
            sb.Append(string.Join("、", meanList));
        }

        var wordPls = parseData["exchange"]?["word_pl"];
        if (wordPls != null && wordPls.Count() != 0)
        {
            sb.Append($"\r\n\r\n复数形式 {string.Join("、", wordPls)}");
        }

        return TranslationResult.Success(sb.ToString());

        Empty:
        return TranslationResult.Success("");
    }

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorKingSoftDict
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
            AutoExecuteTranslateBack = AutoExecuteTranslateBack
        };
    }

    public string? LangConverter(LangEnum lang)
    {
        return null;
    }

    #endregion Interface Implementation
}