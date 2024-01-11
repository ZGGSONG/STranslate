using STranslate.Util.Proxy;
using System.Net.Http;
using System.Threading.Tasks;

namespace STranslate.Helper
{
    /// <summary>
    /// 感谢lindexi(github.com/lindexi)
    /// https://blog.lindexi.com/post/dotnet-6-%E4%B8%BA%E4%BB%80%E4%B9%88%E7%BD%91%E7%BB%9C%E8%AF%B7%E6%B1%82%E4%B8%8D%E8%B7%9F%E9%9A%8F%E7%B3%BB%E7%BB%9F%E7%BD%91%E7%BB%9C%E4%BB%A3%E7%90%86%E5%8F%98%E5%8C%96%E8%80%8C%E5%8A%A8%E6%80%81%E5%88%87%E6%8D%A2%E4%BB%A3%E7%90%86.html
    /// </summary>
    public static class ProxyUtil
    {
        public static void LoadDynamicProxy()
        {
            HttpClient.DefaultProxy = _dynamicProxy;

            ListeningAgent();
        }

        public static void UnLoadDynamicProxy() => _dynamicProxy.Dispose();

        internal static async void ListeningAgent()
        {
            await Task.Delay(500);
            _dynamicProxy.Start();
        }

        private static readonly DynamicHttpWindowsProxy _dynamicProxy = new();
    }
}
