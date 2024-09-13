using System.Runtime.InteropServices;

namespace STranslate.WeChatOcr;

public enum MMMojoInfoMethod
{
    kMMNone = 0,
    kMMPush,
    kMMPullReq,
    kMMPullResp,
    kMMShared
}

public enum MMMojoCallbackType
{
    kMMUserData = 0,
    kMMReadPush,
    kMMReadPull,
    kMMReadShared,
    kMMRemoteConnect,
    kMMRemoteDisconnect,
    kMMRemoteProcessLaunched,
    kMMRemoteProcessLaunchFailed,
    kMMRemoteMojoError
}

public enum MMMojoEnvironmentInitParamType
{
    kMMHostProcess = 0,
    kMMLoopStartThread,
    kMMExePath,
    kMMLogPath,
    kMMLogToStderr,
    kMMAddNumMessagepipe,
    kMMSetDisconnectHandlers,
    kMMDisableDefaultPolicy = 1000,
    kMMElevated,
    kMMCompatible
}

public class MmmojoDll
{
    public MmmojoDll()
    {
    }

#if WIN32
    const string MojoDllName = "mmmojo.dll";
#else
    const string MojoDllName = "mmmojo_64.dll";
#endif

    // Function Definitions (P/Invoke declarations)
    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void InitializeMMMojo(int argc, IntPtr argv);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ShutdownMMMojo();

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CreateMMMojoEnvironment();

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetMMMojoEnvironmentCallbacks(IntPtr mmmojo_env, int type, IntPtr callback);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void SetMMMojoEnvironmentInitParams(IntPtr mmmojo_env, int type, IntPtr param);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void AppendMMSubProcessSwitchNative(IntPtr mmmojo_env, IntPtr switchStringPtr, IntPtr valuePtr);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void StartMMMojoEnvironment(IntPtr mmmojo_env);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void StopMMMojoEnvironment(IntPtr mmmojo_env);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RemoveMMMojoEnvironment(IntPtr mmmojo_env);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetMMMojoReadInfoRequest(IntPtr mmmojo_readinfo, ref uint requestDataSize);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetMMMojoReadInfoAttach(IntPtr mmmojo_readinfo, ref uint attachDataSize);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RemoveMMMojoReadInfo(IntPtr mmmojo_readinfo);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetMMMojoReadInfoMethod(IntPtr mmmojo_readinfo);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool GetMMMojoReadInfoSync(IntPtr mmmojo_readinfo);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CreateMMMojoWriteInfo(int method, int sync, uint requestId);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetMMMojoWriteInfoRequest(IntPtr mmmojo_writeinfo, uint requestDataSize);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void RemoveMMMojoWriteInfo(IntPtr mmmojo_writeinfo);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetMMMojoWriteInfoAttach(IntPtr mmmojo_writeinfo, uint attachDataSize);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetMMMojoWriteInfoMessagePipe(IntPtr mmmojo_writeinfo, int numOfMessagePipe);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetMMMojoWriteInfoResponseSync(IntPtr mmmojo_writeinfo, ref IntPtr mmmojo_readinfo);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool SendMMMojoWriteInfo(IntPtr mmmojo_env, IntPtr mmmojo_writeinfo);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool SwapMMMojoWriteInfoCallback(IntPtr mmmojo_writeinfo, IntPtr mmmojo_readinfo);

    [DllImport(MojoDllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool SwapMMMojoWriteInfoMessage(IntPtr mmmojo_writeinfo, IntPtr mmmojo_readinfo);
}
