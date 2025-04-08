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
using Color = System.Windows.Media.Color;

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

        WordBlocks.Clear();

        foreach (var ocrContent in ocrResult.OcrContents)
        {
            if (ocrContent.BoxPoints.Count < 4 || string.IsNullOrEmpty(ocrContent.Text))
                continue;

            // 创建TextBlock显示OCR识别的文本
            var textBlock = new TextBlock
            {
                Text = ocrContent.Text,
                FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255))  // 半透明背景
            };

            // 计算文本框的位置（使用左上角坐标）
            var left = ocrContent.BoxPoints[0].X;
            var top = ocrContent.BoxPoints[0].Y;

            // 设置Canvas的位置属性
            Canvas.SetLeft(textBlock, left);
            Canvas.SetTop(textBlock, top);

            // 添加到WordBlocks集合中
            WordBlocks.Add(textBlock);
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
