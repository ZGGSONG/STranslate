using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STranslate.Views;

public partial class InputView
{
    public InputView()
    {
        InitializeComponent();
    }

    private void InputTB_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var textBox = (TextBox)sender;

        // 检查是否按住 Ctrl 键，如果按住则进行缩放等特殊操作，否则进行滚动
        var currentFontSize = (double)Application.Current.Resources["FontSize_TextBox"];
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            // 根据滚轮事件更改字体大小
            if (e.Delta > 0)
            {
                if (currentFontSize < 40)
                {
                    currentFontSize += 2.0; // 根据需要调整放大的幅度
                }
            }
            else
            {
                if (currentFontSize > 10)
                {
                    currentFontSize -= 2.0; // 根据需要调整放大的幅度
                }
            }

            // 设置新的字体大小
            Application.Current.Resources["FontSize_TextBox"] = currentFontSize;
        }
        else//修复普通滚动
        {
            // 普通滚动
            if (e.Delta > 0)
            {
                // 向上滚动
                if (textBox.VerticalOffset > 0)
                {
                    textBox.ScrollToVerticalOffset(textBox.VerticalOffset - 30);
                }
            }
            else
            {
                // 向下滚动
                if (textBox.VerticalOffset < textBox.ExtentHeight - textBox.ViewportHeight)
                {
                    textBox.ScrollToVerticalOffset(textBox.VerticalOffset + 30);
                }
            }
        }

        // 防止事件继续传播
        e.Handled = true;
    }
}