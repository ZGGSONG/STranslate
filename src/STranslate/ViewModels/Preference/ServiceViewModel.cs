using CommunityToolkit.Mvvm.ComponentModel;

namespace STranslate.ViewModels.Preference;

public partial class ServiceViewModel : ObservableObject
{
    [ObservableProperty] private uint _selectedIndex = 0;
}