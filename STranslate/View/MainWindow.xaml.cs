using STranslate.Helper;
using STranslate.ViewModel;
using System;
using System.Windows;

namespace STranslate.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = vm;
            vm.Mainwin = this;

            InitialTray();

            //if (HotKeys.InputTranslate.Conflict || HotKeys.CrosswordTranslate.Conflict || HotKeys.ScreenShotTranslate.Conflict)
            //{
            //    MessageBox.Show("全局快捷键有冲突...");
            //}
        }

        /// <summary>
        /// 监听全局快捷键
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            this.Hide();

            HotkeysHelper.InitialHook(this);
            HotkeysHelper.Register(HotkeysHelper.InputTranslateId, () =>
            {
                vm.InputTranslate();
            });

            HotkeysHelper.Register(HotkeysHelper.CrosswordTranslateId, () =>
            {
                vm.CrossWordTranslate();
            });

            HotkeysHelper.Register(HotkeysHelper.ScreenShotTranslateId, () =>
            {
                vm.ScreenShotTranslate();
            });

            HotkeysHelper.Register(HotkeysHelper.OpenMainWindowId, () =>
            {
                vm.OpenMainWin();
            });
        }

        private MainVM vm = MainVM.Instance;

        public System.Windows.Forms.NotifyIcon NotifyIcon = new System.Windows.Forms.NotifyIcon();

        #region TrayIcon
        private void InitialTray()
        {
            var app = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location);
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            NotifyIcon.Text = $"{app} {version.Substring(0, version.Length - 2)}";
            NotifyIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("Images/translate.ico", UriKind.Relative)).Stream);
            NotifyIcon.Visible = true;
            NotifyIcon.BalloonTipText = $"{app} already started...";
            NotifyIcon.ShowBalloonTip(500);

            NotifyIcon.MouseDoubleClick += InputTranslateMenuItem_Click;

            System.Windows.Forms.MenuItem InputTranslateMenuItemBTN = new System.Windows.Forms.MenuItem("输入翻译");
            InputTranslateMenuItemBTN.Click += new EventHandler(InputTranslateMenuItem_Click);

            System.Windows.Forms.MenuItem ScreenshotTranslateMenuItemBTN = new System.Windows.Forms.MenuItem("截图翻译");
            ScreenshotTranslateMenuItemBTN.Click += new EventHandler(ScreenshotTranslateMenuItem_Click);

            System.Windows.Forms.MenuItem CrossWordTranslateMenuItemBTN = new System.Windows.Forms.MenuItem("划词翻译");
            //CrossWordTranslateMenuItemBTN.Click += new EventHandler(CrossWordTranslateMenuItem_Click);
            //当失去焦点后无法从托盘处获取选中文本
            CrossWordTranslateMenuItemBTN.Enabled = false;

            System.Windows.Forms.MenuItem OpenMainWinBTN = new System.Windows.Forms.MenuItem("显示主界面");
            OpenMainWinBTN.Click += new EventHandler(OpenMainWin_Click);

            System.Windows.Forms.MenuItem AutoStartBTN = new System.Windows.Forms.MenuItem("开机自启");
            AutoStartBTN.Click += new EventHandler(AutoStart_Click);

            AutoStartBTN.Checked = StartupHelper.IsStartup();

            System.Windows.Forms.MenuItem ExitBTN = new System.Windows.Forms.MenuItem("退出");
            ExitBTN.Click += new EventHandler(Exit_Click);

            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] {
                InputTranslateMenuItemBTN,
                ScreenshotTranslateMenuItemBTN,
                CrossWordTranslateMenuItemBTN,
                OpenMainWinBTN,
                AutoStartBTN,
                ExitBTN,
            };
            NotifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);
        }

        private void ScreenshotTranslateMenuItem_Click(object sender, EventArgs e)
        {
            vm.ScreenShotTranslate();
        }

        private void OpenMainWin_Click(object sender, EventArgs e)
        {
            vm.OpenMainWin();
        }

        private void AutoStart_Click(object sender, EventArgs e)
        {
            if (StartupHelper.IsStartup()) StartupHelper.UnSetStartup();
            else StartupHelper.SetStartup();
            (sender as System.Windows.Forms.MenuItem).Checked = StartupHelper.IsStartup();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            vm.ExitApp(0);
        }

        private void InputTranslateMenuItem_Click(object sender, EventArgs e)
        {
            vm.InputTranslate();
        }

        #endregion
    }
}