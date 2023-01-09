using STranslate.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STranslate.ViewModel
{
    public class ScreenShotVM : BaseVM
    {
        /// <summary>
        /// reference https://github.com/NPCDW/WpfTool
        /// </summary>
        /// <param name="ui"></param>
        public ScreenShotVM(Window ui)
        {
            _ScreenShotWin = ui;
            EscCmd = new RelayCommand((_) => true, (_) =>
            {
                _ScreenShotWin.Close();
            });

            //鼠标移动
            MouseMoveCmd = new RelayCommand((_) => true, (_) =>
              {
                  if (MouseDown)
                  {
                      System.Windows.Point CurrentPoint = Mouse.GetPosition(_ScreenShotWin);
                      Rectangle = new Rect(StartPoint, CurrentPoint);

                      Canvas.SetLeft(_ScreenShotWin.FindName("LeftMask") as System.Windows.Shapes.Rectangle, 0);
                      Canvas.SetTop(_ScreenShotWin.FindName("LeftMask") as System.Windows.Shapes.Rectangle, 0);
                      (_ScreenShotWin.FindName("LeftMask") as System.Windows.Shapes.Rectangle).Width = Rectangle.X;
                      (_ScreenShotWin.FindName("LeftMask") as System.Windows.Shapes.Rectangle).Height = bitmap.Height;

                      Canvas.SetLeft(_ScreenShotWin.FindName("RightMask") as System.Windows.Shapes.Rectangle, Rectangle.Left + Rectangle.Width);
                      Canvas.SetTop(_ScreenShotWin.FindName("RightMask") as System.Windows.Shapes.Rectangle, 0);
                      (_ScreenShotWin.FindName("RightMask") as System.Windows.Shapes.Rectangle).Width = bitmap.Width - Rectangle.Left - Rectangle.Width;
                      (_ScreenShotWin.FindName("RightMask") as System.Windows.Shapes.Rectangle).Height = bitmap.Height;

                      Canvas.SetLeft(_ScreenShotWin.FindName("UpMask") as System.Windows.Shapes.Rectangle, Rectangle.Left);
                      Canvas.SetTop(_ScreenShotWin.FindName("UpMask") as System.Windows.Shapes.Rectangle, 0);
                      (_ScreenShotWin.FindName("UpMask") as System.Windows.Shapes.Rectangle).Width = Rectangle.Width;
                      (_ScreenShotWin.FindName("UpMask") as System.Windows.Shapes.Rectangle).Height = Rectangle.Y;

                      Canvas.SetLeft(_ScreenShotWin.FindName("DownMask") as System.Windows.Shapes.Rectangle, Rectangle.Left);
                      Canvas.SetTop(_ScreenShotWin.FindName("DownMask") as System.Windows.Shapes.Rectangle, Rectangle.Y + Rectangle.Height);
                      (_ScreenShotWin.FindName("DownMask") as System.Windows.Shapes.Rectangle).Width = Rectangle.Width;
                      (_ScreenShotWin.FindName("DownMask") as System.Windows.Shapes.Rectangle).Height = bitmap.Height - Rectangle.Height - Rectangle.Y;
                  }
              });

            //左键Down
            MouseLeftDownCmd = new RelayCommand((_) => true, (_) =>
              {
                  MouseDown = true;
                  StartPoint = Mouse.GetPosition(_ScreenShotWin);
              });

            //左键Up
            MouseLeftUpCmd = new RelayCommand((_) => true, (_) =>
              {
                  MouseDown = false;

                  int x = (int)(Rectangle.X * dpiScale);
                  int y = (int)(Rectangle.Y * dpiScale);
                  int width = (int)(Rectangle.Width * dpiScale);
                  int height = (int)(Rectangle.Height * dpiScale);
                  if (width <= 0 || height <= 0)
                  {
                      return;
                  }
                  Bitmap bmpOut = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                  Graphics g = Graphics.FromImage(bmpOut);
                  g.DrawImage(bitmap,
                      new Rectangle(0, 0, width, height),
                      new Rectangle(x, y, width, height),
                      GraphicsUnit.Pixel);

                  _ScreenShotWin.Close();

                  
                  TestSaveBmp(bmpOut);
              });

            ClosedCmd = new RelayCommand((_) => true, (_) =>
              {
                  Util.Util.FlushMemory();
              });
        }

        private void TestSaveBmp(Bitmap bmp)
        {
            bmp.Save("D:\\a.png", System.Drawing.Imaging.ImageFormat.Bmp);
        }

        public Tuple<Rect, int, int, int, int> InitView1()
        {
            // 获取鼠标所在屏幕
            System.Drawing.Point ms = System.Windows.Forms.Control.MousePosition;
            Rect bounds = new Rect();
            int x = 0, y = 0, width = 0, height = 0;
            foreach (WpfScreenHelper.Screen screen in WpfScreenHelper.Screen.AllScreens)
            {
                bounds = screen.WpfBounds;
                dpiScale = screen.ScaleFactor;
                x = (int)(bounds.X * dpiScale);
                y = (int)(bounds.Y * dpiScale);
                width = (int)(bounds.Width * dpiScale);
                height = (int)(bounds.Height * dpiScale);
                if (x <= ms.X && ms.X < x + width && y <= ms.Y && ms.Y < y + height)
                {
                    break;
                }
            }
            return new Tuple<Rect, int, int, int, int>(bounds, x, y, width, height);
        }
        public void InitView2(Tuple<Rect, int, int, int, int> tuple)
        {
            // 设置窗体位置、大小（实际宽高，单位unit）
            _ScreenShotWin.Top = tuple.Item1.X;
            _ScreenShotWin.Left = tuple.Item1.Y;
            _ScreenShotWin.Width = tuple.Item1.Width;
            _ScreenShotWin.Height = tuple.Item1.Height;

            // 设置遮罩
            Canvas.SetLeft(_ScreenShotWin, tuple.Item1.X);
            Canvas.SetTop(_ScreenShotWin, tuple.Item1.Y);
            (_ScreenShotWin.FindName("LeftMask") as System.Windows.Shapes.Rectangle).Width = tuple.Item1.Width;
            (_ScreenShotWin.FindName("LeftMask") as System.Windows.Shapes.Rectangle).Height = tuple.Item1.Height;

            // 设置窗体背景（像素宽高，单位px）
            bitmap = new Bitmap(tuple.Item4, tuple.Item5);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(tuple.Item2, tuple.Item3, 0, 0, new System.Drawing.Size(tuple.Item4, tuple.Item5), CopyPixelOperation.SourceCopy);
            }
            _ScreenShotWin.Background = Util.Util.BitmapToImageBrush(bitmap);
        }

        public ICommand EscCmd { get; private set; }
        public ICommand MouseMoveCmd { get; private set; }
        public ICommand MouseLeftDownCmd { get; private set; }
        public ICommand MouseLeftUpCmd { get; private set; }
        public ICommand ClosedCmd { get; private set; }

        private Window _ScreenShotWin;              //Window
        private Rect Rectangle = new Rect();        //保存的矩形
        private System.Windows.Point StartPoint;    //鼠标按下的点
        private bool MouseDown;                     //鼠标是否被按下
        private Bitmap bitmap;                      // 截屏图片
        private double dpiScale = 1;

    }
}
