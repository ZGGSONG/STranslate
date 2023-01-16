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

        public readonly NotifyIcon NotifyIcon = new NotifyIcon();

        #region Initial TrayIcon
        private void InitialTray()
        {
            var app = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            NotifyIcon.Text = $@"{app} {SettingsVM.Instance.Version}";
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

            var preferenceBtn = new MenuItem("首选项");
            preferenceBtn.Click += Preference_Click;

            var exitBtn = new MenuItem("退出");
            exitBtn.Click += Exit_Click;

            var items = new MenuItem[] {
                inputTranslateMenuItemBtn,
                screenshotTranslateMenuItemBtn,
                crossWordTranslateMenuItemBtn,
                openMainWinBtn,
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
            SettingsWindow window = null;
            foreach (Window item in Application.Current.Windows)
            {
                if (item is SettingsWindow)
                {
                    window = (SettingsWindow)item;
                    window.WindowState = WindowState.Normal;
                    window.Activate();
                    break;
                }
            }
            if (window == null)
            {
                window = new SettingsWindow();
                window.Show();
                window.Activate();
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