using System.Diagnostics;
using System.Windows;
using STranslate.Model;

namespace STranslate.Views.Preference.OCR;

public partial class WindowsOCRPage
{
    public WindowsOCRPage(IOCR vm)
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
        Process.Start(new ProcessStartInfo { FileName = "https://learn.microsoft.com/zh-cn/uwp/api/windows.media.ocr?view=winrt-26100", UseShellExecute = true });
    }
}