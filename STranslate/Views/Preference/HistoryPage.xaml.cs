using STranslate.Util;
using STranslate.ViewModels.Preference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STranslate.Views.Preference
{
    /// <summary>
    /// HistoryPage.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryPage : UserControl
    {
        public HistoryPage()
        {
            InitializeComponent();
            DataContext = Singleton<HistoryViewModel>.Instance;
        }

        private void HistoryListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //按住Ctrl滚动时不将滚动冒泡给上一层级的控件
            if (!e.Handled && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // ListBox拦截鼠标滚轮事件
                e.Handled = true;

                // 激发一个鼠标滚轮事件，冒泡给外层ListBox接收到
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = sender
                };
                var parent = ((Control)sender).Parent as UIElement;
                parent!.RaiseEvent(eventArg);
            }
        }
    }
}
