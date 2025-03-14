using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using STranslate.Model;
using STranslate.ViewModels.Preference.OCR;

namespace STranslate.Views.Preference.OCR;

public partial class PaddleOCRPage : UserControl
{
    public PaddleOCRPage(IOCR vm)
    {
        InitializeComponent();

        DataContext = vm;

        Check(vm);
    }

    /// <summary>
    ///     通过缓存加载View时刷新ViewModel
    /// </summary>
    /// <param name="vm"></param>
    public void UpdateVM(IOCR vm)
    {
        DataContext = vm;

        Check(vm);
    }

    private void Check(IOCR vm)
    {
        if (vm is not PaddleOCR paddle)
            return;

        paddle.CheckDataCommand.ExecuteAsync(null);
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
            { FileName = "https://gitee.com/raoyutian/PaddleOCRSharp", UseShellExecute = true });
    }
}