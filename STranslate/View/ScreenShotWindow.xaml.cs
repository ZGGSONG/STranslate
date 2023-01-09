using STranslate.ViewModel;
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
using System.Windows.Shapes;

namespace STranslate.View
{
    /// <summary>
    /// ScreenShotWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenShotWindow : Window
    {
        public ScreenShotWindow()
        {
            DataContext = new ScreenShotVM(this);

            var datas = (DataContext as ScreenShotVM).InitView1();
            
            InitializeComponent();

            (DataContext as ScreenShotVM).InitView2(datas);
        }
    }
}
