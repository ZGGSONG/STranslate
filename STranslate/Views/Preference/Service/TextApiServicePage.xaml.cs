using STranslate.Model;
using System.Windows.Controls;

namespace STranslate.Views.Preference.Service
{
    /// <summary>
    /// TextApiServicePage.xaml 的交互逻辑
    /// </summary>
    public partial class TextApiServicePage : UserControl
    {
        public TextApiServicePage(ITranslator vm)
        {
            InitializeComponent();

            DataContext = vm;
        }

        /// <summary>
        /// 通过缓存加载View时刷新ViewModel
        /// </summary>
        /// <param name="vm"></param>
        public void UpdateVM(ITranslator vm)
        {
            DataContext = vm;
        }
    }
}
