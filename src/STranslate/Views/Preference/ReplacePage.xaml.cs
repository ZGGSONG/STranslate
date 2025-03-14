using STranslate.Util;
using STranslate.ViewModels.Preference;
using System.Windows.Controls;
using System.Windows.Data;

namespace STranslate.Views.Preference;

public partial class ReplacePage
{
    public ReplacePage()
    {
        InitializeComponent();
        DataContext = Singleton<ReplaceViewModel>.Instance;

        // 更新常用语言到 ComboBox
        Singleton<CommonViewModel>.Instance.OnOftenUsedLang +=
            () =>
            {
                BindingOperations.GetMultiBindingExpression(SourceLangCb, ItemsControl.ItemsSourceProperty)?.UpdateTarget();
                BindingOperations.GetMultiBindingExpression(TargetLangCb, ItemsControl.ItemsSourceProperty)?.UpdateTarget();
                BindingOperations.GetMultiBindingExpression(SourceLangIfAutoCb, ItemsControl.ItemsSourceProperty)?.UpdateTarget();
                BindingOperations.GetMultiBindingExpression(TargetLangIfSourceZhCb, ItemsControl.ItemsSourceProperty)?.UpdateTarget();
                BindingOperations.GetMultiBindingExpression(TargetLangIfSourceNotZhCb, ItemsControl.ItemsSourceProperty)?.UpdateTarget();
            };
    }
}