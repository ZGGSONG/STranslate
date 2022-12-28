using System;
using System.Threading;
using System.Windows.Input;

namespace STranslate.Helper
{
    public class GetWordsHelper
    {
        public static String Get()
        {
            SendCtrlC();
            Thread.Sleep(200);
            return NativeMethodHelper.GetText();
        }

        private static void SendCtrlC()
        {
            //IntPtr hWnd = GetForegroundWindow();
            //SetForegroundWindow(hWnd);
            uint KEYEVENTF_KEYUP = 2;

            NativeMethodHelper.keybd_event(System.Windows.Forms.Keys.ControlKey, 0, KEYEVENTF_KEYUP, 0);
            NativeMethodHelper.keybd_event(KeyInterop.VirtualKeyFromKey(Key.LeftAlt), 0, KEYEVENTF_KEYUP, 0);
            NativeMethodHelper.keybd_event(KeyInterop.VirtualKeyFromKey(Key.RightAlt), 0, KEYEVENTF_KEYUP, 0);
            NativeMethodHelper.keybd_event(System.Windows.Forms.Keys.LWin, 0, KEYEVENTF_KEYUP, 0);
            NativeMethodHelper.keybd_event(System.Windows.Forms.Keys.RWin, 0, KEYEVENTF_KEYUP, 0);
            NativeMethodHelper.keybd_event(System.Windows.Forms.Keys.ShiftKey, 0, KEYEVENTF_KEYUP, 0);

            NativeMethodHelper.keybd_event(System.Windows.Forms.Keys.ControlKey, 0, 0, 0);
            NativeMethodHelper.keybd_event(System.Windows.Forms.Keys.C, 0, 0, 0);
            NativeMethodHelper.keybd_event(System.Windows.Forms.Keys.C, 0, KEYEVENTF_KEYUP, 0);
            NativeMethodHelper.keybd_event(System.Windows.Forms.Keys.ControlKey, 0, KEYEVENTF_KEYUP, 0);// 'Left Control Up
        }
    }
}
