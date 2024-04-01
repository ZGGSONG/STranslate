using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using STranslate.Util;
using STranslate.ViewModels;

namespace STranslate.Views
{
    /// <summary>
    /// OCRView.xaml 的交互逻辑
    /// </summary>
    public partial class OCRView : Window
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

        public OCRView()
        {
            InitializeComponent();

            DataContext = Singleton<OCRViewModel>.Instance;
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

        private void InputTb_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            // 检查是否按住 Ctrl 键，如果按住则进行缩放等特殊操作，否则进行滚动
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                double currentFontSize = (double)Application.Current.Resources["FontSize_TextBox"];

                // 根据滚轮事件更改字体大小
                if (e.Delta > 0)
                {
                    if (currentFontSize < 40)
                    {
                        currentFontSize += 2.0; // 根据需要调整放大的幅度
                    }
                }
                else
                {
                    if (currentFontSize > 10)
                    {
                        currentFontSize -= 2.0; // 根据需要调整放大的幅度
                    }
                }

                // 设置新的字体大小
                Application.Current.Resources["FontSize_TextBox"] = currentFontSize;
            }
            else //修复普通滚动
            {
                // 普通滚动
                if (e.Delta > 0)
                {
                    // 向上滚动
                    if (textBox.VerticalOffset > 0)
                    {
                        textBox.ScrollToVerticalOffset(textBox.VerticalOffset - 30);
                    }
                }
                else
                {
                    // 向下滚动
                    if (textBox.VerticalOffset < textBox.ExtentHeight - textBox.ViewportHeight)
                    {
                        textBox.ScrollToVerticalOffset(textBox.VerticalOffset + 30);
                    }
                }
            }

            // 防止事件继续传播
            e.Handled = true;
        }

        #region 鼠标缩放拖拽

        // https://www.cnblogs.com/snake-hand/archive/2012/08/13/2636227.html
        private bool mouseDown;

        private Point mouseXY;

        private void ContentControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ContentControl img)
                return;
            img.CaptureMouse();
            mouseDown = true;
            mouseXY = e.GetPosition(img);
        }

        private void ContentControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ContentControl img)
                return;
            img.ReleaseMouseCapture();
            mouseDown = false;
        }

        private void ContentControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not ContentControl img)
                return;
            if (mouseDown)
            {
                Domousemove(img, e);
            }
        }

        private void Domousemove(ContentControl img, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            var group = (TransformGroup)IMG.FindResource("Imageview");
            var transform = (TranslateTransform)group.Children[1];
            var position = e.GetPosition(img);
            transform.X -= mouseXY.X - position.X;
            transform.Y -= mouseXY.Y - position.Y;
            mouseXY = position;
        }

        private void ContentControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ContentControl img)
                return;
            var point = e.GetPosition(img);
            var group = (TransformGroup)IMG.FindResource("Imageview");
            var delta = e.Delta * 0.001;
            DowheelZoom(group, point, delta);
        }

        private void DowheelZoom(TransformGroup group, Point point, double delta)
        {
            var pointToContent = group.Inverse.Transform(point);
            var transform = (ScaleTransform)group.Children[0];
            if (transform.ScaleX + delta < 0.1)
                return;
            transform.ScaleX += delta;
            transform.ScaleY += delta;
            var transform1 = (TranslateTransform)group.Children[1];
            transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);
            transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);
        }

        #endregion 鼠标缩放拖拽
    }
}
