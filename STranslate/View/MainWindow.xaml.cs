using STranslate.Helper;
using STranslate.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using System.Windows.Media;
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

            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            InitIcon();
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

            if (ViewModel.MainVM.Instance.NHotkeys.InputTranslate.Conflict
                || ViewModel.MainVM.Instance.NHotkeys.CrosswordTranslate.Conflict
                || ViewModel.MainVM.Instance.NHotkeys.ScreenShotTranslate.Conflict
                || ViewModel.MainVM.Instance.NHotkeys.OpenMainWindow.Conflict)
            {
                MessageBox.Show("全局快捷键有冲突，请前往软件首选项中修改...");
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            var o = sender as System.Windows.Controls.Button;
            toastTxt.Text = (o.ToolTip as System.Windows.Controls.ToolTip).Name;
            //创建一个一个对象，对两个值在时间线上进行动画处理（移动距离，移动到的位置）
            var da = new DoubleAnimation();
            //设定动画时间线
            da.Duration = new Duration(TimeSpan.FromSeconds(0.8));
            //设定移动动画的结束值，控件向下移动60个像素，向上移动则是-60
            da.To = 50;
            da.From = 0;
            da.AccelerationRatio = 0.2;
            da.DecelerationRatio = 0.8;
            da.AutoReverse = true;
            //btnFlash要进行动画操作的控件名
            Toast.RenderTransform = new TranslateTransform();
            //开始进行动画处理
            Toast.RenderTransform.BeginAnimation(TranslateTransform.YProperty, da);
        }

        private MainVM vm = MainVM.Instance;

        public readonly NotifyIcon NotifyIcon = new NotifyIcon();

        #region Initial TrayIcon
        private void InitIcon()
        {
            var stream = Application
                .GetResourceStream(new Uri("Images/translate.ico", UriKind.Relative))?.Stream;
            if (NotifyIcon.Icon != null)
            {
                NotifyIcon.Icon.Dispose();
            }
            if (stream != null)
            {
                NotifyIcon.Icon = new System.Drawing.Icon(stream);
            }
        }
        private void InitialTray()
        {
            var app = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            NotifyIcon.Text = $@"{app} {SettingsVM.Instance.Version}";
            InitIcon();
            NotifyIcon.Visible = true;
            NotifyIcon.BalloonTipText = $@"{app} already started...";
            NotifyIcon.ShowBalloonTip(500);

            NotifyIcon.MouseDoubleClick += (o, e) => vm.InputTranslate();

            var menuItems = new[]
            {
                new MenuItem("输入翻译", (o, e) => vm.InputTranslate()),
                new MenuItem("截图翻译", (o, e) => vm.ScreenShotTranslate()),
                new MenuItem("划词翻译") { Enabled = false },
                new MenuItem("-"),
                new MenuItem("显示主界面", (o, e) => vm.OpenMainWin()),
                new MenuItem("首选项", (o, e) => Preference()),
                new MenuItem("-"),
                new MenuItem("退出", (o, e) => vm.ExitApp(0)),
            };
            NotifyIcon.ContextMenu = new ContextMenu(menuItems);
        }

        /// <summary>
        /// 设置
        /// </summary>
        private void Preference()
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