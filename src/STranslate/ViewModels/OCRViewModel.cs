using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
using ZXing.ZKWeb;
using Brushes = System.Drawing.Brushes;
using Image = System.Windows.Controls.Image;
using Pen = System.Drawing.Pen;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;

namespace STranslate.ViewModels;

public partial class OCRViewModel : WindowVMBase
{
    /// <summary>
    ///     原始数据
    /// </summary>
    [ObservableProperty] private BitmapSource? _bs;

    [ObservableProperty] private string _getContent = "";

    /// <summary>
    ///     显示数据
    /// </summary>
    [ObservableProperty] private BitmapSource? _getImg;

    [ObservableProperty] private bool _isExecuting;

    [ObservableProperty] private string _isTopMost = ConstStr.TAGFALSE;

    /// <summary>
    ///     语言类型
    /// </summary>
    private LangEnum _lang = LangEnum.auto;

    [ObservableProperty]
    private double _ocrViewHeight = Singleton<ConfigHelper>.Instance.CurrentConfig?.OcrViewHeight ?? 400;

    [ObservableProperty]
    private double _ocrViewWidth = Singleton<ConfigHelper>.Instance.CurrentConfig?.OcrViewWidth ?? 1000;

    [ObservableProperty] private string _qrCodeContent = "";

    [ObservableProperty] private string _topMostContent = ConstStr.UNTOPMOSTCONTENT;

    public InputViewModel InputVm => Singleton<InputViewModel>.Instance;

    public OCRViewModel()
    {
        Singleton<NotifyIconViewModel>.Instance.OnExit += Save;
        OcrScvVm.OnChangeActivedOcrService += OnChangeOcrServiceorLang;
    }

    public OCRScvViewModel OcrScvVm => Singleton<OCRScvViewModel>.Instance;

    public LangEnum Lang
    {
        get => _lang;
        set
        {
            if (_lang == value)
                return;
            OnPropertyChanging();
            _lang = value;
            OnPropertyChanged();

            OnChangeOcrServiceorLang();
        }
    }

    private void OnChangeOcrServiceorLang()
    {
        if (Singleton<ConfigHelper>.Instance.CurrentConfig?.OcrChangedLang2Execute ?? false)
            Task.Run(() => CommonUtil.InvokeOnUIThread(async () => await RecertificationCommand.ExecuteAsync(null)));
    }

    private void Save()
    {
        if (!Singleton<ConfigHelper>.Instance.WriteOCRConfig(OcrViewHeight, OcrViewWidth))
            LogService.Logger.Warn($"保存OCRView失败，height: {OcrViewHeight}, width: {OcrViewWidth}");
    }

    /// <summary>
    ///     点击置顶按钮
    /// </summary>
    /// <param name="obj"></param>
    [RelayCommand]
    private void Sticky(Window win)
    {
        var tmp = !win.Topmost;
        IsTopMost = tmp ? ConstStr.TAGTRUE : ConstStr.TAGFALSE;
        TopMostContent = tmp ? ConstStr.TOPMOSTCONTENT : ConstStr.UNTOPMOSTCONTENT;
        win.Topmost = tmp;

        ToastHelper.Show(tmp ? "启用置顶" : "关闭置顶", WindowType.OCR);
    }

    /// <summary>
    ///     重置字体大小
    /// </summary>
    [RelayCommand]
    private void ResetFontSize()
    {
        Application.Current.Resources["FontSize_TextBox"] = 18.0;
    }

    public void FreeBitmapSource()
    {
        Bs?.Freeze();
        GetImg?.Freeze();
        Bs = null;
        GetImg = null;
        MemoUtil.FlushMemory();
    }

    public override void Close(Window win)
    {
        Save();
        win.Topmost = false;
        IsTopMost = ConstStr.TAGFALSE;
        TopMostContent = ConstStr.UNTOPMOSTCONTENT;

        RecertificationCancelCommand.Execute(null);
        DropCancelCommand.Execute(null);
        OpenfileCancelCommand.Execute(null);
        ClipboardImgCancelCommand.Execute(null);
        TTSCancelCommand.Execute(null);

        FreeBitmapSource();
        base.Close(win);
    }

    [RelayCommand]
    private void CopyImg()
    {
        if (Bs is null) return;
        Clipboard.SetImage(Bs);

        ToastHelper.Show("复制图片", WindowType.OCR);
    }

    [RelayCommand]
    private void SaveImg()
    {
        if (Bs is null) return;
        // 创建一个 SaveFileDialog
        SaveFileDialog saveFileDialog =
            new()
            {
                Title = "Save Image",
                Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|All Files (*.*)|*.*",
                FileName = $"{DateTime.Now:yyyyMMddHHmmssfff}",
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                AddToRecent = true
            };
        // 打开 SaveFileDialog，并获取用户选择的文件路径
        if (saveFileDialog.ShowDialog() == true)
        {
            var fileName = saveFileDialog.FileName;
            // 根据文件扩展名选择图像格式
            BitmapEncoder encoder;
            if (fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                encoder = new PngBitmapEncoder();
            else
                encoder = new JpegBitmapEncoder();

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

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task TTSAsync(string? content, CancellationToken token)
    {
        if (!string.IsNullOrEmpty(content))
            await Singleton<TTSViewModel>.Instance.SpeakTextAsync(content, WindowType.OCR, token);
    }

    [RelayCommand]
    private void Copy(string? content)
    {
        if (string.IsNullOrEmpty(content)) return;
        ClipboardHelper.Copy(content);

        ToastHelper.Show("复制成功", WindowType.OCR);
    }

    [RelayCommand]
    private void RemoveLineBreaks(TextBox textBox)
    {
        //根据Ctrl+LeftClick
        if ((Keyboard.Modifiers & ModifierKeys.Control) <= 0)
        {
            var oldTxt = textBox.Text;
            var newTxt = StringUtil.RemoveLineBreaks(oldTxt);
            if (string.Equals(oldTxt, newTxt))
                return;

            ToastHelper.Show("移除换行", WindowType.OCR);

            textBox.SelectAll();
            textBox.SelectedText = newTxt;
            return;
        }

        var vm = Singleton<CommonViewModel>.Instance;
        vm.IsRemoveLineBreakGettingWordsOCR = !vm.IsRemoveLineBreakGettingWordsOCR;
        vm.SaveCommand.Execute(null);
        ToastHelper.Show($"{(vm.IsRemoveLineBreakGettingWordsOCR ? "打开" : "关闭")}OCR始终移除换行", WindowType.OCR);
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

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task DropAsync(DragEventArgs e, CancellationToken token)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // 取第一个文件
            var filePath = files[0];

            // 检查文件类型，确保是图片文件
            if (BitmapUtil.IsImageFile(filePath))
                await ImgFileHandlerAsync(filePath, token);
            else
                //MessageBox_S.Show("请选择图片(*.png|*.jpg|*.jpeg|*.bmp)");
                ToastHelper.Show("请选择图片", WindowType.OCR);
        }
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task OpenfileAsync(CancellationToken token)
    {
        var openFileDialog = new OpenFileDialog
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg",
            RestoreDirectory = true
        };
        if (openFileDialog.ShowDialog() == true) await ImgFileHandlerAsync(openFileDialog.FileName, token);
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

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ClipboardImgAsync(CancellationToken token)
    {
        ClearQrContent();
        var img = Clipboard.GetImage();
        if (img != null)
        {
            var bytes = BitmapUtil.ConvertBitmapSource2Bytes(img);

            //TODO: 很奇怪的现象，获取出来直接赋值给前台绑定的Img不显示，转成Byte再转回来就可以显示了
            Bs = BitmapUtil.ConvertBytes2BitmapSource(bytes);
            GetImg = Bs.Clone();

            await OCRHandler(bytes, token);

            QrCodeContent = QrCodeHandler(Bs);

            return;
        }

        ToastHelper.Show("剪贴板最近无图片", WindowType.OCR);
    }

    private async Task ImgFileHandlerAsync(string file, CancellationToken token)
    {
        ClearQrContent();
        await using var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
        var bytes = new byte[fs.Length];
        _ = await fs.ReadAsync(bytes, 0, bytes.Length, token);

        Bs = BitmapUtil.ConvertBytes2BitmapSource(bytes);
        GetImg = Bs.Clone();

        await OCRHandler(bytes, token);

        QrCodeContent = QrCodeHandler(Bs);
    }

    /// <summary>
    ///     重新识别
    /// </summary>
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task RecertificationAsync(CancellationToken token)
    {
        ClearQrContent();

        if (Bs == null)
            return;

        //首先重置显示的内容为原始图片
        GetImg = Bs;

        var bytes = BitmapUtil.ConvertBitmapSource2Bytes(Bs);

        await OCRHandler(bytes, token);

        QrCodeContent = QrCodeHandler(Bs);
    }

    private async Task OCRHandler(byte[] bytes, CancellationToken? token = null)
    {
        try
        {
            IsExecuting = true;
            //清空
            GetContent = "";
            var ocrResult = await Singleton<OCRScvViewModel>.Instance.ExecuteAsync(bytes, WindowType.OCR, token, Lang);
            //判断结果
            if (!ocrResult.Success)
            {
                GetContent = "OCR失败: " + ocrResult.ErrorMsg;
                return;
            }

            //获取结果
            var getText = ocrResult.Text;

            //更新图片
            GetImg = GenerateImg(ocrResult, Bs!);

            //取词前移除换行
            getText =
                Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWordsOCR ??
                (false && !string.IsNullOrEmpty(getText))
                    ? StringUtil.RemoveLineBreaks(getText)
                    : getText;

            //OCR后自动复制
            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsOcrAutoCopyText ??
                (false && !string.IsNullOrEmpty(getText)))
                ClipboardHelper.Copy(getText);

            GetContent = getText;
        }
        catch (TaskCanceledException)
        {
            LogService.Logger.Debug("OCR操作取消");
        }
        catch (Exception ex)
        {
            GetContent = ex.Message;
            LogService.Logger.Error("OCR失败", ex);
        }
        finally
        {
            IsExecuting = false;
        }
    }

    /// <summary>
    ///     图像上生成位置信息
    /// </summary>
    /// <param name="ocrResult"></param>
    /// <param name="bitmapSource"></param>
    /// <returns></returns>
    private BitmapSource GenerateImg(OcrResult ocrResult, BitmapSource bitmapSource)
    {
        //没有位置信息的话返回原图
        if (ocrResult!.OcrContents.All(x => x.BoxPoints.Count == 0)) return bitmapSource;
        // 创建一个WritableBitmap，用于绘制
        var writableBitmap = new WriteableBitmap(bitmapSource);
        // 使用锁定位图来确保线程安全
        writableBitmap.Lock();

        try
        {
            var backBitmap = new Bitmap(
                (int)Bs!.Width,
                (int)Bs!.Height,
                writableBitmap.BackBufferStride,
                PixelFormat.Format32bppArgb,
                writableBitmap.BackBuffer
            );
            // 在这里你可以直接通过指针操作位图的像素数据
            using var g = Graphics.FromImage(backBitmap);
            foreach (var item in ocrResult.OcrContents)
                g.DrawPolygon(new Pen(Brushes.Red, 2), item.BoxPoints.Select(x => new PointF(x.X, x.Y)).ToArray());
        }
        finally
        {
            // 确保释放位图
            writableBitmap.Unlock();
        }

        return writableBitmap;
    }

    /// <summary>
    ///     识别二维码
    /// </summary>
    [RelayCommand]
    private void QRCode()
    {
        IsExecuting = true;
        ClearQrContent();

        if (Bs == null)
            return;
        var ret = QrCodeHandler(Bs);
        if (!string.IsNullOrEmpty(ret))
            GetContent = ret;
        else
            ToastHelper.Show("未识别到二维码", WindowType.OCR);

        IsExecuting = false;
    }

    private string QrCodeHandler(BitmapSource bs)
    {
        var ret = "";
        var reader = new BarcodeReader();
        reader.Options.CharacterSet = "UTF-8";
        using var stream = new MemoryStream();
        var encoder = new BmpBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bs));
        encoder.Save(stream);
        var map = new System.DrawingCore.Bitmap(stream);
        var readerResult = reader.Decode(map);
        if (readerResult != null) ret = readerResult.Text;
        return ret;
    }

    /// <summary>
    ///     翻译
    /// </summary>
    [RelayCommand]
    private void Translate(List<object> obj)
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
        var window = Application.Current.Windows.OfType<MainView>().First();
        AnimationHelper.MainViewAnimation();
        window.Activate();

        //获取文本
        Singleton<InputViewModel>
            .Instance
            .InputContent += content;
        //执行翻译
        Singleton<InputViewModel>.Instance.TranslateCommand.Execute(null);
    }

    [RelayCommand]
    private void HotkeyCopy()
    {
        if (string.IsNullOrEmpty(GetContent))
            return;

        ClipboardHelper.Copy(GetContent);
        ToastHelper.Show("复制成功", WindowType.OCR);
    }

    [RelayCommand]
    private void OCRPreference()
    {
        var view = Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault() ?? new PreferenceView();
        view.UpdateNavigation(PerferenceType.OCR);
        view.Show();
        view.WindowState = WindowState.Normal;
        view.Activate();
    }

    [RelayCommand]
    private void ClearQrContent()
    {
        QrCodeContent = "";
    }

    #region 鼠标缩放、拖拽

    [ObservableProperty] private double _imgScale = 100;

    [RelayCommand]
    private void ResetImg(Image img)
    {
        var group = (TransformGroup)img.RenderTransform;
        var scaleTransform = (ScaleTransform)group.Children[0];
        var translateTransform = (TranslateTransform)group.Children[1];
        // 执行缩放操作
        scaleTransform.ScaleX = 1;
        scaleTransform.ScaleY = 1;
        // 计算平移值并应用
        translateTransform.X = 0;
        translateTransform.Y = 0;

        ImgScale = scaleTransform.ScaleX;
    }

    [RelayCommand]
    private void SwitchImg(Image img)
    {
        img.Source = img.Source == GetImg ? Bs : GetImg;
    }

    // https://www.cnblogs.com/snake-hand/archive/2012/08/13/2636227.html
    private bool mouseDown;

    private Point mouseXY;

    private readonly double min = 0.1,
        max = 3.0; //最小/最大放大倍数

    public void MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ContentControl content)
            return;
        content.CaptureMouse();
        mouseDown = true;
        mouseXY = e.GetPosition(content);
    }

    public void MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ContentControl content)
            return;
        content.ReleaseMouseCapture();
        mouseDown = false;
    }

    public void MouseMove(object sender, MouseEventArgs e)
    {
        if (!mouseDown || sender is not ContentControl content || content.Content is not Image img)
            return;

        var group = (TransformGroup)img.RenderTransform;
        var transform = (TranslateTransform)group.Children[1];
        var position = e.GetPosition(content);
        transform.X -= mouseXY.X - position.X;
        transform.Y -= mouseXY.Y - position.Y;
        mouseXY = position;
    }

    public void MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ContentControl content || content.Content is not Image img)
            return;
        var point = e.GetPosition(content);
        var group = (TransformGroup)img.RenderTransform;
        var delta = e.Delta * 0.001;
        DowheelZoom(group, point, delta);
    }

    private void DowheelZoom(TransformGroup group, Point point, double delta)
    {
        // 将 point 转换为 group 的逆变换后的坐标系中的坐标
        var pointToContent = group.Inverse.Transform(point);
        // 获取 ScaleTransform
        var scaleTransform = (ScaleTransform)group.Children[0];
        // 检查缩放是否超出范围
        if (scaleTransform.ScaleX + delta < min)
            return;
        if (scaleTransform.ScaleX + delta > max)
            return;
        // 执行缩放操作
        scaleTransform.ScaleX += delta;
        scaleTransform.ScaleY += delta;
        // 获取 TranslateTransform
        var translateTransform = (TranslateTransform)group.Children[1];
        // 计算平移值并应用
        translateTransform.X = -1 * (pointToContent.X * scaleTransform.ScaleX - point.X);
        translateTransform.Y = -1 * (pointToContent.Y * scaleTransform.ScaleY - point.Y);

        ImgScale = scaleTransform.ScaleX;
    }

    #endregion 鼠标缩放、拖拽

    #region ContextMenu

    [RelayCommand]
    private void TBSelectAll(object obj)
    {
        if (obj is not TextBox tb) return;
        LogService.Logger.Debug(tb.Name);
        tb.SelectAll();
    }

    [RelayCommand]
    private void TBCopy(object obj)
    {
        if (obj is not TextBox tb) return;
        var text = tb.SelectedText;
        if (!string.IsNullOrEmpty(text))
            ClipboardHelper.Copy(text);
    }

    [RelayCommand]
    private void TBPaste(object obj)
    {
        if (obj is not TextBox tb) return;
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

    [RelayCommand]
    private void TBClear(object obj)
    {
        if (obj is TextBox tb) tb.Clear();
    }

    #endregion ContextMenu
}