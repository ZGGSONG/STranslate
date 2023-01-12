using STranslate.Helper;
using STranslate.ViewModel;
using System;
using System.IO;
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

        private string _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();


        public System.Windows.Forms.NotifyIcon NotifyIcon = new System.Windows.Forms.NotifyIcon();

        #region TrayIcon
        private void InitialTray()
        {
            _version = HandleVersion(_version);
            var app = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location);
            NotifyIcon.Text = $"{app} {_version}";
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

            System.Windows.Forms.MenuItem CheckUpdateBTN = new System.Windows.Forms.MenuItem("检查更新");
            CheckUpdateBTN.Click += CheckUpdateBTN_Click;

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
                CheckUpdateBTN,
                AutoStartBTN,
                ExitBTN,
            };
            NotifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);
        }

        private string HandleVersion(string version)
        {
            var ret = string.Empty;
            ret = version.Substring(0, version.Length - 2);
            var location = ret.LastIndexOf('.');
            ret = ret.Remove(location, 1);
            return ret;
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckUpdateBTN_Click(object sender, EventArgs e)
        {
            try
            {
                string updaterExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "Updater.exe");
                string updaterCacheExePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Updater",
                    "Updater.exe");
                string updateDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater");
                if (!Directory.Exists(updateDirPath))
                {
                    Directory.CreateDirectory(updateDirPath);
                }

                if (!File.Exists(updaterExePath))
                {
                    MessageBox.Show("升级程序似乎已被删除，请手动前往发布页查看新版本");
                    return;
                }
                File.Copy(updaterExePath, updaterCacheExePath, true);

                File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Newtonsoft.Json.dll"), Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Updater",
                    "Newtonsoft.Json.dll"), true);

                ProcessHelper.Run(updaterCacheExePath, new string[] { _version });
            }
            catch (Exception ex)
            {

                MessageBox.Show($"无法正确启动检查更新程序\n{ex.Message}");
            }
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