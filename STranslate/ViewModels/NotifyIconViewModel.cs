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

namespace STranslate.ViewModels
{
    public partial class NotifyIconViewModel : ObservableObject
    {
        public NotifyIconModel NIModel { get; } = new();

        public Action<Window, bool>? OnForbiddenShortcuts;

        public Action<Window>? OnMousehook;

        public event Action? OnExit;

        [ObservableProperty]
        private bool _isForbiddenShortcuts = false;

        public NotifyIconViewModel()
        {
            ProxyUtil.LoadDynamicProxy();

            UpdateToolTip();
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
        }

        public void UpdateToolTip(string msg = "")
        {
            NIModel.ToolTip = string.Format("STranslate {0} #\r\n{1}", Application.ResourceAssembly.GetName().Version!.ToString(), msg);
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
            Clear();
            ShowAndActive(view);
        }

        [RelayCommand]
        private void CrossWordTranslate(Window view)
        {
            var content = GetWordsUtil.Get();
            if (string.IsNullOrWhiteSpace(content))
                return;

            //取词前移除换行
            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false)
                content = StringUtil.RemoveLineBreaks(content);

            //如果重复执行先取消上一步操作
            Singleton<InputViewModel>.Instance.TranslateCancelCommand.Execute(null);
            Clear();

            Singleton<InputViewModel>.Instance.InputContent = content;
            ShowAndActive(view);

            Singleton<InputViewModel>.Instance.TranslateCommand.Execute(null);
        }

        [RelayCommand]
        private void MousehookTranslate(Window view)
        {
            ShowAndActive(view);
            OnMousehook?.Invoke(view);
        }

        [RelayCommand]
        private void QRCode()
        {
            System.Threading.Tasks.Task
                .Delay(200)
                .ContinueWith(_ =>
                {
                    CommonUtil.InvokeOnUIThread(() =>
                    {
                        ScreenshotView view = new();
                        ShowAndActive(view, false);

                        view.BitmapCallback += (
                            bitmap =>
                            {
                                //显示OCR窗口
                                OCRView? view = Application.Current.Windows.OfType<OCRView>().FirstOrDefault();
                                view ??= new OCRView();
                                ShowAndActive(view, false);

                                //显示截图
                                var bs = BitmapUtil.ConvertBitmap2BitmapSource(bitmap);

                                Singleton<OCRViewModel>.Instance.GetImg = bs;

                                Singleton<OCRViewModel>.Instance.QRCodeCommand.Execute(bs);
                            }
                        );
                    });
                });
        }

        [RelayCommand]
        private void OCR(object obj)
        {
            if (obj == null)
            {
                OCRHandler();
                return;
            }
            System.Threading.Tasks.Task
                .Delay(200)
                .ContinueWith(_ =>
                {
                    CommonUtil.InvokeOnUIThread(() =>
                    {
                        OCRHandler();
                    });
                });
        }

        private void OCRHandler()
        {
            ScreenshotView view = new();
            ShowAndActive(view, false);

            view.BitmapCallback += (
                bitmap =>
                {
                    //显示OCR窗口
                    OCRView? view = Application.Current.Windows.OfType<OCRView>().FirstOrDefault();
                    view ??= new OCRView();
                    ShowAndActive(view, false);

                    //显示截图
                    var bs = BitmapUtil.ConvertBitmap2BitmapSource(bitmap);

                    Singleton<OCRViewModel>.Instance.GetImg = bs;

                    Singleton<OCRViewModel>.Instance.RecertificationCommand.Execute(bs);
                }
            );
        }

        [RelayCommand]
        private void ScreenShotTranslate(object obj)
        {
            if (obj == null)
            {
                ScreenShotHandler();
                return;
            }
            System.Threading.Tasks.Task
                .Delay(200)
                .ContinueWith(_ =>
                {
                    CommonUtil.InvokeOnUIThread(() =>
                    {
                        ScreenShotHandler();
                    });
                });
        }

        private void ScreenShotHandler()
        {
            ScreenshotView view = new();
            ShowAndActive(view, false);

            view.BitmapCallback += (
                bitmap =>
                {
                    //var getText = TesseractHelper.TesseractOCR(bitmap, OcrType).Trim();

                    //如果重复执行先取消上一步操作
                    Singleton<InputViewModel>.Instance.TranslateCancelCommand.Execute(null);

                    Clear();

                    MainView view = Application.Current.Windows.OfType<MainView>().FirstOrDefault()!;
                    ShowAndActive(view);

                    var bytes = BitmapUtil.ConvertBitmap2Bytes(bitmap);

                    Singleton<InputViewModel>.Instance.InputContent = "识别中...";
                    Thread thread = new Thread(() =>
                    {
                        string getText = "";
                        try
                        {
                            getText = Singleton<PaddleOCRHelper>.Instance.Excute(bytes).Trim();

                            //取词前移除换行
                            if (
                                Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords
                                ?? false && !string.IsNullOrEmpty(getText)
                            )
                                getText = StringUtil.RemoveLineBreaks(getText);

                            //OCR后自动复制
                            if (
                                Singleton<ConfigHelper>.Instance.CurrentConfig?.IsOcrAutoCopyText ?? false && !string.IsNullOrEmpty(getText)
                            )
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

        [RelayCommand]
        private void OpenMainWindow(Window view)
        {
            ShowAndActive(view);
        }

        private void Clear()
        {
            //清空输入相关
            Singleton<InputViewModel>.Instance.Clear();
            //清空输出相关
            Singleton<OutputViewModel>.Instance.Clear();
        }

        private void ShowAndActive(Window view, bool canFollowMouse = true)
        {
            if ((Singleton<ConfigHelper>.Instance.CurrentConfig?.IsFollowMouse ?? false) && canFollowMouse)
            {
                Point mouseLocation = CommonUtil.GetMousePositionWindowsForms();
                view.Left = mouseLocation.X;
                view.Top = mouseLocation.Y;
            }
            //显示主界面
            view.Show();
            view.Activate();
        }

        [RelayCommand]
        private void OpenPreference()
        {
            PreferenceView? window = Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault();
            window ??= new PreferenceView();

            ShowAndActive(window, false);
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

        [RelayCommand]
        private void Exit()
        {
            ProxyUtil.UnLoadDynamicProxy();

            Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= DisplaySettingsChanged;

            OnExit?.Invoke();

            SaveSelectedLang();

            Application.Current.Shutdown();
        }
    }
}
