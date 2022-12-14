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
    }
}
