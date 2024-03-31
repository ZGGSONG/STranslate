using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Base;
using STranslate.ViewModels.Preference;
using STranslate.Views;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace STranslate.ViewModels
{
    public partial class OCRViewModel : WindowVMBase
    {
        /// <summary>
        /// 原始数据
        /// </summary>
        [ObservableProperty]
        private BitmapSource? _bs;

        /// <summary>
        /// 显示数据
        /// </summary>
        [ObservableProperty]
        private BitmapSource? _getImg;

        [ObservableProperty]
        private string _getContent = "";

        [ObservableProperty]
        private string _isTopMost = ConstStr.TAGFALSE;

        [ObservableProperty]
        private string _topMostContent = ConstStr.UNTOPMOSTCONTENT;

        /// <summary>
        /// 点击置顶按钮
        /// </summary>
        /// <param name="obj"></param>
        [RelayCommand]
        private void Sticky(Window win)
        {
            var tmp = !win.Topmost;
            IsTopMost = tmp ? ConstStr.TAGTRUE : ConstStr.TAGFALSE;
            TopMostContent = tmp ? ConstStr.TOPMOSTCONTENT : ConstStr.UNTOPMOSTCONTENT;
            win.Topmost = tmp;

            if (tmp)
            {
                ToastHelper.Show("启用置顶", WindowType.OCR);
            }
            else
            {
                ToastHelper.Show("关闭置顶", WindowType.OCR);
            }
        }

        /// <summary>
        /// 重置字体大小
        /// </summary>
        [RelayCommand]
        private void ResetFontsize() => Application.Current.Resources["FontSize_TextBox"] = 18.0;

        public override void Close(Window win)
        {
            win.Topmost = false;
            IsTopMost = ConstStr.TAGFALSE;
            TopMostContent = ConstStr.UNTOPMOSTCONTENT;
            base.Close(win);
        }

        [RelayCommand]
        private void CopyImg()
        {
            if (Bs is not null)
            {
                Clipboard.SetImage(Bs);

                ToastHelper.Show("复制图片", WindowType.OCR);
            }
        }

        [RelayCommand]
        private void SaveImg()
        {
            if (Bs is not null)
            {
                // 创建一个 SaveFileDialog
                SaveFileDialog saveFileDialog =
                    new()
                    {
                        Title = "Save Image",
                        Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|All Files (*.*)|*.*",
                        FileName = $"{DateTime.Now:yyyyMMddHHmmssfff}",
                        DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        AddToRecent = true,
                    };
                // 打开 SaveFileDialog，并获取用户选择的文件路径
                if (saveFileDialog.ShowDialog() == true)
                {
                    var fileName = saveFileDialog.FileName;
                    // 根据文件扩展名选择图像格式
                    BitmapEncoder encoder;
                    if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        encoder = new PngBitmapEncoder();
                    }
                    else
                    {
                        encoder = new JpegBitmapEncoder();
                    }

                    // 将 BitmapSource 添加到 BitmapEncoder
                    encoder.Frames.Add(BitmapFrame.Create(Bs));

                    // 使用 FileStream 保存到文件
                    using FileStream fs = new(fileName, FileMode.Create);
                    encoder.Save(fs);

                    ToastHelper.Show("保存图片成功", WindowType.OCR);
                }
                else
                {
                    ToastHelper.Show("取消保存图片", WindowType.OCR);
                }
            }
        }

        [RelayCommand]
        private void Copy(string? content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                Clipboard.SetDataObject(content);

                ToastHelper.Show("复制成功", WindowType.OCR);
            }
        }

        [RelayCommand]
        private void RemoveLineBreaks(TextBox textBox)
        {
            var oldTxt = textBox.Text;
            var newTxt = StringUtil.RemoveLineBreaks(oldTxt);
            if (string.Equals(oldTxt, newTxt))
                return;

            ToastHelper.Show("移除换行", WindowType.OCR);

            textBox.SelectAll();
            textBox.SelectedText = newTxt;
        }

        [RelayCommand]
        private void RemoveSpace(TextBox textBox)
        {
            var oldTxt = textBox.Text;
            var newTxt = StringUtil.RemoveSpace(oldTxt);
            if (string.Equals(oldTxt, newTxt))
                return;

            ToastHelper.Show("移除空格", WindowType.OCR);

            //https://stackoverflow.com/questions/4476282/how-can-i-undo-a-textboxs-text-changes-caused-by-a-binding
            textBox.SelectAll();
            textBox.SelectedText = newTxt;
        }

        [RelayCommand]
        private async Task DropAsync(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // 取第一个文件
                string filePath = files[0];

                // 检查文件类型，确保是图片文件
                if (BitmapUtil.IsImageFile(filePath))
                {
                    await ImgFileHandlerAsync(filePath);
                }
                else
                {
                    //MessageBox_S.Show("请选择图片(*.png|*.jpg|*.jpeg|*.bmp)");
                    ToastHelper.Show("请选择图片", WindowType.OCR);
                }
            }
        }

        [RelayCommand]
        private async Task OpenfileAsync()
        {
            var openfileDialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg",
                RestoreDirectory = true,
            };
            if (openfileDialog.ShowDialog() == true)
            {
                await ImgFileHandlerAsync(openfileDialog.FileName);
            }
        }

        [RelayCommand]
        private void Screenshot(Window view)
        {
            if (IsTopMost == "False")
            {
                view.Hide();
                view.Close();
                Thread.Sleep(200);
            }
            Singleton<NotifyIconViewModel>.Instance.OCRCommand.Execute(null);
        }

        [RelayCommand]
        private async Task ClipboardImgAsync()
        {
            var img = Clipboard.GetImage();
            if (img != null)
            {
                var bytes = BitmapUtil.ConvertBitmapSource2Bytes(img);

                //TODO: 很奇怪的现象，获取出来直接赋值给前台绑定的Img不显示，转成Byte再转回来就可以显示了
                Bs = BitmapUtil.ConvertBytes2BitmapSource(bytes);
                GetImg = Bs.Clone();

                await OCRHandler(bytes);

                return;
            }

            ToastHelper.Show("剪贴板最近无图片", WindowType.OCR);
        }

        private async Task ImgFileHandlerAsync(string file)
        {
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            var bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);

            Bs = BitmapUtil.ConvertBytes2BitmapSource(bytes);
            GetImg = Bs.Clone();

            await OCRHandler(bytes);
        }

        /// <summary>
        /// 重新识别
        /// </summary>
        [RelayCommand]
        private async Task RecertificationAsync()
        {
            if (Bs == null) return;

            var bytes = BitmapUtil.ConvertBitmapSource2Bytes(Bs);

            await OCRHandler(bytes);
        }

        private async Task OCRHandler(byte[] bytes)
        {
            try
            {
                //清空
                GetContent = "";
                ToastHelper.Show("识别中...", WindowType.OCR);
                var ocrResult = await Singleton<OCRScvViewModel>.Instance.ExecuteAsync(bytes, WindowType.OCR);
                var getText = ocrResult.Text;
                //更新图片
                // 创建一个WritableBitmap，用于绘制
                var writableBitmap = new WriteableBitmap(Bs!);
                // 使用锁定位图来确保线程安全
                writableBitmap.Lock();

                try
                {
                    var backBitmap = new System.Drawing.Bitmap((int)Bs!.Width, (int)Bs!.Height, writableBitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, writableBitmap.BackBuffer);
                    // 在这里你可以直接通过指针操作位图的像素数据
                    using var g = System.Drawing.Graphics.FromImage(backBitmap);
                    foreach (var item in ocrResult.OcrContents)
                    {
                        g.DrawPolygon(new System.Drawing.Pen(System.Drawing.Brushes.Red, 2), item.BoxPoints.Select(x => new System.Drawing.PointF(x.X, x.Y)).ToArray());
                    }
                }
                finally
                {
                    // 确保释放位图
                    writableBitmap.Unlock();
                }

                // 更新UI
                GetImg = writableBitmap;

                //取词前移除换行
                getText = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false && !string.IsNullOrEmpty(getText)
                        ? StringUtil.RemoveLineBreaks(getText)
                        : getText;
                //OCR后自动复制
                if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsOcrAutoCopyText ?? false && !string.IsNullOrEmpty(getText))
                    Clipboard.SetDataObject(getText, true);

                GetContent = getText;
                ToastHelper.Show("识别成功", WindowType.OCR);
            }
            catch (Exception ex)
            {
                GetContent = ex.Message;
                LogService.Logger.Error("OCR失败", ex);
            }
        }

        /// <summary>
        /// 识别二维码
        /// </summary>
        [RelayCommand]
        private void QRCode()
        {
            if (Bs == null) return;
            var reader = new ZXing.ZKWeb.BarcodeReader();
            reader.Options.CharacterSet = "UTF-8";
            using var stream = new MemoryStream();
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(Bs));
            encoder.Save(stream);
            var map = new System.DrawingCore.Bitmap(stream);
            var readerResult = reader.Decode(map);
            if (readerResult != null)
            {
                GetContent = readerResult.Text;
                ToastHelper.Show("二维码识别成功", WindowType.OCR);
            }
            else
            {
                ToastHelper.Show("未识别到二维码", WindowType.OCR);
            }
        }

        /// <summary>
        /// 翻译
        /// </summary>
        [RelayCommand]
        private void Translate(System.Collections.Generic.List<object> obj)
        {
            var content = obj.FirstOrDefault() as string ?? "";
            var ocrView = obj.LastOrDefault() as Window;

            //OCR结果翻译关闭界面
            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.CloseUIOcrRetTranslate ?? false)
                ocrView?.Close();

            //如果重复执行先取消上一步操作
            Singleton<OutputViewModel>.Instance.SingleTranslateCancelCommand.Execute(null);
            Singleton<InputViewModel>.Instance.TranslateCancelCommand.Execute(null);
            //增量翻译
            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IncrementalTranslation ?? false)
            {
                var input = Singleton<InputViewModel>.Instance.InputContent;
                Singleton<InputViewModel>.Instance.InputContent = string.IsNullOrEmpty(input) ? string.Empty : input + " ";
            }
            else
            {
                Singleton<InputViewModel>.Instance.Clear();
            }
            //清空输出相关
            Singleton<OutputViewModel>.Instance.Clear();

            //获取主窗口
            MainView window = Application.Current.Windows.OfType<MainView>().FirstOrDefault()!;
            window.ViewAnimation();
            window.Activate();

            //获取文本
            Singleton<InputViewModel>.Instance.InputContent += content;
            //执行翻译
            Singleton<InputViewModel>.Instance.TranslateCommand.Execute(null);
        }

        [RelayCommand]
        private void HotkeyCopy()
        {
            if (string.IsNullOrEmpty(GetContent))
                return;

            Clipboard.SetDataObject(GetContent, true);
            ToastHelper.Show("复制成功", WindowType.OCR);
        }

        #region ContextMenu

        [RelayCommand]
        private void TBSelectAll(object obj)
        {
            if (obj is TextBox tb)
            {
                tb.SelectAll();
            }
        }

        [RelayCommand]
        private void TBCopy(object obj)
        {
            if (obj is TextBox tb)
            {
                var text = tb.SelectedText;
                if (!string.IsNullOrEmpty(text))
                    Clipboard.SetDataObject(text);
            }
        }

        [RelayCommand]
        private void TBPaste(object obj)
        {
            if (obj is TextBox tb)
            {
                var getText = Clipboard.GetText();

                //剪贴板内容为空或者为非字符串则不处理
                if (string.IsNullOrEmpty(getText))
                    return;
                var index = tb.SelectionStart;
                //处理选中字符串
                var selectLength = tb.SelectionLength;
                //删除选中文本再粘贴
                var preHandleStr = tb.Text.Remove(index, selectLength);

                var newText = preHandleStr.Insert(index, getText);
                tb.Text = newText;

                // 重新定位光标索引
                tb.SelectionStart = index + getText.Length;

                // 聚焦光标
                tb.Focus();
            }
        }

        [RelayCommand]
        private void TBClear(object obj)
        {
            if (obj is TextBox tb)
            {
                tb.Clear();
            }
        }

        #endregion ContextMenu
    }
}
