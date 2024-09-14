using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace STranslate.WeChatOcr;

public partial class Utilities
{
    /// <summary>
    ///     微信默认安装目录
    /// </summary>
    private const string WeChatDefaultPath = @"C:\Program Files\Tencent\WeChat";
    private const string MmmojoDll = "mmmojo.dll";
    private const string Mmmojo64Dll = "mmmojo_64.dll";

    /// <summary>
    ///     获取微信安装目录
    /// </summary>
    /// <param name="path">
    ///     C:\Program Files\Tencent\WeChat
    ///     or
    ///     C:\Program Files\Tencent\WeChat\[3.9.12.11]
    /// </param>
    /// <returns></returns>
    public static string? GetWeChatDir(string? path = default)
    {
        path ??= WeChatDefaultPath;

        // 判断是否为带有版本号的完整目录
        if (WeChatVersionRegex().IsMatch(path))
            return path.TrimEnd('\\');

        // 从注册表中获取微信安装版本并根据参数path拼接微信安装目录
        using var key =
            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\WeChat");
        if (key?.GetValue("DisplayVersion") is not string displayVersion) return default;
        var wechatDir = Path.Combine(path, "[" + displayVersion + "]");
        return Directory.Exists(wechatDir) ? wechatDir : default;
    }

    /// <summary>
    ///     获取微信OCR可执行文件路径
    /// </summary>
    /// <returns></returns>
    public static string? GetWeChatOcrExePath()
    {
        var searchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Tencent\WeChat\XPlugin\Plugins\WeChatOCR");
        var ocrExePath = Directory.EnumerateFiles(searchPath, "WeChatOCR.exe", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (ocrExePath != null && File.Exists(ocrExePath)) return ocrExePath;
        return default;
    }

    /// <summary>
    ///     微信路径版本号正则表达式
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\[\d+(\.\d+)*\]$")]
    private static partial Regex WeChatVersionRegex();

    public static void WriteBytesToFile(string filePath, byte[] bytes)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        fileStream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>
    ///     复制微信安装目录下的mmmojo.dll、mmmojo_64.dll到软件目录
    /// </summary>
    /// <param name="wechatDir"></param>
    /// <param name="targetPath">目标目录</param>
    public static void CopyMmmojoDll(string wechatDir, string targetPath)
    {
        var mmmojoFullPath = Path.Combine(wechatDir, MmmojoDll);
        var mmmojo64FullPath = Path.Combine(wechatDir, Mmmojo64Dll);
        var targetMmmojoFullPath = Path.Combine(targetPath, MmmojoDll);
        var targetMmmojo64FullPath = Path.Combine(targetPath, Mmmojo64Dll);
        if (!File.Exists(targetMmmojoFullPath))
            File.Copy(mmmojoFullPath, targetMmmojoFullPath);
        if (!File.Exists(targetMmmojo64FullPath))
            File.Copy(mmmojo64FullPath, targetMmmojo64FullPath);
    }
}