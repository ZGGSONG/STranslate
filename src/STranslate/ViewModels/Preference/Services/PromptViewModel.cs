using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Model;
using System.Linq;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class PromptViewModel : ObservableObject
    {
        [ObservableProperty]
        private UserDefinePrompt _userDefinePrompt;

        private readonly ServiceType _serviceType;

        public PromptViewModel(ServiceType type, UserDefinePrompt definePrompt)
        {
            _serviceType = type;
            UserDefinePrompt = definePrompt;
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
    }
}
