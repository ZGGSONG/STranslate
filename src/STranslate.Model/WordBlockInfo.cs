using System.Windows;

namespace STranslate.Model
{
    public class WordBlockInfo
    {
        public string Text { get; set; } = string.Empty;
        // 使用 System.Drawing.Point 作为位置信息
        public System.Drawing.Point Position { get; set; }
    }
}