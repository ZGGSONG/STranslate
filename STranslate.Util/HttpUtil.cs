using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
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
        /// <param name="req"></param>
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
            using var client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(timeout),
            };

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
        public static async Task<string> GetAsync(string url, Dictionary<string, string> queryParams, CancellationToken token, int timeout = 10)
        {
            using var client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(timeout),
            };

            try
            {
                // 构建带查询参数的URL
                if (queryParams != null && queryParams.Count > 0)
                {
                    var queryBuilder = new StringBuilder();
                    foreach (var kvp in queryParams)
                    {
                        queryBuilder.Append(Uri.EscapeDataString(kvp.Key));
                        queryBuilder.Append("=");
                        queryBuilder.Append(Uri.EscapeDataString(kvp.Value));
                        queryBuilder.Append("&");
                    }

                    string queryString = queryBuilder.ToString().TrimEnd('&');
                    url += "?" + queryString;
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
        public static async Task<string> PostAsync(string url, string req, int timeout = 10) => await PostAsync(url, req, CancellationToken.None, timeout);

        /// <summary>
        /// 异步Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="req"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string req, CancellationToken token, int timeout = 10)
        {
            using var client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(timeout),
            };

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

        /// <summary>
        /// 支持系统代理
        /// </summary>
        public static void SupportSystemAgent()
        {
            WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
        }
        //TODO: 如果一开始没有开系统代理，软件开启后再打开系统代理仍然会代理不上
        //LogService.Logger.Info("START");
        //var ret = await HttpUtil.GetAsync("https://rsshub.zggsong.workers.dev/", timeout: 10);
        //LogService.Logger.Info(ret);
        //LogService.Logger.Info("END");
        //return;
    }
}
