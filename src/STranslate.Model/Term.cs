using CommunityToolkit.Mvvm.ComponentModel;

namespace STranslate.Model;

public partial class Term : ObservableObject
{
    [ObservableProperty] private string _sourceText = string.Empty;
    [ObservableProperty] private string _targetText = string.Empty;
    //[ObservableProperty] private bool _isEnabled = true;
}
