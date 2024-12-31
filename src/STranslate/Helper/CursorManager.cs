using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace STranslate.Helper;


/// <summary>
///     光标管理类，用于设置和恢复系统光标
/// </summary>
public sealed class CursorManager : IDisposable
{
    #region Constants
    private const int OCR_NORMAL = 32512;
    private const int OCR_IBEAM = 32513;
    private const uint IMAGE_CURSOR = 2;
    private const uint LR_LOADFROMFILE = 0x00000010;
    private const uint LR_DEFAULTSIZE = 0x00000040;
    private const uint LR_SHARED = 0x00008000;
    /// <summary>
    ///     重置系统光标。将ulParam参数设为0并且pvParam参数设为NULL。
    /// </summary>
    private const uint SPI_SETCURSORS = 0x0057;
    private const uint SPIF_UPDATEINIFILE = 0x01;
    private const uint SPIF_SENDCHANGE = 0x02;
    private const uint SPI_GETDPISCALINGSIZE = 0x2014;
    private const uint PROCESS_DPI_AWARENESS = 2;
    #endregion

    #region Fields
    private static readonly object _lock = new();
    private static volatile bool _isExecuting;
    private bool _disposed;

    private IntPtr _originalNormalCursor;
    private IntPtr _originalBeamCursor;
    private IntPtr _customNormalCursor;
    private IntPtr _customBeamCursor;
    private IntPtr _errorNormalCursor;
    private IntPtr _errorBeamCursor;

    private static readonly string[] ErrorCursorPaths =
    [
        @"C:\Windows\Cursors\aero_unavail.cur",
        @"C:\Windows\Cursors\aero_unavail_l.cur",
        @"C:\Windows\Cursors\aero_unavail_xl.cur"
    ];
    #endregion

    #region Native Methods
    [DllImport("user32.dll")]
    private static extern IntPtr LoadImage(IntPtr hInst, IntPtr lpszName, uint uType,
        int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadCursorFromFile(string fileName);

    [DllImport("user32.dll")]
    private static extern bool SetSystemCursor(IntPtr hCursor, uint id);

    [DllImport("user32.dll")]
    private static extern IntPtr CopyIcon(IntPtr hIcon);

    [DllImport("user32.dll")]
    private static extern bool DestroyCursor(IntPtr hCursor);

    [DllImport("user32.dll")]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

    [DllImport("shcore.dll")]
    private static extern int SetProcessDpiAwareness(uint value);
    #endregion

    #region Singleton Implementation
    public static CursorManager Instance { get; } = new();

    private CursorManager()
    {
        SetProcessDpiAwareness(PROCESS_DPI_AWARENESS);
    }
    #endregion

    #region Public Methods
    /// <summary>
    ///     执行光标替换操作
    /// </summary>
    /// <exception cref="InvalidOperationException">光标操作失败时抛出</exception>
    public void Execute()
    {
        lock (_lock)
        {
            if (_isExecuting) return;

            try
            {
                _isExecuting = true;
                SaveOriginalCursors();
                LoadAndSetCustomCursors();
            }
            catch (Exception ex)
            {
                Restore();
                throw new InvalidOperationException("Failed to execute cursor replacement", ex);
            }
        }
    }

    /// <summary>
    ///     设置错误状态光标
    /// </summary>
    /// <exception cref="InvalidOperationException">设置错误光标失败时抛出</exception>
    public void Error()
    {
        try
        {
            var cursorPath = GetErrorCursorPath();
            SetErrorCursors(cursorPath);
        }
        catch (Exception ex)
        {
            Restore();
            throw new InvalidOperationException("Failed to set error cursors", ex);
        }
    }

    /// <summary>
    ///     恢复原始光标
    /// </summary>
    public void Restore()
    {
        if (!_isExecuting) return;

        try
        {
            RestoreOriginalCursors();
            CleanupCustomCursors();
            UpdateSystemCursors();
        }
        finally
        {
            _isExecuting = false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        Restore();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Private Methods
    private static int GetCursorSize()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Cursors");
        return key?.GetValue("CursorBaseSize") is int size ? size : 32;
    }

    private void SaveOriginalCursors()
    {
        var normalCursor = LoadImage(IntPtr.Zero, new IntPtr(OCR_NORMAL), IMAGE_CURSOR, 0, 0, LR_SHARED);
        var beamCursor = LoadImage(IntPtr.Zero, new IntPtr(OCR_IBEAM), IMAGE_CURSOR, 0, 0, LR_SHARED);

        ThrowIfInvalidPointer(normalCursor, "Failed to load original normal cursor");
        ThrowIfInvalidPointer(beamCursor, "Failed to load original beam cursor");

        _originalNormalCursor = CopyIcon(normalCursor);
        _originalBeamCursor = CopyIcon(beamCursor);

        ThrowIfInvalidPointer(_originalNormalCursor, "Failed to copy original normal cursor");
        ThrowIfInvalidPointer(_originalBeamCursor, "Failed to copy original beam cursor");
    }

    private void LoadAndSetCustomCursors()
    {
        var tempFilePath = Path.GetTempFileName();
        try
        {
            ExtractCursorResource(tempFilePath);
            SetCustomCursors(tempFilePath);
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }

    private static void ExtractCursorResource(string tempFilePath)
    {
        var size = GetCursorSize();
        var cursorSize = size switch
        {
            128 => "128",
            96 => "96",
            64 => "64",
            48 => "48",
            _ => "32"
        };

        var cursorPath = $"pack://application:,,,/STranslate.Style;Component/Resources/Working_{cursorSize}x.ani";
        using var resourceStream = Application.GetResourceStream(new Uri(cursorPath, UriKind.Absolute))?.Stream
            ?? throw new InvalidOperationException($"Could not load cursor resource: {cursorPath}");

        using var fileStream = File.Create(tempFilePath);
        resourceStream.CopyTo(fileStream);
    }

    private void SetCustomCursors(string cursorPath)
    {
        _customNormalCursor = LoadCursorFromFile(cursorPath);
        _customBeamCursor = LoadCursorFromFile(cursorPath);

        ThrowIfInvalidPointer(_customNormalCursor, "Failed to load custom normal cursor");
        ThrowIfInvalidPointer(_customBeamCursor, "Failed to load custom beam cursor");

        if (!SetSystemCursor(_customNormalCursor, OCR_NORMAL))
            throw new InvalidOperationException($"Failed to set normal cursor: {Marshal.GetLastWin32Error()}");

        if (!SetSystemCursor(_customBeamCursor, OCR_IBEAM))
            throw new InvalidOperationException($"Failed to set beam cursor: {Marshal.GetLastWin32Error()}");
    }

    private static string GetErrorCursorPath()
    {
        var size = GetCursorSize();
        return size switch
        {
            32 => ErrorCursorPaths[0],
            48 => ErrorCursorPaths[1],
            _ => ErrorCursorPaths[2]
        };
    }

    private void SetErrorCursors(string path)
    {
        _errorNormalCursor = LoadCursorFromFile(path);
        _errorBeamCursor = LoadCursorFromFile(path);

        ThrowIfInvalidPointer(_errorNormalCursor, "Failed to load error normal cursor");
        ThrowIfInvalidPointer(_errorBeamCursor, "Failed to load error beam cursor");

        if (!SetSystemCursor(_errorNormalCursor, OCR_NORMAL) || !SetSystemCursor(_errorBeamCursor, OCR_IBEAM))
            throw new InvalidOperationException($"Failed to set error cursors: {Marshal.GetLastWin32Error()}");
    }

    private void RestoreOriginalCursors()
    {
        if (_originalNormalCursor != IntPtr.Zero)
        {
            SetSystemCursor(_originalNormalCursor, OCR_NORMAL);
            DestroyCursor(_originalNormalCursor);
            _originalNormalCursor = IntPtr.Zero;
        }

        if (_originalBeamCursor != IntPtr.Zero)
        {
            SetSystemCursor(_originalBeamCursor, OCR_IBEAM);
            DestroyCursor(_originalBeamCursor);
            _originalBeamCursor = IntPtr.Zero;
        }
    }

    private void CleanupCustomCursors()
    {
        SafeDestroyCursor(ref _customNormalCursor);
        SafeDestroyCursor(ref _customBeamCursor);
        SafeDestroyCursor(ref _errorNormalCursor);
        SafeDestroyCursor(ref _errorBeamCursor);
    }

    private static void SafeDestroyCursor(ref IntPtr cursor)
    {
        if (cursor == IntPtr.Zero) return;

        DestroyCursor(cursor);
        cursor = IntPtr.Zero;
    }

    private static void UpdateSystemCursors()
    {
        // 刷新系统光标
        SystemParametersInfo(SPI_SETCURSORS, 0, IntPtr.Zero, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
    }

    private static void ThrowIfInvalidPointer(IntPtr ptr, string message)
    {
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException($"{message}: {Marshal.GetLastWin32Error()}");
    }
    #endregion
}