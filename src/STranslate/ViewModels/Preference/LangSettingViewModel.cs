using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Model;
using STranslate.Views.Preference;

namespace STranslate.ViewModels.Preference;

public partial class LangSettingViewModel : ObservableObject
{
    [ObservableProperty] private BindingList<LangSetting> _langSettings = [];

    public LangSettingViewModel(string lang)
    {
        // 语种集合
        var langArray = EnumExtensions.GetEnumArray<LangEnum>();
        var langEnabledArray = lang.Length != langArray.Length || string.IsNullOrEmpty(lang)
            ? Enumerable.Repeat(true, langArray.Length).ToArray()
            : lang.Select(c => c == '1').ToArray();
        for (var i = 0; i < langArray.Length; i++)
            LangSettings.Add(new LangSetting(GetDescription(langArray[i]), langEnabledArray[i]));
    }

    [RelayCommand]
    private void Save(LangSettingView window)
    {
        var ret = string.Join("", LangSettings.Select(x => x.IsEnabled ? "1" : "0"));
        window.LangResult = ret;
        window.DialogResult = true;
        window.Close();
    }

    [RelayCommand]
    private void Cancel(LangSettingView window)
    {
        window.DialogResult = false;
        window.Close();
    }

    [RelayCommand]
    private void Select(string? content)
    {
        foreach (var item in LangSettings)
        {
            item.IsEnabled = !string.IsNullOrEmpty(content);
        }
    }

    private string GetDescription(LangEnum @enum)
    {
        var fieldName = @enum.ToString() ?? "";

        // 尝试从资源获取本地化描述
        var fullPath = $"{@enum.GetType().Name}.{fieldName}";
        var localizedDesc = AppLanguageManager.GetString(fullPath);

        // 如果找到本地化描述，则使用它
        if (localizedDesc != fullPath)
            return localizedDesc;

        // 否则使用Description特性或枚举值名称
        return @enum.GetDescription() ?? fieldName;
    }
}