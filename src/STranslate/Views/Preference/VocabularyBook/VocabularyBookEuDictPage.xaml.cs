using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using STranslate.Model;

namespace STranslate.Views.Preference.VocabularyBook;

public partial class VocabularyBookEuDictPage : UserControl
{
    public VocabularyBookEuDictPage(IVocabularyBook vm)
    {
        InitializeComponent();

        DataContext = vm;
    }

    /// <summary>
    ///     通过缓存加载View时刷新ViewModel
    /// </summary>
    /// <param name="vm"></param>
    public void UpdateVM(IVocabularyBook vm)
    {
        DataContext = vm;
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://my.eudic.net/studyList",
            UseShellExecute = true
        });
    }
}