using System.Windows.Controls;
using STranslate.Util;
using STranslate.ViewModels.Preference;

namespace STranslate.Views.Preference;

public partial class BackupPage : UserControl
{
    public BackupPage()
    {
        InitializeComponent();
        DataContext = Singleton<BackupViewModel>.Instance;
    }
}