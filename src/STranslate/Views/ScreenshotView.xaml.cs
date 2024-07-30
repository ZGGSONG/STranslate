using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Util;
using Control = System.Windows.Forms.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Screen = WpfScreenHelper.Screen;
using Size = System.Drawing.Size;

namespace STranslate.Views;

[Obsolete]
public partial class ScreenshotView : Window
{
    private readonly Bitmap? _bitmap; // 截屏图片
    private Rect _bounds; //获取到的屏幕数据
    private readonly double _dpiScale = 1; //缩放比例
    private bool _isMouseDown; //鼠标是否被按下
    private Rect _rectangle; //保存的矩形
    private Point _startPoint; //鼠标按下的点

    public ScreenshotView()
    {
        InitializeComponent();
        _rectangle = new Rect();
        var ms = Control.MousePosition;
        var screen = Screen.AllScreens.FirstOrDefault(screen => screen.Bounds.Contains(new Point(ms.X, ms.Y)));
        if (screen == null)
        {
            LogService.Logger.Error("未获取到屏幕数据");
            return;
        }

        _bounds = screen.WpfBounds;

        if (Singleton<ConfigHelper>.Instance.CurrentConfig?.ShowAuxiliaryLine ?? true)
        {
            //设置辅助线宽高
            HorizontalLine.X1 = 0;
            HorizontalLine.X2 = _bounds.Width;
            VerticalLine.Y1 = 0;
            VerticalLine.Y2 = _bounds.Height;
        }

        _dpiScale = screen.ScaleFactor;

        Top = _bounds.X;
        Left = _bounds.Y;
        Width = _bounds.Width;
        Height = _bounds.Height;

        Canvas.SetLeft(this, _bounds.X);
        Canvas.SetTop(this, _bounds.Y);
        LeftMask.Width = _bounds.Width;
        LeftMask.Height = _bounds.Height;

        _bitmap = new Bitmap((int)(_bounds.Width * _dpiScale), (int)(_bounds.Height * _dpiScale));
        using (var g = Graphics.FromImage(_bitmap))
        {
            g.CopyFromScreen(
                (int)(_bounds.X * _dpiScale),
                (int)(_bounds.Y * _dpiScale),
                0,
                0,
                new Size((int)(_bounds.Width * _dpiScale), (int)(_bounds.Height * _dpiScale)),
                CopyPixelOperation.SourceCopy
            );
        }

        Background = BitmapUtil.ConvertBitmap2ImageBrush(_bitmap);
    }

    /// <summary>
    ///     打开时手动打调用触发事件
    /// </summary>
    public void InvokeCanOpen()
    {
        OnViewVisibilityChanged?.Invoke(false);
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = false;

        var x = (int)(_rectangle.X * _dpiScale);
        var y = (int)(_rectangle.Y * _dpiScale);
        var width = (int)(_rectangle.Width * _dpiScale);
        var height = (int)(_rectangle.Height * _dpiScale);
        if (width <= 0 || height <= 0) return;
        Bitmap bmpOut = new(width, height, PixelFormat.Format32bppArgb);
        var g = Graphics.FromImage(bmpOut);
        if (_bitmap != null)
            g.DrawImage(_bitmap, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height),
                GraphicsUnit.Pixel);

        OnViewVisibilityChanged?.Invoke(true);
        Close();

        var convertX = _bounds.X + (x + width) / _dpiScale;
        var convertY = _bounds.Y + y / _dpiScale;
        BitmapCallback?.Invoke(new Tuple<Bitmap, double, double>(bmpOut, convertX, convertY));
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = true;
        _startPoint = Mouse.GetPosition(this);
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        // Get the current mouse position relative to the canvas
        var currentPoint = e.GetPosition(this);

        if (!_isMouseDown)
        {
            // Update the horizontal line to match the mouse Y position
            HorizontalLine.Y1 = currentPoint.Y;
            HorizontalLine.Y2 = currentPoint.Y;

            // Update the vertical line to match the mouse X position
            VerticalLine.X1 = currentPoint.X;
            VerticalLine.X2 = currentPoint.X;
        }
        else
        {
            HorizontalLine.Visibility = Visibility.Collapsed;
            VerticalLine.Visibility = Visibility.Collapsed;

            _rectangle = new Rect(_startPoint, currentPoint);

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
        OnViewVisibilityChanged?.Invoke(true);
        Close();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape && e.Key != Key.Enter) return;
        OnViewVisibilityChanged?.Invoke(true);
        Close();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        MemoUtil.FlushMemory();
    }

    public event Action<bool>? OnViewVisibilityChanged;
    public event Action<Tuple<Bitmap, double, double>>? BitmapCallback;
}