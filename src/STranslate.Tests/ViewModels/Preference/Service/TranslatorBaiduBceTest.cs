using STranslate.Model;
using STranslate.ViewModels.Preference.Translator;
using Xunit;

namespace STranslate.ViewModels.Preference.OCR.Service;

public class TranslatorBaiduBceTest
{
    [Fact()]
    public async Task ExecuteAsyncTest()
    {
        var baiduBce = new TranslatorBaiduBce { AppID = "xx", AppKey = "xx" };
        try
        {
            await baiduBce.TranslateAsync(
                new RequestModel("hello world", LangEnum.auto, LangEnum.zh_cn),
                msg =>
                {
                    System.Diagnostics.Debug.Write(msg);
                },
                CancellationToken.None
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }
}
