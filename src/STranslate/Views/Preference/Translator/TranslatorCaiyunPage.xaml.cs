using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using STranslate.Model;

namespace STranslate.Views.Preference.Translator;

public partial class TranslatorCaiyunPage : UserControl
{
    public TranslatorCaiyunPage(ITranslator vm)
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
        Process.Start(new ProcessStartInfo
            { FileName = "https://dashboard.caiyunapp.com/user/sign_in/", UseShellExecute = true });
    }
}