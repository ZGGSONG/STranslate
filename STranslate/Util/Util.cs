using Newtonsoft.Json;
using STranslate.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media;
using Tesseract;

namespace STranslate.Util
{
    public class Util
    {
        #region 翻译接口
        public static async Task<string> TranslateDeepLAsync(string url, string text, LanguageEnum target, LanguageEnum source = LanguageEnum.AUTO)
        {
            var req = new DeeplReq()
            {
                Text = text,
                SourceLang = source.ToString(),
                TargetLang = target.ToString(),
            };

            if (source == LanguageEnum.AUTO)
            {
                req.SourceLang = LanguageEnum.AUTO.ToString().ToLower();
            }
            if (target == LanguageEnum.AUTO)
            {
                req.TargetLang = LanguageEnum.AUTO.ToString().ToLower();
            }
            var reqStr = JsonConvert.SerializeObject(req);
            var respStr = await PostAsync(url, reqStr);
            var resp = JsonConvert.DeserializeObject<DeeplResp>(respStr);

            if (resp == null || resp.Code != 200)
            {
                return string.Empty;
            }

            return resp?.Data ?? string.Empty;
        }

        /// <summary>
        /// 百度翻译异步接口
        /// </summary>
        /// <param name="appID">应用ID</param>
        /// <param name="secretKey">应用Secret</param>
        /// <param name="text">需要翻译的文本</param>
        /// <param name="target">目标语言</param>
        /// <param name="source">当前语言</param>
        /// <returns></returns>
        public static async Task<string> TranslateBaiduAsync(string appID, string secretKey, string text, LanguageEnum target, LanguageEnum source = LanguageEnum.AUTO)
        {
            try
            {
                Random rd = new Random();
                string salt = rd.Next(100000).ToString();
                string sign = EncryptString(appID + text + salt + secretKey);
                string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
                url += "q=" + HttpUtility.UrlEncode(text);
                url += "&from=" + source.ToString().ToLower();
                url += "&to=" + target.ToString().ToLower();
                url += "&appid=" + appID;
                url += "&salt=" + salt;
                url += "&sign=" + sign;
#if false
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
#endif

                var retString = await GetAsync(url);
                var resp = JsonConvert.DeserializeObject<BaiduResp>(retString);
                if (resp.From != null)
                {
                    return resp.TransResult[0]?.Dst;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // 计算MD5值
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
        /// 枚举信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<string, T> GetEnumList<T>() where T : Enum
        {
            var dict = new Dictionary<string, T>();
            List<T> list = Enum.GetValues(typeof(T)).OfType<T>().ToList();
            list.ForEach(x =>
            {
                dict.Add(x.GetDescription(), x);
            });
            return dict;
        }
        #endregion

        #region Http
        /// <summary>
        /// 异步Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string req)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(req, Encoding.UTF8, "application/json");

                var respContent = await client.PostAsync(url, content);

                string respStr = await respContent.Content.ReadAsStringAsync();
                ;
                return respStr;
            }
        }

        /// <summary>
        /// 异步Get请求
        /// </summary>
        /// <param name="urlpath"></param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string urlpath)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var respContent = await client.GetAsync(urlpath);

                    string respStr = await respContent.Content.ReadAsStringAsync();

                    return respStr;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        #endregion

        #region GenString
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
            return ret.Substring(1);
        }

        /// <summary>
        /// 构造驼峰结果
        /// </summary>
        /// <param name="req"></param>
        /// <param name="isSmallHump">是否为小驼峰</param>
        /// <returns></returns>
        public static string GenHumpString(List<string> req, bool isSmallHump)
        {
            string ret = string.Empty;
            var array = req.ToArray();
            for (var j = 0; j < array.Length; j++)
            {
                char[] chars = array[j].ToCharArray();
                if (j == 0 && isSmallHump) chars[0] = char.ToLower(chars[0]);
                else chars[0] = char.ToUpper(chars[0]);
                for (int i = 1; i < chars.Length; i++)
                {
                    chars[i] = char.ToLower(chars[i]);
                }
                ret += new string(chars);
            }
            return ret;

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
        #endregion

        #region Screenshot
        /// <summary>
        /// Tesseract 库获取文本
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static string TesseractGetText(Bitmap bmp)
        {
            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                //using (var engine = new TesseractEngine(@"./tessdata", "chi_sim", EngineMode.Default))
                {
                    using(var pix = PixConverter.ToPix(bmp))
                    {
                        using (var page = engine.Process(pix))
                        {
                            return page.GetText();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ImageBrush BitmapToImageBrush(Bitmap bmp)
        {
            ImageBrush brush = new ImageBrush();
            IntPtr hBitmap = bmp.GetHbitmap();
            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            brush.ImageSource = wpfBitmap;
            FlushMemory();
            return brush;
        }
        /// <summary>
        /// 清理内存
        /// </summary>
        public static void FlushMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Helper.NativeMethodHelper.SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        #endregion
    }
}
