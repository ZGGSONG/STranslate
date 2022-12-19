using Newtonsoft.Json;

namespace STranslate.Model
{
    public class DeeplReq
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("source_lang")]
        public string SourceLang { get; set; }

        [JsonProperty("target_lang")]
        public string TargetLang { get; set; }
    }

    public class DeeplResp
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }
}