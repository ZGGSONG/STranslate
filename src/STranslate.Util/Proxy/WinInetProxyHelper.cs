using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace STranslate.Util.Proxy;

/// <summary>
///     Copy From https://github.com/dotnet/runtime/tree/v6.0.5/src/libraries/System.Net.Http
/// </summary>
[SupportedOSPlatform("windows")]
// This class is only used on OS versions where WINHTTP_ACCESS_TYPE_AUTOMATIC_PROXY
// is not supported (i.e. before Win8.1/Win2K12R2) in the WinHttpOpen() function.
internal sealed class WinInetProxyHelper
{
    private const int RecentAutoDetectionInterval = 120_000; // 2 minutes in milliseconds.

    /// <summary>
    ///     最大超时次数
    /// </summary>
    private const ushort MaxTimeoutCount = 3;

    private readonly bool _useProxy;
    private bool _autoDetectionFailed;
    private int _lastTimeAutoDetectionFailed; // Environment.TickCount units (milliseconds).

    /// <summary>
    ///     超时的次数
    /// </summary>
    private short _timeoutCount;

    public WinInetProxyHelper()
    {
        Interop.WinHttp.WINHTTP_CURRENT_USER_IE_PROXY_CONFIG proxyConfig = default;

        try
        {
            if (Interop.WinHttp.WinHttpGetIEProxyConfigForCurrentUser(out proxyConfig))
            {
                AutoConfigUrl = Marshal.PtrToStringUni(proxyConfig.AutoConfigUrl)!;
                AutoDetect = proxyConfig.AutoDetect;
                Proxy = Marshal.PtrToStringUni(proxyConfig.Proxy)!;
                ProxyBypass = Marshal.PtrToStringUni(proxyConfig.ProxyBypass)!;

                //if (NetEventSource.Log.IsEnabled())
                //{
                //    NetEventSource.Info(this, $"AutoConfigUrl={AutoConfigUrl}, AutoDetect={AutoDetect}, Proxy={Proxy}, ProxyBypass={ProxyBypass}");
                //}

                _useProxy = true;
            }

            // We match behavior of WINHTTP_ACCESS_TYPE_AUTOMATIC_PROXY and ignore errors.
            //int lastError = Marshal.GetLastWin32Error();
            //if (NetEventSource.Log.IsEnabled()) NetEventSource.Error(this, $"error={lastError}");
            //if (NetEventSource.Log.IsEnabled()) NetEventSource.Info(this, $"_useProxy={_useProxy}");
        }

        finally
        {
            // FreeHGlobal already checks for null pointer before freeing the memory.
            Marshal.FreeHGlobal(proxyConfig.AutoConfigUrl);
            Marshal.FreeHGlobal(proxyConfig.Proxy);
            Marshal.FreeHGlobal(proxyConfig.ProxyBypass);
        }
    }

    public string? AutoConfigUrl { get; }

    public bool AutoDetect { get; }

    public bool AutoSettingsUsed => AutoDetect || !string.IsNullOrEmpty(AutoConfigUrl);

    public bool ManualSettingsUsed => !string.IsNullOrEmpty(Proxy);

    public bool ManualSettingsOnly => !AutoSettingsUsed && ManualSettingsUsed;

    public string? Proxy { get; }

    public string? ProxyBypass { get; }

    public bool RecentAutoDetectionFailure =>
        _autoDetectionFailed &&
        Environment.TickCount - _lastTimeAutoDetectionFailed <= RecentAutoDetectionInterval;

    public bool GetProxyForUrl(
        Interop.WinHttp.SafeWinHttpHandle? sessionHandle,
        Uri uri,
        out Interop.WinHttp.WINHTTP_PROXY_INFO proxyInfo)
    {
        proxyInfo.AccessType = Interop.WinHttp.WINHTTP_ACCESS_TYPE_NO_PROXY;
        proxyInfo.Proxy = IntPtr.Zero;
        proxyInfo.ProxyBypass = IntPtr.Zero;

        if (!_useProxy) return false;

        if (_timeoutCount >= MaxTimeoutCount)
            // 代理超时次数太多，就不再设置代理
            return false;

        var useProxy = false;

        Interop.WinHttp.WINHTTP_AUTOPROXY_OPTIONS autoProxyOptions;
        autoProxyOptions.AutoConfigUrl = AutoConfigUrl;
        autoProxyOptions.AutoDetectFlags = AutoDetect
            ? Interop.WinHttp.WINHTTP_AUTO_DETECT_TYPE_DHCP | Interop.WinHttp.WINHTTP_AUTO_DETECT_TYPE_DNS_A
            : 0;
        autoProxyOptions.AutoLoginIfChallenged = false;
        autoProxyOptions.Flags =
            (AutoDetect ? Interop.WinHttp.WINHTTP_AUTOPROXY_AUTO_DETECT : 0) |
            (!string.IsNullOrEmpty(AutoConfigUrl) ? Interop.WinHttp.WINHTTP_AUTOPROXY_CONFIG_URL : 0);
        autoProxyOptions.Reserved1 = IntPtr.Zero;
        autoProxyOptions.Reserved2 = 0;

        // AutoProxy Cache.
        // https://docs.microsoft.com/en-us/windows/desktop/WinHttp/autoproxy-cache
        // If the out-of-process service is active when WinHttpGetProxyForUrl is called, the cached autoproxy
        // URL and script are available to the whole computer. However, if the out-of-process service is used,
        // and the fAutoLogonIfChallenged flag in the pAutoProxyOptions structure is true, then the autoproxy
        // URL and script are not cached. Therefore, calling WinHttpGetProxyForUrl with the fAutoLogonIfChallenged
        // member set to TRUE results in additional overhead operations that may affect performance.
        // The following steps can be used to improve performance:
        // 1. Call WinHttpGetProxyForUrl with the fAutoLogonIfChallenged parameter set to false. The autoproxy
        //    URL and script are cached for future calls to WinHttpGetProxyForUrl.
        // 2. If Step 1 fails, with ERROR_WINHTTP_LOGIN_FAILURE, then call WinHttpGetProxyForUrl with the
        //    fAutoLogonIfChallenged member set to TRUE.
        //
        // We match behavior of WINHTTP_ACCESS_TYPE_AUTOMATIC_PROXY and ignore errors.

#pragma warning disable CA1845 // file is shared with a build that lacks string.Concat for spans
        // Underlying code does not understand WebSockets so we need to convert it to http or https.
        var destination = uri.AbsoluteUri;
        if (uri.Scheme == UriScheme.Wss)
            destination = UriScheme.Https + destination.Substring(UriScheme.Wss.Length);
        else if (uri.Scheme == UriScheme.Ws) destination = UriScheme.Http + destination.Substring(UriScheme.Ws.Length);
#pragma warning restore CA1845

        var repeat = false;
        do
        {
            _autoDetectionFailed = false;
            if (Interop.WinHttp.WinHttpGetProxyForUrl(
                    sessionHandle!,
                    destination,
                    ref autoProxyOptions,
                    out proxyInfo))
            {
                //if (NetEventSource.Log.IsEnabled()) NetEventSource.Info(this, "Using autoconfig proxy settings");
                useProxy = true;

                break;
            }

            var lastError = Marshal.GetLastWin32Error();
            //if (NetEventSource.Log.IsEnabled()) NetEventSource.Error(this, $"error={lastError}");

            if (lastError == Interop.WinHttp.ERROR_WINHTTP_LOGIN_FAILURE)
            {
                if (repeat)
                {
                    // We don't retry more than once.
                    break;
                }

                repeat = true;
                autoProxyOptions.AutoLoginIfChallenged = true;
            }
            else if ((uint)lastError is Interop.WinHttp.WINHTTP_OPTION_CONNECT_TIMEOUT
                     or Interop.WinHttp.WINHTTP_OPTION_SEND_TIMEOUT
                     or Interop.WinHttp.WINHTTP_OPTION_RECEIVE_TIMEOUT
                     or Interop.WinHttp.ERROR_WINHTTP_TIMEOUT)
            {
                // 超时相关错误，就不要重试了，也不走手动的代理设置
                // 记录一下，如果连续超过三次超时，那就再也不走代理了
                _timeoutCount++;
                return false;
            }
            else
            {
                if (lastError == Interop.WinHttp.ERROR_WINHTTP_AUTODETECTION_FAILED)
                {
                    _autoDetectionFailed = true;
                    _lastTimeAutoDetectionFailed = Environment.TickCount;
                }

                break;
            }
        } while (repeat);

        // Fall back to manual settings if available.
        if (!useProxy && !string.IsNullOrEmpty(Proxy))
        {
            proxyInfo.AccessType = Interop.WinHttp.WINHTTP_ACCESS_TYPE_NAMED_PROXY;
            proxyInfo.Proxy = Marshal.StringToHGlobalUni(Proxy);
            proxyInfo.ProxyBypass = string.IsNullOrEmpty(ProxyBypass)
                ? IntPtr.Zero
                : Marshal.StringToHGlobalUni(ProxyBypass);

            //if (NetEventSource.Log.IsEnabled()) NetEventSource.Info(this, $"Fallback to Proxy={Proxy}, ProxyBypass={ProxyBypass}");
            useProxy = true;
        }

        //if (NetEventSource.Log.IsEnabled()) NetEventSource.Info(this, $"useProxy={useProxy}");

        if (useProxy)
            // 如果有一次获取代理成功，那就设置非连续超时
            _timeoutCount = 0;

        return useProxy;
    }
}