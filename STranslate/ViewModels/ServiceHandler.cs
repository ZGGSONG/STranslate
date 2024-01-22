using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.Services;

namespace STranslate.ViewModels
{
    /// <summary>
    /// 不同服务处理类
    /// TODO: 新接口需要适配
    /// </summary>
    public class ServiceHandler
    {
        /// <summary>
        /// API
        /// </summary>
        /// <param name="service"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task ApiHandlerAsync(ITranslator service, string content, string source, string target, CancellationToken token)
        {
            var response =
                (Task<object>)
                    await service.TranslateAsync(
                        new RequestApi()
                        {
                            Text = content,
                            SourceLang = source,
                            TargetLang = target
                        },
                        token
                    );
            service.Data = (response.Result as ResponseApi)!.Data?.ToString() ?? "";
        }

        /// <summary>
        /// Baidu
        /// </summary>
        /// <param name="service"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task BaiduHandlerAsync(ITranslator service, string content, string source, string target, CancellationToken token)
        {
            string salt = new Random().Next(100000).ToString();
            string sign = StringUtil.EncryptString(service.AppID + content + salt + service.AppKey);
            var response =
                (Task<object>)
                    await service.TranslateAsync(
                        new RequestBaidu()
                        {
                            Text = content,
                            From = source,
                            TO = target,
                            AppId = service.AppID,
                            Salt = salt,
                            Sign = sign
                        },
                        token
                    );
            var transResults = (response.Result as ResponseBaidu)?.TransResult ?? [];
            service.Data =
                transResults.Length == 0
                    ? string.Empty
                    : string.Join(Environment.NewLine, transResults.Where(trans => !string.IsNullOrEmpty(trans.Dst)).Select(trans => trans.Dst));
        }

        /// <summary>
        /// Bing
        /// </summary>
        /// <param name="service"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task BingHandlerAsync(ITranslator service, string content, string source, string target, CancellationToken token)
        {
            var req = new RequestBing
            {
                From = source,
                To = target,
                Req = [new TextData { Text = content }],
            };
            var response = (Task<object>)await service.TranslateAsync(req, token);
            service.Data = (response.Result as ResponseBing[])!.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text ?? "";
        }

        /// <summary>
        /// OpenAI
        /// </summary>
        /// <param name="service"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task OpenAIHandlerAsync(ITranslator service, string content, string source, string target, CancellationToken token)
        {
            try
            {
                if (string.IsNullOrEmpty(service.Url) || string.IsNullOrEmpty(service.AppKey))
                    throw new Exception("请先完善配置");

                UriBuilder uriBuilder = new(service.Url);

                // 兼容旧版API: https://platform.openai.com/docs/guides/text-generation
                if (!uriBuilder.Path.EndsWith("/v1/chat/completions") && !uriBuilder.Path.EndsWith("/v1/completions"))
                {
                    uriBuilder.Path = "/v1/chat/completions";
                }

                // 选择模型
                var a_model = (service as TranslatorOpenAI)?.Model;
                a_model = string.IsNullOrEmpty(a_model) ? "gpt-3.5-turbo" : a_model;

                // 组织语言
                var a_content = source.Equals("auto", StringComparison.CurrentCultureIgnoreCase)
                    ? $"Translate the following text to {target}: {content}"
                    : $"Translate the following text from {source} to {target}: {content}";

                // 构建请求数据
                var reqData = new
                {
                    model = a_model,
                    messages = new[] { new { role = "user", content = a_content } },
                    temperature = 1.0,
                    stream = true
                };

                // 为了流式输出与MVVM还是放这里吧
                var jsonData = JsonConvert.SerializeObject(reqData);

                // 构建请求
                var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(10) };
                var req = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uriBuilder.Uri,
                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                };
                req.Headers.Add("Authorization", $"Bearer {service.AppKey}");

                // 发送请求
                using var response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token);
                // 获取响应流
                using var responseStream = await response.Content.ReadAsStreamAsync(token);
                using var reader = new System.IO.StreamReader(responseStream);
                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.ReasonPhrase);
                // 逐行读取并输出结果
                while (!reader.EndOfStream || token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(token);

                    if (string.IsNullOrEmpty(line?.Trim()))
                        continue;

                    var preprocessString = line.Replace("data:", "").Trim();

                    // 结束标记
                    if (preprocessString.Equals("[DONE]"))
                        break;

                    // 解析JSON数据
                    var parsedData = JsonConvert.DeserializeObject<JObject>(preprocessString);

                    if (parsedData is null)
                        continue;

                    // 提取content的值
                    var contentValue = parsedData["choices"]?.FirstOrDefault()?["delta"]?["content"]?.ToString();

                    if (string.IsNullOrEmpty(contentValue))
                        continue;

                    // 输出
                    lock (service)
                    {
                        service.Data += contentValue;
                    }
                }

                if (string.IsNullOrEmpty(service.Data?.ToString()))
                    service.Data = "未获取到内容";
            }
            catch (Exception ex)
            {
                service.Data = ex.Message;
            }
        }

        /// <summary>
        /// Gemini
        /// </summary>
        /// <param name="service"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task GeminiHandlerAsync(ITranslator service, string content, string source, string target, CancellationToken token)
        {
            try
            {
                if (string.IsNullOrEmpty(service.Url) || string.IsNullOrEmpty(service.AppKey))
                    throw new Exception("请先完善配置");

                UriBuilder uriBuilder = new(service.Url);

                if (!uriBuilder.Path.EndsWith("/v1beta/models/gemini-pro:streamGenerateContent"))
                {
                    uriBuilder.Path = "/v1beta/models/gemini-pro:streamGenerateContent";
                }

                uriBuilder.Query = $"key={service.AppKey}";

                // 组织语言
                var a_content = source.Equals("auto", StringComparison.CurrentCultureIgnoreCase)
                    ? $"Translate the following text to {target}: {content}"
                    : $"Translate the following text from {source} to {target}: {content}";

                // 构建请求数据
                var reqData = new { contents = new[] { new { parts = new[] { new { text = a_content } } } } };

                // 为了流式输出与MVVM还是放这里吧
                var jsonData = JsonConvert.SerializeObject(reqData);

                // 构建请求
                var client = new HttpClient(new SocketsHttpHandler()) { Timeout = TimeSpan.FromSeconds(10) };
                var req = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = uriBuilder.Uri,
                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                };

                // 发送请求
                using var response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token);
                // 获取响应流
                using var responseStream = await response.Content.ReadAsStreamAsync(token);
                using var reader = new System.IO.StreamReader(responseStream);
                if (!response.IsSuccessStatusCode)
                    throw new Exception(response.ReasonPhrase);
                // 逐行读取并输出结果
                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync(token) ?? "";
                    line = line.Trim();
                    if (line.StartsWith("\"text\":"))
                    {
                        service.Data += line.Replace("\"text\": ", "").Replace("\"", "");
                    }
                }

                if (string.IsNullOrEmpty(service.Data?.ToString()))
                    service.Data = "未获取到内容";
            }
            catch (Exception ex)
            {
                service.Data = ex.Message;
            }
        }
    }
}
