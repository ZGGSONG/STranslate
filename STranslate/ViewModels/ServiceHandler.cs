using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Linq;
using System.Text.RegularExpressions;
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
            var request = new[] { source, target, content };

            await service.TranslateAsync(request, msg =>
            {
                if (string.IsNullOrEmpty(msg?.Trim()))
                    return;

                var preprocessString = msg.Replace("data:", "").Trim();

                // 结束标记
                if (preprocessString.Equals("[DONE]"))
                    return;

                // 解析JSON数据
                var parsedData = JsonConvert.DeserializeObject<JObject>(preprocessString);

                if (parsedData is null)
                    return;

                // 提取content的值
                var contentValue = parsedData["choices"]?.FirstOrDefault()?["delta"]?["content"]?.ToString();

                if (string.IsNullOrEmpty(contentValue))
                    return;

                lock (service)
                {
                    service.Data += contentValue;
                }
            }, token);

            if (string.IsNullOrEmpty(service.Data?.ToString()))
                service.Data = "未获取到内容";
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
            var request = new[] { source, target, content };

            await service.TranslateAsync(request, msg =>
            {
                // 使用正则表达式提取目标字符串
                string pattern = "(?<=\"text\": \")[^\"]+(?=\")";

                var match = Regex.Match(msg, pattern);
                if (match.Success)
                {
                    lock (service)
                    {
                        service.Data += match.Value.Replace("\\n", "\n");
                    }
                }
            }, token);

            if (string.IsNullOrEmpty(service.Data?.ToString()))
                service.Data = "未获取到内容";
        }
    }
}