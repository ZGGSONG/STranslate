using Newtonsoft.Json;
using STranslate.Model;
using System.IO;

namespace STranslate.Util;

public class UpdateUtil
{
    public static async Task<VersionInfo?> CheckForUpdates(string proxy, CancellationToken token = default)
    {
        try
        {
            string jsonContent = await HttpUtil.GetAsync($"{proxy}{Constant.VersionInfoUrl}", token);
            var versionInfo = JsonConvert.DeserializeObject<VersionInfo>(jsonContent)
                ?? throw new Exception("获取版本信息失败");

            if (StringUtil.IsNewer(versionInfo.Version, Constant.AppVersion))
            {
                return versionInfo;
            }
            return default; // 无新版本
        }
        catch (OperationCanceledException)
        {
            return default;
        }
        catch (Exception ex)
        {
            throw new Exception($"检查更新失败: {ex.Message}", ex);
        }
    }


    public static async Task<string> DownloadUpdateAsync(DownloadInfo downloadInfo, string savePath, CancellationToken token = default)
    {
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        return await HttpUtil.DownloadFileAsync(downloadInfo.Url, savePath, downloadInfo.Name, token);
    }

    public static async Task<string> DownloadUpdateAsync(DownloadInfo downloadInfo, string savePath, IProgress<double>? progress, CancellationToken token = default)
    {
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
        return await HttpUtil.DownloadFileAsync(downloadInfo.Url, savePath, downloadInfo.Name, progress, token);
    }
}


public class VersionInfo
{
    public string Version { get; set; } = string.Empty;
    public string Published_at { get; set; } = string.Empty;
    public DownloadInfo[] Downloads { get; set; } = [];
    public string Body { get; set; } = string.Empty;
}

public class DownloadInfo
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
}