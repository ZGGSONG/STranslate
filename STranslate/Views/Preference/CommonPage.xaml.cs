using STranslate.Style.Commons;
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
    /// CommonPage.xaml 的交互逻辑
    /// </summary>
    public partial class CommonPage : UserControl
    {
        public CommonPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 限制输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AcceptOnlyNumber(object sender, KeyEventArgs e)
        {
            if (!IsAllowedKey(e.Key))
            {
                e.Handled = true;
            }
        }

        private bool IsAllowedKey(Key key)
        {
            return (key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9) || key == Key.Back || key == Key.Delete || key == Key.Left || key == Key.Right;
        }
    }
}
