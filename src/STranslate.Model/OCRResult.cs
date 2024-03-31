using System.Collections.Generic;
using System.Linq;

namespace STranslate.Model
{
    public class OcrResult
    {
        public List<OcrContent> OcrContents { get; set; } = [];

        public string Text => ToString();

        public override string ToString()
        {
            if (OcrContents == null)
            {
                return "";
            }

            return string.Join("", OcrContents.Select((OcrContent x) => x.Text).ToArray());
        }
    }

    public class OcrContent
    {
        public string Text { get; set; } = string.Empty;

        public List<BoxPoint> BoxPoints { get; set; } = [];

        public OcrContent()
        { }

        public OcrContent(string text)
        {
            Text = text;
        }

        public OcrContent(string text, List<BoxPoint> boxPoints)
        {
            Text = text;
            BoxPoints = boxPoints;
        }
    }

    public class BoxPoint(int x, int y)
    {
        public int X { get; set; } = x;

        public int Y { get; set; } = y;
    }
}