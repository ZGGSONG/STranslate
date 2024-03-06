using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;

namespace STranslate.Model
{
    /// <summary>
    /// OpenAI Prompt Definition
    /// </summary>
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

    /// <summary>
    /// Gemini Prompt Definition
    /// </summary>
    public partial class GeminiMessage : ObservableObject, ICloneable
    {
        [JsonIgnore]
        [ObservableProperty]
        [property: JsonProperty("role")]
        private string _role = "";

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonProperty("text")]
        private string _text = "";

        public GeminiMessage() { }

        public GeminiMessage(string role)
        {
            Role = role;
        }

        public GeminiMessage(string role, string text)
        {
            Role = role;
            Text = text;
        }

        public object Clone()
        {
            return new GeminiMessage(this.Role, this.Text);
        }
    }
}
