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

            // 执行翻译
            ocrContent.Text = await TranslatorAsync(ocrContent.Text, token);

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
                // 使用左上角坐标作为Position，并考虑DPI缩放
                Position = new Point((int)(minX / dpi.DpiScaleX), (int)(minY / dpi.DpiScaleY)),
                Width = (int)(width / dpi.DpiScaleX),
                Height = (int)(height / dpi.DpiScaleY),
                // 根据文本行高计算合适的字体大小，使用0.8作为比例因子(根据实际效果进行调整)
                FontSize = (int)(height / dpi.DpiScaleY * 0.8)
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
