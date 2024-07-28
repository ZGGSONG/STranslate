using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Model;
using System.Windows;

namespace STranslate.ViewModels.Preference;

public partial class LangSettingViewModel : ObservableObject
{
    public LangSettingViewModel(string lang)
    {
        // 语种集合
        var langArray = EnumExtensions.GetEnumArray<LangEnum>();
        var langEnableds = (lang.Length != langArray.Length || string.IsNullOrEmpty(lang))
            ? Enumerable.Repeat(true, langArray.Length).ToArray()
            : lang.Select(c => c == '1').ToArray();
    }

    [RelayCommand]
    private void Save(Window window)
    {
        //TODO: 保存语种设置

        window.DialogResult = true;
        window.Close();
    }

    [RelayCommand]
    private void Reset(Window window)
    {
        //TODO: 重置语种设置

        window.DialogResult = false;
        window.Close();
    }
}