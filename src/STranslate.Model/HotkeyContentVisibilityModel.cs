using CommunityToolkit.Mvvm.ComponentModel;

namespace STranslate.Model;

public partial class HotkeyContentVisibilityModel : ObservableObject
{
    [ObservableProperty] private string _content = string.Empty;

    [ObservableProperty] private bool _contentVisible;
}