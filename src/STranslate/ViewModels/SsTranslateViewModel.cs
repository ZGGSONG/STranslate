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

        // 获取图像尺寸信息用于后续计算，避免在bs释放后访问它
        var imageWidth = bs.Size.Width;
        var imageHeight = bs.Size.Height;

        // 保存图像宽高比例，用于后续计算
        var imageAspectRatio = (double)imageWidth / imageHeight;

        var bytes = BitmapUtil.ConvertBitmap2Bytes(bs, GetImageFormat());
        var ocrResult = await Singleton<OCRScvViewModel>.Instance.ExecuteAsync(bytes, WindowType.Main, token,
                _configHelper.CurrentConfig?.MainOcrLang ?? LangEnum.auto);
        //TODO: 优化OCR结果
        //var optimizedResult = OcrResultOptimizer.OptimizeForTranslation(ocrResult);

        await Parallel.ForEachAsync(ocrResult.OcrContents, token, async (item, token) =>
        {
            item.Text = await TranslatorAsync(item.Text, token);
        });

        IsExecuting = false;

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

            // 考虑DPI缩放因素，确保坐标正确映射
            int scaledWidth = (int)(width / dpi.DpiScaleX);
            int scaledHeight = (int)(height / dpi.DpiScaleY);

            // 检测翻译后文本是否明显变长（特别是中文翻译成英文的情况）
            // 根据文本长度动态调整宽度和高度
            int originalLength = ocrContent.Text.Length;
            int maxAllowedWidth = Math.Max(scaledWidth, (int)(imageWidth / dpi.DpiScaleX * 0.8)); // 限制最大宽度为图像宽度的80%

            // 如果文本长度超过原始区域宽度的预估容量，增加宽度
            if (originalLength > scaledWidth / 8) // 假设每个字符平均宽度为8像素
            {
                // 动态调整宽度，但不超过最大允许宽度
                scaledWidth = Math.Min(maxAllowedWidth, Math.Max(scaledWidth, originalLength * 8));
            }

            double initialFontSize = scaledHeight * 0.8; // 初始字体大小
            double adjustedFontSize = initialFontSize;
            int additionalHeight = 0;

            // 创建FormattedText对象用于测量文本尺寸
            var formattedText = new FormattedText(
                ocrContent.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                adjustedFontSize,
                System.Windows.Media.Brushes.Black,
                dpi.PixelsPerDip);

            // 设置最大宽度，确保文本能够自动换行
            formattedText.MaxTextWidth = scaledWidth;

            // 使用BuildHighlightGeometry获取实际渲染后的文本边界，这比BuildGeometry更准确地表示换行后的文本区域
            var textGeometry = formattedText.BuildHighlightGeometry(new System.Windows.Point(0, 0));
            //if (textGeometry != null)
            //{
            Rect textBounds = textGeometry.Bounds;

            // 计算实际需要的高度
            double actualTextHeight = textBounds.Height;
            double actualTextWidth = textBounds.Width;

            // 如果文本实际高度超过了计算的高度，增加高度
            if (actualTextHeight > scaledHeight)
            {
                additionalHeight = (int)(actualTextHeight - scaledHeight);
                scaledHeight += additionalHeight;
            }

            // 如果实际宽度仍然超出了设定的宽度，可能需要进一步调整
            if (actualTextWidth > scaledWidth)
            {
                // 计算需要的额外行数
                double extraWidthRatio = actualTextWidth / scaledWidth;
                if (extraWidthRatio > 1.1) // 如果超出10%以上，增加更多高度
                {
                    int extraLines = (int)Math.Ceiling(extraWidthRatio - 1);
                    int extraHeight = (int)(extraLines * formattedText.Height * 0.8);
                    scaledHeight += extraHeight;
                }
            }
            //}

            // 如果文本太长，可能需要减小字体大小
            if (textGeometry != null && (textBounds.Width > scaledWidth || actualTextHeight > scaledHeight * 2))
            {
                // 逐步减小字体大小，直到文本适合或达到最小字体大小
                while ((textBounds.Width > scaledWidth || actualTextHeight > scaledHeight * 1.5) && adjustedFontSize > 8)
                {
                    adjustedFontSize -= 1;

                    // 使用新的字体大小重新创建FormattedText
                    formattedText = new FormattedText(
                        ocrContent.Text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        adjustedFontSize,
                        System.Windows.Media.Brushes.Black,
                        dpi.PixelsPerDip);

                    formattedText.MaxTextWidth = scaledWidth;

                    // 重新计算文本边界
                    textGeometry = formattedText.BuildGeometry(new System.Windows.Point(0, 0));
                    textBounds = textGeometry.Bounds;
                    actualTextHeight = textBounds.Height;
                }

                // 最终调整高度以适应文本
                if (actualTextHeight > scaledHeight)
                {
                    additionalHeight = (int)(actualTextHeight - scaledHeight);
                    scaledHeight += additionalHeight;
                }
            }


            // 确保文本框位置不会超出屏幕边界
            int posX = (int)(minX / dpi.DpiScaleX);
            int posY = (int)(minY / dpi.DpiScaleY);

            // 如果文本框右侧超出图像边界，向左调整位置
            if (posX + scaledWidth > imageWidth / dpi.DpiScaleX)
            {
                posX = Math.Max(0, (int)(imageWidth / dpi.DpiScaleX - scaledWidth));
            }

            // 如果文本框底部超出图像边界，向上调整位置
            if (posY + scaledHeight > imageHeight / dpi.DpiScaleY)
            {
                posY = Math.Max(0, (int)(imageHeight / dpi.DpiScaleY - scaledHeight));
            }

            var wordBlockInfo = new WordBlockInfo
            {
                Text = ocrContent.Text,
                // 使用调整后的位置，并考虑DPI缩放
                Position = new Point(posX, posY),
                Width = scaledWidth,
                Height = scaledHeight,
                // 使用调整后的字体大小
                FontSize = adjustedFontSize
            };

            // 添加到WordBlocks集合中
            WordBlocks.Add(wordBlockInfo);
        }
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

        // 处理翻译结果，如果结果过长，进行智能处理
        string result = ret.Result;

        // 检测翻译前后文本长度变化比例
        double lengthRatio = (double)result.Length / content.Length;

        // 如果翻译后文本明显变长（通常中文翻译成英文会变长）
        if (lengthRatio > 1.5) // 降低阈值，更早进行处理
        {
            // 智能分段处理长文本
            if (result.Length > 120) // 降低长度阈值
            {
                // 尝试在句子边界处分段
                var sentenceEndChars = new[] { '.', '!', '?', ';', '。', '！', '？', '；', ':', '：', ',', '，' };
                int cutPosition = -1;

                // 寻找合适的句子结束位置进行截断，优先选择句号等强分隔符
                var primaryEndChars = new[] { '.', '!', '?', '。', '！', '？' };
                for (int i = 60; i < Math.Min(result.Length, 150); i++)
                {
                    if (primaryEndChars.Contains(result[i]))
                    {
                        cutPosition = i + 1;
                        break;
                    }
                }

                // 如果没找到强分隔符，尝试次要分隔符
                if (cutPosition == -1)
                {
                    for (int i = 60; i < Math.Min(result.Length, 150); i++)
                    {
                        if (sentenceEndChars.Contains(result[i]))
                        {
                            cutPosition = i + 1;
                            break;
                        }
                    }
                }

                // 如果找不到合适的句子结束位置，则在单词边界处截断
                if (cutPosition == -1)
                {
                    cutPosition = Math.Min(result.Length, 100); // 减少默认截断长度
                    // 尝试找到单词边界
                    while (cutPosition < Math.Min(result.Length, 120) && !char.IsWhiteSpace(result[cutPosition]))
                    {
                        cutPosition++;
                    }
                }

                // 应用截断，使用更明显的省略标记
                if (cutPosition < result.Length)
                {
                    result = result.Substring(0, cutPosition) + "... (文本已截断)";
                }
            }
        }

        return result;
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
