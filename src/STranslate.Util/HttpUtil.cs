using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.Util
{
    public class HttpUtil
    {
        /// <summary>
        /// 异步Get请求(不带Token)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, int timeout = 10) => await GetAsync(url, CancellationToken.None, timeout);

        /// <summary>
        /// 异步Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, CancellationToken token, int timeout = 10)
        {
            using var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(timeout) };

            var respContent = await client.GetAsync(url, token);

            string respStr = await respContent.Content.ReadAsStringAsync(token);

            return respStr;
        }

        /// <summary>
        /// 异步Get请求，带查询参数
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="queryParams">查询参数字典</param>
        /// <param name="token">取消令牌</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, Dictionary<string, string> queryParams, CancellationToken token, int timeout = 10)
        {
            using var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(timeout) };
            // 构建带查询参数的URL
            if (queryParams != null && queryParams.Count > 0)
            {
                var uriBuilder = new UriBuilder(url);
                var query = queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
                uriBuilder.Query = string.Join("&", query);
                url = uriBuilder.ToString();
            }

            var respContent = await client.GetAsync(url, token);

            string respStr = await respContent.Content.ReadAsStringAsync(token);

            return respStr;
        }

        /// <summary>
        /// 异步Post请求(不带Token)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string req, int timeout = 10) => await PostAsync(url, req, CancellationToken.None, timeout);

        /// <summary>
        /// 异步Post请求(Body)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="req"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string req, CancellationToken token, int timeout = 10)
        {
            using var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(timeout) };

            var content = new StringContent(req, Encoding.UTF8, "application/json");

            var respContent = await client.PostAsync(url, content, token);

            string respStr = await respContent.Content.ReadAsStringAsync(token);

            return respStr;
        }

        /// <summary>
        /// 异步Post请求(QueryParams、Header、Body)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="req"></param>
        /// <param name="queryParams"></param>
        /// <param name="headers"></param>
        /// <param name="token"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string? req, Dictionary<string, string>? queryParams, Dictionary<string, string> headers, CancellationToken token, int timeout = 10)
        {
            using var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(timeout) };
            // 构建带查询参数的URL
            if (queryParams != null && queryParams.Count > 0)
            {
                var uriBuilder = new UriBuilder(url);
                var query = queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
                uriBuilder.Query = string.Join("&", query);
                url = uriBuilder.ToString();
            }
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
            
            if (!string.IsNullOrEmpty(req))
            {
                request.Content = new StringContent(req, Encoding.UTF8, "application/json");
            }

            // 添加请求头
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            // Send the request and get response.
            HttpResponseMessage response = await client.SendAsync(request, token);
            
            // Read response as a string.
            string result = await response.Content.ReadAsStringAsync(token);

            return result;
        }

        /// <summary>
        /// Tencent OCR
        /// </summary>
        /// <param name="url"></param>
        /// <param name="req"></param>
        /// <param name="headers"></param>
        /// <param name="token"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string? req, Dictionary<string, string> headers, CancellationToken token, int timeout = 10)
        {
            using var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(timeout) };
            
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));

            if (!string.IsNullOrEmpty(req))
            {
                request.Content = new StringContent(req, Encoding.UTF8, "application/json");
            }

            // 添加请求头
            foreach (var header in headers)
            {
                if(header.Key == "Authorization")
                    request.Headers.TryAddWithoutValidation("Authorization", header.Value);
                else
                    request.Headers.Add(header.Key, header.Value);
            }

            // Send the request and get response.
            HttpResponseMessage response = await client.SendAsync(request, token);

            // Read response as a string.
            string result = await response.Content.ReadAsStringAsync(token);

            return result;
        }

        /// <summary>
        /// 异步Post请求(Authorization) 回调返回流数据结果
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="req"></param>
        /// <param name="key"></param>
        /// <param name="OnDataReceived"></param>
        /// <param name="token"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task PostAsync(Uri uri, string req, string? key, Action<string> OnDataReceived, CancellationToken token, int timeout = 10)
        {
            using var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(timeout) };

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = uri,
                Content = new StringContent(req, Encoding.UTF8, "application/json")
            };

            // key不为空时添加
            if (!string.IsNullOrEmpty(key))
            {
                request.Headers.Add("Authorization", $"Bearer {key}");
            }

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.ReasonPhrase);
            }
            using var responseStream = await response.Content.ReadAsStreamAsync(token);
            using var reader = new System.IO.StreamReader(responseStream);
            // 逐行读取并输出结果
            while (!reader.EndOfStream)
            {
                var content = await reader.ReadLineAsync(token);

                if (!string.IsNullOrEmpty(content))
                    OnDataReceived?.Invoke(content);
            }
        }

        /// <summary>
        /// 修改自有道官方Demo
        /// </summary>
        /// <param name="url"></param>
        /// <param name="header"></param>
        /// <param name="param"></param>
        /// <param name="token"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, Dictionary<string, string[]>? headers, Dictionary<string, string[]> parameters, CancellationToken token, int timeout = 10)
        {
            using var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(timeout) };

            if (parameters != null)
            {
                var content = new StringBuilder();
                foreach (var parameter in parameters.SelectMany(p => p.Value.Select(v => new KeyValuePair<string, string>(p.Key, v))))
                {
                    if (content.Length > 0)
                    {
                        content.Append('&');
                    }
                    content.Append($"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}");
                }

                var stringContent = new StringContent(content.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await client.PostAsync(url, stringContent, token);
                return await response.Content.ReadAsStringAsync(token);
            }

            var emptyContent = new StringContent(string.Empty);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    foreach (var value in header.Value)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, value);
                    }
                }
            }

            var result = await client.PostAsync(url, emptyContent, token);
            return await result.Content.ReadAsStringAsync(token);
        }
    }

    #region 有道签名

    public static class AuthV3Util
    {
        /*
            添加鉴权相关参数 -
            appKey : 应用ID
            salt : 随机值
            curtime : 当前时间戳(秒)
            signType : 签名版本
            sign : 请求签名

            @param appKey    您的应用ID
            @param appSecret 您的应用密钥
            @param paramsMap 请求参数表
        */

        public static void AddAuthParams(string appKey, string appSecret, Dictionary<string, string[]> paramsMap)
        {
            string q = "";
            string[] qArray;
            if (paramsMap.ContainsKey("q"))
            {
                qArray = paramsMap["q"];
            }
            else
            {
                qArray = paramsMap["img"];
            }
            foreach (var item in qArray)
            {
                q += item;
            }
            string salt = Guid.NewGuid().ToString();
            string curtime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + "";
            string sign = CalculateSign(appKey, appSecret, q, salt, curtime);
            paramsMap.Add("appKey", [appKey]);
            paramsMap.Add("salt", [salt]);
            paramsMap.Add("curtime", [curtime]);
            paramsMap.Add("signType", ["v3"]);
            paramsMap.Add("sign", [sign]);
        }

        /*
            计算鉴权签名 -
            计算方式 : sign = sha256(appKey + input(q) + salt + curtime + appSecret)

            @param appKey    您的应用ID
            @param appSecret 您的应用密钥
            @param q         请求内容
            @param salt      随机值
            @param curtime   当前时间戳(秒)
            @return 鉴权签名sign
        */

        public static string CalculateSign(string appKey, string appSecret, string q, string salt, string curtime)
        {
            string strSrc = appKey + GetInput(q) + salt + curtime + appSecret;
            return Encrypt(strSrc);
        }

        private static string Encrypt(string strSrc)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(strSrc);
            byte[] hashedBytes = SHA256.HashData(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToUpperInvariant();
        }

        private static string GetInput(string q)
        {
            if (q == null)
            {
                return "";
            }
            int len = q.Length;
            return len <= 20 ? q : q[..10] + len + q.Substring(len - 10, 10);
        }
    }

    #endregion 有道签名
}