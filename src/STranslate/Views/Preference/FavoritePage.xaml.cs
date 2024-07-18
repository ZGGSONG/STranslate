using System.Windows.Controls;
using STranslate.Util;
using STranslate.ViewModels.Preference;

namespace STranslate.Views.Preference;

/// <summary>
///     FavoritePage.xaml 的交互逻辑
/// </summary>
public partial class FavoritePage : UserControl
{
    public FavoritePage()
    {
        InitializeComponent();
        DataContext = Singleton<FavoriteViewModel>.Instance;
    }
}