using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace STranslate.Model
{
    public partial class UserDefinePrompt : ObservableObject, ICloneable
    {
        [JsonIgnore]
        [ObservableProperty]
        [property: JsonProperty("name")]
        private string _name;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonProperty("prompts")]
        private BindingList<Prompt> _prompts;

        public UserDefinePrompt(string name, BindingList<Prompt> prompts)
        {
            Name = name;
            Prompts = prompts;
        }

        public object Clone()
        {
            return new UserDefinePrompt(Name, Prompts);
        }
    }

    /// <summary>
    /// Prompt Definition
    /// </summary>
    public partial class Prompt : ObservableObject, ICloneable
    {
        [JsonIgnore]
        [ObservableProperty]
        [property: JsonProperty("role")]
        private string _role = "";

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonProperty("content")]
        private string _content = "";

        public Prompt() { }

        public Prompt(string role)
        {
            Role = role;
            Content = "";
        }

        public Prompt(string role, string content)
        {
            Role = role;
            Content = content;
        }

        public object Clone()
        {
            return new Prompt(this.Role, this.Content);
        }
    }
}
