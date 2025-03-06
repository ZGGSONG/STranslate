using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using STranslate.Util;
using STranslate.ViewModels.Preference;

namespace STranslate.Views.Preference;

public partial class TTSPage : UserControl
{
    public TTSPage()
    {
        InitializeComponent();
        var vm = Singleton<TTSViewModel>.Instance;

        // 设置滚动到当前选中的服务
        vm.OnSelectedServiceChanged = () =>
            TTSListBox.ScrollIntoView(TTSListBox.SelectedItem);
        DataContext = vm;
    }

    public static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        do
        {
            if (current is T t) return t;
            current = VisualTreeHelper.GetParent(current);
        } while (current != null);

        return null;
    }
}