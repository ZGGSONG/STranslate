using System.IO;

namespace STranslate.Util;

public static class FileUtil
{
    /// <summary>
    ///     将文件读取为stream
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static Stream FileToStream(string fileName)
    {
        // 打开文件
        var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

        // 读取文件的 byte[]
        var bytes = new byte[fileStream.Length];
        fileStream.Read(bytes, 0, bytes.Length);
        fileStream.Close();
        // 把 byte[] 转换成 Stream
        Stream stream = new MemoryStream(bytes);
        return stream;
    }
}