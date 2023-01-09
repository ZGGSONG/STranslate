using STranslate.Helper;
using STranslate.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace STranslate
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

                //取消置顶
                if (_IsTopmost)
                {
                    TopmostBtn.SetResourceReference(TemplateProperty, _UnTopmostTemplateName);
                    _IsTopmost = !_IsTopmost;
                }
            }
            //置顶 Ctrl+Shift+T
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)
                && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift)
                && e.Key == Key.T)
            {
                Top_Click(null, null);
            }
            //退出 Ctrl+Shift+Q
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control)
                && e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift)
                && e.Key == Key.Q)
            {
                vm.ExitApp();
            }

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
        /// 非激活窗口则隐藏起来
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (!_IsTopmost)
            {
                vm.Speech.SpeakAsyncCancelAll();
                this.Hide();
            }
        }

        /// <summary>
        /// 是否置顶(不好拆)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Top_Click(object sender, RoutedEventArgs e)
        {
            if (_IsTopmost)
            {
                TopmostBtn.SetResourceReference(TemplateProperty, _UnTopmostTemplateName);
            }
            else
            {
                TopmostBtn.SetResourceReference(TemplateProperty, _TopmostTemplateName);
            }
            _IsTopmost = !_IsTopmost;
        }
        private bool _IsTopmost { get; set; }
        private readonly string _TopmostTemplateName = "ButtonTemplateTopmost";
        private readonly string _UnTopmostTemplateName = "ButtonTemplateUnTopmost";

        private void SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(vm?.InputTxt))
            {
                vm.IdentifyLanguage = string.Empty;
                _ = vm?.Translate();
            }
        }
    }
}