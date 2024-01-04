using STranslate.Util;
using STranslate.ViewModels;
using System;
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

            InitializeComponent();
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
