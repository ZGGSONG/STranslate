using System.Windows.Controls;
using STranslate.Model;

namespace STranslate.Views.Preference.Translator;

/// <summary>
///     TranslatorApiPage.xaml 的交互逻辑
/// </summary>
public partial class TranslatorApiPage : UserControl
{
    public TranslatorApiPage(ITranslator vm)
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