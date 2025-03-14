using System.Windows.Controls;
using STranslate.Util;
using STranslate.ViewModels.Preference;

namespace STranslate.Views.Preference;

public partial class HotkeyPage : UserControl
{
    public HotkeyPage()
    {
        InitializeComponent();
        DataContext = Singleton<HotkeyViewModel>.Instance;
    }
}