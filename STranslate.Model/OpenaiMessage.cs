using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace STranslate.Model
{
    public partial class OpenaiMessage : ObservableObject
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
    }
}
