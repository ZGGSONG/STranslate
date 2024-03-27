using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

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
        [property: JsonProperty("enabled")]
        private bool _enabled = false;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonProperty("prompts")]
        private BindingList<Prompt> _prompts;

        public UserDefinePrompt(string name, BindingList<Prompt> prompts, bool enabled = false)
        {
            Name = name;
            Prompts = prompts;
            Enabled = enabled;
        }

        public object Clone()
        {
            return new UserDefinePrompt(Name, Prompts, Enabled);
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
