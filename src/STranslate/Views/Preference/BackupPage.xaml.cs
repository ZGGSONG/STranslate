using STranslate.Util;
using STranslate.ViewModels.Preference;
using System.Windows.Controls;

namespace STranslate.Views.Preference
{
    public partial class BackupPage : UserControl
    {
        public BackupPage()
        {
            InitializeComponent();
            DataContext = Singleton<BackupViewModel>.Instance;
        }
    }
}
