using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace STranslate.ViewModels.Preference.OCR.Tests;

public class BaiduOCRTests
{
    [Fact]
    public async Task GetAccessTokenAsyncTest()
    {
        var baiduOcr = new BaiduOCR();
        var resp = await baiduOcr.GetAccessTokenAsync("xx", "xx", CancellationToken.None);
        Assert.NotNull(resp);
        System.Diagnostics.Debug.WriteLine(resp);

        var data = JsonConvert.DeserializeObject<JObject>(resp);
        var token = data?["access_token"] ?? "";
        Assert.NotNull(token);
        System.Diagnostics.Debug.WriteLine(token);
    }
}
