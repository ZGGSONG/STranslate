using System.Windows;
using System.Windows.Controls;

namespace STranslate.Views;

/// <summary>
///     ToastView.xaml 的交互逻辑
/// </summary>
public partial class ToastView : UserControl
{
    public ToastView()
    {
        InitializeComponent();
    }

    public ToastView(VerticalAlignment vertical)
    {
        InitializeComponent();
        VerticalAlignment = vertical;
    }

    public string ToastText
    {
        get => toastText.Text;
        set => toastText.Text = value;
    }
}