using System.Diagnostics;
using System.Windows;
using STranslate.Model;

namespace STranslate.Views.Preference.OCR;

public partial class OpenAIOCRPage
{
    public OpenAIOCRPage(IOCR vm)
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
        Process.Start(new ProcessStartInfo { FileName = "https://openai.com/index/introducing-structured-outputs-in-the-api/", UseShellExecute = true });
    }
}