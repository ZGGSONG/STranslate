using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using STranslate.Model;
using STranslate.ViewModels.Preference.Translator;

namespace STranslate.Views.Preference.Translator;

public partial class TranslatorEcdictPage : UserControl
{
    public TranslatorEcdictPage(ITranslator vm)
    {
        InitializeComponent();

        DataContext = vm;
        Check(vm);
    }

    /// <summary>
    ///     通过缓存加载View时刷新ViewModel
    /// </summary>
    /// <param name="vm"></param>
    public void UpdateVM(ITranslator vm)
    {
        DataContext = vm;
        Check(vm);
    }

    private void Check(ITranslator vm)
    {
        if (vm is not TranslatorEcdict ecdict)
            return;

        ecdict.CheckResourceCommand.ExecuteAsync(null);
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
            { FileName = "https://github.com/skywind3000/ECDICT", UseShellExecute = true });
    }
}