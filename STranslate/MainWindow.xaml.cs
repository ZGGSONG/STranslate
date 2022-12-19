using STranslate.Utils;
using STranslate.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace STranslate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            InitHwnd();
            InitTray();

            this.DataContext = MainVM.Instance;
        }

        private void InitHwnd()
        {
            var helper = new WindowInteropHelper(this);
            helper.EnsureHandle();
        }

        /// <summary>
        /// 初始化托盘
        /// </summary>
        private void InitTray()
        {
            //notifyIcon.BalloonTipText = "程序开始运行";
            //notifyIcon.ShowBalloonTip(1000);
            notifyIcon.Text = "STranslate";
            notifyIcon.Icon = new System.Drawing.Icon(@"D:\CodePepo\STranslate\STranslate\Images\translate2.ico");
            notifyIcon.Visible = true;

            System.Windows.Forms.MenuItem inputTranslateButton = new System.Windows.Forms.MenuItem("输入翻译");
            inputTranslateButton.Click += new EventHandler(InputTranslate_Click);

            System.Windows.Forms.MenuItem crosswordTranslateButton = new System.Windows.Forms.MenuItem("划词翻译");
            crosswordTranslateButton.Click += new EventHandler(CrosswordTranslate_Click);

            System.Windows.Forms.MenuItem screenshotTranslationButton = new System.Windows.Forms.MenuItem("截图翻译");
            screenshotTranslationButton.Click += new EventHandler(ScreenshotTranslation_Click);

            System.Windows.Forms.MenuItem openMainWinButton = new System.Windows.Forms.MenuItem("显示主窗口");
            openMainWinButton.Click += new EventHandler(OpenMainWin_Click);

            System.Windows.Forms.MenuItem exitButton = new System.Windows.Forms.MenuItem("退出");
            exitButton.Click += new EventHandler(Exit_Click);

            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] {
                inputTranslateButton,
                crosswordTranslateButton,
                screenshotTranslationButton,
                openMainWinButton,
                exitButton
            };
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);
        }
        /// <summary>
        /// 显示主窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenMainWin_Click(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
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
        /// 软件运行时快捷键
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
            //退出 Ctrl+Shift+Q
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)
                && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift)
                && e.Key == Key.Q)
            {
                Exit_Click(null, null);
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
        /// <summary>
        /// 监听全局快捷键
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            HotKeysUtil2.RegisterHotKey(handle);

            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(WndProc);

#if false
            HotkeysUtil.InitialHook(this);
            HotkeysUtil.Regist(HotkeyModifiers.MOD_ALT, Key.A, () =>
            {
                this.Show();
                this.Activate();
                this.TextBoxInput.Focus();
            });
            HotkeysUtil.Regist(HotkeyModifiers.MOD_ALT, Key.D, () =>
            {
                //复制内容
                KeyboardUtil.Press(Key.LeftCtrl);
                KeyboardUtil.Type(Key.C);
                KeyboardUtil.Release(Key.LeftCtrl);
                System.Threading.Thread.Sleep(200);
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
        /// <summary>
        /// 热键的功能
        /// </summary>
        /// <param name="m"></param>
        protected IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handle)
        {
            switch (msg)
            {
                case 0x0312: //这个是window消息定义的 注册的热键消息
                    //Console.WriteLine(wParam.ToString());
                    if (wParam.ToString().Equals(HotKeysUtil2.InputTranslateId + ""))
                    {
                        this.InputTranslate_Click(null, null);
                    }
                    else if (wParam.ToString().Equals(HotKeysUtil2.CrosswordTranslateId + ""))
                    {
                        this.CrosswordTranslate_Click(null, null);
                    }
                    else if (wParam.ToString().Equals(HotKeysUtil2.ScreenShotTranslateId + ""))
                    {
                        this.ScreenshotTranslation_Click(null, null);
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 非激活窗口则隐藏起来
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// 截图翻译
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenshotTranslation_Click(object sender, EventArgs e)
        {
            MessageBox.Show("开发中");
        }

        /// <summary>
        /// 划词翻译
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CrosswordTranslate_Click(object sender, EventArgs e)
        {
            ClearTextBox();
            var sentence = GetWords.Get();
            this.Show();
            this.Activate();
            this.TextBoxInput.Text = sentence.Trim();
            _ = MainVM.Instance.Translate();
        }

        /// <summary>
        /// 输入翻译
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputTranslate_Click(object sender, EventArgs e)
        {
            ClearTextBox();
            Show();
            Activate();
            TextBoxInput.Focus();
        }

        /// <summary>
        /// 退出程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Click(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            Environment.Exit(0);
        }

        /// <summary>
        /// 清空输入输出框
        /// </summary>
        private void ClearTextBox()
        {
            MainVM.Instance.InputTxt = string.Empty;
            MainVM.Instance.OutputTxt = string.Empty;

        }
        /// <summary>
        /// 托盘图标
        /// </summary>
        private System.Windows.Forms.NotifyIcon notifyIcon = new System.Windows.Forms.NotifyIcon();
    }
}