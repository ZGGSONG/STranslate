using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using static STranslate.Helper.NativeMethodHelper;

namespace STranslate.Helper
{

    internal class HotkeysHelper
    {
        public static IntPtr mainFormHandle;

        public static int InputTranslateId = 854;
        public static byte InputTranslateModifiers;
        public static int InputTranslateKey;
        public static int CrosswordTranslateId = 855;
        public static byte CrosswordTranslateModifiers;
        public static int CrosswordTranslateKey;
#if true
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
        /// <summary>
        /// 注册快捷键
        /// https://git2.nas.zggsong.cn:5001/zggsong/STranslate/src/commit/2fe17e7f6596b47b33a40d8733a0527ba5d9b2fb/STranslate/Utils/HotKeysUtil.cs
        /// </summary>
        /// <param name="id">InputTranslateId、ScreenShotTranslateId、CrosswordTranslateId、OpenMainWindowId</param>
        /// <param name="callBack"></param>
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
        private static void RegisterHotKey(IntPtr mainFormHandle)
        {
            HotkeysHelper.mainFormHandle = mainFormHandle;

            InputTranslateModifiers = ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Modifiers;
            InputTranslateKey = ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Key;
            CrosswordTranslateModifiers = ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Modifiers;
            CrosswordTranslateKey = ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Key;

#if true
            ScreenShotTranslateModifiers = ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Modifiers;
            ScreenShotTranslateKey = ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Key;
            if (ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Key != 0)
            {
                ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, ScreenShotTranslateId, ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Modifiers, ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Key);
            }
#endif

            if (ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Key != 0)
            {
                ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, InputTranslateId, ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Modifiers, ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Key);
            }
            if (ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Key != 0)
            {
                ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, CrosswordTranslateId, ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Modifiers, ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Key);
            }
            if (ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Key != 0)
            {
                ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, OpenMainWindowId, ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Modifiers, ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Key);
            }
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        public static void UnRegisterHotKey()
        {
            UnregisterHotKey(mainFormHandle, InputTranslateId);
            UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
#if true
            UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
#endif
            UnregisterHotKey(mainFormHandle, OpenMainWindowId);
        }

        /// <summary>
        /// 重新注册快捷键
        /// </summary>
        public static void ReRegisterHotKey()
        {
            if (ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, InputTranslateId);
            }
            else if (InputTranslateModifiers != ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Modifiers || InputTranslateKey != ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Key)
            {
                {
                    UnregisterHotKey(mainFormHandle, InputTranslateId);
                    ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, InputTranslateId, ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Modifiers, ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Key);
                }
            }
            InputTranslateModifiers = ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Modifiers;
            InputTranslateKey = ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Key;

            if (ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
            }
            else if (CrosswordTranslateModifiers != ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Modifiers || CrosswordTranslateKey != ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Key)
            {
                {
                    UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
                    ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, CrosswordTranslateId, ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Modifiers, ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Key);
                }
            }
            CrosswordTranslateModifiers = ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Modifiers;
            CrosswordTranslateKey = ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Key;

#if true
            if (ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
            }
            else if (ScreenShotTranslateModifiers != ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Modifiers || ScreenShotTranslateKey != ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Key)
            {
                UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
                ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, ScreenShotTranslateId, ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Modifiers, ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Key);
            }
            ScreenShotTranslateModifiers = ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Modifiers;
            ScreenShotTranslateKey = ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Key;
#endif

            if (ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, OpenMainWindowId);
            }
            else if (OpenMainWindowModifiers != ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Modifiers || OpenMainWindowKey != ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Key)
            {
                UnregisterHotKey(mainFormHandle, OpenMainWindowId);
                ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Conflict = !NativeMethodHelper.RegisterHotKey(mainFormHandle, OpenMainWindowId, ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Modifiers, ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Key);
            }
            OpenMainWindowModifiers = ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Modifiers;
            OpenMainWindowKey = ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Key;
        }
    }
}