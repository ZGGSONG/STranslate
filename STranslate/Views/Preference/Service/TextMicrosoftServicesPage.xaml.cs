using STranslate.Model;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace STranslate.Views.Preference.Service
{
    public partial class TextMicrosoftServicesPage : UserControl
    {
        public TextMicrosoftServicesPage(ITranslator vm)
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
            Process.Start(new ProcessStartInfo { FileName = "https://azure.microsoft.com/zh-cn/products/ai-services/ai-translator", UseShellExecute = true });
    }
}
