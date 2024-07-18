using Newtonsoft.Json;

namespace STranslate.Model;

public class LanguageInfo
{
    [JsonProperty("isTranslationSupported")]
    public bool IsTranslationSupported { get; set; }

    [JsonProperty("isTransliterationSupported")]
    public bool IsTransliterationSupported { get; set; }

    [JsonProperty("language")] public string Language { get; set; } = "";

    [JsonProperty("score")] public double Score { get; set; }
}