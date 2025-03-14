using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using STranslate.Model;

namespace STranslate.Views.Preference.Translator;

public partial class TranslatorDeepLPage : UserControl
{
    public TranslatorDeepLPage(ITranslator vm)
    {
        InitializeComponent();

        DataContext = vm;
    }

    /// <summary>
    ///     通过缓存加载View时刷新ViewModel
    /// </summary>
    /// <param name="vm"></param>
    public void UpdateVM(ITranslator vm)
    {
        DataContext = vm;
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = "https://deepl.com/", UseShellExecute = true });
    }
}