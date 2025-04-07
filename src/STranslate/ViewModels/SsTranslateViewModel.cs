using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Model;
using STranslate.Util;
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

    public void Execute(Bitmap bs)
    {
        SsTranslateBs = BitmapUtil.ConvertBitmap2BitmapSource(bs, ImageFormat.Png);
        var view = new SsTranslateView(this);

        var dpi = VisualTreeHelper.GetDpi(view);

        // 获取鼠标位置并设置窗口位置为鼠标左上角位置
        var mousePosition = CommonUtil.GetPositionInfos().Item1;
        view.Left = mousePosition.X - bs.Size.Width / dpi.DpiScaleX;
        view.Top = mousePosition.Y - bs.Size.Height / dpi.DpiScaleY;
        
        view.Show();

        bs.Dispose();
    }
}
