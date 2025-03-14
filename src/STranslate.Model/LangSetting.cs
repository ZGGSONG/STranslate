using CommunityToolkit.Mvvm.ComponentModel;

namespace STranslate.Model;

public partial class LangSetting(string name = "", bool isEnabled = false) : ObservableObject
{
    [ObservableProperty] private bool _isEnabled = isEnabled;
    [ObservableProperty] private string _name = name;
}