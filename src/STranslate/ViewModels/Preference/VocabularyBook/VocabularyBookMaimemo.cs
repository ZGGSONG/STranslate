using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.VocabularyBook;

public partial class VocabularyBookMaimemo : ObservableObject, IVocabularyBook
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

    public VocabularyBookMaimemo()
        : this(Guid.NewGuid(), "", "墨墨背单词", isEnabled: false)
    {
    }

    public VocabularyBookMaimemo(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Maimemo,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        VocabularyBookType type = VocabularyBookType.MaimemoVocabularyBook
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
        const string url = "https://open.maimemo.com/open/api/v1/notepads";
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(BookName, $"生词本服务: {Name} 中生词本名称为空");

            var headerParams = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {AppKey}" }
            };
            var bookList = await HttpUtil.GetAsync(url, [], token, headerParams);
            var bookId = GetIdByNameInArray(bookList, BookName);
            if (string.IsNullOrWhiteSpace(bookId))
            {
                var req = new
                {
                    notepad = new
                    {
                        status = "PUBLISHED",
                        content = "first",
                        title = BookName,
                        brief = "create by stranslate",
                        tags = new[] { "其他" }
                    }
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
        const string url = "https://open.maimemo.com/open/api/v1/notepads";
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(BookId, "BookId不可为空");
            var newUrl = $"{url}/{BookId}";
            var newText = text.ToLower().Trim();

            var headerParams = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {AppKey}" }
            };

            var resultJson = await HttpUtil.GetAsync(newUrl, [], token, headerParams);
            var resultList = GetResult(resultJson);

            if (resultList.Contains(newText))
            {
                LogService.Logger.Info($"{Name}生词本 {BookName}({BookId}) 已存在单词: {newText}");
                return false;
            }
            else
            {
                resultList.Add(newText);
            }

            var req = new
            {
                notepad = new
                {
                    status = "PUBLISHED",
                    content = string.Join(",", resultList),
                    title = BookName,
                    brief = "create by stranslate",
                    tags = new[] { "其他" }
                }
            };
            
            var resp = await HttpUtil.PostAsync(newUrl, JsonConvert.SerializeObject(req), null, headerParams, token);
            if (!GetFinalResult(resp))
            {
                LogService.Logger.Error($"{Name}保存至生词本{BookName}({BookId})失败, 原始信息为: {resp}");
                return false;
            }

            // 二次检查结果是否真的插入了
            resultJson = await HttpUtil.GetAsync(newUrl, [], token, headerParams);
            resultList = GetResult(resultJson);

            if (!resultList.Contains(newText))
            {
                LogService.Logger.Warn($"{Name}保存至生词本{BookName}({BookId})失败, 原因是二次检查时发现服务不接受该单词: {newText}");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            LogService.Logger.Error($"{Name}保存至生词本{BookName}({BookId})失败, 请检查配置保存后重试\n错误信息: {ex.Message}");
            return false;
        }
    }

    public IVocabularyBook Clone()
    {
        return new VocabularyBookMaimemo
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

    private static bool GetFinalResult(string json)
    {
        var jObject = JObject.Parse(json);
        if (jObject["success"]?.ToString() != "True")
            throw new Exception($"接口回复: {json}");

        return true;
    }

    public static List<string> GetResult(string json)
    {
        var jObject = JObject.Parse(json);
        var resultList = new List<string>();
        if (jObject["data"]?["notepad"]?["list"] is JArray array)
        {
            foreach (var item in array)
            {
                if (item is not JObject obj) continue;
                if (obj["type"]?.ToString() == "WORD")
                {
                    if (obj["word"]?.ToString() is string str && !string.IsNullOrWhiteSpace(str))
                        resultList.Add(str);
                }
            }
        }

        return resultList;
    }

    private static string GetIdByName(string json)
    {
        var jObject = JObject.Parse(json);
        return jObject["data"]?["notepad"]?["id"]?.ToString() ?? string.Empty;
    }

    private static string GetIdByNameInArray(string json, string name)
    {
        var jObject = JObject.Parse(json);
        if (jObject["data"]?["notepads"] is not JArray jArray) return string.Empty;

        foreach (var jToken in jArray)
        {
            if (jToken is not JObject item) continue;

            if (item["title"]?.ToString() == name)
                return item["id"]?.ToString() ?? string.Empty;
        }

        return string.Empty;
    }

    #endregion
}