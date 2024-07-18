using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using static System.String;

namespace STranslate.Util.Proxy;

/// <summary>
///     <b>RegistryMonitor</b> allows you to monitor specific registry key.
/// </summary>
/// https://www.codeproject.com/Articles/4502/RegistryMonitor-a-NET-wrapper-class-for-RegNotifyC
/// <remarks>
///     If a monitored registry key changes, an event is fired. You can subscribe to these
///     events by adding a delegate to <see cref="RegChanged" />.
///     <para>
///         The Windows API provides a function
///         <a href="http://msdn.microsoft.com/library/en-us/sysinfo/base/regnotifychangekeyvalue.asp">
///             RegNotifyChangeKeyValue
///         </a>
///         , which is not covered by the
///         <see cref="Microsoft.Win32.RegistryKey" /> class. <see cref="RegistryMonitor" /> imports
///         that function and encapsulates it in a convenient manner.
///     </para>
/// </remarks>
/// <example>
///     This sample shows how to monitor <c>HKEY_CURRENT_USER\Environment</c> for changes:
///     <code>
///  public class MonitorSample
///  {
///      static void Main()
///      {
///          RegistryMonitor monitor = new RegistryMonitor(RegistryHive.CurrentUser, "Environment");
///          monitor.RegChanged += new EventHandler(OnRegChanged);
///          monitor.Start();
/// 
///          while(true);
/// 
/// 			monitor.Stop();
///      }
/// 
///      private void OnRegChanged(object sender, EventArgs e)
///      {
///          Console.WriteLine("registry key has changed");
///      }
///  }
///  </code>
/// </example>
[SupportedOSPlatform("windows")]
internal sealed class RegistryMonitor : IDisposable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RegistryMonitor" /> class.
    /// </summary>
    /// <param name="registryKey">The registry key to monitor.</param>
    public RegistryMonitor(RegistryKey registryKey)
    {
        InitRegistryKey(registryKey.Name);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegistryMonitor" /> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public RegistryMonitor(string name)
    {
        if (IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        InitRegistryKey(name);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RegistryMonitor" /> class.
    /// </summary>
    /// <param name="registryHive">The registry hive.</param>
    /// <param name="subKey">The sub key.</param>
    public RegistryMonitor(RegistryHive registryHive, string subKey)
    {
        InitRegistryKey(registryHive, subKey);
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
                _thread = new Thread(MonitorThread) { Name = "RegistryMonitor", IsBackground = true };
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
        var result = RegOpenKeyEx(
            _registryHive,
            _registrySubName,
            0,
            STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_NOTIFY,
            out var registryKey
        );
        if (result != 0)
            throw new Win32Exception(result);

        var eventNotify = new AutoResetEvent(false);
        try
        {
            WaitHandle[] waitHandles = { eventNotify, _eventTerminate };
            while (!_eventTerminate.WaitOne(0, true))
            {
                result = RegNotifyChangeKeyValue(registryKey, true, _regFilter, eventNotify.SafeWaitHandle, true);
                if (result != 0)
                    throw new Win32Exception(result);

                if (WaitHandle.WaitAny(waitHandles) == 0) OnRegChanged();
            }
        }
        finally
        {
            if (registryKey != IntPtr.Zero) RegCloseKey(registryKey);

            eventNotify.Dispose();
        }
    }

    #region P/Invoke

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired,
        out IntPtr phkResult);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RegNotifyChangeKeyValue(
        IntPtr hKey,
        bool bWatchSubtree,
        RegChangeNotifyFilter dwNotifyFilter,
        SafeWaitHandle hEvent,
        bool fAsynchronous
    );

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
    private static readonly IntPtr HKEY_DYN_DATA = new(unchecked((int)0x80000006));

    #endregion P/Invoke

    #region Event handling

    /// <summary>
    ///     Occurs when the specified registry key has changed.
    /// </summary>
    public event EventHandler? RegChanged;

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
    private void OnRegChanged()
    {
        RegChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     Occurs when the access to the registry fails.
    /// </summary>
    public event ErrorEventHandler? Error;

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
    private void OnError(Exception e)
    {
        Error?.Invoke(this, new ErrorEventArgs(e));
    }

    #endregion Event handling

    #region Private member variables

    private IntPtr _registryHive;
    private string _registrySubName = "";
    private readonly object _threadLock = new();
    private Thread? _thread;
    private bool _disposed;
    private readonly ManualResetEvent _eventTerminate = new(false);

    private RegChangeNotifyFilter _regFilter =
        RegChangeNotifyFilter.Key | RegChangeNotifyFilter.Attribute | RegChangeNotifyFilter.Value |
        RegChangeNotifyFilter.Security;

    #endregion Private member variables

    #region Initialization

    private void InitRegistryKey(RegistryHive hive, string name)
    {
        switch (hive)
        {
            case RegistryHive.ClassesRoot:
                _registryHive = HKEY_CLASSES_ROOT;
                break;

            case RegistryHive.CurrentConfig:
                _registryHive = HKEY_CURRENT_CONFIG;
                break;

            case RegistryHive.CurrentUser:
                _registryHive = HKEY_CURRENT_USER;
                break;

            //case RegistryHive.DynData:
            //	_registryHive = HKEY_DYN_DATA;
            //	break;

            case RegistryHive.LocalMachine:
                _registryHive = HKEY_LOCAL_MACHINE;
                break;

            case RegistryHive.PerformanceData:
                _registryHive = HKEY_PERFORMANCE_DATA;
                break;

            case RegistryHive.Users:
                _registryHive = HKEY_USERS;
                break;

            default:
                throw new InvalidEnumArgumentException("hive", (int)hive, typeof(RegistryHive));
        }

        _registrySubName = name;
    }

    private void InitRegistryKey(string name)
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

        _registrySubName = Join("\\", nameParts, 1, nameParts.Length - 1);
    }

    #endregion Initialization
}

/// <summary>
///     Filter for notifications reported by <see cref="RegistryMonitor" />.
/// </summary>
[Flags]
internal enum RegChangeNotifyFilter
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