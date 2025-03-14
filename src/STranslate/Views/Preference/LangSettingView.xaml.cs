using STranslate.ViewModels.Preference;

namespace STranslate.Views.Preference;

public partial class LangSettingView
{
    public LangSettingView(string lang)
    {
        InitializeComponent();

        var vm = new LangSettingViewModel(lang);
        DataContext = vm;
    }

    public string LangResult { get; set; } = string.Empty;
}