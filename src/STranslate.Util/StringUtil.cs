using STranslate.Model;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace STranslate.Util
{
    public class StringUtil
    {
        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }

        /// <summary>
        /// 构造蛇形结果
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static string GenSnakeString(List<string> req)
        {
            var ret = string.Empty;

            req.ForEach(x =>
            {
                ret += "_" + x.ToLower();
            });
            return ret[1..];
        }

        /// <summary>
        /// 构造驼峰结果
        /// </summary>
        /// <param name="req"></param>
        /// <param name="isSmallHump">是否为小驼峰</param>
        /// <returns></returns>
        public static string GenHumpString(List<string> req, bool isSmallHump)
        {
            try
            {
                string ret = string.Empty;
                var array = req.ToArray();
                for (var j = 0; j < array.Length; j++)
                {
                    char[] chars = array[j].ToCharArray();
                    //判断chars是否为空
                    if (chars.Length == 0)
                        continue;
                    if (j == 0 && isSmallHump)
                        chars[0] = char.ToLower(chars[0]);
                    else
                        chars[0] = char.ToUpper(chars[0]);
                    for (int i = 1; i < chars.Length; i++)
                    {
                        chars[i] = char.ToLower(chars[i]);
                    }
                    ret += new string(chars);
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception("[GENHUMP] 构造驼峰异常", ex);
            }
        }

        /// <summary>
        /// 提取英文
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ExtractEngString(string str)
        {
            Regex regex = new Regex("[a-zA-Z]+");

            MatchCollection mMactchCol = regex.Matches(str);
            string strA_Z = string.Empty;
            foreach (Match mMatch in mMactchCol)
            {
                strA_Z += mMatch.Value;
            }
            return strA_Z;
        }

        /// <summary>
        /// 划词文本预处理，例如PDF文字复制出来总含有很多多余的空格
        /// 使用正则表达式[\\s]+匹配连续的空白字符（包括空格、制表符、换行符等），并将其替换为单个空格字符
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
        /// 自动识别语种
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
                .Replace(text, "[1234567890!\"#$%&'()*+,-./:;<=>?@\\[\\]^_`{|}~，。、《》？；‘’：“”【】、{}|·！@#￥%……&*（）——+~\\\\]", string.Empty)
                .Replace(Environment.NewLine, "")
                .Replace(" ", "");

            //2. 取出上一步中所有英文字符
            var engStr = ExtractEngString(text);

            var ratio = (double)engStr.Length / text.Length;

            //3. 判断英文字符个数占第一步所有字符个数比例，若超过一定比例则判定原字符串为英文字符串，否则为中文字符串
            if (ratio > scale)
            {
                return new Tuple<LangEnum, LangEnum>(LangEnum.en, LangEnum.zh_cn);
            }
            else
            {
                return new Tuple<LangEnum, LangEnum>(LangEnum.zh_cn, LangEnum.en);
            }
        }

        /// <summary>
        /// 移除换行
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RemoveLineBreaks(string content) => content.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

        /// <summary>
        /// 移除空格
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RemoveSpace(string content) => content.Replace(" ", "");

        /// <summary>
        /// 检查是否为单词
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsWord(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsLetter(c))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 是否可以升级
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
}
