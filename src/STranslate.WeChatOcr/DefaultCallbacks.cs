using System.Diagnostics;
using System.Runtime.InteropServices;

namespace STranslate.WeChatOcr;


public class DefaultCallbacks
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MMReadPushDelegate(uint request_id, IntPtr request_info, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MMReadPullDelegate(uint request_id, IntPtr request_info, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MMReadSharedDelegate(uint request_id, IntPtr request_info, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MMRemoteConnectDelegate(bool is_connected, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MMRemoteDisconnectDelegate(IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MMRemoteProcessLaunchedDelegate(IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MMRemoteProcessLaunchFailedDelegate(int error_code, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void MMRemoteMojoErrorDelegate(IntPtr errorbuf, int errorsize, IntPtr user_data);

    public Dictionary<string, Delegate> callbacks = [];

    public DefaultCallbacks()
    {
        callbacks["kMMReadPush"] = new MMReadPushDelegate(DefaultReadPush);
        callbacks["kMMReadPull"] = new MMReadPullDelegate(DefaultReadPull);
        callbacks["kMMReadShared"] = new MMReadSharedDelegate(DefaultReadShared);
        callbacks["kMMRemoteConnect"] = new MMRemoteConnectDelegate(DefaultRemoteConnect);
        callbacks["kMMRemoteDisconnect"] = new MMRemoteDisconnectDelegate(DefaultRemoteDisconnect);
        callbacks["kMMRemoteProcessLaunched"] = new MMRemoteProcessLaunchedDelegate(DefaultRemoteProcessLaunched);
        callbacks["kMMRemoteProcessLaunchFailed"] = new MMRemoteProcessLaunchFailedDelegate(DefaultRemoteProcessLaunchFailed);
        callbacks["kMMRemoteMojoError"] = new MMRemoteMojoErrorDelegate(DefaultRemoteMojoError);
    }

    // Callback implementations
    private void DefaultReadPush(uint request_id, IntPtr request_info, IntPtr user_data)
    {
        Debug.WriteLine($"【{nameof(DefaultReadPush)}】被调用, request_id: {request_id}, request_info: {request_info}");
    }

    private void DefaultReadPull(uint request_id, IntPtr request_info, IntPtr user_data)
    {
        Debug.WriteLine($"【{nameof(DefaultReadPull)}】被调用, request_id: {request_id}, request_info: {request_info}");
    }

    private void DefaultReadShared(uint request_id, IntPtr request_info, IntPtr user_data)
    {
        Debug.WriteLine($"【{nameof(DefaultReadShared)}】被调用, request_id: {request_id}, request_info: {request_info}");
    }

    private void DefaultRemoteConnect(bool is_connected, IntPtr user_data)
    {
        Debug.WriteLine($"【{nameof(DefaultRemoteConnect)}】被调用, is_connected: {is_connected}");
    }

    private void DefaultRemoteDisconnect(IntPtr user_data)
    {
        Debug.WriteLine($"【{nameof(DefaultRemoteDisconnect)}】被调用");
    }

    private void DefaultRemoteProcessLaunched(IntPtr user_data)
    {
        Debug.WriteLine($"【{nameof(DefaultRemoteProcessLaunched)}】被调用");
    }

    private void DefaultRemoteProcessLaunchFailed(int error_code, IntPtr user_data)
    {
        Debug.WriteLine($"【{nameof(DefaultRemoteProcessLaunchFailed)}】被调用，error_code: {error_code}");
    }

    private void DefaultRemoteMojoError(IntPtr errorbuf, int errorsize, IntPtr user_data)
    {
        Debug.WriteLine($"【{nameof(DefaultRemoteMojoError)}】被调用，errorsize: {errorsize}");
    }
}