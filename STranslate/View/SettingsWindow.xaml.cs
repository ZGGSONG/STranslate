using STranslate.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STranslate.View
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

#if DEBUG
            this.window.Topmost = true;
#endif

            DataContext = ViewModel.SettingsVM.Instance;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            this.InputTextBox.Text = ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Text;
            this.CrossWordTextBox.Text = ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Text;
            this.ScreenshotTextBox.Text = ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Text;
            this.ShowMainwinTextBox.Text = ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Text;
            HotKeyConflictCheck();
        }

        private byte _hotkeysModifiers;
        private int _hotkeysKey;
        private string _hotkeysText = string.Empty;

        private void HotKeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            _hotkeysModifiers = 0;
            _hotkeysKey = 0;
            _hotkeysText = "";
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }
            StringBuilder shortcutText = new StringBuilder();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                _hotkeysModifiers += 2;
                shortcutText.Append("Ctrl + ");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                _hotkeysModifiers += 4;
                shortcutText.Append("Shift + ");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                _hotkeysModifiers += 1;
                shortcutText.Append("Alt + ");
            }
            if (_hotkeysModifiers == 0 && (key < Key.F1 || key > Key.F12))
            {
                _hotkeysKey = 0;
                shortcutText.Clear();
                ((System.Windows.Controls.TextBox)sender).Text = _hotkeysText = "";
                return;
            }
            _hotkeysKey = KeyInterop.VirtualKeyFromKey(key);
            shortcutText.Append(key.ToString());
            ((System.Windows.Controls.TextBox)sender).Text = _hotkeysText = shortcutText.ToString();
        }
        private void CrossWord_KeyUp(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }
            ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Modifiers = _hotkeysModifiers;
            ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Key = _hotkeysKey;
            ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Text = _hotkeysText.ToString();
            HotkeysHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
        }
        private void Input_KeyUp(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }
            ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Modifiers = _hotkeysModifiers;
            ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Key = _hotkeysKey;
            ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Text = _hotkeysText.ToString();
            HotkeysHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
        }
        private void Screenshot_KeyUp(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }
            ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Modifiers = _hotkeysModifiers;
            ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Key = _hotkeysKey;
            ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Text = _hotkeysText.ToString();
            HotkeysHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
        }
        private void ShowMainwin_KeyUp(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }
            ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Modifiers = _hotkeysModifiers;
            ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Key = _hotkeysKey;
            ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Text = _hotkeysText.ToString();
            HotkeysHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
        }

        private void ResetHoskeys_Click(object sender, RoutedEventArgs e)
        {
            CrossWordTextBox.Text = "Alt + D";
            InputTextBox.Text = "Alt + A";
            ScreenshotTextBox.Text = "Alt + S";
            ShowMainwinTextBox.Text = "Alt + G";

            ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Modifiers = 1;
            ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Key = 68;
            ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Text = "Alt + D";

            ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Modifiers = 1;
            ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Key = 65;
            ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Text = "Alt + A";

            ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Modifiers = 1;
            ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Key = 83;
            ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Text = "Alt + S";

            ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Modifiers = 1;
            ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Key = 71;
            ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Text = "Alt + G";

            HotkeysHelper.ReRegisterHotKey();
            HotKeyConflictCheck();
        }
        private void HotKeyConflictCheck()
        {
            this.CrossWordHotKeyConflictLabel.Visibility = ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Conflict ? Visibility.Visible : Visibility.Hidden;
            this.ScreenshotHotKeyConflictLabel.Visibility = ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Conflict ? Visibility.Visible : Visibility.Hidden;
            this.InputHotKeyConflictLabel.Visibility = ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Conflict ? Visibility.Visible : Visibility.Hidden;
            this.ShowMainwinHotKeyConflictLabel.Visibility = ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Conflict ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
