using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;

namespace STranslate.Util
{
    public class BitmapUtil
    {
        /// <summary>
        /// 图像变成背景
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static ImageBrush ConvertBitmap2ImageBrush(Bitmap bmp)
        {
            ImageBrush brush = new ImageBrush();
            IntPtr hBitmap = bmp.GetHbitmap();
            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions()
            );
            brush.ImageSource = wpfBitmap;
            return brush;
        }

        /// <summary>
        /// Bitmap转为BitmapSource
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapSource ConvertBitmap2BitmapSource(Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap(); //obtain the Hbitmap
            BitmapSource? bitmapSource = null;
            try
            {
                bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }
            finally
            {
                CommonUtil.DeleteObject(ptr); //release the HBitmap
            }
            return bitmapSource;
        }

        /// <summary>
        /// BitmapSource转为Bitmap
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Bitmap ConvertBitmapSource2Bitmap(BitmapSource source)
        {
            using var stream = new MemoryStream();
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);
            var bmp = new Bitmap(stream);
            return bmp;
        }

        /// <summary>
        /// BitmapSource转为byte[]
        /// </summary>
        /// <param name="bitmapSource"></param>
        /// <returns></returns>
        public static byte[] ConvertBitmapSource2Bytes(BitmapSource bitmapSource)
        {
            // 可根据需要选择其他编码器
            BitmapEncoder encoder = new BmpBitmapEncoder();
            // 将BitmapSource转换为byte[]
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using MemoryStream stream = new MemoryStream();
            encoder.Save(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Bitmap转byte
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] ConvertBitmap2Bytes(Bitmap bitmap)
        {
            using MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Bmp);
            byte[] data = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(data, 0, Convert.ToInt32(stream.Length));
            return data;
        }

        /// <summary>
        /// byte转BitmapSource
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapSource ConvertBytes2BitmapSource(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            stream.Position = 0;

            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = stream;
            img.EndInit();
            img.Freeze();
            return img;
        }

        /// <summary>
        /// 是否为图片文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsImageFile(string filePath)
        {
            // 根据文件扩展名检查文件是否为图片文件
            string extension = Path.GetExtension(filePath).ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp";
        }
    }
}
