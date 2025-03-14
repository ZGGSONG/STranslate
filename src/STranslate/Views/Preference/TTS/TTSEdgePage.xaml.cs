using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using STranslate.Model;

namespace STranslate.Views.Preference.TTS;

public partial class TTSEdgePage : UserControl
{
    public TTSEdgePage(ITTS vm)
    {
        InitializeComponent();

        DataContext = vm;
    }

    /// <summary>
    ///     通过缓存加载View时刷新ViewModel
    /// </summary>
    /// <param name="vm"></param>
    public void UpdateVM(ITTS vm)
    {
        DataContext = vm;
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://stranslate.zggsong.com",
            UseShellExecute = true
        });
    }
}