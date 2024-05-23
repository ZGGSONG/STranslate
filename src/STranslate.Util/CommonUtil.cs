using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using STranslate.Model;

namespace STranslate.Util
{
    public class CommonUtil
    {
        #region NativeMethod

        #region 鼠标Hook

        /// <summary>
        /// 获取进程句柄
        /// </summary>
        /// <param name="lpModuleName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        #endregion 鼠标Hook

        /// <summary>
        /// 设置窗口在最前端
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <returns></returns>
        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// 获取最前端的窗口
        /// </summary>
        /// <returns>窗口句柄</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// 获取窗口是否可见
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

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

        /// <summary>
        /// 释放掉对象
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        [DllImport("gdi32")]
        public static extern int DeleteObject(IntPtr obj);

        //监听剪贴板变化
        public const int WM_DRAWCLIPBOARD = 0x0308;

        public const int WM_CHANGECBCHAIN = 0x030D;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        //监听剪贴板变化

        #endregion NativeMethod

        #region FindControl

        /// <summary>
        /// 在UserControl中查找控件的通用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T? FindControlByName<T>(DependencyObject parent, string name)
            where T : FrameworkElement
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T frameworkElement && frameworkElement.Name == name)
                {
                    return frameworkElement;
                }

                T? result = FindControlByName<T>(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 在窗口中查找UserControl的通用方法
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static UserControl? FindUserControlByName(DependencyObject parent, string name)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is UserControl userControl && userControl.Name == name)
                {
                    return userControl;
                }

                UserControl? result = FindUserControlByName(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        #endregion FindControl

        #region Other

        /// <summary>
        /// 使用UI线程执行
        /// </summary>
        /// <param name="action"></param>
        public static void InvokeOnUIThread(Action action)
        {
            if (Application.Current?.Dispatcher is null)
                Dispatcher.CurrentDispatcher.BeginInvoke(action, new object[0]);
            else
                Application.Current?.Dispatcher.BeginInvoke(action);
        }

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
        /// 执行程序
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool ExecuteProgram(string filename, string[] args)
        {
            try
            {
                string arguments = "";
                foreach (string arg in args)
                {
                    arguments += $"\"{arg}\" ";
                }
                arguments = arguments.Trim();
                Process process = new();
                ProcessStartInfo startInfo = new(filename, arguments);
                process.StartInfo = startInfo;
                process.Start();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 枚举信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<string, T> GetEnumList<T>()
            where T : Enum
        {
            var dict = new Dictionary<string, T>();
            List<T> list = Enum.GetValues(typeof(T)).OfType<T>().ToList();
            list.ForEach(x =>
            {
                dict.Add(x.GetDescription(), x);
            });
            return dict;
        }

        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        /// <returns></returns>
        public static Tuple<Point, Rect> GetPositionInfos()
        {
            //获取未进行缩放的position信息
            var ms = System.Windows.Forms.Control.MousePosition;
            //原始数据是否在原始分辨率的屏幕内
            var screen = WpfScreenHelper.Screen.AllScreens.FirstOrDefault(s => s.Bounds.Contains(new Point(ms.X, ms.Y)));

            if (screen != null)
            {
                //获取缩放比例
                double dpiScale = screen.ScaleFactor;
                //获取处理后的屏幕数据
                var bounds = screen.WpfBounds;
                //返回处理后的数据
                return new(new Point(ms.X / dpiScale, ms.Y / dpiScale), bounds);
            }

            throw new ArgumentNullException();
        }

        /// <summary>
        /// 是否为管理员权限
        /// </summary>
        /// <returns></returns>
        public static bool IsUserAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// 计算文件大小函数(保留两位小数),Size为字节大小
        /// </summary>
        /// <param name="Size">初始文件大小</param>
        /// <returns></returns>
        public static string CountSize(long Size)
        {
            string result = "";
            long FactSize = 0;
            FactSize = Size;
            if (FactSize < 1024.00)
                result = FactSize.ToString("F2") + " Byte";
            else if (FactSize >= 1024.00 && FactSize < 1048576)
                result = (FactSize / 1024.00).ToString("F2") + " KB";
            else if (FactSize >= 1048576 && FactSize < 1073741824)
                result = (FactSize / 1024.00 / 1024.00).ToString("F2") + " MB";
            else if (FactSize >= 1073741824)
                result = (FactSize / 1024.00 / 1024.00 / 1024.00).ToString("F2") + " GB";
            return result;
        }

        #endregion Other
    }
}
