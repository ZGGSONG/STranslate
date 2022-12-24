using System;
using static STranslate.Utils.NativeMethod;

namespace STranslate.Utils
{
    public static class HotKeys
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

        public static class ScreenShotTranslate
        {
            public static byte Modifiers = (byte)KeyModifiers.MOD_ALT;
            public static int Key = 83;
            public static String Text = "S";
            public static bool Conflict = false;
        }

        public static class OpenMainWindow
        {
            public static byte Modifiers = (byte)KeyModifiers.MOD_ALT;
            public static int Key = 71;
            public static String Text = "G";
            public static bool Conflict = false;
        }
    }

    internal class HotKeysUtil
    {
        public static IntPtr mainFormHandle;

        public static int InputTranslateId = 854;
        public static byte InputTranslateModifiers;
        public static int InputTranslateKey;
        public static int CrosswordTranslateId = 855;
        public static byte CrosswordTranslateModifiers;
        public static int CrosswordTranslateKey;
        public static int ScreenShotTranslateId = 856;
        public static byte ScreenShotTranslateModifiers;
        public static int ScreenShotTranslateKey;
        public static int OpenMainWindowId = 857;
        public static byte OpenMainWindowModifiers;
        public static int OpenMainWindowKey;

        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="mainFormHandle"></param>
        public static void RegisterHotKey(IntPtr mainFormHandle)
        {
            HotKeysUtil.mainFormHandle = mainFormHandle;

            InputTranslateModifiers = HotKeys.InputTranslate.Modifiers;
            InputTranslateKey = HotKeys.InputTranslate.Key;
            CrosswordTranslateModifiers = HotKeys.CrosswordTranslate.Modifiers;
            CrosswordTranslateKey = HotKeys.CrosswordTranslate.Key;
            ScreenShotTranslateModifiers = HotKeys.ScreenShotTranslate.Modifiers;
            ScreenShotTranslateKey = HotKeys.ScreenShotTranslate.Key;

            if (HotKeys.InputTranslate.Key != 0)
            {
                HotKeys.InputTranslate.Conflict = !NativeMethod.RegisterHotKey(mainFormHandle, InputTranslateId, HotKeys.InputTranslate.Modifiers, HotKeys.InputTranslate.Key);
            }
            if (HotKeys.CrosswordTranslate.Key != 0)
            {
                HotKeys.CrosswordTranslate.Conflict = !NativeMethod.RegisterHotKey(mainFormHandle, CrosswordTranslateId, HotKeys.CrosswordTranslate.Modifiers, HotKeys.CrosswordTranslate.Key);
            }
            if (HotKeys.ScreenShotTranslate.Key != 0)
            {
                HotKeys.ScreenShotTranslate.Conflict = !NativeMethod.RegisterHotKey(mainFormHandle, ScreenShotTranslateId, HotKeys.ScreenShotTranslate.Modifiers, HotKeys.ScreenShotTranslate.Key);
            }
            if (HotKeys.OpenMainWindow.Key != 0)
            {
                HotKeys.OpenMainWindow.Conflict = !NativeMethod.RegisterHotKey(mainFormHandle, OpenMainWindowId, HotKeys.OpenMainWindow.Modifiers, HotKeys.OpenMainWindow.Key);
            }
        }

        /// <summary>
        /// 注销快捷键
        /// </summary>
        public static void UnRegisterHotKey()
        {
            UnregisterHotKey(mainFormHandle, InputTranslateId);
            UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
            UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
            UnregisterHotKey(mainFormHandle, OpenMainWindowId);
        }

        /// <summary>
        /// 重新注册快捷键
        /// </summary>
        public static void ReRegisterHotKey()
        {
            if (HotKeys.InputTranslate.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, InputTranslateId);
            }
            else if (InputTranslateModifiers != HotKeys.InputTranslate.Modifiers || InputTranslateKey != HotKeys.InputTranslate.Key)
            {
                {
                    UnregisterHotKey(mainFormHandle, InputTranslateId);
                    HotKeys.InputTranslate.Conflict = !NativeMethod.RegisterHotKey(mainFormHandle, InputTranslateId, HotKeys.InputTranslate.Modifiers, HotKeys.InputTranslate.Key);
                }
            }
            InputTranslateModifiers = HotKeys.InputTranslate.Modifiers;
            InputTranslateKey = HotKeys.InputTranslate.Key;

            if (HotKeys.CrosswordTranslate.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
            }
            else if (CrosswordTranslateModifiers != HotKeys.CrosswordTranslate.Modifiers || CrosswordTranslateKey != HotKeys.CrosswordTranslate.Key)
            {
                {
                    UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
                    HotKeys.CrosswordTranslate.Conflict = !NativeMethod.RegisterHotKey(mainFormHandle, CrosswordTranslateId, HotKeys.CrosswordTranslate.Modifiers, HotKeys.CrosswordTranslate.Key);
                }
            }
            CrosswordTranslateModifiers = HotKeys.CrosswordTranslate.Modifiers;
            CrosswordTranslateKey = HotKeys.CrosswordTranslate.Key;

            if (HotKeys.ScreenShotTranslate.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
            }
            else if (ScreenShotTranslateModifiers != HotKeys.ScreenShotTranslate.Modifiers || ScreenShotTranslateKey != HotKeys.ScreenShotTranslate.Key)
            {
                UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
                HotKeys.ScreenShotTranslate.Conflict = !NativeMethod.RegisterHotKey(mainFormHandle, ScreenShotTranslateId, HotKeys.ScreenShotTranslate.Modifiers, HotKeys.ScreenShotTranslate.Key);
            }
            ScreenShotTranslateModifiers = HotKeys.ScreenShotTranslate.Modifiers;
            ScreenShotTranslateKey = HotKeys.ScreenShotTranslate.Key;

            if (HotKeys.OpenMainWindow.Key == 0)
            {
                UnregisterHotKey(mainFormHandle, OpenMainWindowId);
            }
            else if (OpenMainWindowModifiers != HotKeys.OpenMainWindow.Modifiers || OpenMainWindowKey != HotKeys.OpenMainWindow.Key)
            {
                UnregisterHotKey(mainFormHandle, OpenMainWindowId);
                HotKeys.OpenMainWindow.Conflict = !NativeMethod.RegisterHotKey(mainFormHandle, OpenMainWindowId, HotKeys.OpenMainWindow.Modifiers, HotKeys.OpenMainWindow.Key);
            }
            OpenMainWindowModifiers = HotKeys.OpenMainWindow.Modifiers;
            OpenMainWindowKey = HotKeys.OpenMainWindow.Key;
        }
    }
}