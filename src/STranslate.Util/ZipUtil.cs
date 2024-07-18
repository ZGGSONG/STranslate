using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace STranslate.Util;

public static class ZipUtil
{
    public static void CompressFile(string filePath, string zipFilePath)
    {
        using var fileStream = new FileStream(zipFilePath, FileMode.Create);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);
        var entryName = Path.GetFileName(filePath);
        archive.CreateEntryFromFile(filePath, entryName);
    }

    public static bool DecompressToDirectory(string zipFilePath, string extractPath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            foreach (var entry in archive.Entries)
            {
                //如果目录不存在则创建
                if (!Directory.Exists(extractPath)) Directory.CreateDirectory(extractPath);
                if (entry.FullName[^1..] == "/")
                {
                    //如果为目录则继续创建该目录但不解压
                    Directory.CreateDirectory(Path.Combine(extractPath, entry.FullName));
                    continue;
                }

                var entryDestination = Path.Combine(extractPath, entry.FullName);
                entry.ExtractToFile(entryDestination, true);
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[ZipUtil] DecompressToDirectory Error, {0}", ex);
            return false;
        }
    }
}