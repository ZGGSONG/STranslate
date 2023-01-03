using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using static STranslate.Helper.NativeMethodHelper;

namespace STranslate.Helper
{
    public static class Hotkeys
    {
        public static class InputTranslate
        {
            public static byte Modifiers = (byte)KeyModifiers.MOD_ALT;
            public static int Key = 65;
            public static String Text = "A";
            public static bool Conflict = false;
        }

        public static class CrosswordTranslate
        {
            public static byte Modifiers = (byte)KeyModifiers.MOD_ALT;
            public static int Key = 68;
            public static String Text = "D";
            public static bool Conflict = false;
        }

#if false
        public static class ScreenShotTranslate
        {
            public static byte Modifiers = (byte)KeyModifiers.MOD_ALT;
            public static int Key = 83;
            public static String Text = "S";
            public static bool Conflict = false;
        }
#endif
        public static class OpenMainWindow
        {
            public static byte Modifiers = (byte)KeyModifiers.MOD_ALT;
            public static int Key = 71;
            public static String Text = "G";
            public static bool Conflict = false;
        }
    }

    internal class HotkeysHelper
    {
        public static IntPtr mainFormHandle;

        public static int InputTranslateId = 854;
        public static byte InputTranslateModifiers;
        public static int InputTranslateKey;
        public static int CrosswordTranslateId = 855;
        public static byte CrosswordTranslateModifiers;
        public static int CrosswordTranslateKey;
#if false
        public static int ScreenShotTranslateId = 856;
        public static byte ScreenShotTranslateModifiers;
        public static int ScreenShotTranslateKey;
#endif
        public static int OpenMainWindowId = 857;
        public static byte OpenMainWindowModifiers;
        public static int OpenMainWindowKey;

        public delegate void HotKeyCallBackHanlder();
        private static Dictionary<int, HotKeyCallBackHanlder> keymap = new Dictionary<int, HotKeyCallBackHanlder>();

        public static void InitialHook(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            RegisterHotKey(hwnd);
            var _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource.AddHook(WndProc);
        }
        public static void Register(int id, HotKeyCallBackHanlder callBack)
        {
            keymap[id] = callBack;
        }
        /// <summary>
        /// 快捷键消息处理
        /// </summary>
        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0312: //这个是window消息定义的 注册的热键消息
                    int id = wParam.ToInt32();
                    if (keymap.TryGetValue(id, out var callback))
                    {
                        callback();
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="mainFormHandle"></param>
        public static void RegisterHotKey(IntPtr mainFormHandle)
        {
            HotkeysHelper.mainFormHandle = mainFormHandle;

            InputTranslateModifiers = Hotkeys.InputTranslate.Modifiers;
            InputTranslateKey = Hotkeys.InputTranslate.Key;
            CrosswordTranslateModifiers = Hotkeys.CrosswordTranslate.Modifiers;
            CrosswordTranslateKey = Hotkeys.CrosswordTranslate.Key;

#if false
            ScreenShotTranslateModifiers = Hotkeys.ScreenShotTranslate.Modifiers;
            ScreenShotTranslateKey = Hotkeys.ScreenShotTranslate.Key;
            if (Hotkeys.ScreenShotTranslate.Key != 0)
            {
                Hotkeys.ScreenShotTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, ScreenShotTranslateId, Hotkeys.ScreenShotTranslate.Modifiers, Hotkeys.ScreenShotTranslate.Key);
            }
#endif

            if (Hotkeys.InputTranslate.Key != 0)
            {
                Hotkeys.InputTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, InputTranslateId, Hotkeys.InputTranslate.Modifiers, Hotkeys.InputTranslate.Key);
            }
            if (Hotkeys.CrosswordTranslate.Key != 0)
            {
                Hotkeys.CrosswordTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, CrosswordTranslateId, Hotkeys.CrosswordTranslate.Modifiers, Hotkeys.CrosswordTranslate.Key);
            }
            if (Hotkeys.OpenMainWindow.Key != 0)
            {
                Hotkeys.OpenMainWindow.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, OpenMainWindowId, Hotkeys.OpenMainWindow.Modifiers, Hotkeys.OpenMainWindow.Key);
            }
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        public static void UnRegisterHotKey()
        {
            UnregisterHotKey(mainFormHandle, InputTranslateId);
            UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
#if false
            UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
#endif
            UnregisterHotKey(mainFormHandle, OpenMainWindowId);
        }

        /// <summary>
        /// 重新注册快捷键
        /// </summary>
        public static void ReRegisterHotKey()
        {
            if (Hotkeys.InputTranslate.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, InputTranslateId);
            }
            else if (InputTranslateModifiers != Hotkeys.InputTranslate.Modifiers || InputTranslateKey != Hotkeys.InputTranslate.Key)
            {
                {
                    UnregisterHotKey(mainFormHandle, InputTranslateId);
                    Hotkeys.InputTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, InputTranslateId, Hotkeys.InputTranslate.Modifiers, Hotkeys.InputTranslate.Key);
                }
            }
            InputTranslateModifiers = Hotkeys.InputTranslate.Modifiers;
            InputTranslateKey = Hotkeys.InputTranslate.Key;

            if (Hotkeys.CrosswordTranslate.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
            }
            else if (CrosswordTranslateModifiers != Hotkeys.CrosswordTranslate.Modifiers || CrosswordTranslateKey != Hotkeys.CrosswordTranslate.Key)
            {
                {
                    UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
                    Hotkeys.CrosswordTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, CrosswordTranslateId, Hotkeys.CrosswordTranslate.Modifiers, Hotkeys.CrosswordTranslate.Key);
                }
            }
            CrosswordTranslateModifiers = Hotkeys.CrosswordTranslate.Modifiers;
            CrosswordTranslateKey = Hotkeys.CrosswordTranslate.Key;

#if false
            if (Hotkeys.ScreenShotTranslate.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
            }
            else if (ScreenShotTranslateModifiers != Hotkeys.ScreenShotTranslate.Modifiers || ScreenShotTranslateKey != Hotkeys.ScreenShotTranslate.Key)
            {
                UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
                Hotkeys.ScreenShotTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, ScreenShotTranslateId, Hotkeys.ScreenShotTranslate.Modifiers, Hotkeys.ScreenShotTranslate.Key);
            }
            ScreenShotTranslateModifiers = Hotkeys.ScreenShotTranslate.Modifiers;
            ScreenShotTranslateKey = Hotkeys.ScreenShotTranslate.Key;
#endif

            if (Hotkeys.OpenMainWindow.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, OpenMainWindowId);
            }
            else if (OpenMainWindowModifiers != Hotkeys.OpenMainWindow.Modifiers || OpenMainWindowKey != Hotkeys.OpenMainWindow.Key)
            {
                UnregisterHotKey(mainFormHandle, OpenMainWindowId);
                Hotkeys.OpenMainWindow.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, OpenMainWindowId, Hotkeys.OpenMainWindow.Modifiers, Hotkeys.OpenMainWindow.Key);
            }
            OpenMainWindowModifiers = Hotkeys.OpenMainWindow.Modifiers;
            OpenMainWindowKey = Hotkeys.OpenMainWindow.Key;
        }
    }
}