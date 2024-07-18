namespace STranslate.Model;

public class OcrResult
{
    public List<OcrContent> OcrContents { get; set; } = [];

    /// <summary>
    ///     精简版文本通过换行组合
    /// </summary>
    public string Text => string.Join(Environment.NewLine, OcrContents.Select(x => x.Text).ToArray()).Trim();

    public static OcrResult Empty => new();

    public bool Success { get; set; } = true;

    public string ErrorMsg { get; set; } = string.Empty;

    /// <summary>
    ///     重写ToString方法,以空格组合结果
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return string.Join(" ", OcrContents.Select(x => x.Text).ToArray()).Trim();
    }

    public static OcrResult Fail(string msg)
    {
        return new OcrResult { Success = false, ErrorMsg = msg };
    }
}

public class OcrContent
{
    public OcrContent()
    {
    }

    public OcrContent(string text)
    {
        Text = text;
    }

    public OcrContent(string text, List<BoxPoint> boxPoints)
    {
        Text = text;
        BoxPoints = boxPoints;
    }

    public string Text { get; set; } = string.Empty;

    public List<BoxPoint> BoxPoints { get; set; } = [];
}

public class BoxPoint(int x, int y)
{
    public int X { get; set; } = x;

    public int Y { get; set; } = y;
}