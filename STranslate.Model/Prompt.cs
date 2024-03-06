using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;

namespace STranslate.Model
{
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
