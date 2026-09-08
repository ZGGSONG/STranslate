namespace STranslate.Plugin.Translate.Youdao;

/// <summary>
/// 根据有道官方错误码表解析译文，并提供本地化错误原因。
/// </summary>
internal static class TranslationResponse
{
    private const string ResourcePrefix = "STranslate_Plugin_Translate_Youdao_Error_";

    /// <summary>
    /// 从接口响应中提取译文；失败时保留原始响应，便于排查未知错误。
    /// </summary>
    /// <param name="response">有道接口返回的 JSON。</param>
    /// <param name="getTranslation">按资源键获取当前语言文案。</param>
    /// <returns>接口返回的第一条译文。</returns>
    internal static string Parse(string response, Func<string, string> getTranslation)
    {
        var parsedData = System.Text.Json.Nodes.JsonNode.Parse(response);
        var errorCode = parsedData?["errorCode"]?.ToString();

        // 有道的业务错误也返回 HTTP 200，必须先检查错误码，避免被空译文掩盖。
        if (!string.IsNullOrEmpty(errorCode) && errorCode != "0")
        {
            var message = getTranslation(ResourcePrefix + GetErrorKey(errorCode));
            throw new Exception($"[{errorCode}] {message}\nRaw: {response}");
        }

        var data = (parsedData?["translation"] as System.Text.Json.Nodes.JsonArray)?.FirstOrDefault()?.ToString();
        if (string.IsNullOrEmpty(data))
            throw new Exception($"{getTranslation(ResourcePrefix + "NoResult")}\nRaw: {response}");

        return data;
    }

    // 错误码来源：https://ai.youdao.com/DOCSIRMA/html/trans/api/wbfy/index.html
    private static string GetErrorKey(string errorCode) => errorCode switch
    {
        "101" => "MissingParameter",
        "102" or "3005" or "4005" or "5003" or "11005" => "UnsupportedLanguage",
        "103" => "TextTooLong",
        "104" => "UnsupportedApi",
        "105" => "UnsupportedSignature",
        "106" => "UnsupportedResponse",
        "107" => "UnsupportedEncryption",
        "108" => "InvalidAppId",
        "109" => "InvalidBatchLog",
        "110" => "ServiceNotBound",
        "111" => "InvalidAccount",
        "112" => "InvalidService",
        "113" => "EmptyText",
        "114" => "UnsupportedImageTransfer",
        "116" => "InvalidStrict",
        "201" => "RequestDecryption",
        "202" => "InvalidSignature",
        "203" => "IpDenied",
        "205" => "PlatformMismatch",
        "206" => "InvalidTimestamp",
        "207" => "Replay",
        "301" => "DictionaryFailed",
        "302" => "TranslationQueryFailed",
        "303" or "2301" or "3303" or "4303" or "11303" => "ServiceError",
        "304" => "TranslationFailed",
        "308" => "InvalidFallback",
        "309" => "InvalidDomain",
        "310" => "DomainNotEnabled",
        "401" => "InsufficientBalance",
        "402" => "OfflineUnavailable",
        "411" or "1411" or "2411" or "3411" or "4411" or "5411" or "9411" or "10411" or "11411" => "RateLimited",
        "412" => "LongRequestRateLimited",
        "1001" or "5001" or "10001" => "InvalidOcr",
        "1002" or "5002" or "10002" => "UnsupportedOcrImage",
        "1003" => "UnsupportedOcrLanguage",
        "1004" or "5004" or "10004" => "ImageTooLarge",
        "1201" or "5201" or "10201" or "12002" => "ImageDecryption",
        "1301" or "5301" or "10301" => "OcrParagraphFailed",
        "1412" => "RecognitionBytesExceeded",
        "2003" or "9005" => "UnsupportedRecognitionLanguage",
        "2004" => "SynthesisTextTooLong",
        "2005" or "3009" => "UnsupportedAudioFile",
        "2006" or "3010" => "UnsupportedVoice",
        "2201" or "3201" or "4201" or "11201" => "DecryptionFailed",
        "2412" or "3412" => "RequestTextTooLong",
        "3001" or "9001" => "UnsupportedSpeechFormat",
        "3002" or "9002" => "UnsupportedSampleRate",
        "3003" or "9003" => "UnsupportedChannels",
        "3004" or "4004" or "9004" or "11004" => "UnsupportedAudioUpload",
        "3006" or "17004" => "UnsupportedRecognition",
        "3007" or "4006" or "11006" => "AudioTooLarge",
        "3008" or "4007" => "AudioTooLong",
        "3301" or "4301" or "9301" or "11301" => "SpeechRecognitionFailed",
        "3302" => "SpeechTranslationFailed",
        "4001" or "11001" => "UnsupportedRecognitionFormat",
        "4002" or "11002" => "UnsupportedRecognitionSampleRate",
        "4003" or "11003" => "UnsupportedRecognitionChannels",
        "4412" or "11412" => "RequestDurationExceeded",
        "5005" or "12005" => "UnsupportedImage",
        "5006" or "13004" => "EmptyFile",
        "5412" or "10412" => "RecognitionTrafficExceeded",
        "9303" => "InternalServerError",
        "9412" => "SpeechLengthExceeded",
        "11007" => "AudioOver30Seconds",
        "12001" => "ImageDimensionsExceeded",
        "12003" => "EngineError",
        "12004" => "EmptyImage",
        "12006" => "NoImageMatch",
        "13001" => "UnsupportedAngle",
        "13002" => "UnsupportedFile",
        "13003" => "TableImageTooLarge",
        "13301" => "TableRecognitionFailed",
        "15001" or "17001" => "ImageRequired",
        "15002" or "17002" => "ImageOver1Mb",
        "15003" or "17005" => "ServiceCallFailed",
        "17003" => "RecognitionNotFound",
        _ => "Unknown"
    };
}
