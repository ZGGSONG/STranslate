using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using STranslate.Model;

namespace STranslate.Style.Controls;

/// <summary>
///     MessageBox_S.xaml 的交互逻辑
/// </summary>
public partial class MessageBox_S : Window
{
    /// <summary>
    ///     默认标题名为主项目程序集名称
    /// </summary>
    private readonly string PruductName = ConstStr.AppName;

    /// <summary>
    ///     默认按钮
    /// </summary>
    private readonly MessageBoxButton CurrentButton = MessageBoxButton.OK;

    public MessageBox_S(string msg)
    {
        InitializeComponent();
        Messages.Text = msg;
        TitleTxt.Text = PruductName;
        OkBTN.Click += OkBTN_Click;
        CancelBTN.Visibility = Visibility.Collapsed;
    }

    public MessageBox_S(string msg, string caption)
    {
        InitializeComponent();

        CancelBTN.Visibility = Visibility.Collapsed;
        OkBTN.Click += OkBTN_Click;
        Messages.Text = msg;
        TitleTxt.Text = caption;
    }

    public MessageBox_S(string msg, string caption, MessageBoxButton messageBoxButton)
    {
        InitializeComponent();

        Messages.Text = msg;
        TitleTxt.Text = caption;
        CurrentButton = messageBoxButton;
        switch (CurrentButton)
        {
            case MessageBoxButton.OK:
                OkBTN.Content = "确认";
                CancelBTN.Visibility = Visibility.Collapsed;
                OkBTN.Click += OkBTN_Click;
                break;

            case MessageBoxButton.OKCancel:
                OkBTN.Content = "确认";
                CancelBTN.Content = "取消";
                OkBTN.Click += OkBTN_Click;
                CancelBTN.Click += CancelBTN_Click;
                break;

            case MessageBoxButton.YesNo:
                OkBTN.Content = "是";
                CancelBTN.Content = "否";
                OkBTN.Click += OkBTN_Click;
                CancelBTN.Click += CancelBTN_Click;
                break;
        }
    }

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

    public static bool? Show(string msg)
    {
        bool? result = null;
        var messageBox = new MessageBox_S(msg);

        result = messageBox.ShowDialog();

        return result;
    }

    /// <summary>
    ///     确定取消
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="caption"></param>
    /// <returns></returns>
    public static bool? Show(string msg, string caption)
    {
        bool? result = false;
        var messageBox = new MessageBox_S(msg, caption);

        messageBox.ShowDialog();

        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="caption"></param>
    /// <param name="messageBoxButton"></param>
    /// <returns></returns>
    [STAThread]
    public static MessageBoxResult Show(string msg, string caption, MessageBoxButton messageBoxButton)
    {
        var messageBoxResult = MessageBoxResult.Yes;

        var messageBox = new MessageBox_S(msg, caption, messageBoxButton);

        messageBox.ShowDialog();
        try
        {
            if (!messageBox.DialogResult!.Value)
                switch (messageBoxButton)
                {
                    case MessageBoxButton.OK:
                        break;

                    case MessageBoxButton.OKCancel:
                        messageBoxResult = MessageBoxResult.Cancel;
                        break;

                    case MessageBoxButton.YesNoCancel:
                        messageBoxResult = MessageBoxResult.No;
                        break;

                    case MessageBoxButton.YesNo:
                        messageBoxResult = MessageBoxResult.No;
                        break;
                }
            else
                switch (messageBoxButton)
                {
                    case MessageBoxButton.OK:
                        messageBoxResult = MessageBoxResult.OK;

                        break;

                    case MessageBoxButton.OKCancel:
                        messageBoxResult = MessageBoxResult.OK;
                        break;
                }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message, ex);
        }

        return messageBoxResult;
    }

    /// <summary>
    ///     确定取消
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CancelBTN_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>
    ///     确定返回
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OkBTN_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var wndHelper = new WindowInteropHelper(this);
        ReleaseCapture();
        SendMessage(wndHelper.Handle, 0x0112, 0xF010 + 0x0002, 0);
    }
}