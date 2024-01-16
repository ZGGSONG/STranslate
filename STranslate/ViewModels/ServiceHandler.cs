using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

                if (!service.Url.EndsWith("completions"))
                {
                    service.Url = service.Url.TrimEnd('/') + "/completions";
                }
                // 构建请求数据
                var reqData = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[] {
                    new { role = "user", content = $"Translate the following text to {target}: {content}" }
                },
                    temperature = 0,
                    stream = true
                };

                // 为了流式输出与MVVM还是放这里吧
                var jsonData = JsonConvert.SerializeObject(reqData);

                // 构建请求
                var client = new HttpClient(new SocketsHttpHandler());
                var req = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(service.Url),
                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                };
                req.Headers.Add("Authorization", $"Bearer {service.AppKey}");

                // 发送请求
                using var response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, token);
                // 获取响应流
                using var responseStream = await response.Content.ReadAsStreamAsync(token);
                using var reader = new System.IO.StreamReader(responseStream);
                // 逐行读取并输出结果
                while (!reader.EndOfStream || token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(token);

                    if (string.IsNullOrEmpty(line?.Trim())) continue;

                    var preprocessString = line.Replace("data:", "").Trim();

                    // 结束标记
                    if (preprocessString.Equals("[DONE]")) break;

                    // 解析JSON数据
                    var parsedData = JsonConvert.DeserializeObject<JObject>(preprocessString);

                    if (parsedData is null) continue;

                    // 提取content的值
                    var contentValue = parsedData["choices"]?.FirstOrDefault()?["delta"]?["content"]?.ToString();

                    if (string.IsNullOrEmpty(contentValue)) continue;

                    // 输出
                    service.Data += contentValue;
                }
            }
            catch (Exception ex)
            {
                service.Data = ex.Message;
            }
        }
    }
}