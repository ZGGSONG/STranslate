using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STranslate.Views
{
    /// <summary>
    /// PreferenceView.xaml 的交互逻辑
    /// </summary>
    public partial class PreferenceView : Window
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        //Windows消息: https://www.cnblogs.com/cncc/articles/8004771.html
        /// <summary>
        /// 执行系统命令，如移动、最小化、最大化
        /// </summary>
        private const int WM_SYSCOMMAND = 0x0112;

        /// <summary>
        /// 移动窗口的系统命令
        /// </summary>
        private const int SC_MOVE = 0xF010;

        /// <summary>
        /// 一个窗口被销毁
        /// </summary>
        private const int WM_DESTROY = 0x0002;

        /// <summary>
        /// 空消息
        /// </summary>
        private const int WM_NULL = 0x0000;


        public PreferenceView()
        {
            InitializeComponent();
#if false
            Topmost = true;
#endif
        }

        /// <summary>
        /// 左键按住拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            ReleaseCapture();
            SendMessage(wndHelper.Handle, WM_SYSCOMMAND, SC_MOVE + WM_DESTROY, WM_NULL);
        }

        /// <summary>
        /// 双击最大/恢复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Maximized;
            }
        }
    }
}
