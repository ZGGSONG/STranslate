using System.Net;
using System.Net.Http;
using STranslate.Model;
using STranslate.Util.Proxy;

namespace STranslate.Helper;

/// <summary>
///     感谢lindexi(github.com/lindexi)
///     https://blog.lindexi.com/post/dotnet-6-%E4%B8%BA%E4%BB%80%E4%B9%88%E7%BD%91%E7%BB%9C%E8%AF%B7%E6%B1%82%E4%B8%8D%E8%B7%9F%E9%9A%8F%E7%B3%BB%E7%BB%9F%E7%BD%91%E7%BB%9C%E4%BB%A3%E7%90%86%E5%8F%98%E5%8C%96%E8%80%8C%E5%8A%A8%E6%80%81%E5%88%87%E6%8D%A2%E4%BB%A3%E7%90%86.html
/// </summary>
public static class ProxyUtil
{
    private static readonly DynamicHttpWindowsProxy _dynamicProxy = new();

    /// <summary>
    ///     开始加载动态监听系统代理
    /// </summary>
    public static void LoadDynamicProxy()
    {
        HttpClient.DefaultProxy = _dynamicProxy;

        ListeningAgent();
    }

    /// <summary>
    ///     更新是否使用监听到的系统代理
    ///     有时代理不稳定，还不如禁用使用系统代理访问
    /// </summary>
    /// <param name="flag">false: 使用默认系统代理，true: 不使用任何代理</param>
    public static void UpdateDynamicProxy(bool flag)
    {
        HttpClient.DefaultProxy = flag ? new HttpNoProxy() : _dynamicProxy;
    }

    public static void UpdateProxy(ProxyMethodEnum proxyMethod, string ip = "", int port = 0, bool isAuth = false,
        string username = "", string pwd = "")
    {
        //Socks5方式参考: https://github.com/MihaZupan/HttpToSocks5Proxy
        HttpClient.DefaultProxy = proxyMethod switch
        {
            ProxyMethodEnum.不使用代理 => new HttpNoProxy(),
            ProxyMethodEnum.系统代理 => _dynamicProxy,
            _ => new WebProxy($"{proxyMethod.ToString().ToLower()}://{ip}:{port}", true, null,
                isAuth
                    ? new NetworkCredential(username, pwd)
                    : null) //当设置为 true 时，如果请求的目标地址是本地地址（比如 localhost 或 127.0.0.1），将不会使用代理而是直接连接。
        };
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public static void UnLoadDynamicProxy()
    {
        _dynamicProxy.Dispose();
    }

    internal static async void ListeningAgent()
    {
        await Task.Delay(500);
        _dynamicProxy.Start();
    }
}