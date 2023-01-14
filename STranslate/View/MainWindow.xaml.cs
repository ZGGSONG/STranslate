using STranslate.Helper;
using STranslate.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

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


        public readonly NotifyIcon NotifyIcon = new NotifyIcon();

        #region Initial TrayIcon
        private void InitialTray()
        {
            _version = HandleVersion(_version);
            var app = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            NotifyIcon.Text = $@"{app} {_version}";
            var stream = Application
                .GetResourceStream(new Uri("Images/translate.ico", UriKind.Relative))?.Stream;
            if (stream != null)
                NotifyIcon.Icon = new System.Drawing.Icon(stream);
            NotifyIcon.Visible = true;
            NotifyIcon.BalloonTipText = $@"{app} already started...";
            NotifyIcon.ShowBalloonTip(500);

            NotifyIcon.MouseDoubleClick += InputTranslateMenuItem_Click;
            var inputTranslateMenuItemBtn = new MenuItem("输入翻译\tAlt+A");
            inputTranslateMenuItemBtn.Click += InputTranslateMenuItem_Click;

            var screenshotTranslateMenuItemBtn = new MenuItem("截图翻译\tAlt+S");
            screenshotTranslateMenuItemBtn.Click += ScreenshotTranslateMenuItem_Click;

            var crossWordTranslateMenuItemBtn = new MenuItem("划词翻译\tAlt+D");
            //CrossWordTranslateMenuItemBTN.Click += CrossWordTranslateMenuItem_Click;
            //当失去焦点后无法从托盘处获取选中文本
            crossWordTranslateMenuItemBtn.Enabled = false;

            var openMainWinBtn = new MenuItem("显示主界面\tAlt+G");
            openMainWinBtn.Click += OpenMainWin_Click;

            var checkUpdateBtn = new MenuItem("检查更新");
            checkUpdateBtn.Click += CheckUpdateBTN_Click;

            var autoStartBtn = new MenuItem("开机自启");
            autoStartBtn.Click += AutoStart_Click;

            var preferenceBtn = new MenuItem("首选项");
            preferenceBtn.Click += Preference_Click;

            autoStartBtn.Checked = StartupHelper.IsStartup();

            var exitBtn = new MenuItem("退出");
            exitBtn.Click += Exit_Click;

            var items = new MenuItem[] {
                inputTranslateMenuItemBtn,
                screenshotTranslateMenuItemBtn,
                crossWordTranslateMenuItemBtn,
                openMainWinBtn,
                checkUpdateBtn,
                autoStartBtn,
                preferenceBtn,
                exitBtn,
            };
            NotifyIcon.ContextMenu = new ContextMenu(items);
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Preference_Click(object sender, EventArgs e)
        {
            var setting = new SettingsWindow();
            setting.Show();
            setting.Activate();
        }

        /// <summary>
        /// 同步Github版本命名
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static string HandleVersion(string version)
        {
            var ret = string.Empty;
            ret = version.Substring(0, version.Length - 2);
            var location = ret.LastIndexOf('.');
            ret = ret.Remove(location, 1);
            return ret;
        }

        /// <summary>
        /// 检查更新 by https://github.com/Planshit/Tai
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckUpdateBTN_Click(object sender, EventArgs e)
        {
            try
            {
                var updaterExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "Updater.exe");
                var updaterCacheExePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Updater",
                    "Updater.exe");
                var updateDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater");
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
            ((MenuItem)sender).Checked = StartupHelper.IsStartup();
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