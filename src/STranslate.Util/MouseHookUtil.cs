using System.Diagnostics;
using System.Runtime.InteropServices;

namespace STranslate.Util;

public class MouseHookUtil : GlobalHook
{
    public MouseHookUtil()
    {
        _hookType = 14;
    }

    public event MouseEventHandler? MouseDown;

    public event MouseEventHandler? MouseUp;

    public event MouseEventHandler? MouseMove;

    public event MouseEventHandler? MouseWheel;

    public event EventHandler? Click;

    public event EventHandler? DoubleClick;

    protected override int HookCallbackProcedure(int nCode, int wParam, IntPtr lParam)
    {
        if (nCode > -1 && (MouseDown != null || MouseUp != null || MouseMove != null))
        {
            var mouseLLHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseLLHookStruct))!;
            var button = GetButton(wParam);
            var mouseEventType = GetEventType(wParam);
            var e = new MouseEventArgs(
                button,
                mouseEventType != MouseEventType.DoubleClick ? 1 : 2,
                mouseLLHookStruct.pt.x,
                mouseLLHookStruct.pt.y,
                mouseEventType == MouseEventType.MouseWheel ? (short)((mouseLLHookStruct.mouseData >> 16) & 0xFFFF) : 0
            );
            if (button == MouseButtons.Right && mouseLLHookStruct.flags != 0) mouseEventType = MouseEventType.None;
            switch (mouseEventType)
            {
                case MouseEventType.MouseDown:
                    MouseDown?.Invoke(this, e);
                    break;

                case MouseEventType.MouseUp:
                    Click?.Invoke(this, new EventArgs());
                    MouseUp?.Invoke(this, e);
                    break;

                case MouseEventType.DoubleClick:
                    DoubleClick?.Invoke(this, new EventArgs());
                    break;

                case MouseEventType.MouseWheel:
                    MouseWheel?.Invoke(this, e);
                    break;

                case MouseEventType.MouseMove:
                    MouseMove?.Invoke(this, e);
                    break;
            }
        }

        return CommonUtil.CallNextHookEx(_handleToHook, nCode, wParam, lParam);
    }

    private MouseButtons GetButton(int wParam)
    {
        return wParam switch
        {
            513 or 514 or 515 => MouseButtons.Left,
            516 or 517 or 518 => MouseButtons.Right,
            519 or 520 or 521 => MouseButtons.Middle,
            _ => MouseButtons.None
        };
    }

    private MouseEventType GetEventType(int wParam)
    {
        return wParam switch
        {
            513 or 516 or 519 => MouseEventType.MouseDown,
            514 or 517 or 520 => MouseEventType.MouseUp,
            515 or 518 or 521 => MouseEventType.DoubleClick,
            522 => MouseEventType.MouseWheel,
            512 => MouseEventType.MouseMove,
            _ => MouseEventType.None
        };
    }

    private enum MouseEventType
    {
        None,
        MouseDown,
        MouseUp,
        DoubleClick,
        MouseWheel,
        MouseMove
    }
}

public abstract class GlobalHook
{
    protected const int WH_MOUSE_LL = 14;

    protected const int WH_KEYBOARD_LL = 13;

    protected const int WH_MOUSE = 7;

    protected const int WH_KEYBOARD = 2;

    protected const int WM_MOUSEMOVE = 512;

    protected const int WM_LBUTTONDOWN = 513;

    protected const int WM_RBUTTONDOWN = 516;

    protected const int WM_MBUTTONDOWN = 519;

    protected const int WM_LBUTTONUP = 514;

    protected const int WM_RBUTTONUP = 517;

    protected const int WM_MBUTTONUP = 520;

    protected const int WM_LBUTTONDBLCLK = 515;

    protected const int WM_RBUTTONDBLCLK = 518;

    protected const int WM_MBUTTONDBLCLK = 521;

    protected const int WM_MOUSEWHEEL = 522;

    protected const int WM_KEYDOWN = 256;

    protected const int WM_KEYUP = 257;

    protected const int WM_SYSKEYDOWN = 260;

    protected const int WM_SYSKEYUP = 261;

    protected const byte VK_SHIFT = 16;

    protected const byte VK_CAPITAL = 20;

    protected const byte VK_NUMLOCK = 144;

    protected const byte VK_LSHIFT = 160;

    protected const byte VK_RSHIFT = 161;

    protected const byte VK_LCONTROL = 162;

    protected const byte VK_RCONTROL = 3;

    protected const byte VK_LALT = 164;

    protected const byte VK_RALT = 165;

    protected const byte LLKHF_ALTDOWN = 32;

    protected int _handleToHook;

    protected HookProc? _hookCallback;

    protected int _hookType;

    public bool _isStarted;

    public GlobalHook()
    {
        Application.ApplicationExit += Application_ApplicationExit;
    }

    public bool IsStarted => _isStarted;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookExW(int idHook, HookProc lpfn, IntPtr hmod, uint dwThreadID);

    public void Start()
    {
        if (_isStarted || _hookType == 0) return;
        _hookCallback = HookCallbackProcedure;
        using (var process = Process.GetCurrentProcess())
        {
            using var processModule = process.MainModule!;
            _handleToHook = (int)SetWindowsHookExW(_hookType, _hookCallback,
                CommonUtil.GetModuleHandle(processModule.ModuleName), 0u);
        }

        if (_handleToHook != 0) _isStarted = true;
    }

    public void Stop()
    {
        if (_isStarted)
        {
            CommonUtil.UnhookWindowsHookEx(_handleToHook);
            _isStarted = false;
        }
    }

    protected virtual int HookCallbackProcedure(int nCode, int wParam, IntPtr lParam)
    {
        return 0;
    }

    protected void Application_ApplicationExit(object? sender, EventArgs e)
    {
        if (_isStarted) Stop();
    }

    [StructLayout(LayoutKind.Sequential)]
    protected class POINT
    {
        public int x;

        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    protected class MouseHookStruct
    {
        public POINT pt = new();

        public int hwnd;

        public int wHitTestCode;

        public int dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    protected class MouseLLHookStruct
    {
        public POINT pt = new();

        public int mouseData;

        public int flags;

        public int time;

        public int dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    protected class KeyboardHookStruct
    {
        public int vkCode;

        public int scanCode;

        public int flags;

        public int time;

        public int dwExtraInfo;
    }

    protected delegate int HookProc(int nCode, int wParam, IntPtr lParam);
}