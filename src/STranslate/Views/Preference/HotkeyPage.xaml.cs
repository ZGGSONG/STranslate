using STranslate.Util;
using STranslate.ViewModels.Preference;
using System.Windows.Controls;

namespace STranslate.Views.Preference
{
    public partial class HotkeyPage : UserControl
    {
        public HotkeyPage()
        {
            InitializeComponent();
            DataContext = Singleton<HotkeyViewModel>.Instance;
        }
    }
}
