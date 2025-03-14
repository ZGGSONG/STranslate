using Newtonsoft.Json;

namespace STranslate.Model;

#region Request

public class RequestModel(string text, LangEnum source, LangEnum target)
{
    [JsonProperty("text")] public string Text { get; set; } = text;

    [JsonProperty("source_lang")] public LangEnum SourceLang { get; set; } = source;

    [JsonProperty("target_lang")] public LangEnum TargetLang { get; set; } = target;
}

#endregion Request

#region API

public class ResponseApi
{
    [JsonProperty("code")] public int Code { get; set; }

    [JsonProperty("data")] public object Data { get; set; } = "";

    public string ErrMsg { get; set; } = "";
}

#endregion API

#region Baidu

public class ResponseBaidu
{
    [JsonProperty("from")] public string From { get; set; } = "";

    [JsonProperty("to")] public string To { get; set; } = "";

    [JsonProperty("trans_result")] public TransResult[]? TransResult { get; set; }
}

public class TransResult
{
    [JsonProperty("src")] public string Src { get; set; } = "";

    [JsonProperty("dst")] public string Dst { get; set; } = "";
}

#endregion Baidu

#region Bing

/// <summary>
///     Response
/// </summary>
public class ResponseBing
{
    [JsonProperty("detectedLanguage")] public DetectedLanguage? DetectedLanguage { get; set; }

    [JsonProperty("translations")] public Translation[]? Translations { get; set; }
}

public class DetectedLanguage
{
    [JsonProperty("language")] public string Language { get; set; } = "";

    [JsonProperty("score")] public double Score { get; set; }
}

public class Translation
{
    [JsonProperty("text")] public string Text { get; set; } = "";

    [JsonProperty("to")] public string To { get; set; } = "";
}

#endregion Bing