using STranslate.Utils;
using STranslate.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STranslate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 监听全局快捷键
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            HotkeysUtil.InitialHook(this);
            HotkeysUtil.Regist(HotkeyModifiers.MOD_ALT, Key.A, () =>
            {
                this.Show();
                this.Activate();
                this.TextBoxInput.Focus();
            });
#if false
            HotkeysUtil.Regist(HotkeyModifiers.MOD_ALT, Key.D, () =>
            {
                //复制内容
                KeyboardUtil.Press(Key.LeftCtrl);
                KeyboardUtil.Type(Key.C);
                KeyboardUtil.Release(Key.LeftCtrl);
                System.Diagnostics.Debug.Print(Clipboard.GetText());

                //this.Show();
                //this.Activate();
                //this.TextBoxInput.Text = "123";

                //this.TextBoxInput.Text = Clipboard.GetText();
                //this.TextBoxInput.Focus();

                //KeyboardUtil.Type(Key.Enter);
            });
#endif
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = MainVM.Instance;
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// 快捷键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //最小化 Esc
            if (e.Key == Key.Escape)
            {
                this.Hide();
                MainVM.Instance.InputTxt = string.Empty;
                MainVM.Instance.OutputTxt = string.Empty;
            }
            //退出 Ctrl+Q
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.Q)
            {
                Application.Current.Shutdown();
            }
#if false
            //置顶/取消置顶 Ctrl+T
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.T)
            {
                Topmost = Topmost != true;
                Opacity = Topmost ? 1 : 0.9;
            }
            //缩小 Ctrl+[
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.OemOpenBrackets)
            {
                if (Width < 245)
                {
                    return;
                }
                Width /= 1.2;
                Height /= 1.2;
            }
            //放大 Ctrl+]
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.OemCloseBrackets)
            {
                if (Width > 600)
                {
                    return;
                }
                Width *= 1.2;
                Height *= 1.2;
            }
            //恢复界面大小 Ctrl+P
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && e.Key == Key.P)
            {
                Width = 400;
                Height = 450;
            }
#endif
        }

        private void NotifyIcon_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.Activate();
        }

        /// <summary>
        /// 非激活窗口则隐藏起来
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Hide();
            MainVM.Instance.InputTxt = string.Empty;
            MainVM.Instance.OutputTxt = string.Empty;
        }
    }
}
