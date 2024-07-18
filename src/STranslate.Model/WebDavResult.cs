using CommunityToolkit.Mvvm.ComponentModel;

namespace STranslate.Model;

public partial class WebDavResult : ObservableObject
{
    [ObservableProperty] private string _fullName = string.Empty;

    [ObservableProperty] private bool _isEdit;

    [ObservableProperty] private string _name = string.Empty;

    public WebDavResult()
    {
    }

    public WebDavResult(string fullName, bool isEdit = false)
    {
        FullName = fullName;
        Name = FullName.Replace(ConstStr.ZIP, string.Empty);
        IsEdit = isEdit;
    }
}