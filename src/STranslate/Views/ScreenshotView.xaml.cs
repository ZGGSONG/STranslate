using STranslate.Helper;
using STranslate.Util;
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Point = System.Windows.Point;

namespace STranslate.Views
{
    public partial class ScreenshotView : Window
    {
        public ScreenshotView()
        {
            InitializeComponent();

            _rectangle = new();
            var ms = System.Windows.Forms.Control.MousePosition;
            var screen = WpfScreenHelper.Screen.AllScreens.FirstOrDefault(screen => screen.Bounds.Contains(new Point(ms.X, ms.Y)));
            if (screen == null)
            {
                Log.LogService.Logger.Error("未获取到屏幕数据");
                return;
            }

            Rect bounds = screen.WpfBounds;
            _dpiScale = screen.ScaleFactor;
            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.UnconventionalScreen ?? false)
                bounds = new Rect((int)(bounds.X * _dpiScale), (int)(bounds.Y * _dpiScale), (int)(bounds.Width * _dpiScale), (int)(bounds.Height * _dpiScale));

            Top = bounds.X;
            Left = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;

            Canvas.SetLeft(this, bounds.X);
            Canvas.SetTop(this, bounds.Y);
            LeftMask.Width = bounds.Width;
            LeftMask.Height = bounds.Height;

            _bitmap = new Bitmap((int)(bounds.Width * _dpiScale), (int)(bounds.Height * _dpiScale));
            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.CopyFromScreen(
                    (int)(bounds.X * _dpiScale),
                    (int)(bounds.Y * _dpiScale),
                    0,
                    0,
                    new System.Drawing.Size((int)(bounds.Width * _dpiScale), (int)(bounds.Height * _dpiScale)),
                    CopyPixelOperation.SourceCopy
                );
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
            Bitmap bmpOut = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
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
                LeftMask.Height = _bitmap!.Height;

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
        private Bitmap? _bitmap; // 截屏图片
        private double _dpiScale = 1; //缩放比例
    }
}
