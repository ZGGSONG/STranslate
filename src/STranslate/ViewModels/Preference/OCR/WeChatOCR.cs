using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using WeChatOcr;
using OcrResult = STranslate.Model.OcrResult;

namespace STranslate.ViewModels.Preference.OCR;

public partial class WeChatOCR : OCRBase, IOCR
{
    #region Location Support

    public List<BoxPoint> Converter(float x, float y, float width, float height)
    {
        return
        [
            //left top
            new BoxPoint(x, y),

            //right top
            new BoxPoint(x + width, y),

            //right bottom
            new BoxPoint(x + width, y + height),

            //left bottom
            new BoxPoint(x, y + height)
        ];
    }

    #endregion

    #region Constructor

    public WeChatOCR()
        : this(Guid.NewGuid(), "", "微信OCR", isEnabled: false)
    {
    }

    public WeChatOCR(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.WeChat,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        OCRType type = OCRType.WeChatOCR
    )
    {
        Identify = guid;
        Url = url;
        Name = name;
        Icon = icon;
        AppID = appID;
        AppKey = appKey;
        IsEnabled = isEnabled;
        Type = type;
    }

    #endregion Constructor

    #region Properties

    /// <summary>
    ///     微信OCR可执行文件路径
    /// </summary>
    [ObservableProperty] private string _weChatPath = @"C:\Program Files\Tencent\WeChat";

    #endregion Properties

    #region Command

    [RelayCommand]
    private void RemoveMmmojoDll()
    {
        if (!Utilities.RemoveMmmojoDll(out var error))
        {
            ToastHelper.Show(AppLanguageManager.GetString("Toast.DeleteFailedInfo"), WindowType.Preference);
            LogService.Logger.Error($"WeChatOCR|CleanMmMo Error: {error}");
        }
        else
        {
            ToastHelper.Show(AppLanguageManager.GetString("Toast.DeleteSuccess"), WindowType.Preference);
        }
    }

    #endregion

    #region Interface Implementation

    public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        var imgType =
            (Singleton<ConfigHelper>.Instance.CurrentConfig?.OcrImageQuality ?? OcrImageQualityEnum.Medium) switch
            {
                OcrImageQualityEnum.Medium => ImageType.Png,
                OcrImageQualityEnum.Low => ImageType.Jpeg,
                _ => ImageType.Bmp
            };
        var tcs = new TaskCompletionSource<OcrResult>();

        using var ocr = new ImageOcr(WeChatPath);
        ocr.Run(bytes, (path, result) =>
        {
            if (result == null) return;
            //System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(result));
            var ocrResult = new OcrResult();
            var list = result?.OcrResult?.SingleResult;
            if (list == null)
            {
                //避免重复set
                tcs.SetResult(OcrResult.Fail("WeChatOCR get result is null"));
                return;
            }

            for (var i = 0; i < list?.Count; i++)
            {
                if (list[i] is not { } item || string.IsNullOrEmpty(item.SingleStrUtf8))
                    continue;

                var content = new OcrContent(item.SingleStrUtf8);
                var width = item.Right - item.Left;
                var height = item.Bottom - item.Top;
                var x = item.Left;
                var y = item.Top;
                Converter(x, y, width, height).ForEach(pg =>
                {
                    //仅位置不全为0时添加
                    if (!pg.X.Equals(pg.Y) || pg.X != 0)
                        content.BoxPoints.Add(new BoxPoint(pg.X, pg.Y));
                });
                ocrResult.OcrContents.Add(content);
            }

            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // ignore
            }

            tcs.SetResult(ocrResult);
        }, imgType);

        var timeoutTask = Task.Delay(10000, cancelToken);
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        if (completedTask == timeoutTask) throw new TimeoutException("WeChatOCR operation timed out.");
        // 提取content的值
        var finalResult = await tcs.Task;

        return finalResult;
    }

    public IOCR Clone()
    {
        return new WeChatOCR
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            AppID = AppID,
            AppKey = AppKey,
            Icons = Icons,
            WeChatPath = WeChatPath
        };
    }

    public string? LangConverter(LangEnum lang)
    {
        throw new NotImplementedException();
    }

    #endregion Interface Implementation
}