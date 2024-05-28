using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace STranslate.Util;

/// <summary>
/// https://github.com/CopyText/TextCopy/blob/main/src/TextCopy/WindowsClipboard.cs
/// </summary>
public static class ClipboardUtil
{
    public static async Task SetTextAsync(string text, CancellationToken cancellation)
    {
        await TryOpenClipboardAsync(cancellation);

        InnerSet(text);
    }

    public static void SetText(string text)
    {
        TryOpenClipboard();

        InnerSet(text);
    }

    private static void InnerSet(string text)
    {
        EmptyClipboard();
        IntPtr hGlobal = default;
        try
        {
            var bytes = (text.Length + 1) * 2;
            hGlobal = Marshal.AllocHGlobal(bytes);

            if (hGlobal == default)
            {
                ThrowWin32();
            }

            var target = GlobalLock(hGlobal);

            if (target == default)
            {
                ThrowWin32();
            }

            try
            {
                Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
            }
            finally
            {
                GlobalUnlock(target);
            }

            if (SetClipboardData(cfUnicodeText, hGlobal) == default)
            {
                ThrowWin32();
            }

            hGlobal = default;
        }
        finally
        {
            if (hGlobal != default)
            {
                Marshal.FreeHGlobal(hGlobal);
            }

            CloseClipboard();
        }
    }

    private static async Task TryOpenClipboardAsync(CancellationToken cancellation)
    {
        var num = 10;
        while (true)
        {
            if (OpenClipboard(default))
            {
                break;
            }

            if (--num == 0)
            {
                ThrowWin32();
            }

            await Task.Delay(100, cancellation);
        }
    }

    private static void TryOpenClipboard()
    {
        var num = 10;
        while (true)
        {
            if (OpenClipboard(default))
            {
                break;
            }

            if (--num == 0)
            {
                ThrowWin32();
            }

            Thread.Sleep(100);
        }
    }

    public static async Task<string?> GetTextAsync(CancellationToken cancellation)
    {
        if (!IsClipboardFormatAvailable(cfUnicodeText))
        {
            return null;
        }
        await TryOpenClipboardAsync(cancellation);

        return InnerGet();
    }

    public static string? GetText()
    {
        if (!IsClipboardFormatAvailable(cfUnicodeText))
        {
            return null;
        }
        TryOpenClipboard();

        return InnerGet();
    }

    private static string? InnerGet()
    {
        IntPtr handle = default;

        IntPtr pointer = default;
        try
        {
            handle = GetClipboardData(cfUnicodeText);
            if (handle == default)
            {
                return null;
            }

            pointer = GlobalLock(handle);
            if (pointer == default)
            {
                return null;
            }

            var size = GlobalSize(handle);
            var buff = new byte[size];

            Marshal.Copy(pointer, buff, 0, size);

            return Encoding.Unicode.GetString(buff).TrimEnd('\0');
        }
        finally
        {
            if (pointer != default)
            {
                GlobalUnlock(handle);
            }

            CloseClipboard();
        }
    }

    private const uint cfUnicodeText = 13;

    private static void ThrowWin32()
    {
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public static string? GetSelectedText(int interval = 0)
    {
        SendCtrlCV(interval: interval);
        return GetText();
    }

    public static string? GetSelectedText2(int interval = 0)
    {
        var oldTxt = GetText();
        SendCtrlCV(interval: interval);

        //比较，如果有变化则返回新的文本, 否则返回空
        var newTxt = GetText();
        return newTxt == oldTxt ? null : newTxt?.Trim() ?? "";
    }

    private static void SendCtrlCV(bool isCopy = true, int interval = 0)
    {
        uint KEYEVENTF_KEYUP = 2;

        CommonUtil.keybd_event(Keys.ControlKey, 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(KeyInterop.VirtualKeyFromKey(Key.LeftAlt), 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(KeyInterop.VirtualKeyFromKey(Key.RightAlt), 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(Keys.LWin, 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(Keys.RWin, 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(Keys.ShiftKey, 0, KEYEVENTF_KEYUP, 0);

        CommonUtil.keybd_event(Keys.ControlKey, 0, 0, 0);
        CommonUtil.keybd_event(isCopy ? Keys.C : Keys.V, 0, 0, 0);
        CommonUtil.keybd_event(isCopy ? Keys.C : Keys.V, 0, KEYEVENTF_KEYUP, 0);
        CommonUtil.keybd_event(Keys.ControlKey, 0, KEYEVENTF_KEYUP, 0);// 'Left Control Up

        Thread.Sleep(interval);
    }

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("User32.dll", SetLastError = true)]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("Kernel32.dll", SetLastError = true)]
    private static extern int GlobalSize(IntPtr hMem);
}
