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

        Singleton<CommonViewModel>.Instance.OnOftenUsedLang +=
            () => BindingOperations.GetMultiBindingExpression(LangCb, ItemsControl.ItemsSourceProperty)?.UpdateTarget();
    }
}