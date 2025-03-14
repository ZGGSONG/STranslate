using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using STranslate.Model;

namespace STranslate.Views.Preference.OCR;

public partial class TencentOCRPage : UserControl
{
    public TencentOCRPage(IOCR vm)
    {
        InitializeComponent();

        DataContext = vm;
    }

    /// <summary>
    ///     通过缓存加载View时刷新ViewModel
    /// </summary>
    /// <param name="vm"></param>
    public void UpdateVM(IOCR vm)
    {
        DataContext = vm;
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
            { FileName = "https://cloud.tencent.com/product/ocr", UseShellExecute = true });
    }
}