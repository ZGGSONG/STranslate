using STranslate.Model;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace STranslate.Views.Preference.OCR
{
    public partial class PaddleOCRPage : UserControl
    {
        public PaddleOCRPage(IOCR vm)
        {
            InitializeComponent();

            DataContext = vm;
        }

        /// <summary>
        /// 通过缓存加载View时刷新ViewModel
        /// </summary>
        /// <param name="vm"></param>
        public void UpdateVM(IOCR vm)
        {
            DataContext = vm;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) =>
            Process.Start(new ProcessStartInfo { FileName = "https://gitee.com/raoyutian/PaddleOCRSharp", UseShellExecute = true });
    }
}
