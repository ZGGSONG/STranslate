using System.Text.RegularExpressions;
using System.Text;

namespace STranslate.Model;

public class OcrResult
{
    public List<OcrContent> OcrContents { get; set; } = [];

    /// <summary>
    ///     精简版文本通过换行组合
    /// </summary>
    public string Text => string.Join(Environment.NewLine, [.. OcrContents.Select(x => x.Text)]).Trim();

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

public class BoxPoint(float x, float y)
{
    public float X { get; set; } = x;

    public float Y { get; set; } = y;
}

/// <summary>
/// OCR结果优化器-Claude
/// </summary>
public class OcrResultOptimizer
{
    /// <summary>
    /// 优化OCR结果使其更适合翻译
    /// </summary>
    /// <param name="ocrResult">原始OCR结果</param>
    /// <returns>优化后的OCR结果，段落已合并</returns>
    public static OcrResult OptimizeForTranslation(OcrResult ocrResult)
    {
        if (!ocrResult.Success || ocrResult.OcrContents.Count == 0)
        {
            return ocrResult; // 如果OCR失败或无内容，直接返回原结果
        }

        // 1. 基于位置信息优化
        var optimizedByPosition = OptimizeByPosition(ocrResult);

        // 2. 基于内容优化段落
        var optimizedByContent = OptimizeByContent(optimizedByPosition);

        // 3. 清理优化后的文本（确保保留英文单词之间的空格）
        var finalResult = CleanOptimizedText(optimizedByContent);

        return finalResult;
    }

    /// <summary>
    /// 基于位置信息合并相邻行
    /// </summary>
    private static OcrResult OptimizeByPosition(OcrResult ocrResult)
    {
        if (ocrResult.OcrContents.Count <= 1)
        {
            return ocrResult; // 只有一行或没有内容，无需优化
        }

        // 对内容按Y坐标排序（从上到下）
        var sortedContents = ocrResult.OcrContents
            .OrderBy(c => c.BoxPoints.Count > 0 ? c.BoxPoints.Average(p => p.Y) : 0)
            .ToList();

        List<OcrContent> mergedContents = new List<OcrContent>();
        OcrContent currentLine = sortedContents[0];

        // Y坐标差异阈值，用于判断是否为同一行
        float yThreshold = CalculateYThreshold(sortedContents);

        for (int i = 1; i < sortedContents.Count; i++)
        {
            var nextLine = sortedContents[i];

            // 跳过空文本
            if (string.IsNullOrWhiteSpace(nextLine.Text))
                continue;

            // 计算当前行和下一行的平均Y坐标
            float currentAvgY = currentLine.BoxPoints.Count > 0
                ? currentLine.BoxPoints.Average(p => p.Y)
                : 0;

            float nextAvgY = nextLine.BoxPoints.Count > 0
                ? nextLine.BoxPoints.Average(p => p.Y)
                : float.MaxValue;

            // 判断是否为同一行
            if (Math.Abs(nextAvgY - currentAvgY) < yThreshold)
            {
                // 判断是否需要添加空格
                string mergedText = currentLine.Text;

                if (NeedsSpaceBetween(currentLine.Text, nextLine.Text))
                {
                    mergedText += " ";
                }

                mergedText += nextLine.Text;

                // 合并坐标点 - 正确处理合并后的坐标
                List<BoxPoint> mergedBoxPoints = MergeBoxPoints(currentLine.BoxPoints, nextLine.BoxPoints);

                currentLine = new OcrContent(mergedText, mergedBoxPoints);
            }
            else
            {
                // 不是同一行，保存当前行并开始新行
                mergedContents.Add(currentLine);
                currentLine = nextLine;
            }
        }

        // 添加最后一行
        if (!string.IsNullOrWhiteSpace(currentLine.Text))
        {
            mergedContents.Add(currentLine);
        }

        return new OcrResult
        {
            OcrContents = mergedContents,
            Success = ocrResult.Success,
            ErrorMsg = ocrResult.ErrorMsg
        };
    }

    /// <summary>
    /// 合并两组坐标点，保持外边界
    /// </summary>
    private static List<BoxPoint> MergeBoxPoints(List<BoxPoint> points1, List<BoxPoint> points2)
    {
        // 如果其中一个为空，直接返回另一个
        if (points1.Count == 0)
            return new List<BoxPoint>(points2);
        if (points2.Count == 0)
            return new List<BoxPoint>(points1);

        // 找出合并后的外边界
        float minX = Math.Min(points1.Min(p => p.X), points2.Min(p => p.X));
        float minY = Math.Min(points1.Min(p => p.Y), points2.Min(p => p.Y));
        float maxX = Math.Max(points1.Max(p => p.X), points2.Max(p => p.X));
        float maxY = Math.Max(points1.Max(p => p.Y), points2.Max(p => p.Y));

        // 创建新的外边界坐标点
        return new List<BoxPoint>
            {
                new BoxPoint(minX, minY),  // 左上
                new BoxPoint(maxX, minY),  // 右上
                new BoxPoint(maxX, maxY),  // 右下
                new BoxPoint(minX, maxY)   // 左下
            };
    }

    /// <summary>
    /// 计算行间距阈值
    /// </summary>
    private static float CalculateYThreshold(List<OcrContent> contents)
    {
        // 动态计算行间距阈值
        List<float> lineHeights = new List<float>();
        List<float> lineGaps = new List<float>();

        for (int i = 0; i < contents.Count; i++)
        {
            if (contents[i].BoxPoints.Count >= 4)
            {
                // 估算行高
                float minY = contents[i].BoxPoints.Min(p => p.Y);
                float maxY = contents[i].BoxPoints.Max(p => p.Y);
                lineHeights.Add(maxY - minY);

                // 计算行间距
                if (i > 0 && contents[i - 1].BoxPoints.Count >= 4)
                {
                    float prevMaxY = contents[i - 1].BoxPoints.Max(p => p.Y);
                    lineGaps.Add(minY - prevMaxY);
                }
            }
        }

        // 没有足够的数据，使用默认值
        if (lineHeights.Count < 2)
            return 10.0f;

        // 使用平均行高的60%作为阈值
        float avgLineHeight = lineHeights.Average();

        // 如果有行间距数据，使用它来优化阈值
        if (lineGaps.Count > 0 && lineGaps.Average() > 0)
        {
            // 取平均行高的60%和平均行间距的30%的较小值
            return Math.Min(avgLineHeight * 0.6f, lineGaps.Average() * 0.3f);
        }

        return avgLineHeight * 0.6f;
    }

    /// <summary>
    /// 基于内容优化段落
    /// </summary>
    private static OcrResult OptimizeByContent(OcrResult ocrResult)
    {
        List<OcrContent> optimizedContents = new List<OcrContent>();
        OcrContent currentParagraph = null;

        foreach (var content in ocrResult.OcrContents)
        {
            string text = content.Text;  // 不去除前后空格，确保保留原有格式

            // 跳过完全空行
            if (string.IsNullOrEmpty(text))
            {
                // 如果当前有段落且空行，则结束当前段落
                if (currentParagraph != null)
                {
                    optimizedContents.Add(currentParagraph);
                    currentParagraph = null;
                }
                continue;
            }

            // 检查是否应该开始新段落
            bool isNewParagraphStart = IsNewParagraphStart(text);
            bool isParagraphEnd = currentParagraph != null && IsParagraphEnd(currentParagraph.Text);

            if ((isNewParagraphStart || isParagraphEnd) && currentParagraph != null)
            {
                // 完成当前段落并开始新段落
                optimizedContents.Add(currentParagraph);
                currentParagraph = null;
            }

            // 创建或追加到当前段落
            if (currentParagraph == null)
            {
                // 新段落
                currentParagraph = new OcrContent(text, new List<BoxPoint>(content.BoxPoints));
            }
            else
            {
                // 追加到现有段落
                string mergedText = currentParagraph.Text;

                if (NeedsSpaceBetween(currentParagraph.Text, text))
                {
                    mergedText += " ";
                }

                mergedText += text;

                // 合并坐标点
                List<BoxPoint> mergedBoxPoints = MergeBoxPoints(currentParagraph.BoxPoints, content.BoxPoints);

                currentParagraph = new OcrContent(mergedText, mergedBoxPoints);
            }
        }

        // 添加最后一个段落
        if (currentParagraph != null)
        {
            optimizedContents.Add(currentParagraph);
        }

        return new OcrResult
        {
            OcrContents = optimizedContents,
            Success = ocrResult.Success,
            ErrorMsg = ocrResult.ErrorMsg
        };
    }

    /// <summary>
    /// 清理优化后的文本，确保保留必要的空格
    /// </summary>
    private static OcrResult CleanOptimizedText(OcrResult ocrResult)
    {
        List<OcrContent> cleanedContents = new List<OcrContent>();

        foreach (var content in ocrResult.OcrContents)
        {
            // 关键改动：保留文本中的所有空格，只处理标点符号相关的空格问题
            string cleaned = content.Text;

            // 修复英文标点周围的空格问题
            cleaned = Regex.Replace(cleaned, @"\s+,", ",");
            cleaned = Regex.Replace(cleaned, @"\s+\.", ".");
            cleaned = Regex.Replace(cleaned, @"\s+\?", "?");
            cleaned = Regex.Replace(cleaned, @"\s+!", "!");
            cleaned = Regex.Replace(cleaned, @"\s+:", ":");
            cleaned = Regex.Replace(cleaned, @"\s+;", ";");
            cleaned = Regex.Replace(cleaned, @"\s+\)", ")");
            cleaned = Regex.Replace(cleaned, @"\(\s+", "(");

            // 修复中文标点周围的空格
            cleaned = Regex.Replace(cleaned, @"\s+。", "。");
            cleaned = Regex.Replace(cleaned, @"。\s+", "。");
            cleaned = Regex.Replace(cleaned, @"\s+，", "，");
            cleaned = Regex.Replace(cleaned, @"，\s+", "，");
            cleaned = Regex.Replace(cleaned, @"\s+：", "：");
            cleaned = Regex.Replace(cleaned, @"：\s+", "：");
            cleaned = Regex.Replace(cleaned, @"\s+；", "；");
            cleaned = Regex.Replace(cleaned, @"；\s+", "；");
            cleaned = Regex.Replace(cleaned, @"\s+（", "（");
            cleaned = Regex.Replace(cleaned, @"）\s+", "）");
            cleaned = Regex.Replace(cleaned, @"\s+「", "「");
            cleaned = Regex.Replace(cleaned, @"」\s+", "」");
            //cleaned = Regex.Replace(cleaned, @"\s+", "");
            //cleaned = Regex.Replace(cleaned, @"\s + ", "");

            // 修复连续多个空格，但保留单个空格
            cleaned = Regex.Replace(cleaned, @"[ \t]{2,}", " ");

            // 修复行尾连字符合并问题
            cleaned = Regex.Replace(cleaned, @"-\s+", "");

            cleanedContents.Add(new OcrContent(cleaned, content.BoxPoints));
        }

        return new OcrResult
        {
            OcrContents = cleanedContents,
            Success = ocrResult.Success,
            ErrorMsg = ocrResult.ErrorMsg
        };
    }

    /// <summary>
    /// 判断两段文本之间是否需要空格
    /// </summary>
    private static bool NeedsSpaceBetween(string prev, string next)
    {
        if (string.IsNullOrEmpty(prev) || string.IsNullOrEmpty(next))
            return false;

        // 获取末尾和开头的实际字符（忽略空格）
        char lastChar = prev.TrimEnd()[prev.TrimEnd().Length - 1];
        char firstChar = next.TrimStart()[0];

        // 中文字符不需要空格
        bool lastIsChinese = Regex.IsMatch(lastChar.ToString(), @"[\u4e00-\u9fa5]");
        bool firstIsChinese = Regex.IsMatch(firstChar.ToString(), @"[\u4e00-\u9fa5]");

        // 标点符号处理
        bool lastIsPunctuation = char.IsPunctuation(lastChar);
        bool firstIsPunctuation = char.IsPunctuation(firstChar);

        // 英文和数字之间需要空格
        bool lastIsLatinOrDigit = Regex.IsMatch(lastChar.ToString(), @"[a-zA-Z0-9]");
        bool firstIsLatinOrDigit = Regex.IsMatch(firstChar.ToString(), @"[a-zA-Z0-9]");

        // 保留原始文本末尾的空格
        if (prev.EndsWith(" "))
            return true;

        // 保留原始文本开头的空格
        if (next.StartsWith(" "))
            return true;

        return !lastIsChinese && !firstIsChinese &&
               lastIsLatinOrDigit && firstIsLatinOrDigit &&
               !lastIsPunctuation && !firstIsPunctuation;
    }

    /// <summary>
    /// 判断是否是段落开始
    /// </summary>
    private static bool IsNewParagraphStart(string text)
    {
        string trimmedText = text.Trim();
        if (string.IsNullOrEmpty(trimmedText))
            return false;

        // 段落标志：数字列表项、缩进、大写字母开头等
        return
            Regex.IsMatch(trimmedText, @"^\d+[\.\)、]") || // 数字列表
            Regex.IsMatch(trimmedText, @"^[一二三四五六七八九十]+[、]") || // 中文数字列表
            text.StartsWith("    ") ||
            text.StartsWith("\t") ||
            Regex.IsMatch(trimmedText, @"^第[一二三四五六七八九十\d]+[章节篇部]") || // "第一章"、"第1节"等
            (trimmedText.Length > 5 && Regex.IsMatch(trimmedText[0].ToString(), @"[A-Z]")); // 大写字母开头且长度足够
    }

    /// <summary>
    /// 判断是否是段落结束
    /// </summary>
    private static bool IsParagraphEnd(string text)
    {
        string trimmedText = text.Trim();
        if (string.IsNullOrEmpty(trimmedText))
            return false;

        // 中英文句号、问号、感叹号结尾视为段落可能结束
        char lastChar = trimmedText[trimmedText.Length - 1];
        return lastChar == '.' || lastChar == '。' ||
               lastChar == '!' || lastChar == '！' ||
               lastChar == '?' || lastChar == '？';
    }
}
