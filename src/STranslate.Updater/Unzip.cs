using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace STranslate.Updater;

public class Unzip
{
    /// <summary>
    ///     忽略的文件列表
    /// </summary>
    private static readonly string[] IgnoreFiles = [];

    public static bool ExtractZipFile(string zipPath, string extractPath)
    {
        try
        {
            if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                extractPath += Path.DirectorySeparatorChar;

            using var archive = ZipFile.OpenRead(zipPath);
            foreach (var entry in archive.Entries)
                if (!IsIgnoreFile(entry.FullName))
                {
                    // Gets the full path to ensure that relative segments are removed.
                    var destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));
                    if (!IsDir(destinationPath))
                    {
                        // 判断路径是否存在
                        var dir = Path.GetDirectoryName(destinationPath);
                        if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        if (File.Exists(destinationPath)) File.Delete(destinationPath);
                        // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                        // are case-insensitive.
                        Debug.WriteLine($"抽取：{destinationPath}");
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                            entry.ExtractToFile(destinationPath);
                    }
                    else
                    {
                        //创建目录
                        Directory.CreateDirectory(destinationPath);
                    }
                }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     指示文件是否是忽略的
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private static bool IsIgnoreFile(string fileName)
    {
        return Array.IndexOf(IgnoreFiles, fileName) != -1;
    }

    /// <summary>
    ///     指示路径是否是目录
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static bool IsDir(string path)
    {
        return path.Last() == '\\';
    }
}