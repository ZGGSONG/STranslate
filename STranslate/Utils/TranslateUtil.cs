using Newtonsoft.Json;
using STranslate.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace STranslate.Utils
{
    public class TranslateUtil
    {
        private static readonly string _url = "http://172.17.209.47:8080/translate";
        public static string Translate(string input, LanguageEnum source, LanguageEnum target)
        {
            var req = new DeeplReq()
            {
                Text = input,
                SourceLang = source.ToString(),
                TargetLang = target.ToString(),
            };
            var resp = HttpUtil.Post(_url, req);
            if (resp.Code == 200)
            {
                return resp.Data;
            }
            return string.Empty;
        }

        public static async Task<string> TranslateDeepLAsync(string text, LanguageEnum target, LanguageEnum source = LanguageEnum.auto)
        {
            var req = new DeeplReq()
            {
                Text = text,
                SourceLang = source.ToString(),
                TargetLang = target.ToString(),
            };
            var reqStr = JsonConvert.SerializeObject(req);
            var respStr = await HttpUtil.PostAsync(_url, reqStr);
            var resp = JsonConvert.DeserializeObject<DeeplResp>(respStr);

            if (resp.Code == 200)
            {
                return resp.Data;
            }
            return string.Empty;
        }

        public static async Task<string> TranslateBaiduAsync(string appID, string secretKey, string text, LanguageEnum target, LanguageEnum source = LanguageEnum.auto)
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
            
            var retString = await HttpUtil.GetAsync(url);
            var resp = JsonConvert.DeserializeObject<BaiduResp>(retString);
            if (resp.From != null)
            {
                return resp.TransResult[0]?.Dst;
            }
            return string.Empty;
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
    }
}
