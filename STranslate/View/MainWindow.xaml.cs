using STranslate.Helper;
using STranslate.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace STranslate.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainVM vm;
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainVM(this);
            vm = (MainVM)DataContext;


            //if (HotKeys.InputTranslate.Conflict || HotKeys.CrosswordTranslate.Conflict || HotKeys.ScreenShotTranslate.Conflict)
            //{
            //    MessageBox.Show("全局快捷键有冲突，请您到设置中重新设置");
            //}
        }

        /// <summary>
        /// 监听全局快捷键
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            //base.OnSourceInitialized(e);
            HotkeysHelper.InitialHook(this);
            HotkeysHelper.Register(HotkeysHelper.InputTranslateId, () =>
            {
                vm.InputTranslate();
            });

            HotkeysHelper.Register(HotkeysHelper.CrosswordTranslateId, () =>
            {
                vm.CrossWordTranslate();
            });

            //HotkeysHelper.Register(HotkeysHelper.ScreenShotTranslateId, () =>
            //{
            //    ScreenshotTranslateMenuItem_Click(null, null);
            //});

            HotkeysHelper.Register(HotkeysHelper.OpenMainWindowId, () =>
            {
                vm.OpenMainWin();
            });
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
        /// 非激活窗口则隐藏起来
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (vm == null) return;
            if (!vm.IsTopmost)
            {
                vm.Speech.SpeakAsyncCancelAll();
                this.Hide();
            }
        }

        private void Mwin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HotkeysHelper.UnRegisterHotKey();
        }
    }
}