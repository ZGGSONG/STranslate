using STranslate.Log;
using STranslate.Util;
using Xunit;
using Assert = Xunit.Assert;

namespace STranslate.Helper.Tests;

public class ExternalCallHelperTests
{
    [Fact()]
    public void StartServiceTest()
    {
        //注册日志服务
        LogService.Register();

        string prefix = "http://127.0.0.1:9000/";
        Singleton<ExternalCallHelper>.Instance.StartService(prefix);

        Assert.True(Singleton<ExternalCallHelper>.Instance.IsStarted);

        while (true) { Thread.Sleep(200); }

        Singleton<ExternalCallHelper>.Instance.StopService();
        Assert.False(Singleton<ExternalCallHelper>.Instance.IsStarted);
    }
}
