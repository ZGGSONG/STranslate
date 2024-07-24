using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace STranslate.Util;

public class BitmapUtil
{
    /// <summary>
    ///     图像变成背景
    /// </summary>
    /// <param name="bmp"></param>
    /// <returns></returns>
    public static ImageBrush ConvertBitmap2ImageBrush(Bitmap bmp)
    {
        var brush = new ImageBrush();
        var hBitmap = bmp.GetHbitmap();
        ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
            hBitmap,
            IntPtr.Zero,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions()
        );
        brush.ImageSource = wpfBitmap;
        return brush;
    }

    /// <summary>
    ///     Bitmap转为BitmapSource
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public static BitmapSource ConvertBitmap2BitmapSource(Bitmap bitmap)
    {
        var ptr = bitmap.GetHbitmap(); //obtain the Hbitmap
        BitmapSource? bitmapSource = null;
        try
        {
            bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(ptr, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            CommonUtil.DeleteObject(ptr); //release the HBitmap
        }

        return bitmapSource;
    }

    /// <summary>
    ///     BitmapSource转为Bitmap
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static Bitmap ConvertBitmapSource2Bitmap(BitmapSource source)
    {
        using var stream = new MemoryStream();
        var encoder = new BmpBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(source));
        encoder.Save(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var bmp = new Bitmap(stream);
        return bmp;
    }

    /// <summary>
    ///     BitmapSource转为byte[]
    /// </summary>
    /// <param name="bitmapSource"></param>
    /// <returns></returns>
    public static byte[] ConvertBitmapSource2Bytes(BitmapSource bitmapSource)
    {
        // 可根据需要选择其他编码器
        BitmapEncoder encoder = new BmpBitmapEncoder();
        // 将BitmapSource转换为byte[]
        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    /// <summary>
    ///     Bitmap转byte
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public static byte[] ConvertBitmap2Bytes(Bitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Bmp);
        var data = new byte[stream.Length];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(data, 0, Convert.ToInt32(stream.Length));
        return data;
    }

    /// <summary>
    ///     byte转BitmapSource
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
    ///     是否为图片文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool IsImageFile(string filePath)
    {
        // 根据文件扩展名检查文件是否为图片文件
        var extension = Path.GetExtension(filePath).ToLower();
        return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".bmp";
    }

    /// <summary>
    ///     通过FileStream 来打开文件，这样就可以实现不锁定Image文件，到时可以让多用户同时访问Image文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Bitmap? ReadImageFile(string path)
    {
        if (!File.Exists(path)) return null; // 文件不存在

        using var fs = File.OpenRead(path);
        var result = Image.FromStream(fs); // 从流中创建图像
        return new Bitmap(result); // 创建并返回位图
    }

    public static byte[] BytesToBase64StringBytes(byte[] bytes)
    {
        var str = Convert.ToBase64String(bytes);
        return Encoding.UTF8.GetBytes(str);
    }
}