using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace STranslate.Utils
{
    /// <summary>
    /// 引用自 https://zhuanlan.zhihu.com/p/445050708
    /// </summary>
    static class HotkeysUtil
    {
        #region 系统api
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, HotkeyModifiers fsModifiers, uint vk);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        #endregion

        public static IntPtr hwnd;
        public static void InitialHook(Window window)
        {
            hwnd = new WindowInteropHelper(window).Handle;
            var _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource.AddHook(WndProc);
        }
        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="window">持有快捷键窗口</param>
        /// <param name="fsModifiers">组合键</param>
        /// <param name="key">快捷键</param>
        /// <param name="callBack">回调函数</param>
        public static void Regist(HotkeyModifiers fsModifiers, Key key, HotKeyCallBackHanlder callBack)
        {
            int id = keyid++;

            var vk = KeyInterop.VirtualKeyFromKey(key);
            if (!RegisterHotKey(hwnd, id, fsModifiers, (uint)vk))
                throw new Exception("regist hotkey fail.");
            keymap[id] = callBack;
        }

        /// <summary>
        /// 快捷键消息处理
        /// </summary>
        static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (keymap.TryGetValue(id, out var callback))
                {
                    callback();
                }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        /// <param name="hWnd">持有快捷键窗口的句柄</param>
        /// <param name="callBack">回调函数</param>
        public static void UnRegist(IntPtr hWnd, HotKeyCallBackHanlder callBack)
        {
            foreach (KeyValuePair<int, HotKeyCallBackHanlder> var in keymap)
            {
                if (var.Value == callBack)
                    UnregisterHotKey(hWnd, var.Key);
            }
        }


        const int WM_HOTKEY = 0x312;
        static int keyid = 10;
        static Dictionary<int, HotKeyCallBackHanlder> keymap = new Dictionary<int, HotKeyCallBackHanlder>();

        public delegate void HotKeyCallBackHanlder();
    }

    enum HotkeyModifiers
    {
        MOD_ALT = 0x1,
        MOD_CONTROL = 0x2,
        MOD_SHIFT = 0x4,
        MOD_WIN = 0x8
    }
    enum MyHotkeys
    {
        KEY_A = Key.A,
        KEY_S = Key.S,
        KEY_D = Key.D,
    }

}
