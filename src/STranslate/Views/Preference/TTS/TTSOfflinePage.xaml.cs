using System.Windows.Controls;
using STranslate.Model;

namespace STranslate.Views.Preference.TTS;

public partial class TTSOfflinePage : UserControl
{
    public TTSOfflinePage(ITTS vm)
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
}