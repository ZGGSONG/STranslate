using STranslate.Util;
using STranslate.ViewModels.Preference;
using System.Windows.Controls;
using System.Windows.Data;

namespace STranslate.Views;

/// <summary>
///     LangView.xaml 的交互逻辑
/// </summary>
public partial class LangView : UserControl
{
    public LangView()
    {
        InitializeComponent();

        Singleton<CommonViewModel>.Instance.OnOftenUsedLang += () =>
        {
            BindingOperations.GetMultiBindingExpression(SourceLangCb, ItemsControl.ItemsSourceProperty)?.UpdateTarget();
            BindingOperations.GetMultiBindingExpression(TargetLangCb, ItemsControl.ItemsSourceProperty)?.UpdateTarget();
        };
    }
}