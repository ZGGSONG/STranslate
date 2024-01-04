using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;

namespace STranslate.Helper
{
    public class HotkeyHelper
    {
        public static Hotkeys? Hotkeys;

        public static IntPtr MainIntPtr;

        public static int InputTranslateId = 854;
        public static KeyModifiers InputTranslateModifiers;
        public static KeyCodes InputTranslateKey;

        public static int CrosswordTranslateId = 855;
        public static KeyModifiers CrosswordTranslateModifiers;
        public static KeyCodes CrosswordTranslateKey;

        public static int ScreenShotTranslateId = 856;
        public static KeyModifiers ScreenShotTranslateModifiers;
        public static KeyCodes ScreenShotTranslateKey;

        public static int OpenMainWindowId = 857;
        public static KeyModifiers OpenMainWindowModifiers;
        public static KeyCodes OpenMainWindowKey;

        public static int MousehookTranslateId = 858;
        public static KeyModifiers MousehookTranslateModifiers;
        public static KeyCodes MousehookTranslateKey;

        public static int OCRId = 859;
        public static KeyModifiers OCRModifiers;
        public static KeyCodes OCRKey;

        public delegate void HotKeyCallBackHanlder();

        private static readonly Dictionary<int, HotKeyCallBackHanlder> keymap = new();

        private static HwndSource? _hwndSource;

        /// <summary>
        /// 初始化Hook
        /// </summary>
        /// <param name="window"></param>
        public static void InitialHook(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;

            RegisterHotKey(hwnd);
            _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource.AddHook(WndProc);
        }

        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callBack"></param>
        public static void Register(int id, HotKeyCallBackHanlder callBack)
        {
            keymap[id] = callBack;
        }

        /// <summary>
        /// 屏幕分辨率以及文本显示比例变更对应的消息标志
        /// </summary>
        private const int WmDisplayChange = 0x007e;

        /// <summary>
        /// 文本显示比例变更消息标志
        /// </summary>
        private const int WmTextChange = 0x02E0;

        /// <summary>
        /// window消息定义的 注册的热键消息标志
        /// </summary>
        private const int WmHotkeys = 0x0312;

        /// <summary>
        /// 快捷键消息处理
        /// </summary>
        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WmHotkeys:
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
        /// <param name="handle"></param>
        private static void RegisterHotKey(IntPtr handle)
        {
            MainIntPtr = handle;

            InputTranslateModifiers = Hotkeys!.InputTranslate.Modifiers;
            InputTranslateKey = Hotkeys!.InputTranslate.Key;

            CrosswordTranslateModifiers = Hotkeys!.CrosswordTranslate.Modifiers;
            CrosswordTranslateKey = Hotkeys!.CrosswordTranslate.Key;

            ScreenShotTranslateModifiers = Hotkeys!.ScreenShotTranslate.Modifiers;
            ScreenShotTranslateKey = Hotkeys!.ScreenShotTranslate.Key;

            OpenMainWindowModifiers = Hotkeys!.OpenMainWindow.Modifiers;
            OpenMainWindowKey = Hotkeys!.OpenMainWindow.Key;

            MousehookTranslateModifiers = Hotkeys!.MousehookTranslate.Modifiers;
            MousehookTranslateKey = Hotkeys!.MousehookTranslate.Key;

            OCRModifiers = Hotkeys!.OCR.Modifiers;
            OCRKey = Hotkeys!.OCR.Key;

            if (Hotkeys!.ScreenShotTranslate.Key != 0)
            {
                Hotkeys!.ScreenShotTranslate.Conflict = !CommonUtil.RegisterHotKey(
                    handle,
                    ScreenShotTranslateId,
                    (byte)Hotkeys!.ScreenShotTranslate.Modifiers,
                    (int)Hotkeys!.ScreenShotTranslate.Key
                );
            }

            if (Hotkeys!.InputTranslate.Key != 0)
            {
                Hotkeys!.InputTranslate.Conflict = !CommonUtil.RegisterHotKey(
                    handle,
                    InputTranslateId,
                    (byte)Hotkeys!.InputTranslate.Modifiers,
                    (int)Hotkeys!.InputTranslate.Key
                );
            }
            if (Hotkeys!.CrosswordTranslate.Key != 0)
            {
                Hotkeys!.CrosswordTranslate.Conflict = !CommonUtil.RegisterHotKey(
                    handle,
                    CrosswordTranslateId,
                    (byte)Hotkeys!.CrosswordTranslate.Modifiers,
                    (int)Hotkeys!.CrosswordTranslate.Key
                );
            }
            if (Hotkeys!.OpenMainWindow.Key != 0)
            {
                Hotkeys!.OpenMainWindow.Conflict = !CommonUtil.RegisterHotKey(
                    handle,
                    OpenMainWindowId,
                    (byte)Hotkeys!.OpenMainWindow.Modifiers,
                    (int)Hotkeys!.OpenMainWindow.Key
                );
            }
            if (Hotkeys!.MousehookTranslate.Key != 0)
            {
                Hotkeys!.MousehookTranslate.Conflict = !CommonUtil.RegisterHotKey(
                    handle,
                    MousehookTranslateId,
                    (byte)Hotkeys!.MousehookTranslate.Modifiers,
                    (int)Hotkeys!.MousehookTranslate.Key
                );
            }
            if (Hotkeys!.OCR.Key != 0)
            {
                Hotkeys!.OCR.Conflict = !CommonUtil.RegisterHotKey(
                    handle,
                    OCRId,
                    (byte)Hotkeys!.OCR.Modifiers,
                    (int)Hotkeys!.OCR.Key
                );
            }
        }

        /// <summary>
        /// 重新注册快捷键
        /// </summary>
        public static void ReRegisterHotKey()
        {
            if (Hotkeys!.InputTranslate.Key == 0)
            {
                CommonUtil.UnregisterHotKey(MainIntPtr, InputTranslateId);
            }
            else if (
                InputTranslateModifiers != Hotkeys!.InputTranslate.Modifiers
                || InputTranslateKey != Hotkeys!.InputTranslate.Key
            )
            {
                {
                    CommonUtil.UnregisterHotKey(MainIntPtr, InputTranslateId);
                    Hotkeys!.InputTranslate.Conflict = !CommonUtil.RegisterHotKey(
                        MainIntPtr,
                        InputTranslateId,
                        (byte)Hotkeys!.InputTranslate.Modifiers,
                        (int)Hotkeys!.InputTranslate.Key
                    );
                }
            }
            InputTranslateModifiers = Hotkeys!.InputTranslate.Modifiers;
            InputTranslateKey = Hotkeys!.InputTranslate.Key;

            if (Hotkeys!.CrosswordTranslate.Key == 0)
            {
                CommonUtil.UnregisterHotKey(MainIntPtr, CrosswordTranslateId);
            }
            else if (
                CrosswordTranslateModifiers != Hotkeys!.CrosswordTranslate.Modifiers
                || CrosswordTranslateKey != Hotkeys!.CrosswordTranslate.Key
            )
            {
                {
                    CommonUtil.UnregisterHotKey(MainIntPtr, CrosswordTranslateId);
                    Hotkeys!.CrosswordTranslate.Conflict = !CommonUtil.RegisterHotKey(
                        MainIntPtr,
                        CrosswordTranslateId,
                        (byte)Hotkeys!.CrosswordTranslate.Modifiers,
                        (int)Hotkeys!.CrosswordTranslate.Key
                    );
                }
            }
            CrosswordTranslateModifiers = Hotkeys!.CrosswordTranslate.Modifiers;
            CrosswordTranslateKey = Hotkeys!.CrosswordTranslate.Key;

            if (Hotkeys!.ScreenShotTranslate.Key == 0)
            {
                CommonUtil.UnregisterHotKey(MainIntPtr, ScreenShotTranslateId);
            }
            else if (
                ScreenShotTranslateModifiers != Hotkeys!.ScreenShotTranslate.Modifiers
                || ScreenShotTranslateKey != Hotkeys!.ScreenShotTranslate.Key
            )
            {
                CommonUtil.UnregisterHotKey(MainIntPtr, ScreenShotTranslateId);
                Hotkeys!.ScreenShotTranslate.Conflict = !CommonUtil.RegisterHotKey(
                    MainIntPtr,
                    ScreenShotTranslateId,
                    (byte)Hotkeys!.ScreenShotTranslate.Modifiers,
                    (int)Hotkeys!.ScreenShotTranslate.Key
                );
            }
            ScreenShotTranslateModifiers = Hotkeys!.ScreenShotTranslate.Modifiers;
            ScreenShotTranslateKey = Hotkeys!.ScreenShotTranslate.Key;

            if (Hotkeys!.OpenMainWindow.Key == 0)
            {
                CommonUtil.UnregisterHotKey(MainIntPtr, OpenMainWindowId);
            }
            else if (
                OpenMainWindowModifiers != Hotkeys!.OpenMainWindow.Modifiers
                || OpenMainWindowKey != Hotkeys!.OpenMainWindow.Key
            )
            {
                CommonUtil.UnregisterHotKey(MainIntPtr, OpenMainWindowId);
                Hotkeys!.OpenMainWindow.Conflict = !CommonUtil.RegisterHotKey(
                    MainIntPtr,
                    OpenMainWindowId,
                    (byte)Hotkeys!.OpenMainWindow.Modifiers,
                    (int)Hotkeys!.OpenMainWindow.Key
                );
            }
            OpenMainWindowModifiers = Hotkeys!.OpenMainWindow.Modifiers;
            OpenMainWindowKey = Hotkeys!.OpenMainWindow.Key;

            if (Hotkeys!.MousehookTranslate.Key == 0)
            {
                CommonUtil.UnregisterHotKey(MainIntPtr, MousehookTranslateId);
            }
            else if (
                MousehookTranslateModifiers != Hotkeys!.MousehookTranslate.Modifiers
                || MousehookTranslateKey != Hotkeys!.MousehookTranslate.Key
            )
            {
                CommonUtil.UnregisterHotKey(MainIntPtr, MousehookTranslateId);
                Hotkeys!.MousehookTranslate.Conflict = !CommonUtil.RegisterHotKey(
                    MainIntPtr,
                    MousehookTranslateId,
                    (byte)Hotkeys!.MousehookTranslate.Modifiers,
                    (int)Hotkeys!.MousehookTranslate.Key
                );
            }
            MousehookTranslateModifiers = Hotkeys!.MousehookTranslate.Modifiers;
            MousehookTranslateKey = Hotkeys!.MousehookTranslate.Key;

            if (Hotkeys!.OCR.Key == 0)
            {
                CommonUtil.UnregisterHotKey(MainIntPtr, OCRId);
            }
            else if (
                OCRModifiers != Hotkeys!.OCR.Modifiers
                || OCRKey != Hotkeys!.OCR.Key
            )
            {
                CommonUtil.UnregisterHotKey(MainIntPtr, OCRId);
                Hotkeys!.OCR.Conflict = !CommonUtil.RegisterHotKey(
                    MainIntPtr,
                    OCRId,
                    (byte)Hotkeys!.OCR.Modifiers,
                    (int)Hotkeys!.OCR.Key
                );
            }
            OCRModifiers = Hotkeys!.OCR.Modifiers;
            OCRKey = Hotkeys!.OCR.Key;
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        public static void UnRegisterHotKey(Window window)
        {
            CommonUtil.UnregisterHotKey(MainIntPtr, InputTranslateId);
            CommonUtil.UnregisterHotKey(MainIntPtr, CrosswordTranslateId);
            CommonUtil.UnregisterHotKey(MainIntPtr, ScreenShotTranslateId);
            CommonUtil.UnregisterHotKey(MainIntPtr, OpenMainWindowId);
            CommonUtil.UnregisterHotKey(MainIntPtr, MousehookTranslateId);
            CommonUtil.UnregisterHotKey(MainIntPtr, OCRId);
            var hwnd = new WindowInteropHelper(window).Handle;
            _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource.RemoveHook(WndProc);
        }
    }
}