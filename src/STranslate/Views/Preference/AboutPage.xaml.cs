using System.Windows.Controls;
using STranslate.Util;
using STranslate.ViewModels.Preference;

namespace STranslate.Views.Preference;

/// <summary>
///     AboutPage.xaml 的交互逻辑
/// </summary>
public partial class AboutPage : UserControl
{
    public AboutPage()
    {
        InitializeComponent();
        DataContext = Singleton<AboutViewModel>.Instance;
    }
}