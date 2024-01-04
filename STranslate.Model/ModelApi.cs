using Newtonsoft.Json;

namespace STranslate.Model
{
    public class RequestApi
    {
        [JsonProperty("text")]
        public string Text { get; set; } = "";

        [JsonProperty("source_lang")]
        public string SourceLang { get; set; } = "";

        [JsonProperty("target_lang")]
        public string TargetLang { get; set; } = "";
    }

    public class ResponseApi
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; } = "";

        public string ErrMsg { get; set; } = "";
    }
}
