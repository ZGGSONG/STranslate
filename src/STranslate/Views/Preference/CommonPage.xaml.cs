using STranslate.Util;
using STranslate.ViewModels.Preference;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace STranslate.Views.Preference;

/// <summary>
///     CommonPage.xaml 的交互逻辑
/// </summary>
public partial class CommonPage : UserControl
{
    public CommonPage()
    {
        InitializeComponent();

        Singleton<CommonViewModel>.Instance.OnOftenUsedLang +=
            () => BindingOperations.GetMultiBindingExpression(LangCb, ItemsControl.ItemsSourceProperty)?.UpdateTarget();
    }

    /// <summary>
    ///     限制输入数字
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AcceptOnlyNumber(object sender, KeyEventArgs e)
    {
        if (!IsAllowedKey(e.Key)) e.Handled = true;
    }

    private bool IsAllowedKey(Key key)
    {
        return (key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9) || key == Key.Back ||
               key == Key.Delete || key == Key.Left || key == Key.Right;
    }
}