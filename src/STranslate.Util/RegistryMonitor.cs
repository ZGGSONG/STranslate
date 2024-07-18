using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace STranslate.Util;

/// <summary>
///     https://www.codeproject.com/Articles/4502/RegistryMonitor-a-NET-wrapper-class-for-RegNotifyC
/// </summary>
public class RegistryMonitor : IDisposable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RegistryMonitor" /> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="monitorKey">The monitor key.</param>
    public RegistryMonitor(string name, string monitorKey = "")
    {
        if (name == null || name.Length == 0)
            throw new ArgumentNullException("name");

        InitRegistryKey(name, monitorKey);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegistryMonitor" /> class.
    /// </summary>
    /// <param name="registryHive">The registry hive.</param>
    /// <param name="subKey">The sub key.</param>
    /// <param name="monitorKey">Should Monitor key.</param>
    public RegistryMonitor(RegistryHive registryHive, string subKey, string monitorKey)
    {
        InitRegistryKey(registryHive, subKey, monitorKey);
    }

    /// <summary>
    ///     Gets or sets the <see cref="RegChangeNotifyFilter">RegChangeNotifyFilter</see>.
    /// </summary>
    public RegChangeNotifyFilter RegChangeNotifyFilter
    {
        get => _regFilter;
        set
        {
            lock (_threadLock)
            {
                if (IsMonitoring)
                    throw new InvalidOperationException("Monitoring thread is already running");

                _regFilter = value;
            }
        }
    }

    /// <summary>
    ///     <b>true</b> if this <see cref="RegistryMonitor" /> object is currently monitoring;
    ///     otherwise, <b>false</b>.
    /// </summary>
    public bool IsMonitoring => _thread != null;

    /// <summary>
    ///     Disposes this object.
    /// </summary>
    public void Dispose()
    {
        Stop();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Start monitoring.
    /// </summary>
    public void Start()
    {
        if (_disposed)
            throw new ObjectDisposedException(null, "This instance is already disposed");

        lock (_threadLock)
        {
            if (!IsMonitoring)
            {
                _eventTerminate.Reset();
                _thread = new Thread(MonitorThread)
                {
                    IsBackground = true
                };
                _thread.Start();
            }
        }
    }

    /// <summary>
    ///     Stops the monitoring thread.
    /// </summary>
    public void Stop()
    {
        if (_disposed)
            throw new ObjectDisposedException(null, "This instance is already disposed");

        lock (_threadLock)
        {
            var thread = _thread;
            if (thread != null)
            {
                _eventTerminate.Set();
                thread.Join();
            }
        }
    }

    private void MonitorThread()
    {
        try
        {
            ThreadLoop();
        }
        catch (Exception e)
        {
            OnError(e);
        }

        _thread = null;
    }

    private void ThreadLoop()
    {
        var result = RegOpenKeyEx(_registryHive, _registrySubName, 0,
            STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_NOTIFY, out var registryKey);
        if (result != 0)
            throw new Win32Exception(result);

        try
        {
            while (!_eventTerminate.WaitOne(0, true))
            {
                using AutoResetEvent _eventNotify = new(false);
                result = RegNotifyChangeKeyValue(registryKey, true, _regFilter,
                    _eventNotify.SafeWaitHandle.DangerousGetHandle(), true);
                if (result != 0)
                    throw new Win32Exception(result);

                WaitHandle[] waitHandles = [_eventNotify, _eventTerminate];
                if (WaitHandle.WaitAny(waitHandles) == 0)
                {
                    // 获取变化的参数
                    var changedValue = GetRegistryValue(_registrySubName, _monitorKey);
                    OnRegChanged(changedValue);
                }
            }
        }
        finally
        {
            if (registryKey != IntPtr.Zero) _ = RegCloseKey(registryKey);
        }
    }

    /// <summary>
    ///     获取注册表键值的方法
    /// </summary>
    /// <param name="registryKey"></param>
    /// <param name="valueName"></param>
    /// <returns></returns>
    public static string GetRegistryValue(string registryKey, string? valueName)
    {
        if (string.IsNullOrEmpty(valueName)) return "";
        using var key = Registry.CurrentUser.OpenSubKey(registryKey);
        return key?.GetValue(valueName)?.ToString() ?? "";
    }

    #region P/Invoke

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired,
        out IntPtr phkResult);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree,
        RegChangeNotifyFilter dwNotifyFilter, IntPtr hEvent, bool fAsynchronous);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegCloseKey(IntPtr hKey);

    private const int KEY_QUERY_VALUE = 0x0001;
    private const int KEY_NOTIFY = 0x0010;
    private const int STANDARD_RIGHTS_READ = 0x00020000;

    private static readonly IntPtr HKEY_CLASSES_ROOT = new(unchecked((int)0x80000000));
    private static readonly IntPtr HKEY_CURRENT_USER = new(unchecked((int)0x80000001));
    private static readonly IntPtr HKEY_LOCAL_MACHINE = new(unchecked((int)0x80000002));
    private static readonly IntPtr HKEY_USERS = new(unchecked((int)0x80000003));
    private static readonly IntPtr HKEY_PERFORMANCE_DATA = new(unchecked((int)0x80000004));
    private static readonly IntPtr HKEY_CURRENT_CONFIG = new(unchecked((int)0x80000005));

    #endregion

    #region Event handling

    /// <summary>
    ///     Occurs when the specified registry key has changed.
    /// </summary>
    public event Action<string>? RegChanged;

    /// <summary>
    ///     Occurs when the access to the registry fails.
    /// </summary>
    public event ErrorEventHandler? Error;

    /// <summary>
    ///     Raises the <see cref="RegChanged" /> event.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         <b>OnRegChanged</b> is called when the specified registry key has changed.
    ///     </p>
    ///     <note type="inheritinfo">
    ///         When overriding <see cref="OnRegChanged" /> in a derived class, be sure to call
    ///         the base class's <see cref="OnRegChanged" /> method.
    ///     </note>
    /// </remarks>
    protected virtual void OnRegChanged(string arg)
    {
        RegChanged?.Invoke(arg);
    }

    /// <summary>
    ///     Raises the <see cref="Error" /> event.
    /// </summary>
    /// <param name="e">The <see cref="Exception" /> which occured while watching the registry.</param>
    /// <remarks>
    ///     <p>
    ///         <b>OnError</b> is called when an exception occurs while watching the registry.
    ///     </p>
    ///     <note type="inheritinfo">
    ///         When overriding <see cref="OnError" /> in a derived class, be sure to call
    ///         the base class's <see cref="OnError" /> method.
    ///     </note>
    /// </remarks>
    protected virtual void OnError(Exception e)
    {
        Error?.Invoke(this, new ErrorEventArgs(e));
    }

    #endregion

    #region Private member variables

    private IntPtr _registryHive;
    private string _registrySubName = "";
    private string _monitorKey = "";
    private readonly object _threadLock = new();
    private Thread? _thread;
    private bool _disposed;
    private readonly ManualResetEvent _eventTerminate = new(false);

    private RegChangeNotifyFilter _regFilter = RegChangeNotifyFilter.Key | RegChangeNotifyFilter.Attribute |
                                               RegChangeNotifyFilter.Value | RegChangeNotifyFilter.Security;

    #endregion

    #region Initialization

    private void InitRegistryKey(RegistryHive hive, string name, string key = "")
    {
        _registryHive = hive switch
        {
            RegistryHive.ClassesRoot => HKEY_CLASSES_ROOT,
            RegistryHive.CurrentConfig => HKEY_CURRENT_CONFIG,
            RegistryHive.CurrentUser => HKEY_CURRENT_USER,
            RegistryHive.LocalMachine => HKEY_LOCAL_MACHINE,
            RegistryHive.PerformanceData => HKEY_PERFORMANCE_DATA,
            RegistryHive.Users => HKEY_USERS,
            _ => throw new InvalidEnumArgumentException("hive", (int)hive, typeof(RegistryHive))
        };
        _registrySubName = name;
        _monitorKey = key;
    }

    private void InitRegistryKey(string name, string key = "")
    {
        var nameParts = name.Split('\\');

        switch (nameParts[0])
        {
            case "HKEY_CLASSES_ROOT":
            case "HKCR":
                _registryHive = HKEY_CLASSES_ROOT;
                break;

            case "HKEY_CURRENT_USER":
            case "HKCU":
                _registryHive = HKEY_CURRENT_USER;
                break;

            case "HKEY_LOCAL_MACHINE":
            case "HKLM":
                _registryHive = HKEY_LOCAL_MACHINE;
                break;

            case "HKEY_USERS":
                _registryHive = HKEY_USERS;
                break;

            case "HKEY_CURRENT_CONFIG":
                _registryHive = HKEY_CURRENT_CONFIG;
                break;

            default:
                _registryHive = IntPtr.Zero;
                throw new ArgumentException("The registry hive '" + nameParts[0] + "' is not supported", "value");
        }

        _registrySubName = string.Join("\\", nameParts, 1, nameParts.Length - 1);
        _monitorKey = key;
    }

    #endregion
}

/// <summary>
///     Filter for notifications reported by <see cref="RegistryMonitor" />.
/// </summary>
[Flags]
public enum RegChangeNotifyFilter
{
    /// <summary>Notify the caller if a subkey is added or deleted.</summary>
    Key = 1,

    /// <summary>
    ///     Notify the caller of changes to the attributes of the key,
    ///     such as the security descriptor information.
    /// </summary>
    Attribute = 2,

    /// <summary>
    ///     Notify the caller of changes to a value of the key. This can
    ///     include adding or deleting a value, or changing an existing value.
    /// </summary>
    Value = 4,

    /// <summary>
    ///     Notify the caller of changes to the security descriptor
    ///     of the key.
    /// </summary>
    Security = 8
}