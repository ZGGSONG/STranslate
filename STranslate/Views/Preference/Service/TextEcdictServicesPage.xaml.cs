using STranslate.Model;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace STranslate.Views.Preference.Service
{
    public partial class TextEcdictServicesPage : UserControl
    {
        public TextEcdictServicesPage(ITranslator vm)
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

        private void Hyperlink_Click(object sender, RoutedEventArgs e) =>
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/skywind3000/ECDICT", UseShellExecute = true });
    }
}
