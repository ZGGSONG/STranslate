using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference;
using STranslate.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace STranslate.ViewModels
{
    public partial class NotifyIconViewModel : ObservableObject
    {
        /// <summary>
        /// 屏蔽快捷键事件
        /// </summary>
        public event Action<Window, bool>? OnForbiddenShortcuts;

        /// <summary>
        /// 监听鼠标划词事件
        /// </summary>
        public event Action<Window>? OnMousehook;

        /// <summary>
        /// 退出事件
        /// </summary>
        public event Action? OnExit;

        /// <summary>
        /// Toast通知事件
        /// </summary>
        public event Action<string>? OnShowBalloonTip;

        /// <summary>
        /// 图标、提示
        /// </summary>
        public NotifyIconModel NIModel { get; } = new();

        [ObservableProperty]
        private bool _isMousehook = false;

        [ObservableProperty]
        private bool _isForbiddenShortcuts = false;

        [ObservableProperty]
        private bool _isClipboardMonitor = false;

        [ObservableProperty]
        private string _isEnabledClipboardMonitor = ConstStr.TAGFALSE;

        private ClipboardHelper? _clipboardHelper;

        public NotifyIconViewModel()
        {
            UpdateToolTip();
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
            WeakReferenceMessenger.Default.Register<ExternalCallMessenger>(this, (o, e) => ExternalCallHandler(e));
        }

        /// <summary>
        /// 外部调用注册
        /// </summary>
        /// <param name="e"></param>
        private void ExternalCallHandler(ExternalCallMessenger e)
        {
            var actions = new Dictionary<ExternalCallAction, Action<Window, string>>
            {
                {
                    ExternalCallAction.translate,
                    (view, content) =>
                    {
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            InputTranslate(view);
                        }
                        else
                        {
                            TranslateHandler(view, content);
                        }
                    }
                },
                {
                    ExternalCallAction.translate_force,
                    (view, content) =>
                    {
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            InputTranslate(view);
                        }
                        else
                        {
                            TranslateHandler(view, content, true);//表示对象非空，强制翻译
                        }
                    }
                },
                { ExternalCallAction.translate_input, (view, _) => InputTranslate(view) },
                {
                    ExternalCallAction.translate_ocr,
                    async (_, content) =>
                    {
                        if (string.IsNullOrWhiteSpace(content))
                            ScreenShotHandler();
                        else
                            await ScreenshotCallback(BitmapUtil.ReadImageFile(content));
                    }
                },
                { ExternalCallAction.translate_crossword, (view, _) => CrossWordTranslate(view) },
                { ExternalCallAction.translate_mousehook, (view, _) => MousehookTranslate(view) },
                { ExternalCallAction.listenclipboard, (view, _) => ClipboardMonitor(view) },
                {
                    ExternalCallAction.ocr,
                    async (_, content) =>
                    {
                        if (string.IsNullOrWhiteSpace(content))
                            OCRHandler();
                        else
                            await OCRCallback(BitmapUtil.ReadImageFile(content));
                    }
                },
                {
                    ExternalCallAction.ocr_silence,
                    async (_, content) =>
                    {
                        if (string.IsNullOrWhiteSpace(content))
                            SilentOCRHandler();
                        else
                            await SilentOCRCallback(BitmapUtil.ReadImageFile(content));
                    }
                },
                {
                    ExternalCallAction.ocr_qrcode,
                    (_, content) =>
                    {
                        if (string.IsNullOrWhiteSpace(content))
                            QRCodeHandler();
                        else
                            QRCodeCallback(BitmapUtil.ReadImageFile(content));
                    }
                },
                { ExternalCallAction.open_window, (view, _) => OpenMainWindow(view) },
                { ExternalCallAction.open_preference, (_, _) => OpenPreference() },
                { ExternalCallAction.open_history, (_, _) => OpenHistory() },
                { ExternalCallAction.forbiddenhotkey, (view, _) => ForbiddenShortcuts(view) },
            };

            CommonUtil.InvokeOnUIThread(() =>
            {
                var view = Application.Current.Windows.OfType<MainView>().First();

                if (actions.TryGetValue(e.ECAction, out var handler))
                {
                    handler?.Invoke(view, e.Content);
                }
            });
        }

        public void UpdateToolTip(string msg = "")
        {
            bool isAdmin = CommonUtil.IsUserAdministrator();

            string toolTipFormat = isAdmin ? "STranslate {0}\r\n[Administrator] #\r\n{1}" : "STranslate {0} #\r\n{1}";

            NIModel.ToolTip = string.Format(toolTipFormat, Application.ResourceAssembly.GetName().Version!, msg);
        }

        [RelayCommand]
        private void DoubleClick(Window view)
        {
            switch (Singleton<ConfigHelper>.Instance.CurrentConfig?.DoubleTapTrayFunc ?? DoubleTapFuncEnum.InputFunc)
            {
                case DoubleTapFuncEnum.InputFunc:
                    InputTranslateCommand.Execute(view);
                    break;

                case DoubleTapFuncEnum.ScreenFunc:
                    ScreenShotTranslateCommand.Execute(null);
                    break;

                case DoubleTapFuncEnum.MouseHookFunc:
                    MousehookTranslateCommand.Execute(view);
                    break;

                case DoubleTapFuncEnum.OCRFunc:
                    OCRCommand.Execute(null);
                    break;

                case DoubleTapFuncEnum.ShowViewFunc:
                    OpenMainWindowCommand.Execute(view);
                    break;

                case DoubleTapFuncEnum.PreferenceFunc:
                    OpenPreferenceCommand.Execute(null);
                    break;

                case DoubleTapFuncEnum.ForbidShortcutFunc:
                    ForbiddenShortcutsCommand.Execute(view);
                    break;

                case DoubleTapFuncEnum.ExitFunc:
                    ExitCommand.Execute(null);
                    break;

                default:
                    break;
            }
        }

        [RelayCommand]
        private void InputTranslate(Window view)
        {
            //如果重复执行先取消上一步操作
            Singleton<OutputViewModel>.Instance.SingleTranslateCancelCommand.Execute(null);
            Singleton<InputViewModel>.Instance.TranslateCancelCommand.Execute(null);
            ClearAll();
            ShowAndActive(view, Singleton<ConfigHelper>.Instance.CurrentConfig?.IsFollowMouse ?? false);
        }

        [RelayCommand]
        private void CrossWordTranslate(Window view)
        {
            var content = GetWordsUtil.Get(Singleton<ConfigHelper>.Instance.CurrentConfig?.WordPickingInterval ?? 100).Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                LogService.Logger.Warn($"取词失败，取词内容为空，请尝试延长取词间隔...");
                return;
            }

            //取词前移除换行
            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false)
                content = StringUtil.RemoveLineBreaks(content);

            TranslateHandler(view, content);
        }

        internal void TranslateHandler(Window view, string content, object? obj = null)
        {
            //如果重复执行先取消上一步操作
            Singleton<OutputViewModel>.Instance.SingleTranslateCancelCommand.Execute(null);
            Singleton<InputViewModel>.Instance.TranslateCancelCommand.Execute(null);

            //增量翻译
            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IncrementalTranslation ?? false)
            {
                ClearOutput();
                var input = Singleton<InputViewModel>.Instance.InputContent;
                Singleton<InputViewModel>.Instance.InputContent = string.IsNullOrEmpty(input) ? string.Empty : input + " ";
            }
            else
            {
                ClearAll();
            }

            Singleton<InputViewModel>.Instance.InputContent += content;
            ShowAndActive(view, Singleton<ConfigHelper>.Instance.CurrentConfig?.IsFollowMouse ?? false);

            Singleton<InputViewModel>.Instance.TranslateCommand.Execute(obj);
        }

        [RelayCommand]
        private void MousehookTranslate(Window view)
        {
            ShowAndActive(view);
            OnMousehook?.Invoke(view);
        }

        [RelayCommand]
        private void QRCode(object obj)
        {
            if (!CanOpenScreenshot)
                return;

            if (obj is null)
            {
                goto Last;
            }

            if (obj.Equals("header"))
            {
                HideMainView();
            }

            Task.Delay(200).ContinueWith(_ => CommonUtil.InvokeOnUIThread(() => QRCodeHandler()));

            return;

            Last:
            QRCodeHandler();
        }

        internal void QRCodeHandler()
        {
            ScreenshotView view = new();
            ShowAndActive(view);

            view.BitmapCallback += (bitmap => QRCodeCallback(bitmap));
            view.OnViewVisibilityChanged += (o) => CanOpenScreenshot = o;
            view.InvokeCanOpen();
        }

        private void QRCodeCallback(Bitmap? bitmap)
        {
            if (bitmap == null)
            {
                ShowBalloonTip("图像不存在");
                return;
            }

            //显示OCR窗口
            OCRView? view = Application.Current.Windows.OfType<OCRView>().FirstOrDefault();
            view ??= new OCRView();
            ShowAndActive(view);

            //显示截图
            var bs = BitmapUtil.ConvertBitmap2BitmapSource(bitmap);

            Singleton<OCRViewModel>.Instance.GetImg = bs;

            Singleton<OCRViewModel>.Instance.QRCodeCommand.Execute(bs);
        }

        [RelayCommand]
        private void OCR(object obj)
        {
            if (!CanOpenScreenshot)
                return;

            if (obj == null)
            {
                goto Last;
            }

            if (obj.Equals("header"))
            {
                HideMainView();
            }

            Task.Delay(200).ContinueWith(_ => CommonUtil.InvokeOnUIThread(() => OCRHandler()));

            return;

            Last:
            OCRHandler();
        }

        internal void OCRHandler()
        {
            ScreenshotView view = new();
            ShowAndActive(view);

            view.BitmapCallback += async bitmap => await OCRCallback(bitmap);
            view.OnViewVisibilityChanged += (o) => CanOpenScreenshot = o;
            view.InvokeCanOpen();
        }

        private async Task OCRCallback(Bitmap? bitmap)
        {
            if (bitmap == null)
            {
                ShowBalloonTip("图像不存在");
                return;
            }

            //显示OCR窗口
            OCRView? view = Application.Current.Windows.OfType<OCRView>().FirstOrDefault();
            view ??= new OCRView();
            ShowAndActive(view);

            //显示截图
            var bs = BitmapUtil.ConvertBitmap2BitmapSource(bitmap);
            Singleton<OCRViewModel>.Instance.ResetImgCommand.Execute(view.FindName("ImgCtl"));
            Singleton<OCRViewModel>.Instance.GetImg = bs;
            Singleton<OCRViewModel>.Instance.Bs = bs.Clone();

            await Singleton<OCRViewModel>.Instance.RecertificationCommand.ExecuteAsync(bs);
        }

        [RelayCommand]
        private void SilentOCR(object? obj)
        {
            if (!CanOpenScreenshot)
                return;

            if (obj == null)
            {
                goto Last;
            }

            if (obj.Equals("header"))
            {
                HideMainView();
            }

            Task.Delay(200).ContinueWith(_ => CommonUtil.InvokeOnUIThread(() => SilentOCRHandler()));

            return;

            Last:
            SilentOCRHandler();
        }

        internal void SilentOCRHandler()
        {
            ScreenshotView view = new();
            ShowAndActive(view);

            view.BitmapCallback += async bitmap => await SilentOCRCallback(bitmap);
            view.OnViewVisibilityChanged += (o) => CanOpenScreenshot = o;
            view.InvokeCanOpen();
        }

        private async Task SilentOCRCallback(Bitmap? bitmap)
        {
            try
            {
                if (bitmap == null)
                {
                    ShowBalloonTip("图像不存在");
                    return;
                }

                string getText = "";
                var bytes = BitmapUtil.ConvertBitmap2Bytes(bitmap);
                var ocrResult = await Singleton<OCRScvViewModel>.Instance.ExecuteAsync(bytes, WindowType.Main);
                //判断结果
                if (!ocrResult.Success)
                {
                    throw new Exception(ocrResult.ErrorMsg);
                }
                getText = ocrResult.Text;

                //取词前移除换行
                getText = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false ? StringUtil.RemoveLineBreaks(getText) : getText;

                //写入剪贴板
                ClipboardHelper.Copy(getText);

                var tmp = getText.Length >= 9 ? getText[..9] + "..." : getText;
                ShowBalloonTip($"OCR识别成功: {tmp}");
            }
            catch (Exception ex)
            {
                ShowBalloonTip($"OCR识别失败: {ex.Message}");
                LogService.Logger.Error("静默OCR失败", ex);
            }
        }

        [RelayCommand]
        private void ScreenShotTranslate(object obj)
        {
            if (!CanOpenScreenshot)
                return;

            if (obj == null)
            {
                goto Last;
            }

            if (obj.Equals("header"))
            {
                HideMainView();
            }

            Task.Delay(200).ContinueWith(_ => CommonUtil.InvokeOnUIThread(() => ScreenShotHandler()));

            return;

            Last:
            ScreenShotHandler();
        }

        internal void ScreenShotHandler()
        {
            ScreenshotView view = new();
            ShowAndActive(view);
            view.BitmapCallback += async bitmap => await ScreenshotCallback(bitmap);
            view.OnViewVisibilityChanged += (o) => CanOpenScreenshot = o;
            view.InvokeCanOpen();
        }

        internal async Task ScreenshotCallback(Bitmap? bitmap)
        {
            if (bitmap == null)
            {
                ShowBalloonTip("图像不存在");
                return;
            }

            //如果重复执行先取消上一步操作
            Singleton<OutputViewModel>.Instance.SingleTranslateCancelCommand.Execute(null);
            Singleton<InputViewModel>.Instance.TranslateCancelCommand.Execute(null);

            //增量翻译
            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IncrementalTranslation ?? false)
            {
                ClearOutput();
                var input = Singleton<InputViewModel>.Instance.InputContent;
                Singleton<InputViewModel>.Instance.InputContent = string.IsNullOrEmpty(input) ? string.Empty : input + " ";
            }
            else
            {
                ClearAll();
            }

            MainView view = Application.Current.Windows.OfType<MainView>().First();
            ShowAndActive(view, Singleton<ConfigHelper>.Instance.CurrentConfig?.IsFollowMouse ?? false);

            var bytes = BitmapUtil.ConvertBitmap2Bytes(bitmap);
            try
            {
                string getText = "";
                var ocrResult = await Singleton<OCRScvViewModel>.Instance.ExecuteAsync(bytes, WindowType.Main);
                //判断结果
                if (!ocrResult.Success)
                {
                    throw new Exception("OCR失败: " + ocrResult.ErrorMsg);
                }
                getText = ocrResult.Text;
                //取词前移除换行
                if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false && !string.IsNullOrEmpty(getText))
                    getText = StringUtil.RemoveLineBreaks(getText);
                //OCR后自动复制
                if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsOcrAutoCopyText ?? false && !string.IsNullOrEmpty(getText))
                    ClipboardHelper.Copy(getText);
                Singleton<InputViewModel>.Instance.InputContent += getText;
                Singleton<InputViewModel>.Instance.TranslateCommand.Execute(null);
            }
            catch (Exception ex)
            {
                Singleton<InputViewModel>.Instance.InputContent = ex.Message;
            }
        }

        /// <summary>
        /// 是否可以调用截图View
        /// </summary>
        public bool CanOpenScreenshot { get; set; } = true;

        /// <summary>
        /// 隐藏主窗口
        /// </summary>
        internal void HideMainView()
        {
            var view = Application.Current.Windows.OfType<MainView>().First();
            // 判断是否置顶，置顶的话则不隐藏
            if (!view.Topmost)
            {
                view.ViewAnimation(false);
            }
        }

        [RelayCommand]
        private void OpenMainWindow(Window view)
        {
            if ((Singleton<ConfigHelper>.Instance.CurrentConfig?.IsTriggerShowHide ?? false) && view.IsActive)
                HideMainView();
            else
                ShowAndActive(view);
        }

        internal void ClearOutput()
        {
            //清空输出相关
            Singleton<OutputViewModel>.Instance.Clear();
        }

        internal void ClearAll()
        {
            ClearOutput();
            //清空输入相关
            Singleton<InputViewModel>.Instance.Clear();
        }

        private void ShowAndActive(Window view, bool canFollowMouse = false)
        {
            if (canFollowMouse)
            {
                var position = FollowMouseHandler(view);

                view.Left = position.Item1;
                view.Top = position.Item2;
            }

            SpecialWindowActiveHandler(view);

            if (view is MainView mview)
            {
                mview.ViewAnimation();
            }
            else
            {
                view.Show();
            }
            view.Activate();

            //激活输入框
            if (view is MainView mainView && (mainView.FindName("InputView") as UserControl)?.FindName("InputTB") is TextBox inputTextBox)
            {
                // 获取焦点
                inputTextBox.Focus();

                // 光标移动至末尾
                if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsCaretLast ?? false)
                    inputTextBox.CaretIndex = inputTextBox.Text.Length;
            }
        }

        [RelayCommand]
        private void OpenPreference()
        {
            PreferenceView? view = Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault();
            view ??= new PreferenceView();
            view.UpdateNavigation();

            ShowAndActive(view);
        }

        [RelayCommand]
        private void OpenHistory()
        {
            PreferenceView? view = Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault();
            view ??= new PreferenceView();
            view.UpdateNavigation(PerferenceType.History);

            ShowAndActive(view);
        }

        [RelayCommand]
        private void ForbiddenShortcuts(Window view)
        {
            IsForbiddenShortcuts = !IsForbiddenShortcuts;
            if (IsForbiddenShortcuts)
                NIModel.IconSource = ConstStr.ICONFORBIDDEN;
            else
                NIModel.IconSource = ConstStr.ICON;

            OnForbiddenShortcuts?.Invoke(view, IsForbiddenShortcuts);
        }

        [RelayCommand]
        private void ClipboardMonitor(Window view)
        {
            IsClipboardMonitor = !IsClipboardMonitor;
            IsEnabledClipboardMonitor = IsClipboardMonitor ? ConstStr.TAGTRUE : ConstStr.TAGFALSE;

            if (IsClipboardMonitor)
            {
                _clipboardHelper ??= new ClipboardHelper();
                // 开始监听剪贴板变化
                if (_clipboardHelper.Start(out string error))
                {
                    // 清除热键复制标记
                    Singleton<MainViewModel>
                        .Instance
                        .IsHotkeyCopy = false;
                    _clipboardHelper.OnClipboardChanged += (c) => ClipboardChanged(c, view);

                    ShowBalloonTip("已启用监听剪贴板");
                }
                else
                {
                    ShowBalloonTip(error);
                }
            }
            else
            {
                if (_clipboardHelper == null)
                    return;
                else if (_clipboardHelper.Stop(out string error))
                {
                    _clipboardHelper.OnClipboardChanged -= (c) => ClipboardChanged(c, view);
                    _clipboardHelper = null;
                    ShowBalloonTip("已关闭监听剪贴板");
                }
                else
                {
                    ShowBalloonTip(error);
                }
            }
        }

        private void ClipboardChanged(string content, Window view)
        {
            //热键复制时略过
            if (Singleton<MainViewModel>.Instance.IsHotkeyCopy == true)
            {
                Singleton<MainViewModel>.Instance.IsHotkeyCopy = false;
                return;
            }
            //取词前移除换行
            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false)
                content = StringUtil.RemoveLineBreaks(content);

            //如果重复执行先取消上一步操作
            Singleton<OutputViewModel>.Instance.SingleTranslateCancelCommand.Execute(null);
            Singleton<InputViewModel>.Instance.TranslateCancelCommand.Execute(null);
            //增量翻译
            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IncrementalTranslation ?? false)
            {
                ClearOutput();
                var input = Singleton<InputViewModel>.Instance.InputContent;
                Singleton<InputViewModel>.Instance.InputContent = string.IsNullOrEmpty(input) ? string.Empty : input + " ";
            }
            else
            {
                ClearAll();
            }

            Singleton<InputViewModel>.Instance.InputContent += content;
            ShowAndActive(view, Singleton<ConfigHelper>.Instance.CurrentConfig?.IsFollowMouse ?? false);

            Singleton<InputViewModel>.Instance.TranslateCommand.Execute(null);
        }

        private void SaveSelectedLang()
        {
            //写入配置
            var vm = Singleton<MainViewModel>.Instance;
            if (!Singleton<ConfigHelper>.Instance.WriteConfig(vm.SourceLang, vm.TargetLang))
            {
                LogService.Logger.Debug($"保存源语言({vm.SourceLang.GetDescription()})、目标语言({vm.TargetLang.GetDescription()})配置失败...");
            }
        }

        /// <summary>
        /// 系统显示变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplaySettingsChanged(object? sender, EventArgs e)
        {
            NIModel.IconSource = ConstStr.ICON;
        }

        /// <summary>
        /// 托盘程序BallonTip消息入口
        /// </summary>
        /// <param name="msg"></param>
        public void ShowBalloonTip(string msg) => OnShowBalloonTip?.Invoke(msg);

        [RelayCommand]
        private void Exit()
        {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= DisplaySettingsChanged;

            OnExit?.Invoke();

            SaveSelectedLang();

            Application.Current.Shutdown();
        }

        /// <summary>
        /// 跟随鼠标处理
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        private Tuple<double, double> FollowMouseHandler(Window view)
        {
            var infos = CommonUtil.GetPositionInfos();
            var position = infos.Item1;
            var bounds = infos.Item2;
            var left = position.X;
            var top = position.Y;

            //保持页面在屏幕上方三分之一处
            if ((top - bounds.Top) * 3 > bounds.Height)
            {
                top = bounds.Height / 3 + bounds.Top;
            }

            //如果当前高度不足以容纳最大高度的内容，则使用最大高度为窗口Top值
            if (bounds.Height - top + bounds.Top < view.MaxHeight)
            {
                top = bounds.Height - view.MaxHeight + bounds.Top - 48;
            }

            //右侧不超出当前屏幕区域
            if (left + view.Width > (bounds.Left + bounds.Width))
            {
                left = bounds.Left + bounds.Width - view.Width;
            }
            return new Tuple<double, double>(left, top);
        }

        /// <summary>
        /// 特定情况下窗口无法激活的问题
        /// 1. 主窗口非置顶并且设置页面已经存在时激活设置页面
        /// 2. 设置页面最小化再激活
        /// </summary>
        /// <param name="view"></param>
        private void SpecialWindowActiveHandler(Window view)
        {
            if (view.WindowState == WindowState.Minimized)
            {
                view.WindowState = WindowState.Normal; // Restore the window if it was minimized.
            }
            if (!view.Topmost) // Ensure the window is topmost if it's not already.
            {
                view.Topmost = true; // Temporarily make the window topmost.
                view.Topmost = false; // Then set it back to normal state, this is a trick to bring it to the front.
            }
        }
    }
}
