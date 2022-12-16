using Newtonsoft.Json;
using STranslate.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.Utils
{
    public static class HttpUtil
    {
        public static DeeplResp Post(string url, DeeplReq req)
        {
            //json参数
            string jsonParam = JsonConvert.SerializeObject(req);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json;charset=UTF-8";
            byte[] byteData = Encoding.UTF8.GetBytes(jsonParam);
            int length = byteData.Length;
            request.ContentLength = length;
            Stream writer = request.GetRequestStream();
            writer.Write(byteData, 0, length);
            writer.Close();
            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")).ReadToEnd();
            var resp = JsonConvert.DeserializeObject<DeeplResp>(responseString);
            return resp;
        }

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
    }
}
