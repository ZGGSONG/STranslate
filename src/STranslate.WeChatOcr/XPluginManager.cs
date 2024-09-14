using System.IO;
using System.Runtime.InteropServices;

namespace STranslate.WeChatOcr;

/// <summary>
///     原理参考：<see href="https://bbs.kanxue.com/thread-278161.htm#msg_header_h2_1"/>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public class XPluginManager : IDisposable
{
    private bool isMmmojoEnvInited;
    private IntPtr intPtrMmmojoEnv = IntPtr.Zero;
    private IntPtr intPtrUserData;
    private IntPtr intPtrOcrExePath;
    private List<string> cmdLines = [];
    private readonly DefaultCallbacks defaultCallbacks = new();
    private readonly Dictionary<string, Delegate> callbacks = [];
    private readonly Dictionary<string, string> m_switch_native = [];

    ~XPluginManager()
    {
        if (isMmmojoEnvInited) StopMmMojoEnv();
    }

    public void SetExePath(string exePath)
    {
        const string ocrExeName = "WeChatOCR.exe";
        if (!exePath.EndsWith(ocrExeName) && Directory.Exists(exePath)) exePath = Path.Combine(exePath, ocrExeName);
        if (!File.Exists(exePath)) throw new Exception($"指定的 {ocrExeName} 路径不存在!");
        intPtrOcrExePath = Marshal.StringToHGlobalUni(exePath);
    }

    public void AppendSwitchNativeCmdLine(string arg, string value)
    {
        m_switch_native[arg] = value;
    }

    public void SetCommandLine(List<string> cmdline)
    {
        cmdLines = cmdline;
    }

    public void SetOneCallback(string name, Delegate func)
    {
        callbacks[name] = func;
    }

    public void SetCallbacks(Dictionary<string, Delegate> callBacks)
    {
        foreach (var callback in callBacks) callbacks[callback.Key] = callback.Value;
    }

    public void SetCallbackUsrData(IntPtr cbUsrData)
    {
        intPtrUserData = cbUsrData;
    }

    public void InitMmMojoEnv()
    {
        var exePath = Marshal.PtrToStringUni(intPtrOcrExePath);
        if (!File.Exists(exePath)) throw new Exception($"给定的 WeChatOcr.exe 路径错误 (m_exe_path): {exePath}");
        if (isMmmojoEnvInited && intPtrMmmojoEnv != IntPtr.Zero) return;

        MmmojoDll.InitializeMMMojo(0, IntPtr.Zero);
        intPtrMmmojoEnv = MmmojoDll.CreateMMMojoEnvironment();
        if (intPtrMmmojoEnv == IntPtr.Zero) throw new Exception("CreateMMMojoEnvironment 失败!");
        MmmojoDll.SetMMMojoEnvironmentCallbacks(intPtrMmmojoEnv, (int)MMMojoCallbackType.kMMUserData, intPtrUserData);
        SetDefaultCallbacks();
        MmmojoDll.SetMMMojoEnvironmentInitParams(intPtrMmmojoEnv, (int)MMMojoEnvironmentInitParamType.kMMHostProcess,
            new IntPtr(1));
        MmmojoDll.SetMMMojoEnvironmentInitParams(intPtrMmmojoEnv, (int)MMMojoEnvironmentInitParamType.kMMExePath,
            intPtrOcrExePath);

        foreach (var item in m_switch_native)
        {
            var keyPtr = Marshal.StringToHGlobalAnsi(item.Key);
            var valuePtr = Marshal.StringToHGlobalUni(item.Value);
            MmmojoDll.AppendMMSubProcessSwitchNative(intPtrMmmojoEnv, keyPtr, valuePtr);
        }

        MmmojoDll.StartMMMojoEnvironment(intPtrMmmojoEnv);
        isMmmojoEnvInited = true;
    }

    private void SetDefaultCallbacks()
    {
        foreach (MMMojoCallbackType type in Enum.GetValues(typeof(MMMojoCallbackType)))
        {
            if (type == MMMojoCallbackType.kMMUserData) continue;
            try
            {
                var fName = type.ToString();
                var callback = defaultCallbacks.callbacks[fName];
                if (callbacks.TryGetValue(fName, out var cb)) callback = cb;
                var callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
                MmmojoDll.SetMMMojoEnvironmentCallbacks(intPtrMmmojoEnv, (int)type, callbackPtr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public void StopMmMojoEnv()
    {
        if (!isMmmojoEnvInited || intPtrMmmojoEnv == IntPtr.Zero)
            return;

        MmmojoDll.StopMMMojoEnvironment(intPtrMmmojoEnv);
        MmmojoDll.RemoveMMMojoEnvironment(intPtrMmmojoEnv);
        intPtrMmmojoEnv = IntPtr.Zero;
        isMmmojoEnvInited = false;
    }

    public void SendPbSerializedData(byte[] pbData, int pbSize, int method, int sync, uint requestId)
    {
        var writeInfo = MmmojoDll.CreateMMMojoWriteInfo(method, sync, requestId);
        var request = MmmojoDll.GetMMMojoWriteInfoRequest(writeInfo, (uint)pbSize);
        Marshal.Copy(pbData, 0, request, pbSize);
        MmmojoDll.SendMMMojoWriteInfo(intPtrMmmojoEnv, writeInfo);
    }

    public IntPtr GetPbSerializedData(IntPtr requestInfo, ref uint dataSize)
    {
        return MmmojoDll.GetMMMojoReadInfoRequest(requestInfo, ref dataSize);
    }

    public IntPtr GetReadInfoAttachData(IntPtr requestInfo, ref uint dataSize)
    {
        return MmmojoDll.GetMMMojoReadInfoAttach(requestInfo, ref dataSize);
    }

    public void RemoveReadInfo(IntPtr requestInfo)
    {
        MmmojoDll.RemoveMMMojoReadInfo(requestInfo);
    }

    public void Dispose()
    {
        if (isMmmojoEnvInited) StopMmMojoEnv();
    }
}