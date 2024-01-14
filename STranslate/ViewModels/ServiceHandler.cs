using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STranslate.Model;
using STranslate.Util;

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
        public static async Task<string> ApiHandlerAsync(ITranslator service, string content, string source, string target, CancellationToken token)
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
            var ret = (response.Result as ResponseApi)!.Data?.ToString() ?? "";

            return ret;
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
        public static async Task<string> BaiduHandlerAsync(ITranslator service, string content, string source, string target, CancellationToken token)
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
            var ret =
                transResults.Length == 0
                    ? string.Empty
                    : string.Join(Environment.NewLine, transResults.Where(trans => !string.IsNullOrEmpty(trans.Dst)).Select(trans => trans.Dst));
            return ret;
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
        public static async Task<string> BingHandlerAsync(ITranslator service, string content, string source, string target, CancellationToken token)
        {
            var req = new RequestBing
            {
                From = source,
                To = target,
                Req = [new TextData { Text = content }],
            };
            var response = (Task<object>)await service.TranslateAsync(req, token);
            var ret = (response.Result as ResponseBing[])!.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text;

            return ret ?? "";
        }
    }
}
