using System.Windows.Controls;
using STranslate.Model;

namespace STranslate.Views.Preference.Translator;

public partial class TranslatorSTranslatePage : UserControl
{
    public TranslatorSTranslatePage(ITranslator vm)
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
}