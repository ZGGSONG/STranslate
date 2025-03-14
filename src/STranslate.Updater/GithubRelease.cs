using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace STranslate.Updater;

public class GithubRelease
{
    private readonly string GithubUrl;
    private readonly string NowVersion;

    public GithubRelease(string githubUrl, string nowVersion)
    {
        GithubUrl = githubUrl;
        NowVersion = nowVersion;
        Info = new VersionInfo();
    }

    public VersionInfo Info { get; set; }

    public bool IsCanUpdate()
    {
        // 获取版本移除小数点后数字大小
        var remoteVersion = Convert.ToInt64(Info.Version.Replace(".", ""));
        var localVersion = Convert.ToInt64(NowVersion.Replace(".", ""));

        // 如果远端版本号数字大于本地版本号数字即可升级
        return localVersion < remoteVersion;
    }

    public async Task<VersionInfo?> GetRequest()
    {
        try
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using HttpClient httpClient = new(new SocketsHttpHandler());
            httpClient.Timeout = TimeSpan.FromMilliseconds(60000);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");

            var response = await httpClient.GetAsync(GithubUrl);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();

                var data = JsonConvert.DeserializeObject<GithubModel>(body) ??
                           throw new Exception($"Response Error, body is {body}");

                Info.IsPre = data.Prerelease;
                Info.Title = data.Name;
                Info.Version = data.TagName;
                Info.DownloadUrl = data.Assets?.FirstOrDefault()?.BrowserDownloadUrl ??
                                   throw new Exception("BrowserDownloadUrl is empty");
                Info.HtmlUrl = data.HtmlUrl;

                return Info;
            }

            // 处理非成功的响应，例如 404 Not Found
            return null;
        }
        catch (Exception)
        {
            // 处理其他异常
            return null;
        }
    }
}

public class VersionInfo
{
    /// <summary>
    ///     版本标题
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    ///     版本号
    /// </summary>
    public string Version { get; set; } = "";

    /// <summary>
    ///     是否是预览版本
    /// </summary>
    public bool IsPre { get; set; }

    /// <summary>
    ///     下载路径
    /// </summary>
    public string DownloadUrl { get; set; } = "";

    /// <summary>
    ///     版本更新内容网页链接
    /// </summary>
    public string HtmlUrl { get; set; } = "";
}

public class GithubModel
{
    [JsonProperty("tag_name")] public string TagName { get; set; } = "";

    [JsonProperty("html_url")] public string HtmlUrl { get; set; } = "";

    [JsonProperty("name")] public string Name { get; set; } = "";

    [JsonProperty("prerelease")] public bool Prerelease { get; set; }

    [JsonProperty("assets")] public List<GithubAssetsModel>? Assets { get; set; }
}

public class GithubAssetsModel
{
    [JsonProperty("browser_download_url")] public string BrowserDownloadUrl { get; set; } = "";
}