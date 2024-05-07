using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace STranslate.ViewModels.Preference
{
    public partial class HotkeyViewModel : ObservableObject
    {
        private readonly ConfigHelper conf = Singleton<ConfigHelper>.Instance;
        private KeyModifiers _hotkeysModifiers;
        private KeyCodes _hotkeysKey;
        private string _hotkeysText = string.Empty;

        #region Property

        [ObservableProperty]
        private HotkeyContentVisibilityModel _inputHk = new();

        [ObservableProperty]
        private HotkeyContentVisibilityModel _crosswordHk = new();

        [ObservableProperty]
        private HotkeyContentVisibilityModel _screenshotHk = new();

        [ObservableProperty]
        private HotkeyContentVisibilityModel _openHk = new();

        [ObservableProperty]
        private HotkeyContentVisibilityModel _mousehookHk = new();

        [ObservableProperty]
        private HotkeyContentVisibilityModel _ocrHk = new();

        [ObservableProperty]
        private HotkeyContentVisibilityModel _silentOcrHk = new();

        [ObservableProperty]
        private HotkeyContentVisibilityModel _clipboardMonitorHk = new();

        #endregion Property

        public HotkeyViewModel()
        {
            InputHk.Content = conf.CurrentConfig?.Hotkeys?.InputTranslate?.Text ?? "";
            CrosswordHk.Content = conf.CurrentConfig?.Hotkeys?.CrosswordTranslate?.Text ?? "";
            ScreenshotHk.Content = conf.CurrentConfig?.Hotkeys?.ScreenShotTranslate?.Text ?? "";
            OpenHk.Content = conf.CurrentConfig?.Hotkeys?.OpenMainWindow?.Text ?? "";
            MousehookHk.Content = conf.CurrentConfig?.Hotkeys?.MousehookTranslate?.Text ?? "";
            OcrHk.Content = conf.CurrentConfig?.Hotkeys?.OCR?.Text ?? "";
            SilentOcrHk.Content = conf.CurrentConfig?.Hotkeys?.SilentOCR?.Text ?? "";
            ClipboardMonitorHk.Content = conf.CurrentConfig?.Hotkeys?.ClipboardMonitor?.Text ?? "";
            HotKeyConflictCheck();
        }

        #region Command

        [RelayCommand]
        private void Save()
        {
            if (!conf.WriteConfig(conf.CurrentConfig!.Hotkeys!))
            {
                LogService.Logger.Debug($"保存全局热键配置失败，{JsonConvert.SerializeObject(conf.CurrentConfig.Hotkeys)}");

                ToastHelper.Show("保存热键失败", WindowType.Preference);
            }
            else
            {
                RefreshNotifyToolTip();
                ToastHelper.Show("保存热键成功", WindowType.Preference);
            }
        }

        [RelayCommand]
        private void Reset()
        {
            var cHotkeys = conf.ResetConfig.Hotkeys!;

            InputHk.Content = cHotkeys.InputTranslate.Text ?? "";
            CrosswordHk.Content = cHotkeys.CrosswordTranslate.Text ?? "";
            ScreenshotHk.Content = cHotkeys.ScreenShotTranslate.Text ?? "";
            OpenHk.Content = cHotkeys.OpenMainWindow.Text ?? "";
            MousehookHk.Content = cHotkeys.MousehookTranslate.Text ?? "";
            OcrHk.Content = cHotkeys.OCR.Text ?? "";
            SilentOcrHk.Content = cHotkeys.SilentOCR.Text ?? "";
            ClipboardMonitorHk.Content = cHotkeys.ClipboardMonitor.Text ?? "";

            conf.CurrentConfig!.Hotkeys!.InputTranslate.Modifiers = cHotkeys.InputTranslate.Modifiers;
            conf.CurrentConfig!.Hotkeys!.InputTranslate.Key = cHotkeys.InputTranslate.Key;
            conf.CurrentConfig!.Hotkeys!.InputTranslate.Text = cHotkeys.InputTranslate.Text;

            conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Modifiers = cHotkeys.CrosswordTranslate.Modifiers;
            conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Key = cHotkeys.CrosswordTranslate.Key;
            conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Text = cHotkeys.CrosswordTranslate.Text;

            conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Modifiers = cHotkeys.ScreenShotTranslate.Modifiers;
            conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Key = cHotkeys.ScreenShotTranslate.Key;
            conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Text = cHotkeys.ScreenShotTranslate.Text;

            conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Modifiers = cHotkeys.OpenMainWindow.Modifiers;
            conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Key = cHotkeys.OpenMainWindow.Key;
            conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Text = cHotkeys.OpenMainWindow.Text;

            conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Modifiers = cHotkeys.MousehookTranslate.Modifiers;
            conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Key = cHotkeys.MousehookTranslate.Key;
            conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Text = cHotkeys.MousehookTranslate.Text;

            conf.CurrentConfig!.Hotkeys!.OCR.Modifiers = cHotkeys.OCR.Modifiers;
            conf.CurrentConfig!.Hotkeys!.OCR.Key = cHotkeys.OCR.Key;
            conf.CurrentConfig!.Hotkeys!.OCR.Text = cHotkeys.OCR.Text;

            conf.CurrentConfig!.Hotkeys!.SilentOCR.Modifiers = cHotkeys.SilentOCR.Modifiers;
            conf.CurrentConfig!.Hotkeys!.SilentOCR.Key = cHotkeys.SilentOCR.Key;
            conf.CurrentConfig!.Hotkeys!.SilentOCR.Text = cHotkeys.SilentOCR.Text;

            conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Modifiers = cHotkeys.ClipboardMonitor.Modifiers;
            conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Key = cHotkeys.ClipboardMonitor.Key;
            conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Text = cHotkeys.ClipboardMonitor.Text;

            HotkeyHelper.Hotkeys!.InputTranslate.Modifiers = cHotkeys.InputTranslate.Modifiers;
            HotkeyHelper.Hotkeys!.InputTranslate.Key = cHotkeys.InputTranslate.Key;
            HotkeyHelper.Hotkeys!.InputTranslate.Text = cHotkeys.InputTranslate.Text;

            HotkeyHelper.Hotkeys!.CrosswordTranslate.Modifiers = cHotkeys.CrosswordTranslate.Modifiers;
            HotkeyHelper.Hotkeys!.CrosswordTranslate.Key = cHotkeys.CrosswordTranslate.Key;
            HotkeyHelper.Hotkeys!.CrosswordTranslate.Text = cHotkeys.CrosswordTranslate.Text;

            HotkeyHelper.Hotkeys!.ScreenShotTranslate.Modifiers = cHotkeys.ScreenShotTranslate.Modifiers;
            HotkeyHelper.Hotkeys!.ScreenShotTranslate.Key = cHotkeys.ScreenShotTranslate.Key;
            HotkeyHelper.Hotkeys!.ScreenShotTranslate.Text = cHotkeys.ScreenShotTranslate.Text;

            HotkeyHelper.Hotkeys!.OpenMainWindow.Modifiers = cHotkeys.OpenMainWindow.Modifiers;
            HotkeyHelper.Hotkeys!.OpenMainWindow.Key = cHotkeys.OpenMainWindow.Key;
            HotkeyHelper.Hotkeys!.OpenMainWindow.Text = cHotkeys.OpenMainWindow.Text;

            HotkeyHelper.Hotkeys!.MousehookTranslate.Modifiers = cHotkeys.MousehookTranslate.Modifiers;
            HotkeyHelper.Hotkeys!.MousehookTranslate.Key = cHotkeys.MousehookTranslate.Key;
            HotkeyHelper.Hotkeys!.MousehookTranslate.Text = cHotkeys.MousehookTranslate.Text;

            HotkeyHelper.Hotkeys!.OCR.Modifiers = cHotkeys.OCR.Modifiers;
            HotkeyHelper.Hotkeys!.OCR.Key = cHotkeys.OCR.Key;
            HotkeyHelper.Hotkeys!.OCR.Text = cHotkeys.OCR.Text;

            HotkeyHelper.Hotkeys!.SilentOCR.Modifiers = cHotkeys.SilentOCR.Modifiers;
            HotkeyHelper.Hotkeys!.SilentOCR.Key = cHotkeys.SilentOCR.Key;
            HotkeyHelper.Hotkeys!.SilentOCR.Text = cHotkeys.SilentOCR.Text;

            HotkeyHelper.Hotkeys!.ClipboardMonitor.Modifiers = cHotkeys.ClipboardMonitor.Modifiers;
            HotkeyHelper.Hotkeys!.ClipboardMonitor.Key = cHotkeys.ClipboardMonitor.Key;
            HotkeyHelper.Hotkeys!.ClipboardMonitor.Text = cHotkeys.ClipboardMonitor.Text;

            HotkeyHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
            RefreshNotifyToolTip();

            ToastHelper.Show("重置成功", WindowType.Preference);
        }

        [RelayCommand]
        private void Keyup(UserdefineKeyArgsModel model)
        {
            var e = model.KeyEventArgs!;
            var hke = (HotkeyEnum)model.Obj!;

            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (
                key == Key.LeftShift
                || key == Key.RightShift
                || key == Key.LeftCtrl
                || key == Key.RightCtrl
                || key == Key.LeftAlt
                || key == Key.RightAlt
                || key == Key.LWin
                || key == Key.RWin
            )
            {
                return;
            }
            UpdateCnf(hke);
            HotkeyHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
            RefreshNotifyToolTip();
        }

        [RelayCommand]
        public void Keydown(UserdefineKeyArgsModel model)
        {
            var e = model.KeyEventArgs!;
            var control = (TextBox)model.Obj!;

            e.Handled = true;
            _hotkeysModifiers = KeyModifiers.MOD_NONE;
            _hotkeysKey = KeyCodes.None;
            _hotkeysText = string.Empty;
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (
                key == Key.LeftShift
                || key == Key.RightShift
                || key == Key.LeftCtrl
                || key == Key.RightCtrl
                || key == Key.LeftAlt
                || key == Key.RightAlt
                || key == Key.LWin
                || key == Key.RWin
            )
            {
                return;
            }
            StringBuilder shortcutText = new();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                _hotkeysModifiers += 2;
                shortcutText.Append("Ctrl + ");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                _hotkeysModifiers += 1;
                shortcutText.Append("Alt + ");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                _hotkeysModifiers += 4;
                shortcutText.Append("Shift + ");
            }
            //backspace and delete 
            if (_hotkeysModifiers == 0 && (key == Key.Back || key == Key.Delete))
            {
                _hotkeysKey = 0;
                shortcutText.Clear();
                control.Text = _hotkeysText = "";
                return;
            }
            else if (_hotkeysModifiers == 0 && (key < Key.F1 || key > Key.F12))
            {
                ToastHelper.Show("单字符可能会影响使用", WindowType.Preference);
            }
            _hotkeysKey = (KeyCodes)KeyInterop.VirtualKeyFromKey(key);
            shortcutText.Append(key.ToString());
            control.Text = _hotkeysText = shortcutText.ToString();
        }

        #endregion Command

        #region 私有方法

        private void UpdateCnf(HotkeyEnum hke)
        {
            switch (hke)
            {
                case HotkeyEnum.InputHk:
                    conf.CurrentConfig!.Hotkeys!.InputTranslate.Modifiers = _hotkeysModifiers;
                    conf.CurrentConfig!.Hotkeys!.InputTranslate.Key = _hotkeysKey;
                    conf.CurrentConfig!.Hotkeys!.InputTranslate.Text = _hotkeysText.ToString();
                    break;

                case HotkeyEnum.CrosswordHk:
                    conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Modifiers = _hotkeysModifiers;
                    conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Key = _hotkeysKey;
                    conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Text = _hotkeysText.ToString();
                    break;

                case HotkeyEnum.ScreenshotHk:
                    conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Modifiers = _hotkeysModifiers;
                    conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Key = _hotkeysKey;
                    conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Text = _hotkeysText.ToString();
                    break;

                case HotkeyEnum.OpenHk:
                    conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Modifiers = _hotkeysModifiers;
                    conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Key = _hotkeysKey;
                    conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Text = _hotkeysText.ToString();
                    break;

                case HotkeyEnum.MousehookHk:
                    conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Modifiers = _hotkeysModifiers;
                    conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Key = _hotkeysKey;
                    conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Text = _hotkeysText.ToString();
                    break;

                case HotkeyEnum.OcrHk:
                    conf.CurrentConfig!.Hotkeys!.OCR.Modifiers = _hotkeysModifiers;
                    conf.CurrentConfig!.Hotkeys!.OCR.Key = _hotkeysKey;
                    conf.CurrentConfig!.Hotkeys!.OCR.Text = _hotkeysText.ToString();
                    break;

                case HotkeyEnum.SilentOcrHk:
                    conf.CurrentConfig!.Hotkeys!.SilentOCR.Modifiers = _hotkeysModifiers;
                    conf.CurrentConfig!.Hotkeys!.SilentOCR.Key = _hotkeysKey;
                    conf.CurrentConfig!.Hotkeys!.SilentOCR.Text = _hotkeysText.ToString();
                    break;

                case HotkeyEnum.ClipboardMonitorHk:
                    conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Modifiers = _hotkeysModifiers;
                    conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Key = _hotkeysKey;
                    conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Text = _hotkeysText.ToString();
                    break;

                default:
                    break;
            }
        }

        private void HotKeyConflictCheck()
        {
            InputHk.ContentVisible = conf.CurrentConfig!.Hotkeys!.InputTranslate.Conflict;
            CrosswordHk.ContentVisible = conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Conflict;
            ScreenshotHk.ContentVisible = conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Conflict;
            OpenHk.ContentVisible = conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Conflict;
            MousehookHk.ContentVisible = conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Conflict;
            OcrHk.ContentVisible = conf.CurrentConfig!.Hotkeys!.OCR.Conflict;
            SilentOcrHk.ContentVisible = conf.CurrentConfig!.Hotkeys!.SilentOCR.Conflict;
            ClipboardMonitorHk.ContentVisible = conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Conflict;
        }

        private void RefreshNotifyToolTip()
        {
            var msg = "";
            if (!conf.CurrentConfig!.Hotkeys!.InputTranslate.Conflict && !string.IsNullOrEmpty(conf.CurrentConfig!.Hotkeys!.InputTranslate.Text))
                msg += $"输入: {conf.CurrentConfig!.Hotkeys!.InputTranslate.Text}\r\n";
            if (!conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Conflict && !string.IsNullOrEmpty(conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Text))
                msg += $"划词: {conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Text}\r\n";
            if (!conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Conflict && !string.IsNullOrEmpty(conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Text))
                msg += $"截图: {conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Text}\r\n";
            if (!conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Conflict && !string.IsNullOrEmpty(conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Text))
                msg += $"显示: {conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Text}\r\n";
            if (!conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Conflict && !string.IsNullOrEmpty(conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Text))
                msg += $"鼠标: {conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Text}\r\n";
            if (!conf.CurrentConfig!.Hotkeys!.OCR.Conflict && !string.IsNullOrEmpty(conf.CurrentConfig!.Hotkeys!.OCR.Text))
                msg += $"识字: {conf.CurrentConfig!.Hotkeys!.OCR.Text}\r\n";
            if (!conf.CurrentConfig!.Hotkeys!.SilentOCR.Conflict && !string.IsNullOrEmpty(conf.CurrentConfig!.Hotkeys!.SilentOCR.Text))
                msg += $"静默: {conf.CurrentConfig!.Hotkeys!.SilentOCR.Text}\r\n";
            if (!conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Conflict && !string.IsNullOrEmpty(conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Text))
                msg += $"剪贴板: {conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Text}\r\n";
            Singleton<NotifyIconViewModel>.Instance.UpdateToolTip(msg.TrimEnd(['\r', '\n']));
        }

        #endregion 私有方法
    }
}