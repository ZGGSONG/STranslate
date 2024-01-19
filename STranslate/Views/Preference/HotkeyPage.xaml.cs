using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Style.Controls;
using STranslate.Util;
using STranslate.ViewModels;
using STranslate.ViewModels.Preference;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STranslate.Views.Preference
{
    /// <summary>
    /// HotkeyPage.xaml 的交互逻辑
    /// </summary>
    public partial class HotkeyPage : UserControl
    {
        private readonly ConfigHelper conf = Singleton<ConfigHelper>.Instance;

        private KeyModifiers _hotkeysModifiers;
        private KeyCodes _hotkeysKey;
        private string _hotkeysText = string.Empty;

        public HotkeyPage()
        {
            InitializeComponent();
            DataContext = Singleton<HotkeyViewModel>.Instance;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Text = conf.CurrentConfig!.Hotkeys!.InputTranslate.Text;
            CrossWordTextBox.Text = conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Text;
            ScreenshotTextBox.Text = conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Text;
            OpenMainWindowTextBox.Text = conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Text;
            MousehookTextBox.Text = conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Text;
            OCRTextBox.Text = conf.CurrentConfig!.Hotkeys!.OCR.Text;
            SilentOCRTextBox.Text = conf.CurrentConfig!.Hotkeys!.SilentOCR.Text;
            HotKeyConflictCheck();
        }

        private void HotKeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
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
            StringBuilder shortcutText = new StringBuilder();
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
            if (_hotkeysModifiers == 0 && (key < Key.F1 || key > Key.F12))
            {
                _hotkeysKey = 0;
                shortcutText.Clear();
                ((TextBox)sender).Text = _hotkeysText = "";
                return;
            }
            _hotkeysKey = (KeyCodes)KeyInterop.VirtualKeyFromKey(key);
            shortcutText.Append(key.ToString());
            ((TextBox)sender).Text = _hotkeysText = shortcutText.ToString();
        }

        private void Input_KeyUp(object sender, KeyEventArgs e)
        {
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
            conf.CurrentConfig!.Hotkeys!.InputTranslate.Modifiers = _hotkeysModifiers;
            conf.CurrentConfig!.Hotkeys!.InputTranslate.Key = _hotkeysKey;
            conf.CurrentConfig!.Hotkeys!.InputTranslate.Text = _hotkeysText.ToString();
            HotkeyHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
            RefreshNotifyToolTip();
        }

        private void CrossWord_KeyUp(object sender, KeyEventArgs e)
        {
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
            conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Modifiers = _hotkeysModifiers;
            conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Key = _hotkeysKey;
            conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Text = _hotkeysText.ToString();
            HotkeyHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
            RefreshNotifyToolTip();
        }

        private void Screenshot_KeyUp(object sender, KeyEventArgs e)
        {
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
            conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Modifiers = _hotkeysModifiers;
            conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Key = _hotkeysKey;
            conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Text = _hotkeysText.ToString();
            HotkeyHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
            RefreshNotifyToolTip();
        }

        private void ShowMainwin_KeyUp(object sender, KeyEventArgs e)
        {
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
            conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Modifiers = _hotkeysModifiers;
            conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Key = _hotkeysKey;
            conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Text = _hotkeysText.ToString();
            HotkeyHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
            RefreshNotifyToolTip();
        }

        private void Mousehook_KeyUp(object sender, KeyEventArgs e)
        {
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
            conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Modifiers = _hotkeysModifiers;
            conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Key = _hotkeysKey;
            conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Text = _hotkeysText.ToString();
            HotkeyHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
            RefreshNotifyToolTip();
        }

        private void OCR_KeyUp(object sender, KeyEventArgs e)
        {
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
            conf.CurrentConfig!.Hotkeys!.OCR.Modifiers = _hotkeysModifiers;
            conf.CurrentConfig!.Hotkeys!.OCR.Key = _hotkeysKey;
            conf.CurrentConfig!.Hotkeys!.OCR.Text = _hotkeysText.ToString();
            HotkeyHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
            RefreshNotifyToolTip();
        }

        private void SilentOCR_KeyUp(object sender, KeyEventArgs e)
        {
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
            conf.CurrentConfig!.Hotkeys!.SilentOCR.Modifiers = _hotkeysModifiers;
            conf.CurrentConfig!.Hotkeys!.SilentOCR.Key = _hotkeysKey;
            conf.CurrentConfig!.Hotkeys!.SilentOCR.Text = _hotkeysText.ToString();
            HotkeyHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
            RefreshNotifyToolTip();
        }

        private void HotKeyConflictCheck()
        {
            InputHotKeyConflictLabel.Visibility = conf.CurrentConfig!.Hotkeys!.InputTranslate.Conflict
                ? Visibility.Visible
                : Visibility.Hidden;
            CrossWordHotKeyConflictLabel.Visibility = conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Conflict
                ? Visibility.Visible
                : Visibility.Hidden;
            ScreenshotHotKeyConflictLabel.Visibility = conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Conflict
                ? Visibility.Visible
                : Visibility.Hidden;
            ShowMainwinHotKeyConflictLabel.Visibility = conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Conflict
                ? Visibility.Visible
                : Visibility.Hidden;
            MousehookHotKeyConflictLabel.Visibility = conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Conflict
                ? Visibility.Visible
                : Visibility.Hidden;
            OCRHotKeyConflictLabel.Visibility = conf.CurrentConfig!.Hotkeys!.OCR.Conflict
                ? Visibility.Visible
                : Visibility.Hidden;
            SilentOCRHotKeyConflictLabel.Visibility = conf.CurrentConfig!.Hotkeys!.SilentOCR.Conflict
                ? Visibility.Visible
                : Visibility.Hidden;
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
            Singleton<NotifyIconViewModel>.Instance.UpdateToolTip(msg.TrimEnd(['\r', '\n']));
        }

        private void ResetHoskeys(object sender, RoutedEventArgs e)
        {
            var cHotkeys = conf.ResetConfig.Hotkeys!;

            CrossWordTextBox.Text = cHotkeys.CrosswordTranslate.Text;
            InputTextBox.Text = cHotkeys.InputTranslate.Text;
            ScreenshotTextBox.Text = cHotkeys.ScreenShotTranslate.Text;
            OpenMainWindowTextBox.Text = cHotkeys.OpenMainWindow.Text;
            MousehookTextBox.Text = cHotkeys.MousehookTranslate.Text;
            OCRTextBox.Text = cHotkeys.OCR.Text;
            SilentOCRTextBox.Text = cHotkeys.SilentOCR.Text;

            conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Modifiers = cHotkeys.CrosswordTranslate.Modifiers;
            conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Key = cHotkeys.CrosswordTranslate.Key;
            conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Text = cHotkeys.CrosswordTranslate.Text;

            conf.CurrentConfig!.Hotkeys!.InputTranslate.Modifiers = cHotkeys.InputTranslate.Modifiers;
            conf.CurrentConfig!.Hotkeys!.InputTranslate.Key = cHotkeys.InputTranslate.Key;
            conf.CurrentConfig!.Hotkeys!.InputTranslate.Text = cHotkeys.InputTranslate.Text;

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

            HotkeyHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
            RefreshNotifyToolTip();

            ToastHelper.Show("重置成功", WindowType.Preference);
        }

        private void SaveHotkeys(object sender, RoutedEventArgs e)
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
    }
}