using STranslate.Log;
using STranslate.Util;
using STranslate.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STranslate.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly MainViewModel vm = Singleton<MainViewModel>.Instance;

        public MainView()
        {
            DataContext = vm;

            vm.NotifyIconVM.OnExit += UnLoadPosition;

            InitializeComponent();

            LoadPosition();
        }

        private void UnLoadPosition()
        {
            //写入配置
            if (!Singleton<ConfigHelper>.Instance.WriteConfig(Left, Top))
            {
                LogService.Logger.Debug($"保存位置({Left},{Top})失败...");
            }
        }

        private void LoadPosition()
        {
            var position = Singleton<ConfigHelper>.Instance.CurrentConfig?.Position;
            try
            {
                var args = position?.Split(',');
                if (string.IsNullOrEmpty(position) || args?.Length != 2)
                {
                    throw new Exception();
                }

                bool ret = true;
                ret &= double.TryParse(args[0], out var left);
                ret &= double.TryParse(args[1], out var top);
                if (!ret) throw new Exception();

                Left = left;
                Top = top;
            }
            catch (Exception)
            {
                Top = (SystemParameters.WorkArea.Height - Height) / 2;
                Left = (SystemParameters.WorkArea.Width - Width) / 2;

                LogService.Logger.Warn($"加载上次窗口位置({position})失败，启用默认位置");
            }
        }

        private void Mwin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 开始拖动窗体
            DragMove();
        }

        private void Mwin_Deactivated(object sender, EventArgs e)
        {
            if (!Topmost)
            {
                Hide();
            }
        }

        /// <summary>
        /// 保证每次打开都激活输入框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mwin_Activated(object sender, EventArgs e)
        {
            if (InputView.FindName("InputTB") is TextBox tb)
            {
                // 执行激活控件的操作，例如设置焦点
                tb.Focus();

                //光标移动至末尾
                tb.CaretIndex = tb.Text?.Length ?? 0;

                //tb?.SelectAll();
            }
        }
    }
}
