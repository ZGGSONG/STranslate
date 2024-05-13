using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.Helper;

public class LangDetectHelper
{
    /// <summary>
    /// 识别语种
    /// </summary>
    /// <param name="content"></param>
    /// <param name="type"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<LangEnum> DetectAsync(string content, LangDetectType type = LangDetectType.Local, CancellationToken? token = null) =>
        type switch
        {
            LangDetectType.Local => LocalLangDetect(content),
            LangDetectType.Baidu => await BaiduLangDetectAsync(content, token),
            LangDetectType.Tencent => await TencentLangDetectAsync(content, token),
            _ => LangEnum.auto
        };

    /// <summary>
    /// 本地识别
    /// 仅能识别中英文
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private static LangEnum LocalLangDetect(string content)
    {
        var ret = StringUtil.AutomaticLanguageRecognition(content, Singleton<ConfigHelper>.Instance?.CurrentConfig?.AutoScale ?? 0.8);
        return ret.Item1;
    }

    /// <summary>
    /// 百度识别
    /// </summary>
    /// <param name="content"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<LangEnum> BaiduLangDetectAsync(string content, CancellationToken? token = null)
    {
        LangEnum lang = LangEnum.auto;
        try
        {
            var url = "https://fanyi.baidu.com/langdetect";
            var resp = await HttpUtil.PostAsync(url, req: null, queryParams: new Dictionary<string, string> { { "query", content } }, headers: [], token ?? CancellationToken.None);

            var parseData = JsonConvert.DeserializeObject<JObject>(resp);
            var lan = parseData?["lan"]?.ToString() ?? "";

            lang = lan switch
            {
                "zh" => LangEnum.zh_cn,
                "cht" => LangEnum.zh_tw,
                "en" => LangEnum.en,
                "jp" => LangEnum.ja,
                "kor" => LangEnum.ko,
                "fra" => LangEnum.fr,
                "spa" => LangEnum.es,
                "ru" => LangEnum.ru,
                "de" => LangEnum.de,
                "it" => LangEnum.it,
                "tr" => LangEnum.tr,
                "pt" => LangEnum.pt_pt,
                "vie" => LangEnum.vi,
                "id" => LangEnum.id,
                "th" => LangEnum.th,
                "may" => LangEnum.ms,
                "ar" => LangEnum.ar,
                "hi" => LangEnum.hi,
                "nob" => LangEnum.nb_no,
                "nno" => LangEnum.nn_no,
                "per" => LangEnum.fa,
                "ukr" => LangEnum.uk,

                _ => LangEnum.auto
            };
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("百度语种识别出错, " + ex.Message);
        }

        return lang;
    }

    /// <summary>
    /// 腾讯识别
    /// </summary>
    /// <param name="content"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<LangEnum> TencentLangDetectAsync(string content, CancellationToken? token = null)
    {
        LangEnum lang = LangEnum.auto;
        try
        {
            var url = "https://fanyi.qq.com/api/translate";
            var resp = await HttpUtil.PostAsync(url, new Tuple<string, string>("sourceText", content), token ?? CancellationToken.None);

            var parseData = JsonConvert.DeserializeObject<JObject>(resp);
            var lan = parseData?["translate"]?["source"]?.ToString() ?? "";

            lang = lan switch
            {
                "zh" => LangEnum.zh_cn,
                "en" => LangEnum.en,
                "ja" => LangEnum.ja,
                "ko" => LangEnum.ko,
                "fr" => LangEnum.fr,
                "es" => LangEnum.es,
                "ru" => LangEnum.ru,
                "de" => LangEnum.de,
                "it" => LangEnum.it,
                "tr" => LangEnum.tr,
                "pt" => LangEnum.pt_pt,
                "vi" => LangEnum.vi,
                "id" => LangEnum.id,
                "th" => LangEnum.th,
                "ms" => LangEnum.ms,
                "ar" => LangEnum.ar,
                "hi" => LangEnum.hi,

                _ => LangEnum.auto
            };
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("腾讯语种识别出错, " + ex.Message);
        }

        return lang;
    }
}
