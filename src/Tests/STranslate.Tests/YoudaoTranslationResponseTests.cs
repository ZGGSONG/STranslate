using STranslate.Plugin.Translate.Youdao;

namespace STranslate.Tests;

/// <summary>
/// 验证有道业务错误优先级及异常响应的诊断信息。
/// </summary>
public class YoudaoTranslationResponseTests
{
    /// <summary>
    /// 成功响应应保留原有的第一条译文返回行为。
    /// </summary>
    [Fact]
    public void Parse_Success_ReturnsFirstTranslation()
    {
        Assert.Equal("Test", TranslationResponse.Parse(
            """{"errorCode":"0","translation":["Test","Testing"]}""", key => key));
    }

    /// <summary>
    /// 已知错误必须优先于译文处理，同时保留服务端原始信息。
    /// </summary>
    /// <param name="code">服务端错误码。</param>
    /// <param name="resource">预期使用的错误文案资源。</param>
    [Theory]
    [InlineData("206", "InvalidTimestamp")]
    [InlineData("202", "InvalidSignature")]
    [InlineData("401", "InsufficientBalance")]
    [InlineData("411", "RateLimited")]
    [InlineData("17005", "ServiceCallFailed")]
    [InlineData("99999", "Unknown")]
    public void Parse_Error_PreservesCodeAndRawResponse(string code, string resource)
    {
        var response = $$"""{"errorCode":"{{code}}","requestId":"test-id","translation":["ignored"]}""";
        var error = Assert.Throws<Exception>(() => TranslationResponse.Parse(response, key =>
        {
            Assert.Equal("STranslate_Plugin_Translate_Youdao_Error_" + resource, key);
            return "本地化错误";
        }));

        Assert.Equal($"[{code}] 本地化错误\nRaw: {response}", error.Message);
    }

    /// <summary>
    /// 兼容接口将错误码序列化为数字的响应。
    /// </summary>
    [Fact]
    public void Parse_NumericErrorCode_RecognizesTimestampError()
    {
        var error = Assert.Throws<Exception>(() => TranslationResponse.Parse(
            """{"errorCode":206}""", key => key));
        Assert.Contains("[206]", error.Message);
        Assert.Contains("InvalidTimestamp", error.Message);
    }

    /// <summary>
    /// 空译文和缺失字段应给出可诊断的兜底错误。
    /// </summary>
    /// <param name="response">不含有效译文的响应。</param>
    [Theory]
    [InlineData("""{"errorCode":"0","translation":[]}""")]
    [InlineData("""{"errorCode":"0","translation":[""]}""")]
    [InlineData("""{"errorCode":"0"}""")]
    [InlineData("{}")]
    [InlineData("null")]
    public void Parse_NoTranslation_PreservesRawResponse(string response)
    {
        var error = Assert.Throws<Exception>(() => TranslationResponse.Parse(response, key => key));
        Assert.Contains("Error_NoResult", error.Message);
        Assert.Contains(response, error.Message);
    }
}
