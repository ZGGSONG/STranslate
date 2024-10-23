using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.VocabularyBook;

public partial class VocabularyBookEuDict : ObservableObject, IVocabularyBook
{
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

    [JsonIgnore] [ObservableProperty] private string _bookId = string.Empty;

    [JsonIgnore] [ObservableProperty] private string _bookName = Constant.AppName.ToLower();

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.Azure;

    [ObservableProperty] private Guid _identify = Guid.Empty;

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private VocabularyBookType _type = VocabularyBookType.EuDictVocabularyBook;

    [JsonIgnore]
    [ObservableProperty]
    [property: DefaultValue("")]
    [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public string _url = string.Empty;

    public VocabularyBookEuDict()
        : this(Guid.NewGuid(), "", "欧路词典", isEnabled: false)
    {
    }

    public VocabularyBookEuDict(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.EuDict,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        VocabularyBookType type = VocabularyBookType.EuDictVocabularyBook
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

    [JsonIgnore] public Dictionary<IconType, string> Icons { get; private set; } = Constant.IconDict;

    public async Task<bool> CheckAsync(CancellationToken token)
    {
        const string url = "https://api.frdic.com/api/open/v1/studylist/category";
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(BookName, $"生词本服务: {Name} 中生词本名称为空");

            var queryParams = new Dictionary<string, string>
            {
                { "language", "en" }
            };
            var headerParams = new Dictionary<string, string>
            {
                { "Authorization", AppKey }
            };
            var bookList = await HttpUtil.GetAsync(url, queryParams, token, headerParams);
            var bookId = GetIdByNameInArray(bookList, BookName);
            if (string.IsNullOrWhiteSpace(bookId))
            {
                var req = new
                {
                    language = "en",
                    name = BookName
                };
                var resp = await HttpUtil.PostAsync(url, JsonConvert.SerializeObject(req), null, headerParams, token);
                bookId = GetIdByName(resp);
                ArgumentException.ThrowIfNullOrWhiteSpace(bookId,
                    $"创建生词本服务: {Name}->生词本名称: {BookName} 失败, 接口回复: {resp}");
            }

            BookId = bookId;
            return true;
        }
        catch (Exception ex)
        {
            LogService.Logger.Error($"检查生词本服务： {Name} 配置失败, {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ExecuteAsync(string text, CancellationToken token)
    {
        const string url = "https://api.frdic.com/api/open/v1/studylist/words";
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(BookId, "BookId不可为空");
            var req = new
            {
                id = BookId,
                language = "en",
                words = new[] { text }
            };
            var headerParams = new Dictionary<string, string>
            {
                { "Authorization", AppKey }
            };
            var resp = await HttpUtil.PostAsync(url, JsonConvert.SerializeObject(req), null, headerParams, token);

            return GetResult(resp);
        }
        catch (Exception ex)
        {
            LogService.Logger.Error($"{Name}保存至生词本{BookName}({BookId})失败, 请检查配置保存后重试\n错误信息: {ex.Message}");
            return false;
        }
    }

    public IVocabularyBook Clone()
    {
        return new VocabularyBookEuDict
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            BookName = BookName,
            BookId = BookId,
            Url = Url,
            AppID = AppID,
            AppKey = AppKey,
            IdHide = IdHide,
            KeyHide = KeyHide
        };
    }

    #region Show/Hide Encrypt Info

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _idHide = true;

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _keyHide = true;

    private void ShowEncryptInfo(string? obj)
    {
        if (obj == null)
            return;

        if (obj.Equals(nameof(AppID)))
            IdHide = !IdHide;
        else if (obj.Equals(nameof(AppKey))) KeyHide = !KeyHide;
    }

    private RelayCommand<string>? showEncryptInfoCommand;

    [JsonIgnore]
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

    #endregion Show/Hide Encrypt Info

    #region Json Handle

    private static bool GetResult(string json)
    {
        var jObject = JObject.Parse(json);
        if (!string.IsNullOrEmpty(jObject["status"]?.ToString())) throw new Exception($"接口回复: {json}");

        return true;
    }

    private static string GetIdByName(string json)
    {
        var jObject = JObject.Parse(json);
        return jObject["data"]?["id"]?.ToString() ?? string.Empty;
    }

    private static string GetIdByNameInArray(string json, string name)
    {
        var jObject = JObject.Parse(json);
        if (jObject["data"] is not JArray jArray) return string.Empty;

        foreach (var jToken in jArray)
        {
            var item = (JObject)jToken;
            if (item["name"]?.ToString() == name) return item["id"]?.ToString() ?? string.Empty;
        }

        return string.Empty;
    }

    #endregion
}