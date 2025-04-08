using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference;
using STranslate.Views;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace STranslate.ViewModels;

public partial class SsTranslateViewModel : ObservableObject
{
    private readonly ConfigHelper _configHelper;

    public SsTranslateViewModel()
    {
        _configHelper = Singleton<ConfigHelper>.Instance;
    }

    [ObservableProperty]
    private ObservableCollection<TextBlock> _wordBlocks = [];

    [ObservableProperty]
    private BitmapSource? _ssTranslateBs;

    private OcrResult OcrResult { get; set; } = OcrResult.Empty;

    [RelayCommand]
    private void Exit(Window window)
    {
        window.Close();
    }

    public async Task ExecuteAsync(Bitmap bs, CancellationToken token)
    {
        SsTranslateBs = BitmapUtil.ConvertBitmap2BitmapSource(bs, GetImageFormat());

        var view = new SsTranslateView(this);

        var dpi = VisualTreeHelper.GetDpi(view);

        // 获取鼠标位置并设置窗口位置为鼠标左上角位置
        var mousePosition = CommonUtil.GetPositionInfos().Item1;
        view.Left = mousePosition.X - bs.Size.Width / dpi.DpiScaleX;
        view.Top = mousePosition.Y - bs.Size.Height / dpi.DpiScaleY;
        view.Show();

        var bytes = BitmapUtil.ConvertBitmap2Bytes(bs, GetImageFormat());
        var ocrResult = await Singleton<OCRScvViewModel>.Instance.ExecuteAsync(bytes, WindowType.Main, token,
                _configHelper.CurrentConfig?.MainOcrLang ?? LangEnum.auto);
        LogService.Logger.Debug(ocrResult.Text);

        foreach (var ocrContent in ocrResult.OcrContents)
        {

        }

        bs.Dispose();
    }

    private ImageFormat GetImageFormat()
    {
        return (_configHelper.CurrentConfig?.OcrImageQuality ?? OcrImageQualityEnum.Medium) switch
        {
            OcrImageQualityEnum.Medium => ImageFormat.Png,
            OcrImageQualityEnum.Low => ImageFormat.Jpeg,
            _ => ImageFormat.Bmp
        };
    }
}
