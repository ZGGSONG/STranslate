using Newtonsoft.Json;

namespace STranslate.Model
{
    public class BaiduModel
    {
        public string Text { get; set; }
        public string From { get; set; }
        public string TO { get; set; }
        public string AppId { get; set; }
        public string Salt { get; set; }
        public string Sign { get; set; }
    }

    public class BaiduResp
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("trans_result")]
        public TransResult[] TransResult { get; set; }
    }

    public class TransResult
    {
        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("dst")]
        public string Dst { get; set; }
    }
}