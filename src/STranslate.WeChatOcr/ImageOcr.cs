using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;

namespace STranslate.WeChatOcr;

public class ImageOcr : IDisposable
{
    private const int MaxRetries = 99;
    public ImageOcr()
    {
        OcrManager = new OcrManager();
        var ocrExePath = GetWeChatOcrExePath();
        var wechatDir = GetWeChatDir();
        //var ocrExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"extracted\WeChatOCR.exe");
        //var wechatDir = AppDomain.CurrentDomain.BaseDirectory;

        var ocrPtr = GCHandle.ToIntPtr(GCHandle.Alloc(OcrManager));
        OcrManager = (GCHandle.FromIntPtr(ocrPtr).Target as OcrManager)!;
        OcrManager.SetExePath(ocrExePath);
        OcrManager.SetUsrLibDir(wechatDir);
        OcrManager.StartWeChatOCR(ocrPtr);
    }

    internal OcrManager OcrManager { get; }
    public void Dispose()
        => OcrManager.KillWeChatOCR();

    public void Run(string imagePath, Action<string, WeiOcrResult?>? callback)
    {
        if (callback != null) OcrManager.SetOcrResultCallback(callback);
        int retryCount = 0;
        while (retryCount <= MaxRetries)
        {
            try
            {
                OcrManager.DoOcrTask(imagePath);
                return;
            }
            catch (OverflowException)
            {
                Thread.Sleep(10);
                retryCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
    }

    public static string GetWeChatDir()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\WeChat");
        if (key == null) return string.Empty;
        if (key.GetValue("DisplayVersion") is not string displayVersion) return string.Empty;
        return Path.Combine(@"C:\Program Files\Tencent\WeChat", "[" + displayVersion + "]");
    }

    public string? GetWeChatOcrExePath()
    {
        string searchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Tencent\WeChat\XPlugin\Plugins\WeChatOCR");
        return Directory.EnumerateFiles(searchPath, "WeChatOCR.exe", SearchOption.AllDirectories).FirstOrDefault();
    }
}
