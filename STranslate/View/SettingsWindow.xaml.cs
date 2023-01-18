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

        }
    }
}
