using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Helper;
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

    [ObservableProperty]
    private bool _isExecuting;

    [RelayCommand]
    private void Exit(Window window)
    {
        window.Close();
    }

    public async Task ExecuteAsync(Bitmap bs, CancellationToken token)
    {
        WordBlocks.Clear();

        IsExecuting = true;

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
        var optimizedResult = OcrResultOptimizer.OptimizeForTranslation(ocrResult);

        await Parallel.ForEachAsync(optimizedResult.OcrContents, token, async (item, token) =>
        {
            item.Text = await TranslatorAsync(item.Text, token);
        });

        IsExecuting = false;

        foreach (var ocrContent in optimizedResult.OcrContents)
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

            // 考虑DPI缩放因素，确保坐标正确映射
            int scaledWidth = (int)(width / dpi.DpiScaleX);
            int scaledHeight = (int)(height / dpi.DpiScaleY);
            double initialFontSize = scaledHeight * 0.8; // 初始字体大小
            
            // 使用FormattedText测量文本在当前字体大小下的宽度
            var formattedText = new FormattedText(
                ocrContent.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"), // 使用默认字体
                initialFontSize,
                System.Windows.Media.Brushes.Black,
                dpi.PixelsPerDip);
            
            // 如果文本宽度超过了计算的宽度，则逐步减小字体大小
            double adjustedFontSize = initialFontSize;
            while (formattedText.Width > scaledWidth && adjustedFontSize > 8) // 最小字体大小为8
            {
                adjustedFontSize -= 1;
                formattedText = new FormattedText(
                    ocrContent.Text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    adjustedFontSize,
                    System.Windows.Media.Brushes.Black,
                    dpi.PixelsPerDip);
                // 对应的增高高度避免无法容纳文本
                scaledHeight += 1;
            }
            
            var wordBlockInfo = new WordBlockInfo
            {
                Text = ocrContent.Text,
                // 使用左上角坐标作为Position，并考虑DPI缩放
                Position = new Point((int)(minX / dpi.DpiScaleX), (int)(minY / dpi.DpiScaleY)),
                Width = scaledWidth,
                Height = scaledHeight,
                // 使用调整后的字体大小
                FontSize = adjustedFontSize
            };

            // 添加到WordBlocks集合中
            WordBlocks.Add(wordBlockInfo);
        }

        bs.Dispose();
    }

    private async Task<string> TranslatorAsync(string content, CancellationToken token = default)
    {
        var detectType = _configHelper.CurrentConfig?.DetectType ?? LangDetectType.Local;
        var rate = _configHelper.CurrentConfig?.AutoScale ?? 0.8;

        var identify = await LangDetectHelper.DetectAsync(content, detectType, rate, token);

        //如果identify也是自动（只有服务识别服务出错的情况下才是auto）
        identify = identify == LangEnum.auto
            ? _configHelper.CurrentConfig?.SourceLangIfAuto ?? LangEnum.en
            : identify;

        var source = identify;

        var target = identify is LangEnum.zh_cn or LangEnum.zh_tw or LangEnum.yue
            ? _configHelper.CurrentConfig?.TargetLangIfSourceZh ?? LangEnum.en
            : _configHelper.CurrentConfig?.TargetLangIfSourceNotZh ?? LangEnum.zh_cn;

        var svc = Singleton<TranslatorViewModel>.Instance.CurTransServiceList.First(x => x.Type == ServiceType.BaiduService);
        var ret = await svc.TranslateAsync(new RequestModel(content, source, target), token);

        return ret.Result;
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
