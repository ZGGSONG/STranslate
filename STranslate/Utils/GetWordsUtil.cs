using System;
using System.Threading;
using System.Windows;

namespace STranslate.Utils
{
    public class GetWords
    {
        public static String Get()
        {
            SendCtrlC();
            Thread.Sleep(200);
            return NativeMethod.GetText();
        }

        private static void SendCtrlC()
        {
            //IntPtr hWnd = GetForegroundWindow();
            //SetForegroundWindow(hWnd);
            uint KEYEVENTF_KEYUP = 2;
            NativeMethod.keybd_event(System.Windows.Forms.Keys.ControlKey, 0, 0, 0);
            NativeMethod.keybd_event(System.Windows.Forms.Keys.C, 0, 0, 0);
            NativeMethod.keybd_event(System.Windows.Forms.Keys.C, 0, KEYEVENTF_KEYUP, 0);
            NativeMethod.keybd_event(System.Windows.Forms.Keys.ControlKey, 0, KEYEVENTF_KEYUP, 0);// 'Left Control Up
        }

        private static String GetDataFromClipboard()
        {
            try
            {
                if (Clipboard.ContainsText()) //检查是否存在文本
                {
                    string res = Clipboard.GetText();
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        return res;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            return null;
        }
    }
}