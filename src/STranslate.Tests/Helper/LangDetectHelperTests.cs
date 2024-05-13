using Xunit;
using Assert = Xunit.Assert;

namespace STranslate.Helper.Tests;

public class LangDetectHelperTests
{
    [Fact()]
    public async Task BaiduLangDetectTest()
    {
        var resp = await LangDetectHelper.BaiduLangDetectAsync("我將執行一些東西");
        System.Diagnostics.Debug.WriteLine(resp);
    }

    [Fact()]
    public async Task TencentLangDetectTest()
    {
        var resp = await LangDetectHelper.TencentLangDetectAsync("我將執行一些東西");
        System.Diagnostics.Debug.WriteLine(resp);
    }

    [Fact()]
    public async Task NiutransLangDetectTest()
    {
        var resp = await LangDetectHelper.NiutransLangDetectAsync("我將執行一些東西");
        System.Diagnostics.Debug.WriteLine(resp);
    }

    [Fact()]
    public async Task YandexLangDetectTest()
    {
        var resp = await LangDetectHelper.YandexLangDetectAsync("我將執行一些東西");
        System.Diagnostics.Debug.WriteLine(resp);
    }

    [Fact()]
    public async Task BingLangDetectTest()
    {
        var resp = await LangDetectHelper.BingLangDetectAsync("我將執行一些東西");
        System.Diagnostics.Debug.WriteLine(resp);
    }

    [Fact()]
    public async Task GoogleLangDetectTest()
    {
        var resp = await LangDetectHelper.GoogleLangDetectAsync("我將執行一些東西");
        System.Diagnostics.Debug.WriteLine(resp);
    }
}
