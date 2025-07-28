using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using STranslate.Model;

namespace STranslate.Style.Controls;

/// <summary>
/// MessageBox_S.xaml 的交互逻辑
/// </summary>
public partial class MessageBox_S : Window
{
    /// <summary>
    /// 默认标题名为主项目程序集名称
    /// </summary>
    private readonly string ProductName = Constant.AppName;

    /// <summary>
    /// 当前按钮类型
    /// </summary>
    private MessageBoxButton CurrentButton = MessageBoxButton.OK;

    /// <summary>
    /// 对话框结果
    /// </summary>
    private MessageBoxResult _result = MessageBoxResult.None;

    /// <summary>
    /// 消息框类型
    /// </summary>
    public enum MessageBoxType
    {
        Information,
        Warning,
        Error,
        Success,
        Question
    }

    #region 构造函数

    public MessageBox_S(string msg)
    {
        InitializeComponent();
        InitializeMessageBox(msg, ProductName, MessageBoxButton.OK, MessageBoxType.Information);
    }

    public MessageBox_S(string msg, string caption)
    {
        InitializeComponent();
        InitializeMessageBox(msg, caption, MessageBoxButton.OK, MessageBoxType.Information);
    }

    public MessageBox_S(string msg, string caption, MessageBoxButton messageBoxButton)
    {
        InitializeComponent();
        InitializeMessageBox(msg, caption, messageBoxButton, MessageBoxType.Information);
    }

    public MessageBox_S(string msg, MessageBoxType type = MessageBoxType.Information)
    {
        InitializeComponent();
        InitializeMessageBox(msg, ProductName, MessageBoxButton.OK, type);
    }

    public MessageBox_S(string msg, string caption, MessageBoxType type = MessageBoxType.Information)
    {
        InitializeComponent();
        InitializeMessageBox(msg, caption, MessageBoxButton.OK, type);
    }

    public MessageBox_S(string msg, string caption, MessageBoxButton messageBoxButton, MessageBoxType type = MessageBoxType.Information)
    {
        InitializeComponent();
        InitializeMessageBox(msg, caption, messageBoxButton, type);
    }

    #endregion

    #region 私有方法

    private void InitializeMessageBox(string msg, string caption, MessageBoxButton messageBoxButton, MessageBoxType type)
    {
        Messages.Text = msg;
        TitleTxt.Text = caption;
        CurrentButton = messageBoxButton;

        // 设置按钮
        SetupButtons(messageBoxButton);

        // 设置默认焦点
        SetDefaultFocus();
    }

    private void SetupButtons(MessageBoxButton messageBoxButton)
    {
        // 先隐藏所有按钮
        FirstButton.Visibility = Visibility.Collapsed;
        SecondButton.Visibility = Visibility.Collapsed;
        ThirdButton.Visibility = Visibility.Collapsed;

        switch (messageBoxButton)
        {
            case MessageBoxButton.OK:
                FirstButton.Content = AppLanguageManager.GetString("Confirm");
                FirstButton.Visibility = Visibility.Visible;
                FirstButton.Click += (s, e) => { _result = MessageBoxResult.OK; DialogResult = true; Close(); };
                FirstButton.IsDefault = true;
                break;

            case MessageBoxButton.OKCancel:
                FirstButton.Content = AppLanguageManager.GetString("Confirm");
                SecondButton.Content = AppLanguageManager.GetString("Cancel");
                FirstButton.Visibility = Visibility.Visible;
                SecondButton.Visibility = Visibility.Visible;
                FirstButton.Click += (s, e) => { _result = MessageBoxResult.OK; DialogResult = true; Close(); };
                SecondButton.Click += (s, e) => { _result = MessageBoxResult.Cancel; DialogResult = false; Close(); };
                FirstButton.IsDefault = true;
                SecondButton.IsCancel = true;
                break;

            case MessageBoxButton.YesNo:
                FirstButton.Content = AppLanguageManager.GetString("Yes");
                SecondButton.Content = AppLanguageManager.GetString("No");
                FirstButton.Visibility = Visibility.Visible;
                SecondButton.Visibility = Visibility.Visible;
                FirstButton.Click += (s, e) => { _result = MessageBoxResult.Yes; DialogResult = true; Close(); };
                SecondButton.Click += (s, e) => { _result = MessageBoxResult.No; DialogResult = false; Close(); };
                FirstButton.IsDefault = true;
                break;

            case MessageBoxButton.YesNoCancel:
                FirstButton.Content = AppLanguageManager.GetString("Yes");
                SecondButton.Content = AppLanguageManager.GetString("No");
                ThirdButton.Content = AppLanguageManager.GetString("Cancel");
                FirstButton.Visibility = Visibility.Visible;
                SecondButton.Visibility = Visibility.Visible;
                ThirdButton.Visibility = Visibility.Visible;
                FirstButton.Click += (s, e) => { _result = MessageBoxResult.Yes; DialogResult = true; Close(); };
                SecondButton.Click += (s, e) => { _result = MessageBoxResult.No; DialogResult = false; Close(); };
                ThirdButton.Click += (s, e) => { _result = MessageBoxResult.Cancel; DialogResult = null; Close(); };
                FirstButton.IsDefault = true;
                ThirdButton.IsCancel = true;
                break;
        }
    }

    private void SetDefaultFocus()
    {
        Loaded += (s, e) =>
        {
            if (FirstButton.Visibility == Visibility.Visible)
            {
                FirstButton.Focus();
            }
        };
    }

    #endregion

    #region Win32 API

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

    #endregion

    #region 静态显示方法

    /// <summary>
    /// 显示只有确定按钮的消息框 (兼容原始接口)
    /// </summary>
    public static bool? Show(string msg)
    {
        var messageBox = new MessageBox_S(msg);
        return messageBox.ShowDialog();
    }

    /// <summary>
    /// 显示带标题的消息框 (兼容原始接口)
    /// </summary>
    public static bool? Show(string msg, string caption)
    {
        var messageBox = new MessageBox_S(msg, caption);
        messageBox.ShowDialog();
        return messageBox.DialogResult;
    }

    /// <summary>
    /// 显示带按钮类型的消息框 (兼容原始接口)
    /// </summary>
    [STAThread]
    public static MessageBoxResult Show(string msg, string caption, MessageBoxButton messageBoxButton)
    {
        var messageBox = new MessageBox_S(msg, caption, messageBoxButton);
        messageBox.ShowDialog();
        return messageBox._result;
    }

    /// <summary>
    /// 显示带消息类型的消息框 (新增功能)
    /// </summary>
    public static MessageBoxResult Show(string msg, MessageBoxType type)
    {
        var messageBox = new MessageBox_S(msg, type);
        messageBox.ShowDialog();
        return messageBox._result;
    }

    /// <summary>
    /// 显示带标题和消息类型的消息框 (新增功能)
    /// </summary>
    public static MessageBoxResult Show(string msg, string caption, MessageBoxType type)
    {
        var messageBox = new MessageBox_S(msg, caption, type);
        messageBox.ShowDialog();
        return messageBox._result;
    }

    /// <summary>
    /// 显示完整参数的消息框 (新增功能)
    /// </summary>
    public static MessageBoxResult Show(string msg, string caption, MessageBoxButton messageBoxButton, MessageBoxType type)
    {
        var messageBox = new MessageBox_S(msg, caption, messageBoxButton, type);
        messageBox.ShowDialog();
        return messageBox._result;
    }

    #endregion

    #region 事件处理

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // 根据按钮类型设置不同的关闭结果
        switch (CurrentButton)
        {
            case MessageBoxButton.OK:
                _result = MessageBoxResult.OK;
                DialogResult = true;
                break;
            case MessageBoxButton.OKCancel:
                _result = MessageBoxResult.Cancel;
                DialogResult = false;
                break;
            case MessageBoxButton.YesNo:
                _result = MessageBoxResult.No;
                DialogResult = false;
                break;
            case MessageBoxButton.YesNoCancel:
                _result = MessageBoxResult.Cancel;
                DialogResult = null;
                break;
        }
        Close();
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            try
            {
                var wndHelper = new WindowInteropHelper(this);
                ReleaseCapture();
                SendMessage(wndHelper.Handle, 0x0112, 0xF010 + 0x0002, 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"拖拽窗口时发生错误: {ex.Message}");
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // 处理 Esc 键
        if (e.Key == Key.Escape)
        {
            if (ThirdButton.Visibility == Visibility.Visible) // YesNoCancel
            {
                _result = MessageBoxResult.Cancel;
                DialogResult = null;
            }
            else if (SecondButton.Visibility == Visibility.Visible) // OKCancel 或 YesNo
            {
                _result = CurrentButton == MessageBoxButton.OKCancel ? MessageBoxResult.Cancel : MessageBoxResult.No;
                DialogResult = false;
            }
            else // OK
            {
                _result = MessageBoxResult.OK;
                DialogResult = true;
            }
            Close();
        }
    }

    #endregion
}