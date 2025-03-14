using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Model;

namespace STranslate.ViewModels.Preference.Translator;

public partial class PromptViewModel : ObservableObject
{
    private readonly object _serviceType;

    private readonly UserDefinePrompt _tmpPrompt;

    [ObservableProperty] private UserDefinePrompt _userDefinePrompt;

    public PromptViewModel(object type, UserDefinePrompt definePrompt)
    {
        _serviceType = type;
        UserDefinePrompt = definePrompt;
        _tmpPrompt = (UserDefinePrompt)definePrompt.Clone();
    }

    [RelayCommand]
    private void Add()
    {
        var last = UserDefinePrompt.Prompts.LastOrDefault()?.Role ?? "";
        var newOne = _serviceType switch
        {
            //TODO: 新OCR服务需要适配
            #region OCR Type

            OCRType.GeminiOCR
                    => last switch
                    {
                        "user" => new Prompt("model"),
                        _ => new Prompt("user")
                    },
            OCRType.OpenAIOCR
                => last switch
                {
                    "" => new Prompt("system"),
                    "user" => new Prompt("assistant"),
                    _ => new Prompt("user")
                },

            #endregion

            //TODO: 新接口需要适配
            #region Translator Type
            ServiceType.GeminiService
                    => last switch
                    {
                        "user" => new Prompt("model"),
                        _ => new Prompt("user")
                    },
            ServiceType.ChatglmService
                => last switch
                {
                    "user" => new Prompt("assistant"),
                    _ => new Prompt("user")
                },
            ServiceType.BaiduBceService
                => last switch
                {
                    "user" => new Prompt("assistant"),
                    _ => new Prompt("user")
                },
            _
                => last switch
                {
                    "" => new Prompt("system"),
                    "user" => new Prompt("assistant"),
                    _ => new Prompt("user")
                }

                #endregion
        };
        UserDefinePrompt.Prompts.Add(newOne);
    }

    [RelayCommand]
    private void Del(Prompt prompt)
    {
        UserDefinePrompt.Prompts.Remove(prompt);
    }

    [RelayCommand]
    private void Save(Window window)
    {
        window.DialogResult = true;
    }

    [RelayCommand]
    private void Cancel(Window window)
    {
        UserDefinePrompt.Name = _tmpPrompt.Name;
        UserDefinePrompt.Enabled = _tmpPrompt.Enabled;
        UserDefinePrompt.Prompts.Clear();
        _tmpPrompt.Prompts.ToList().ForEach(UserDefinePrompt.Prompts.Add);
        window.DialogResult = false;
    }
}