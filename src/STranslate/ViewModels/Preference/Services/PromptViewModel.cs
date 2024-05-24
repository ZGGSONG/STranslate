using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Model;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class PromptViewModel : ObservableObject
    {
        [ObservableProperty]
        private UserDefinePrompt _userDefinePrompt;

        private readonly UserDefinePrompt _tmpPrompt;

        private readonly ServiceType _serviceType;

        public PromptViewModel(ServiceType type, UserDefinePrompt definePrompt)
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
                    },
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
}
