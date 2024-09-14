using System.IO;
using System.Runtime.InteropServices;

namespace STranslate.WeChatOcr;

public class ImageOcr : IDisposable
{
    private const uint MaxRetryTimes = 99;
    private readonly OcrManager _ocrManager;

    public ImageOcr(string? path = default)
    {
        _ocrManager = new OcrManager();
        var ocrExePath = Utilities.GetWeChatOcrExePath() ?? throw new Exception("get wechat ocr exe path is null");
        var wechatDir = Utilities.GetWeChatDir(path) ??
                        throw new Exception($"get wechat path failed: {path ?? "NULL"}");


        Utilities.CopyMmmojoDll(wechatDir, AppDomain.CurrentDomain.BaseDirectory);

        var ocrPtr = GCHandle.ToIntPtr(GCHandle.Alloc(_ocrManager));
        _ocrManager = (GCHandle.FromIntPtr(ocrPtr).Target as OcrManager)!;
        _ocrManager.SetExePath(ocrExePath);
        _ocrManager.SetUsrLibDir(wechatDir);
        _ocrManager.StartWeChatOcr(ocrPtr);
    }

    public void Dispose()
    {
        _ocrManager.KillWeChatOcr();
    }

    public void Run(string imagePath, Action<string, WeChatOcrResult?>? callback)
    {
        if (callback != null) _ocrManager.SetOcrResultCallback(callback);
        var retryCount = 0;
        while (retryCount <= MaxRetryTimes)
            try
            {
                _ocrManager.DoOcrTask(imagePath);
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

    public void Run(byte[] bytes, Action<string, WeChatOcrResult?>? callback, ImageType imgType = ImageType.Png)
    {
        var imgPath = Path.Combine(Path.GetTempPath(), $"generate4wechat.{imgType.ToString().ToLower()}");
        Utilities.WriteBytesToFile(imgPath, bytes);
        Run(imgPath, callback);
    }
}

public enum ImageType
{
    Png,
    Jpeg,
    Bmp
}