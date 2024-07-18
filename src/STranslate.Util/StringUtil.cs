using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using STranslate.Model;

namespace STranslate.Util;

public class StringUtil
{
    /// <summary>
    ///     计算MD5值
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string EncryptString(string str)
    {
        var md5 = MD5.Create();
        // 将字符串转换成字节数组
        var byteOld = Encoding.UTF8.GetBytes(str);
        // 调用加密方法
        var byteNew = md5.ComputeHash(byteOld);
        // 将加密结果转换为字符串
        var sb = new StringBuilder();
        foreach (var b in byteNew)
            // 将字节转换成16进制表示的字符串，
            sb.Append(b.ToString("x2"));
        // 返回加密的字符串
        return sb.ToString();
    }

    /// <summary>
    ///     构造蛇形结果
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string GenSnakeString(string content)
    {
        var sb = new StringBuilder();
        content.Split(' ').ToList().ForEach(x => sb.Append("_").Append(x.ToLower()));
        return sb.ToString()[1..];
    }

    /// <summary>
    ///     构造驼峰结果
    /// </summary>
    /// <param name="content"></param>
    /// <param name="isSmallHump">是否为小驼峰</param>
    /// <returns></returns>
    public static string GenHumpString(string content, bool isSmallHump = false)
    {
        try
        {
            var lines = content.Split(Environment.NewLine);
            var processedLines = lines.Select(line =>
                GenHumpString(line.Split(' '), isSmallHump));
            return string.Join(Environment.NewLine, processedLines);
        }
        catch (Exception e)
        {
            throw new Exception("[GEN-HUMP] 构造驼峰异常", e);
        }
    }

    internal static string GenHumpString(string[] req, bool isSmallHump)
    {
        var sb = new StringBuilder();
        for (var j = 0; j < req.Count(); j++)
        {
            if (string.IsNullOrEmpty(req[j])) continue;

            var word = req[j];
            if (j == 0 && isSmallHump)
                sb.Append(char.ToLower(word[0]));
            else
                sb.Append(char.ToUpper(word[0]));
            if (word.Length > 1) sb.Append(word[1..].ToLower());
        }

        return sb.ToString();
    }

    /// <summary>
    ///     提取英文
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ExtractEngString(string str)
    {
        var regex = new Regex("[a-zA-Z]+");

        var matchCollection = regex.Matches(str);
        var ret = string.Empty;
        foreach (Match mMatch in matchCollection) ret += mMatch.Value;
        return ret;
    }

    /// <summary>
    ///     划词文本预处理，例如PDF文字复制出来总含有很多多余的空格
    ///     使用正则表达式[\\s]+匹配连续的空白字符（包括空格、制表符、换行符等），并将其替换为单个空格字符
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string PreProcessTexts(string text)
    {
        try
        {
            text = new Regex("[\\s]+").Replace(text, " ");
        }
        catch (Exception)
        {
            text = string.Empty;
        }

        return text.Trim();
    }

    /// <summary>
    ///     自动识别语种
    /// </summary>
    /// <param name="text">输入语言</param>
    /// <param name="scale">英文占比</param>
    /// <returns>
    ///     Item1: SourceLang
    ///     Item2: TargetLang
    /// </returns>
    public static Tuple<LangEnum, LangEnum> AutomaticLanguageRecognition(string text, double scale = 0.8)
    {
        //1. 首先去除所有数字、标点及特殊符号
        //https://www.techiedelight.com/zh/strip-punctuations-from-a-string-in-csharp/
        text = Regex
            .Replace(text, "[1234567890!\"#$%&'()*+,-./:;<=>?@\\[\\]^_`{|}~，。、《》？；‘’：“”【】、{}|·！@#￥%……&*（）——+~\\\\]",
                string.Empty)
            .Replace(Environment.NewLine, "")
            .Replace(" ", "");

        //2. 取出上一步中所有英文字符
        var engStr = ExtractEngString(text);

        var ratio = (double)engStr.Length / text.Length;

        //3. 判断英文字符个数占第一步所有字符个数比例，若超过一定比例则判定原字符串为英文字符串，否则为中文字符串
        return ratio > scale
            ? new Tuple<LangEnum, LangEnum>(LangEnum.en, LangEnum.zh_cn)
            : new Tuple<LangEnum, LangEnum>(LangEnum.zh_cn, LangEnum.en);
    }

    /// <summary>
    ///     移除换行
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string RemoveLineBreaks(string content)
    {
        return content.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
    }

    /// <summary>
    ///     移除空格
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string RemoveSpace(string content)
    {
        return content.Replace(" ", "");
    }

    /// <summary>
    ///     检查是否为单词
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsWord(string input)
    {
        return input.All(char.IsLetter);
    }

    /// <summary>
    ///     是否可以升级
    /// </summary>
    /// <param name="rVer"></param>
    /// <param name="lVer"></param>
    /// <returns></returns>
    public static bool IsCanUpdate(string rVer, string lVer)
    {
        // 获取版本移除小数点后数字大小
        var remoteVersion = Convert.ToInt64(rVer.Replace(".", ""));
        var localVersion = Convert.ToInt64(lVer.Replace(".", ""));

        // 如果远端版本号数字大于本地版本号数字即可升级
        return localVersion < remoteVersion;
    }
}