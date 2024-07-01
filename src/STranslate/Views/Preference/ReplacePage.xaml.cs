using System.Windows.Controls;
using STranslate.Util;
using STranslate.ViewModels.Preference;

namespace STranslate.Views.Preference;

public partial class ReplacePage
{
    public ReplacePage()
    {
        InitializeComponent();
        DataContext = Singleton<ReplaceViewModel>.Instance;
    }
}
