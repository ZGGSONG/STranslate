using STranslate.Model;
using System;

namespace STranslate.Utils
{
    internal class HotKeysUtil2
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
        public static void RegisterHotKey(IntPtr mainFormHandle)
        {
            HotKeysUtil2.mainFormHandle = mainFormHandle;

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
        }

        public static void UnRegisterHotKey()
        {
            NativeMethod.UnregisterHotKey(mainFormHandle, InputTranslateId);
            NativeMethod.UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
            NativeMethod.UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
        }

        public static void ReRegisterHotKey()
        {
            if (HotKeys.InputTranslate.Key == 0)
            {
                NativeMethod.UnregisterHotKey(mainFormHandle, InputTranslateId);
            }
            else if (InputTranslateModifiers != HotKeys.InputTranslate.Modifiers || InputTranslateKey != HotKeys.InputTranslate.Key)
            {
                {
                    NativeMethod.UnregisterHotKey(mainFormHandle, InputTranslateId);
                    HotKeys.InputTranslate.Conflict = !NativeMethod.RegisterHotKey(mainFormHandle, InputTranslateId, HotKeys.InputTranslate.Modifiers, HotKeys.InputTranslate.Key);
                }
            }
            InputTranslateModifiers = HotKeys.InputTranslate.Modifiers;
            InputTranslateKey = HotKeys.InputTranslate.Key;

            if (HotKeys.CrosswordTranslate.Key == 0)
            {
                NativeMethod.UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
            }
            else if (CrosswordTranslateModifiers != HotKeys.CrosswordTranslate.Modifiers || CrosswordTranslateKey != HotKeys.CrosswordTranslate.Key)
            {
                {
                    NativeMethod.UnregisterHotKey(mainFormHandle, CrosswordTranslateId);
                    HotKeys.CrosswordTranslate.Conflict = !NativeMethod.RegisterHotKey(mainFormHandle, CrosswordTranslateId, HotKeys.CrosswordTranslate.Modifiers, HotKeys.CrosswordTranslate.Key);
                }
            }
            CrosswordTranslateModifiers = HotKeys.CrosswordTranslate.Modifiers;
            CrosswordTranslateKey = HotKeys.CrosswordTranslate.Key;

            if (HotKeys.ScreenShotTranslate.Key == 0)
            {
                NativeMethod.UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
            }
            else if (ScreenShotTranslateModifiers != HotKeys.ScreenShotTranslate.Modifiers || ScreenShotTranslateKey != HotKeys.ScreenShotTranslate.Key)
            {
                NativeMethod.UnregisterHotKey(mainFormHandle, ScreenShotTranslateId);
                HotKeys.ScreenShotTranslate.Conflict = !NativeMethod.RegisterHotKey(mainFormHandle, ScreenShotTranslateId, HotKeys.ScreenShotTranslate.Modifiers, HotKeys.ScreenShotTranslate.Key);
            }
            ScreenShotTranslateModifiers = HotKeys.ScreenShotTranslate.Modifiers;
            ScreenShotTranslateKey = HotKeys.ScreenShotTranslate.Key;
        }
    }
}
