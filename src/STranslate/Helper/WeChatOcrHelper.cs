using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using STranslate.Log;

namespace STranslate.Helper;

public static class WeChatOcrHelper
{
    // 注意要把生成的64位dll拷到 c_sharp\bin\x64\Debug\net8.0 目录下
    // 声明外部函数
    [DllImport("wcocr.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool wechat_ocr(
        [MarshalAs(UnmanagedType.LPWStr)] string ocr_exe,
        [MarshalAs(UnmanagedType.LPWStr)] string wechat_dir,
        [MarshalAs(UnmanagedType.LPStr)] string imgfn,
        SetResultDelegate set_res);

    /// <summary>
    ///     Get the OCR result from the image.
    /// </summary>
    /// <param name="ocrExe"></param>
    /// <param name="wechatDir"></param>
    /// <param name="imgPath"></param>
    /// <returns></returns>
    public static (bool, string) GetOcrResult(string ocrExe, string wechatDir, string imgPath)
    {
        var stringResult = new StringResult();
        var setRes = new SetResultDelegate(stringResult.SetResult);
        var isSuccess = wechat_ocr(ocrExe, wechatDir, imgPath, setRes);
        return (isSuccess, stringResult.GetResult());
    }

    /// <summary>
    ///     自动查找 WeChatOCR 可执行文件路径
    /// </summary>
    /// <returns></returns>
    public static string? FindWeChatOcrExe()
    {
        // 获取APPDATA路径
        var appdataPath = Environment.GetEnvironmentVariable("APPDATA");
        if (string.IsNullOrEmpty(appdataPath))
        {
            LogService.Logger.Debug("WeChatOcrHelper|APPDATA environment variable not found.");
            return default;
        }

        // 定义WeChatOCR的基本路径
        var basePath = Path.Combine(appdataPath, @"Tencent\WeChat\XPlugin\Plugins\WeChatOCR");

        // 定义匹配版本号文件夹的正则表达式
        var versionPattern = new Regex(@"\d+");

        try
        {
            // 获取路径下的所有文件夹
            var pathTemp = Directory.GetDirectories(basePath);
            foreach (var temp in pathTemp)
                // 使用正则表达式匹配版本号文件夹
                if (versionPattern.IsMatch(Path.GetFileName(temp)))
                {
                    var weChatOcrPath = Path.Combine(basePath, temp, "extracted", "WeChatOCR.exe");
                    if (File.Exists(weChatOcrPath)) return weChatOcrPath;
                }
        }
        catch (DirectoryNotFoundException)
        {
            LogService.Logger.Debug($"WeChatOcrHelper|The path {basePath} does not exist.");
            return default;
        }

        // 如果没有找到匹配的文件夹，返回 null
        return default;
    }

    public delegate void SetResultDelegate(IntPtr result);

    public class StringResult
    {
        private string _result = "";

        public void SetResult(IntPtr dt)
        {
            var length = 0;
            while (Marshal.ReadByte(dt, length) != 0) length++;
            var byteArray = new byte[length];
            Marshal.Copy(dt, byteArray, 0, length);
            _result = Encoding.UTF8.GetString(byteArray);
        }

        public string GetResult()
        {
            return _result;
        }
    }
}