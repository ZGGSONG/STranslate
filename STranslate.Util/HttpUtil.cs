using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public static async Task<string> PostAsync(
            string url,
            string req,
            Dictionary<string, string> queryParams,
            Dictionary<string, string> headers,
            CancellationToken token,
            int timeout = 10
        )
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
            request.Content = new StringContent(req, Encoding.UTF8, "application/json");
            headers.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));

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
    }
}
