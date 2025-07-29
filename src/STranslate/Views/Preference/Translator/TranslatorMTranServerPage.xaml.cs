using STranslate.Model;
using System.Diagnostics;
using System.Windows.Controls;

namespace STranslate.Views.Preference.Translator;

public partial class TranslatorMTranServerPage : UserControl
{
    public TranslatorMTranServerPage(ITranslator vm)
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

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = e.Uri.ToString(), UseShellExecute = true });
        e.Handled = true;
    }
}