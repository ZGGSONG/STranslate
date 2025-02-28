using Newtonsoft.Json;
using STranslate.Model;

namespace STranslate.Util;

public class UpdateUtil
{
    public static async Task<VersionInfo?> CheckForUpdates(CancellationToken token = default)
    {
        try
        {
            string jsonContent = await HttpUtil.GetAsync(Constant.VersionInfoUrl, token);
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


    public static async Task<string?> DownloadUpdate(DownloadInfo downloadInfo, string savePath, CancellationToken token = default)
    {
        try
        {
            return await HttpUtil.DownloadFileAsync(downloadInfo.Url, savePath, downloadInfo.Name, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"下载更新失败: {ex.Message}");
            return null;
        }
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