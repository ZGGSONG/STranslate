using Newtonsoft.Json;
using System.Text;

namespace STranslate.WeChatOcr;

public class WeChatOcrResult
{
    [JsonProperty("taskId")]
    public int TaskId { get; set; }

    [JsonProperty("ocrResult")]
    public OcrResult? OcrResult { get; set; }
}

public class OcrResult
{
    [JsonProperty("singleResult")]
    public List<SingleResult>? SingleResult { get; set; }
}

public class SingleResult
{
    [JsonProperty("singlePos")]
    public SinglePos? SinglePos { get; set; }

    [JsonProperty("singleStrUtf8")]
    public string? SingleStrUtf8 { get; set; }

    [JsonProperty("singleRate")]
    public float SingleRate { get; set; }

    // Location
    [JsonProperty("left")]
    public float Left { get; set; }

    [JsonProperty("top")]
    public float Top { get; set; }

    [JsonProperty("right")]
    public float Right { get; set; }

    [JsonProperty("bottom")]
    public float Bottom { get; set; }

    [JsonProperty("oneResult")]
    public List<OneResult>? OneResult { get; set; }
}

public class SinglePos
{
    [JsonProperty("pos")]
    public List<Pos>? Pos { get; set; }
}

public class Pos
{
    [JsonProperty("x")]
    public float X { get; set; }

    [JsonProperty("y")]
    public float Y { get; set; }
}

public class OneResult
{
    [JsonProperty("onePos")]
    public OnePos? OnePos { get; set; }

    [JsonProperty("oneStrUtf8")]
    public string? OneStrUtf8 { get; set; }
}

public class OnePos
{
    [JsonProperty("pos")]
    public List<Pos>? Pos { get; set; }
}

public class ParseOcrResult
{
    public static WeChatOcrResult? ParseJson(string jsonResponseStr)
    {
        var rt = JsonConvert.DeserializeObject<WeChatOcrResult>(jsonResponseStr);
        if (rt is not { OcrResult.SingleResult: not null }) return rt;
        foreach (var item in rt.OcrResult.SingleResult)
        {
            //转换整行
            if (item.SingleStrUtf8 != null)
                item.SingleStrUtf8 = Encoding.UTF8.GetString(Convert.FromBase64String(item.SingleStrUtf8));
            if (item.OneResult == null) continue;
            //逐个转换
            foreach (var oneResult in item.OneResult)
            {
                if (oneResult.OneStrUtf8 != null)
                    oneResult.OneStrUtf8 = Encoding.UTF8.GetString(Convert.FromBase64String(oneResult.OneStrUtf8));
            }
        }
        return rt;
    }
}
