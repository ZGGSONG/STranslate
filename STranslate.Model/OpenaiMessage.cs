using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;

namespace STranslate.Model
{
    public partial class OpenaiMessage : ObservableObject, ICloneable
    {
        [JsonIgnore]
        [ObservableProperty]
        [property: JsonProperty("role")]
        private string _role = "";

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonProperty("content")]
        private string _content = "";

        public OpenaiMessage() { }

        public OpenaiMessage(string role)
        {
            Role = role;
            Content = "";
        }

        public OpenaiMessage(string role, string content)
        {
            Role = role;
            Content = content;
        }

        public object Clone()
        {
            return new OpenaiMessage(this.Role, this.Content);
        }
    }
}
