using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace STranslate.ViewModels.Preference.OCR.Tests;

[TestClass()]
public class BaiduOCRTests
{
    [TestMethod()]
    public async Task GetAccessTokenAsyncTest()
    {
        var baiduOCR = new BaiduOCR();
        var resp = await baiduOCR.GetAccessTokenAsync("xx", "xx", CancellationToken.None);
        Assert.IsNotNull(resp);
        System.Diagnostics.Debug.WriteLine(resp);

        var data = JsonConvert.DeserializeObject<JObject>(resp);
        var token = data?["access_token"] ?? "";
        Assert.IsNotNull(token);
        System.Diagnostics.Debug.WriteLine(token);
    }
}
