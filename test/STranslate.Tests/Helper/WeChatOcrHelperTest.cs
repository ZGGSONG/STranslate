using STranslate.Helper;
using Xunit;
using Xunit.Abstractions;

namespace STranslate.Tests.Helper;

public class WeChatOcrHelperTest(ITestOutputHelper outputHelper)
{
    [Fact]
    public void TestExecute()
    {
        // Arrange
        var path = @"D:\Apps\WeChat\[3.9.12.11]";
        var ocrPath = WeChatOcrHelper.FindWeChatOcrExe();
        // Act
        var result = WeChatOcrHelper.GetOcrResult(ocrPath, path,
            @"C:\Users\20230508\AppData\Local\Temp\NotifyIconGeneratedAumid_16390793662218304994.png");
        var isSuccess = result.Item1;
        var ocrResult = result.Item2;
        outputHelper.WriteLine("OCR Result: " + ocrResult);
        // Assert
        Assert.True(isSuccess);
        Assert.NotNull(ocrResult);
    }
}