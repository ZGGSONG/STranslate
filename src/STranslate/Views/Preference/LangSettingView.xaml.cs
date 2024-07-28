using STranslate.ViewModels.Preference;
using System.Windows;

namespace STranslate.Views.Preference;

public partial class LangSettingView : Window
{
    public LangSettingView(string lang)
    {
        InitializeComponent();

        var vm = new LangSettingViewModel(lang);
        DataContext = vm;
    }
}