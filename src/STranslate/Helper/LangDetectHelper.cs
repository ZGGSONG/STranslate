using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.Helper;

/// <summary>
///     https://github1s.com/pot-app/pot-desktop/blob/master/src/utils/lang_detect.js
/// </summary>
public class LangDetectHelper
{
    /// <summary>
    ///     识别语种
    /// </summary>
    /// <param name="content"></param>
    /// <param name="type"></param>
    /// <param name="rate"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<LangEnum> DetectAsync(string content, LangDetectType type = LangDetectType.Local,
        double rate = 0.8, CancellationToken? token = null)
    {
        return type switch
        {
            LangDetectType.Local => LocalLangDetect(content, rate),
            LangDetectType.Baidu => await BaiduLangDetectAsync(content, token),
            LangDetectType.Tencent => await TencentLangDetectAsync(content, token),
            LangDetectType.Niutrans => await NiutransLangDetectAsync(content, token),
            LangDetectType.Bing => await BingLangDetectAsync(content, token),
            LangDetectType.Yandex => await YandexLangDetectAsync(content, token),
            LangDetectType.Google => await GoogleLangDetectAsync(content, token),
            _ => LangEnum.auto
        };
    }

    /// <summary>
    ///     本地识别
    ///     仅能识别中英文
    /// </summary>
    /// <param name="content"></param>
    /// <param name="rate"></param>
    /// <returns></returns>
    private static LangEnum LocalLangDetect(string content, double rate)
    {
        var ret = StringUtil.AutomaticLanguageRecognition(content, rate);
        return ret.Item1;
    }

    /// <summary>
    ///     百度识别
    /// </summary>
    /// <param name="content"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<LangEnum> BaiduLangDetectAsync(string content, CancellationToken? token = null)
    {
        var lang = LangEnum.auto;
        try
        {
            var url = "https://fanyi.baidu.com/langdetect";
            var resp = await HttpUtil.PostAsync(url, null, new Dictionary<string, string> { { "query", content } }, [],
                token ?? CancellationToken.None);

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
    ///     腾讯识别
    /// </summary>
    /// <param name="content"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<LangEnum> TencentLangDetectAsync(string content, CancellationToken? token = null)
    {
        var lang = LangEnum.auto;
        try
        {
            var url = "https://fanyi.qq.com/api/translate";
            var resp = await HttpUtil.PostAsync(url, new Tuple<string, string>("sourceText", content),
                token ?? CancellationToken.None);

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

    /// <summary>
    ///     小牛识别
    /// </summary>
    /// <param name="content"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<LangEnum> NiutransLangDetectAsync(string content, CancellationToken? token = null)
    {
        var lang = LangEnum.auto;
        try
        {
            var url = "https://test.niutrans.com/NiuTransServer/language";
            var queryparams = new Dictionary<string, string>
            {
                { "src_text", content },
                { "source", "text" },
                { "time", ((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds).ToString() }
            };
            var resp = await HttpUtil.GetAsync(url, queryparams, token ?? CancellationToken.None);

            var parseData = JsonConvert.DeserializeObject<JObject>(resp);
            var lan = parseData?["language"]?.ToString() ?? "";

            lang = lan switch
            {
                "zh" => LangEnum.zh_cn,
                "cht" => LangEnum.zh_cn,
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
                "mn" => LangEnum.mn_cy,
                "mo" => LangEnum.mn_mo,
                "km" => LangEnum.km,
                "nb" => LangEnum.nb_no,
                "nn" => LangEnum.nn_no,
                "fa" => LangEnum.fa,
                "uk" => LangEnum.uk,

                _ => LangEnum.auto
            };
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("小牛语种识别出错, " + ex.Message);
        }

        return lang;
    }

    /// <summary>
    ///     必应识别
    /// </summary>
    /// <param name="content"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<LangEnum> BingLangDetectAsync(string content, CancellationToken? token = null)
    {
        var lang = LangEnum.auto;
        try
        {
            var tokenUrl = "https://edge.microsoft.com/translate/auth";
            var getToken = await HttpUtil.GetAsync(tokenUrl, token ?? CancellationToken.None);

            var url = "https://api-edge.cognitive.microsofttranslator.com/detect";
            var headers = new Dictionary<string, string>
            {
                { "accept", "*/*" },
                { "accept-language", "zh-TW,zh;q=0.9,ja;q=0.8,zh-CN;q=0.7,en-US;q=0.6,en;q=0.5" },
                { "authorization", "Bearer " + getToken },
                { "cache-control", "no-cache" },
                { "pragma", "no-cache" },
                { "sec-ch-ua", "\"Microsoft Edge\";v=\"113\", \"Chromium\";v=\"113\", \"Not-A.Brand\";v=\"24\"" },
                { "sec-ch-ua-mobile", "?0" },
                { "sec-ch-ua-platform", "\"Windows\"" },
                { "sec-fetch-dest", "empty" },
                { "sec-fetch-mode", "cors" },
                { "sec-fetch-site", "cross-site" },
                { "Referer", "https://appsumo.com/" },
                { "Referrer-Policy", "strict-origin-when-cross-origin" },
                {
                    "User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36 Edg/113.0.1774.42"
                }
            };

            var queryParams = new Dictionary<string, string> { { "api-version", "3.0" } };

            var data = new[] { new { Text = content } };
            var req = JsonConvert.SerializeObject(data);

            var resp = await HttpUtil.PostAsync(url, req, queryParams, headers, token ?? CancellationToken.None);

            var langInfos = JsonConvert.DeserializeObject<List<LanguageInfo>>(resp);
            var lan = langInfos?.FirstOrDefault()?.Language ?? "";

            lang = lan switch
            {
                "zh-Hans" => LangEnum.zh_cn,
                "zh-Hant" => LangEnum.zh_tw,
                "en" => LangEnum.en,
                "ja" => LangEnum.ja,
                "ko" => LangEnum.ko,
                "fr" => LangEnum.fr,
                "es" => LangEnum.es,
                "ru" => LangEnum.ru,
                "de" => LangEnum.de,
                "it" => LangEnum.it,
                "tr" => LangEnum.tr,
                "pt-pt" => LangEnum.pt_pt,
                "pt" => LangEnum.pt_br,
                "vi" => LangEnum.vi,
                "id" => LangEnum.id,
                "th" => LangEnum.th,
                "ms" => LangEnum.ms,
                "ar" => LangEnum.ar,
                "hi" => LangEnum.hi,
                "mn-Cyrl" => LangEnum.mn_cy,
                "mn-Mong" => LangEnum.mn_mo,
                "km" => LangEnum.km,
                "nb" => LangEnum.nb_no,
                "fa" => LangEnum.fa,
                "uk" => LangEnum.uk,

                _ => LangEnum.auto
            };
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("必应语种识别出错, " + ex.Message);
        }

        return lang;
    }

    /// <summary>
    ///     Yandex识别
    /// </summary>
    /// <param name="content"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<LangEnum> YandexLangDetectAsync(string content, CancellationToken? token = null)
    {
        var lang = LangEnum.auto;
        try
        {
            var url = "https://translate.yandex.net/api/v1/tr.json/detect";
            var queryparams = new Dictionary<string, string>
            {
                { "id", Guid.NewGuid().ToString().Replace("-", "") + "-0-0" },
                { "srv", "android" },
                { "text", content }
            };
            var resp = await HttpUtil.GetAsync(url, queryparams, token ?? CancellationToken.None);

            var parseData = JsonConvert.DeserializeObject<JObject>(resp);
            var lan = parseData?["lang"]?.ToString() ?? "";

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
                "no" => LangEnum.nb_no,
                "fa" => LangEnum.fa,
                "uk" => LangEnum.uk,

                _ => LangEnum.auto
            };
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("Yandex语种识别出错, " + ex.Message);
        }

        return lang;
    }

    /// <summary>
    ///     谷歌识别
    /// </summary>
    /// <param name="content"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<LangEnum> GoogleLangDetectAsync(string content, CancellationToken? token = null)
    {
        var lang = LangEnum.auto;
        try
        {
            var url =
                "https://translate.google.com/translate_a/single?dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t";

            var queryParams = new Dictionary<string, string>
            {
                { "client", "gtx" },
                { "sl", "auto" },
                { "tl", "zh-CN" },
                { "hl", "zh-CN" },
                { "ie", "UTF-8" },
                { "oe", "UTF-8" },
                { "otf", "1" },
                { "ssel", "0" },
                { "tsel", "0" },
                { "kc", "7" },
                { "q", content }
            };

            var resp = await HttpUtil.GetAsync(url, queryParams, token ?? CancellationToken.None);
            var jsonArray = JArray.Parse(resp);
            var lan = jsonArray[2]?.ToString() ?? "";

            lang = lan switch
            {
                "zh-CN" => LangEnum.zh_cn,
                "zh-TW" => LangEnum.zh_tw,
                "ja" => LangEnum.ja,
                "en" => LangEnum.en,
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
                "mn" => LangEnum.mn_cy,
                "km" => LangEnum.km,
                "fa" => LangEnum.fa,
                "no" => LangEnum.nb_no,
                "uk" => LangEnum.uk,

                _ => LangEnum.auto
            };
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("谷歌语种识别出错, " + ex.Message);
        }

        return lang;
    }
}