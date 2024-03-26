using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.Views;
using System;
using System.Linq;
using System.Threading;
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
            Clear();
            ShowAndActive(view, Singleton<ConfigHelper>.Instance.CurrentConfig?.IsFollowMouse ?? false);
        }

        [RelayCommand]
        private void CrossWordTranslate(Window view)
        {
            var content = GetWordsUtil.Get(Singleton<ConfigHelper>.Instance.CurrentConfig?.WordPickingInterval ?? 100);
            if (string.IsNullOrWhiteSpace(content))
            {
                LogService.Logger.Warn($"取词失败，取词内容为空，请尝试延长取词间隔...");
                return;
            }

            //取词前移除换行
            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false)
                content = StringUtil.RemoveLineBreaks(content);

            //如果重复执行先取消上一步操作
            Singleton<OutputViewModel>.Instance.SingleTranslateCancelCommand.Execute(null);
            Singleton<InputViewModel>.Instance.TranslateCancelCommand.Execute(null);
            Clear();

            Singleton<InputViewModel>.Instance.InputContent = content;
            ShowAndActive(view, Singleton<ConfigHelper>.Instance.CurrentConfig?.IsFollowMouse ?? false);

            Singleton<InputViewModel>.Instance.TranslateCommand.Execute(null);
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
            if (obj is null)
            {
                goto Last;
            }

            if (obj.Equals("header"))
            {
                HideMainView();

                goto Last;
            }

            System.Threading.Tasks.Task.Delay(200).ContinueWith(_ => CommonUtil.InvokeOnUIThread(() => QRCodeHandler()));

            return;

            Last:
            QRCodeHandler();
        }

        internal void QRCodeHandler()
        {
            ScreenshotView view = new();
            ShowAndActive(view);

            view.BitmapCallback += (
                bitmap =>
                {
                    //显示OCR窗口
                    OCRView? view = Application.Current.Windows.OfType<OCRView>().FirstOrDefault();
                    view ??= new OCRView();
                    ShowAndActive(view);

                    //显示截图
                    var bs = BitmapUtil.ConvertBitmap2BitmapSource(bitmap);

                    Singleton<OCRViewModel>.Instance.GetImg = bs;

                    Singleton<OCRViewModel>.Instance.QRCodeCommand.Execute(bs);
                }
            );
        }

        [RelayCommand]
        private void OCR(object obj)
        {
            if (obj == null)
            {
                goto Last;
            }

            if (obj.Equals("header"))
            {
                HideMainView();

                goto Last;
            }

            System.Threading.Tasks.Task.Delay(200).ContinueWith(_ => CommonUtil.InvokeOnUIThread(() => OCRHandler()));

            return;

            Last:
            OCRHandler();
        }

        internal void OCRHandler()
        {
            ScreenshotView view = new();
            ShowAndActive(view);

            view.BitmapCallback += (
                bitmap =>
                {
                    //显示OCR窗口
                    OCRView? view = Application.Current.Windows.OfType<OCRView>().FirstOrDefault();
                    view ??= new OCRView();
                    ShowAndActive(view);

                    //显示截图
                    var bs = BitmapUtil.ConvertBitmap2BitmapSource(bitmap);

                    Singleton<OCRViewModel>.Instance.GetImg = bs;

                    Singleton<OCRViewModel>.Instance.RecertificationCommand.Execute(bs);
                }
            );
        }

        [RelayCommand]
        private void SilentOCR(object? obj)
        {
            if (obj == null)
            {
                goto Last;
            }

            if (obj.Equals("header"))
            {
                HideMainView();

                goto Last;
            }

            System.Threading.Tasks.Task.Delay(200).ContinueWith(_ => CommonUtil.InvokeOnUIThread(() => SilentOCRHandler()));

            return;

            Last:
            SilentOCRHandler();
        }

        internal void SilentOCRHandler()
        {
            ScreenshotView view = new();
            ShowAndActive(view);

            view.BitmapCallback += (
                bitmap =>
                {
                    try
                    {
                        var bytes = BitmapUtil.ConvertBitmap2Bytes(bitmap);
                        var getText = Singleton<PaddleOCRHelper>.Instance.Execute(bytes).Trim();

                        //取词前移除换行
                        getText = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false ? StringUtil.RemoveLineBreaks(getText) : getText;

                        //写入剪贴板
                        Clipboard.SetDataObject(getText, true);

                        var tmp = getText.Length >= 9 ? getText[..9] + "..." : getText;
                        ShowBalloonTip($"OCR识别成功: {tmp}");
                    }
                    catch (Exception ex)
                    {
                        ShowBalloonTip($"OCR识别失败: {ex.Message}");
                        LogService.Logger.Error("静默OCR失败", ex);
                    }
                }
            );
        }

        [RelayCommand]
        private void ScreenShotTranslate(object obj)
        {
            if (obj == null)
            {
                goto Last;
            }

            if (obj.Equals("header"))
            {
                HideMainView();

                goto Last;
            }

            System.Threading.Tasks.Task.Delay(200).ContinueWith(_ => CommonUtil.InvokeOnUIThread(() => ScreenShotHandler()));

            return;

            Last:
            ScreenShotHandler();
        }

        internal void ScreenShotHandler()
        {
            ScreenshotView view = new();
            ShowAndActive(view);

            view.BitmapCallback += (
                bitmap =>
                {
                    //var getText = TesseractHelper.TesseractOCR(bitmap, OcrType).Trim();

                    //如果重复执行先取消上一步操作
                    Singleton<OutputViewModel>.Instance.SingleTranslateCancelCommand.Execute(null);
                    Singleton<InputViewModel>.Instance.TranslateCancelCommand.Execute(null);

                    Clear();

                    MainView view = Application.Current.Windows.OfType<MainView>().FirstOrDefault()!;
                    ShowAndActive(view, Singleton<ConfigHelper>.Instance.CurrentConfig?.IsFollowMouse ?? false);

                    var bytes = BitmapUtil.ConvertBitmap2Bytes(bitmap);

                    Singleton<InputViewModel>.Instance.InputContent = "识别中...";
                    Thread thread = new Thread(() =>
                    {
                        string getText = "";
                        try
                        {
                            getText = Singleton<PaddleOCRHelper>.Instance.Execute(bytes).Trim();

                            //取词前移除换行
                            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false && !string.IsNullOrEmpty(getText))
                                getText = StringUtil.RemoveLineBreaks(getText);

                            //OCR后自动复制
                            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsOcrAutoCopyText ?? false && !string.IsNullOrEmpty(getText))
                                Clipboard.SetDataObject(getText, true);

                            CommonUtil.InvokeOnUIThread(() =>
                            {
                                Singleton<InputViewModel>.Instance.InputContent = getText;
                                Singleton<InputViewModel>.Instance.TranslateCommand.Execute(null);
                            });
                        }
                        catch (Exception ex)
                        {
                            CommonUtil.InvokeOnUIThread(() => Singleton<InputViewModel>.Instance.InputContent = ex.Message);
                        }
                    });
                    thread.IsBackground = true;
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            );
        }

        /// <summary>
        /// 隐藏主窗口
        /// </summary>
        internal void HideMainView()
        {
            var view = Application.Current.Windows.OfType<MainView>().FirstOrDefault()!;
            if (!view.Topmost)
            {
                view.ViewAnimation(false);
            }
        }

        [RelayCommand]
        private void OpenMainWindow(Window view)
        {
            ShowAndActive(view);
        }

        internal void Clear()
        {
            //清空输入相关
            Singleton<InputViewModel>.Instance.Clear();
            //清空输出相关
            Singleton<OutputViewModel>.Instance.Clear();
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
            if (view is MainView mainView && (mainView.FindName("InputView") as UserControl)?.FindName("InputTb") is TextBox inputTextBox)
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
                    Singleton<MainViewModel>.Instance.IsHotkeyCopy = false;
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
                if (_clipboardHelper == null) return;

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
            Clear();

            Singleton<InputViewModel>.Instance.InputContent = content;
            ShowAndActive(view, Singleton<ConfigHelper>.Instance.CurrentConfig?.IsFollowMouse ?? false);

            Singleton<InputViewModel>.Instance.TranslateCommand.Execute(null);
        }

        private void SaveSelectedLang()
        {
            //写入配置
            var vm = Singleton<MainViewModel>.Instance;
            var source = vm.SelectedSourceLanguage ?? "自动选择";
            var target = vm.SelectedTargetLanguage ?? "自动选择";
            if (!Singleton<ConfigHelper>.Instance.WriteConfig(source, target))
            {
                LogService.Logger.Debug($"保存源语言({source})、目标语言({target})配置失败...");
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
