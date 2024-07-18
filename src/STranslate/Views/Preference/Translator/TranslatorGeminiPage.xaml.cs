using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using STranslate.Model;

namespace STranslate.Views.Preference.Translator;

public partial class TranslatorGeminiPage : UserControl
{
    public TranslatorGeminiPage(ITranslator vm)
    {
        InitializeComponent();

        DataContext = vm;
    }

    /// <summary>
    ///     通过缓存加载View时刷新ViewModel
    /// </summary>
    /// <param name="vm"></param>
    public void UpdateVM(ITranslator vm)
    {
        DataContext = vm;
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
            { FileName = "https://makersuite.google.com/app/apikey", UseShellExecute = true });
    }

    /// <summary>
    ///     限制输入数字
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AcceptOnlyNumber(object sender, KeyEventArgs e)
    {
        if (!IsAllowedKey(e.Key)) e.Handled = true;
    }

    private bool IsAllowedKey(Key key)
    {
        return (key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9) || key == Key.Back ||
               key == Key.Delete || key == Key.Left || key == Key.Right;
    }

    /// <summary>
    ///     ListBox鼠标滚轮事件处理函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        //按住Ctrl滚动时不将滚动冒泡给上一层级的控件
        if (!e.Handled && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
        {
            // ListBox拦截鼠标滚轮事件
            e.Handled = true;

            // 激发一个鼠标滚轮事件，冒泡给外层ListBox接收到
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                { RoutedEvent = MouseWheelEvent, Source = sender };
            var parent = ((Control)sender).Parent as UIElement;
            parent!.RaiseEvent(eventArg);
        }
    }
}