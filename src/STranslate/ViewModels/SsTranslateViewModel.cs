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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;

namespace STranslate.ViewModels;

public partial class SsTranslateViewModel : ObservableObject
{
    private readonly ConfigHelper _configHelper;

    public SsTranslateViewModel()
    {
        _configHelper = Singleton<ConfigHelper>.Instance;
    }

    [ObservableProperty]
    private ObservableCollection<WordBlockInfo> _wordBlocks = [];

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
        WordBlocks.Clear();

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

        foreach (var ocrContent in ocrResult.OcrContents)
        {
            if (ocrContent.BoxPoints.Count < 4 || string.IsNullOrEmpty(ocrContent.Text))
                continue;

            // 计算文本框的宽度和高度
            float minX = ocrContent.BoxPoints.Min(p => p.X);
            float minY = ocrContent.BoxPoints.Min(p => p.Y);
            float maxX = ocrContent.BoxPoints.Max(p => p.X);
            float maxY = ocrContent.BoxPoints.Max(p => p.Y);
            int width = (int)(maxX - minX);
            int height = (int)(maxY - minY);

            // 创建 WordBlockInfo 对象，将 BoxPoint 转换为 Position (System.Drawing.Point)
            // 考虑DPI缩放因素，确保坐标正确映射
            var wordBlockInfo = new WordBlockInfo
            {
                Text = ocrContent.Text,
                Position = new Point(
                    (int)(minX / dpi.DpiScaleX), 
                    (int)(minY / dpi.DpiScaleY)), // 使用左上角坐标作为Position，并考虑DPI缩放
                Width = (int)(width / dpi.DpiScaleX),
                Height = (int)(height / dpi.DpiScaleY),
                // 根据文本行高计算合适的字体大小，使用0.8作为比例因子
                // 这个比例可以根据实际效果进行调整
                FontSize = (int)(height / dpi.DpiScaleY * 0.8)
            };

            // 添加到WordBlocks集合中
            WordBlocks.Add(wordBlockInfo);
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
