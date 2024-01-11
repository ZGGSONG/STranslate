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

            try
            {
                var respContent = await client.GetAsync(url, token);

                string respStr = await respContent.Content.ReadAsStringAsync(token);

                return respStr;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步Get请求，带查询参数
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="queryParams">查询参数字典</param>
        /// <param name="token">取消令牌</param>
        /// <param name="timeout">超时时间（秒）</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(
            string url,
            Dictionary<string, string> queryParams,
            CancellationToken token,
            int timeout = 10
        )
        {
            using var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(timeout), };

            try
            {
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
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 异步Post请求(不带Token)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string req, int timeout = 10) =>
            await PostAsync(url, req, CancellationToken.None, timeout);

        /// <summary>
        /// 异步Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="req"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string req, CancellationToken token, int timeout = 10)
        {
            using var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(timeout), };

            try
            {
                var content = new StringContent(req, Encoding.UTF8, "application/json");

                var respContent = await client.PostAsync(url, content, token);

                string respStr = await respContent.Content.ReadAsStringAsync(token);

                return respStr;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
