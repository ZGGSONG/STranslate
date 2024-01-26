using STranslate.Util;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using STranslate.Helper;
using Point = System.Windows.Point;

namespace STranslate.Views
{
    /// <summary>
    /// ScreenshotView.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenshotView : Window
    {
        public ScreenshotView()
        {
            // 初始化
            _rectangle = new();

            // 获取鼠标所在屏幕
            System.Drawing.Point ms = System.Windows.Forms.Control.MousePosition;
            Rect bounds = new Rect();
            int x = 0;
            int y = 0;
            int width = 0;
            int height = 0;

            foreach (WpfScreenHelper.Screen screen in WpfScreenHelper.Screen.AllScreens)
            {
                bounds = screen.WpfBounds;
                _dpiScale = screen.ScaleFactor;
                x = (int)(bounds.X * _dpiScale);
                y = (int)(bounds.Y * _dpiScale);
                width = (int)(bounds.Width * _dpiScale);
                height = (int)(bounds.Height * _dpiScale);

                //目前就发现在非16:9的多显示器上出现该问题，尝试解决该问题
                if (Singleton<ConfigHelper>.Instance.CurrentConfig?.UnconventionalScreen ?? false)
                    bounds = new Rect(x, y, width, height);
                if (x <= ms.X && ms.X < x + width && y <= ms.Y && ms.Y < y + height)
                {
                    break;
                }
            }

            InitializeComponent();

            // 设置窗体位置、大小（实际宽高，单位unit）
            Top = bounds.X;
            Left = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;

            // 设置遮罩
            Canvas.SetLeft(this, bounds.X);
            Canvas.SetTop(this, bounds.Y);
            LeftMask.Width = bounds.Width;
            LeftMask.Height = bounds.Height;

            // 设置窗体背景（像素宽高，单位px）
            _bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
            }
            Background = BitmapUtil.ConvertBitmap2ImageBrush(_bitmap);
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;

            int x = (int)(_rectangle.X * _dpiScale);
            int y = (int)(_rectangle.Y * _dpiScale);
            int width = (int)(_rectangle.Width * _dpiScale);
            int height = (int)(_rectangle.Height * _dpiScale);
            if (width <= 0 || height <= 0)
            {
                return;
            }
            Bitmap bmpOut = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmpOut);
            if (_bitmap != null)
                g.DrawImage(_bitmap, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);

            Close();

            BitmapCallback?.Invoke(bmpOut);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            _startPoint = Mouse.GetPosition(this);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                Point CurrentPoint = Mouse.GetPosition(this);
                _rectangle = new Rect(_startPoint, CurrentPoint);

                Canvas.SetLeft(LeftMask, 0);
                Canvas.SetTop(LeftMask, 0);
                LeftMask.Width = _rectangle.X;
                LeftMask.Height = _bitmap.Height;

                Canvas.SetLeft(RightMask, _rectangle.Left + _rectangle.Width);
                Canvas.SetTop(RightMask, 0);
                RightMask.Width = _bitmap.Width - _rectangle.Left - _rectangle.Width;
                RightMask.Height = _bitmap.Height;

                Canvas.SetLeft(UpMask, _rectangle.Left);
                Canvas.SetTop(UpMask, 0);
                UpMask.Width = _rectangle.Width;
                UpMask.Height = _rectangle.Y;

                Canvas.SetLeft(DownMask, _rectangle.Left);
                Canvas.SetTop(DownMask, _rectangle.Y + _rectangle.Height);
                DownMask.Width = _rectangle.Width;
                DownMask.Height = _bitmap.Height - _rectangle.Height - _rectangle.Y;
            }
        }

        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                Close();
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            MemoUtil.FlushMemory();
        }

        public Action<Bitmap>? BitmapCallback;
        private Rect _rectangle; //保存的矩形
        private Point _startPoint; //鼠标按下的点
        private bool _isMouseDown; //鼠标是否被按下
        private Bitmap _bitmap; // 截屏图片
        private double _dpiScale = 1; //缩放比例
    }
}
