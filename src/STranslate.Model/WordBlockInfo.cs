namespace STranslate.Model;

public class WordBlockInfo
{
    public string Text { get; set; } = string.Empty;
    // 使用 System.Drawing.Point 作为位置信息
    public System.Drawing.Point Position { get; set; }

    // 可选：添加文本宽度和高度属性，用于更精确的定位
    public int Width { get; set; }
    public int Height { get; set; }
    
    // 根据OCR识别的文本行高计算出的字体大小
    public double FontSize { get; set; }
}