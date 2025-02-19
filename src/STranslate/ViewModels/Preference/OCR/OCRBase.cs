using CommunityToolkit.Mvvm.ComponentModel;

namespace STranslate.ViewModels.Preference.OCR;

public partial class OCRBase : ObservableObject
{
    public void ManualPropChanged(params string[] array)
    {
        foreach (var str in array) OnPropertyChanged(str);
    }
}
