using Newtonsoft.Json;

namespace STranslate.Model
{
    /// <summary>
    /// Request
    /// </summary>
    public class RequestBing
    {
        public TextData[]? Req { get; set; }

        public string From { get; set; } = "";

        public string To { get; set; } = "";
    }

    public class TextData
    {
        [JsonProperty("text")]
        public string Text { get; set; } = "";
    }

    /// <summary>
    /// Response
    /// </summary>
    public class ResponseBing
    {
        [JsonProperty("detectedLanguage")]
        public DetectedLanguage? DetectedLanguage { get; set; }

        [JsonProperty("translations")]
        public Translation[]? Translations { get; set; }
    }

    public class DetectedLanguage
    {
        [JsonProperty("language")]
        public string Language { get; set; } = "";

        [JsonProperty("score")]
        public double Score { get; set; }
    }

    public class Translation
    {
        [JsonProperty("text")]
        public string Text { get; set; } = "";

        [JsonProperty("to")]
        public string To { get; set; } = "";
    }
}
