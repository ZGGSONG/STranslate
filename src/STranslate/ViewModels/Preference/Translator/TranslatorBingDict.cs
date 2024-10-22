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

public partial class TranslatorBingDict : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorBingDict()
        : this(Guid.NewGuid(), "https://cn.bing.com", "必应词典")
    {
    }

    public TranslatorBingDict(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Bing,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.BingDictService
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

    [JsonIgnore][ObservableProperty] private ServiceType _type = 0;

    [JsonIgnore][ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore][ObservableProperty] private string _name = string.Empty;

    [JsonIgnore][ObservableProperty] private IconType _icon = IconType.Baidu;

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

    [JsonIgnore][ObservableProperty] private bool _autoExecute = true;

    [JsonIgnore]
    [ObservableProperty]
    [property: JsonIgnore]
    public TranslationResult _data = TranslationResult.Reset;

    [JsonIgnore]
    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isExecuting;

    #region Show/Hide Encrypt Info

    [JsonIgnore]
    [ObservableProperty]
    [property: JsonIgnore]
    private bool _idHide = true;

    [JsonIgnore]
    [ObservableProperty]
    [property: JsonIgnore]
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

    [property: JsonIgnore]
    [ObservableProperty]
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

        const string displayFormatDefault = "发音, 快速释义, 变形";

        //英文似乎只接收小写
        var content = req.Text.ToLower();

        //https://github1s.com/pot-app/pot-desktop/blob/master/src/services/translate/bing_dict/index.jsx
        var uriBuilder = new UriBuilder(Url)
        {
            Path = "/api/v6/dictionarywords/search"
        };
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["q"] = content;
        query["appid"] = "371E7B2AF0F9B84EC491D731DF90A55719C7D209";
        query["mkt"] = "zh-cn";
        query["pname"] = "bingdict";
        uriBuilder.Query = query.ToString();

        var resp = "";
        JObject parseData;
        try
        {
            resp = await HttpUtil.GetAsync(uriBuilder.Uri.AbsoluteUri, token).ConfigureAwait(false);
            parseData = JObject.Parse(resp) ?? throw new Exception($"反序列化失败: {resp}");
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
            LogService.Logger.Error($"必应词典服务失效,访问结果: {resp}");
            return TranslationResult.Success("");
        }

        var meaningGroups = parseData["value"]?.FirstOrDefault()?["meaningGroups"] ?? throw new Exception($"获取meaningGroups失败: {resp}");
        if (!meaningGroups.Any())
        {
            //throw new Exception($"Words not yet included: {content}");
            //无结果
            return TranslationResult.Success("");
        }
        var formats = displayFormatDefault.Trim().Split([", "], StringSplitOptions.None);
        var formatGroups = formats.ToDictionary(f => f, f => new List<JToken>());

        foreach (var cur in meaningGroups)
        {
            var partsOfSpeech = cur["partsOfSpeech"];
            if (partsOfSpeech != null && partsOfSpeech.Any())
            {
                var part = partsOfSpeech.FirstOrDefault();
                var description = part?["description"]?.ToString();
                var name = part?["name"]?.ToString();
                var group = description ?? name;

                if (group != null && formatGroups.ContainsKey(group))
                {
                    formatGroups[group].Add(cur);
                }
            }
        }
        var target = new
        {
            pronunciations = new List<Pron>(),
            explanations = new List<Expl>(),
            associations = new List<string>()
        };

        foreach (var pronunciation in formatGroups["发音"])
        {
            target.pronunciations.Add(new Pron
            {
                Region = pronunciation["partsOfSpeech"]?.FirstOrDefault()?["name"]?.ToString() ?? "",
                Symbol = pronunciation["meanings"]?.FirstOrDefault()?["richDefinitions"]?.FirstOrDefault()?["fragments"]?.FirstOrDefault()?["text"]?.ToString() ?? "",
                Voice = ""
            });
        }

        foreach (var explanation in formatGroups["快速释义"])
        {
            target.explanations.Add(new Expl
            {
                Trait = explanation["partsOfSpeech"]?.FirstOrDefault()?["name"]?.ToString() ?? "",
                Explains = explanation["meanings"]?.FirstOrDefault()?["richDefinitions"]?.FirstOrDefault()?["fragments"]?.Select(x => x["text"]?.ToString() ?? "")?.ToList() ?? []
            });
        }

        var collection =
            formatGroups["变形"]?.FirstOrDefault()?["meanings"]?.FirstOrDefault()?["richDefinitions"]
                ?.FirstOrDefault()?["fragments"];
        if (collection != null)
        {
            foreach (var association in collection)
            {
                var text = association["text"]?.ToString();
                if (string.IsNullOrEmpty(text)) continue;
                target.associations.Add(text);
            }
        }
        
        StringBuilder sb = new();
        sb.AppendLine(content);
        foreach (var item in target.pronunciations)
        {
            sb.Append($"\n{(item.Region.Equals("PY", StringComparison.CurrentCultureIgnoreCase) ? "zh" : item.Region.ToLower())} · [{item.Symbol}]");
        }

        if (target.pronunciations.Any())
            sb.AppendLine();
        foreach (var item in target.explanations)
        {
            sb.Append($"\n[{item.Trait}] {string.Join("、", item.Explains)}");
        }

        if (target.associations.Any())
            sb.AppendLine();

        foreach (var item in target.associations)
        {
            sb.Append($"\n{item}");
        }

        return TranslationResult.Success(sb.ToString());
    }

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorBingDict()
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

    #region Support


    public class Pron
    {
        public string Region { get; set; } = "";
        public string Symbol { get; set; } = "";
        public string Voice { get; set; } = "";
    }
    public class Expl
    {
        public string Trait { get; set; } = "";
        public List<string> Explains { get; set; } = [];
    }

    #endregion
}