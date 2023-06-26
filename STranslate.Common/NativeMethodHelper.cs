using System;
using System.Runtime.InteropServices;
using System.Text;

namespace STranslate.Helper
{
    internal class NativeMethodHelper
    {
        /// <summary>
        /// 获取进程句柄
        /// </summary>
        /// <param name="lpModuleName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// 设置窗口在最前端
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// 获取最前端的窗口
        /// </summary>
        /// <returns>窗口句柄</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// 模拟触发键盘的按键
        /// </summary>
        /// <param name="vk">按下的键</param>
        /// <param name="bScan"></param>
        /// <param name="dwFlags">触发的方式，0按下，2抬起</param>
        /// <param name="dwExtraInfo"></param>
        [DllImport("user32.dll")]
        public static extern void keybd_event(System.Windows.Forms.Keys vk, byte bScan, uint dwFlags, uint dwExtraInfo);

        /// <summary>
        /// 模拟触发键盘的按键
        /// </summary>
        /// <param name="vk">按下的键</param>
        /// <param name="bScan"></param>
        /// <param name="dwFlags">触发的方式，0按下，2抬起</param>
        /// <param name="dwExtraInfo"></param>
        [DllImport("user32.dll")]
        public static extern void keybd_event(int vk, byte bScan, uint dwFlags, uint dwExtraInfo);

        /// <summary>
        /// 注册全局热键
        /// </summary>
        /// <param name="hWnd">要定义热键的窗口的句柄</param>
        /// <param name="id">定义热键ID（不能与其它ID重复，全局唯一）</param>
        /// <param name="fsModifiers">标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效</param>
        /// <param name="vk">定义热键的内容</param>
        /// <returns>成功，返回值不为0，失败，返回值为0。要得到扩展错误信息，调用GetLastError</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, byte fsModifiers, int vk);

        /// <summary>
        /// 取消注册全局热键
        /// </summary>
        /// <param name="hWnd">要取消热键的窗口的句柄</param>
        /// <param name="id">要取消热键的ID</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// 加载鼠标样式从文件中
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursorFromFile(string fileName);

        /// <summary>
        /// 设置鼠标样式
        /// </summary>
        /// <param name="cursorHandle"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SetCursor(IntPtr cursorHandle);

        /// <summary>
        /// 销毁鼠标样式
        /// </summary>
        /// <param name="cursorHandle"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern uint DestroyCursor(IntPtr cursorHandle);

        /// <summary>
        /// 隐藏焦点，隐藏光标闪烁
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32", EntryPoint = "HideCaret")]
        public static extern bool HideCaret(IntPtr hWnd);

        [DllImport("kernel32.dll")]
        public static extern bool SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);

        /// <summary>
        /// 打开剪切板
        /// </summary>
        /// <param name="hWndNewOwner"></param>
        /// <returns></returns>
        [DllImport("User32")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        /// <summary>
        /// 关闭剪切板
        /// </summary>
        /// <returns></returns>
        [DllImport("User32")]
        internal static extern bool CloseClipboard();

        /// <summary>
        /// 清空剪切板
        /// </summary>
        /// <returns></returns>
        [DllImport("User32")]
        internal static extern bool EmptyClipboard();

        /// <summary>
        /// 剪切板格式化的数据是否可用
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        [DllImport("User32")]
        internal static extern bool IsClipboardFormatAvailable(int format);

        /// <summary>
        /// 获取剪切板数据
        /// </summary>
        /// <param name="uFormat"></param>
        /// <returns></returns>
        [DllImport("User32")]
        internal static extern IntPtr GetClipboardData(int uFormat);

        /// <summary>
        /// 设置剪切板数据
        /// </summary>
        /// <param name="uFormat"></param>
        /// <param name="hMem"></param>
        /// <returns></returns>
        [DllImport("User32", CharSet = CharSet.Unicode)]
        internal static extern IntPtr SetClipboardData(int uFormat, IntPtr hMem);

        private const int HORZRES = 8;
        private const int VERTRES = 10;
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;
        private const int DESKTOPVERTRES = 117;
        private const int DESKTOPHORZRES = 118;

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(
        IntPtr hdc, // handle to DC
        int nIndex // index of capability
        );

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        /// <summary>
        /// 获取系统dpi
        /// </summary>
        /// <returns></returns>
        public static double GetDpi()
        {
            double dDpi = 1;
            IntPtr desktopDc = GetDC(IntPtr.Zero);
            float horizontalDPI = GetDeviceCaps(desktopDc, LOGPIXELSX);
            float verticalDPI = GetDeviceCaps(desktopDc, LOGPIXELSY);
            int dpi = (int)(horizontalDPI + verticalDPI) / 2;
            dDpi = 1 + ((dpi - 96) / 24) * 0.25;
            if (dDpi < 1)
            {
                dDpi = 1;
            }
            ReleaseDC(IntPtr.Zero, desktopDc);
            return dDpi;
        }

        /// <summary>
        /// 获取窗口标题
        /// </summary>
        /// <returns></returns>
        [DllImport("user32", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// 获取类的名字
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// 根据坐标获取窗口句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("user32")]
        public static extern IntPtr WindowFromPoint(System.Drawing.Point Point);

        /// <summary>
        /// 窗口置顶与取消置顶
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hPos, int x, int y, int cx, int cy, uint nflags);

        internal enum KeyModifiers
        {
            MOD_NONE = 0x0,
            MOD_ALT = 0x1,
            MOD_CTRL = 0x2,
            MOD_SHIFT = 0x4,
            MOD_WIN = 0x8
        }

        #region Clipboard

        internal static void SetText(string text)
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                SetText(text);
                return;
            }
            EmptyClipboard();
            SetClipboardData(13, Marshal.StringToHGlobalUni(text));
            CloseClipboard();
        }

        internal static string GetText()
        {
            string value = string.Empty;
            OpenClipboard(IntPtr.Zero);
            if (IsClipboardFormatAvailable(13))
            {
                IntPtr ptr = GetClipboardData(13);
                if (ptr != IntPtr.Zero)
                {
                    value = Marshal.PtrToStringUni(ptr);
                }
            }
            CloseClipboard();
            return value;
        }

        #endregion Clipboard
    }
}
