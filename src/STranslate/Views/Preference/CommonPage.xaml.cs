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
}