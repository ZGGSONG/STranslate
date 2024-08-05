using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels;
using WpfScreenHelper;
using WindowHelper = STranslate.Helper.WindowHelper;

namespace STranslate.Views;

public partial class MainView : Window
{
    private readonly ConfigHelper _configHelper = Singleton<ConfigHelper>.Instance;
    private readonly MainViewModel _vm = Singleton<MainViewModel>.Instance;

    public MainView()
    {
        DataContext = _vm;

        _vm.NotifyIconVM.OnExit += UnLoadPosition;

        InitializeComponent();
    }

    /// <summary>
    ///     保存退出前位置
    /// </summary>
    private void UnLoadPosition()
    {
        //写入配置
        if (!_configHelper.WriteConfig(Left, Top))
            LogService.Logger.Warn($"保存位置({Left},{Top})失败...");
    }

    /// <summary>
    ///     加载上次退出前位置
    /// </summary>
    private void LoadPosition()
    {
        if (!(_configHelper.CurrentConfig?.UseCacheLocation ?? false))
            return;
        var position = _configHelper.CurrentConfig?.Position;
        try
        {
            var args = position?.Split(',');
            if (string.IsNullOrEmpty(position) || args?.Length != 2) throw new Exception();

            var ret = true;
            ret &= double.TryParse(args[0], out var left);
            ret &= double.TryParse(args[1], out var top);
            if (!ret) throw new Exception();

            // 判断是否在屏幕上
            // 增加偏移量，防止窗口被遮挡
            const double offsetRatio = 0.1; // 偏移比例
            var screen = Screen.AllScreens.FirstOrDefault(screen =>
            {
                var workingArea = screen.WpfWorkingArea;
                var hOffset = workingArea.Width * offsetRatio;
                var vOffset = workingArea.Height * offsetRatio;
                var point = new Point(left + hOffset, top + vOffset);
                return screen.WpfBounds.Contains(point);
            });
            if (screen == null) throw new Exception();

            Left = left;
            Top = top;
        }
        catch
        {
            // 计算窗口左上角在屏幕上的位置
            var left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            var top = (SystemParameters.PrimaryScreenHeight - 600) / 2;

            // 设置窗口位置
            Left = left;
            Top = top;

            LogService.Logger.Warn($"加载上次窗口位置({position})失败，启用默认位置");
        }
    }

    private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 开始拖动窗体
        DragMove();
    }

    private void MainWindow_Deactivated(object sender, EventArgs e)
    {
        if (Topmost || StayView()) return;

        AnimationHelper.MainViewAnimation(false);
    }

    internal bool StayView()
    {
        return _configHelper.CurrentConfig?.StayMainViewWhenLoseFocus ?? false;
    }

    #region 隐藏系统窗口菜单

    //方法来自于 Lindexi
    //https://blog.lindexi.com/post/WPF-%E9%9A%90%E8%97%8F%E7%B3%BB%E7%BB%9F%E7%AA%97%E5%8F%A3%E8%8F%9C%E5%8D%95.html

    protected override void OnSourceInitialized(EventArgs e)
    {
        #region 加载缓存位置

        LoadPosition();

        #endregion

        #region 初始化失焦保持显示

        _configHelper.MainViewStayOperate(_configHelper.CurrentConfig?.StayMainViewWhenLoseFocus ?? false);

        #endregion 初始化失焦保持显示

        #region 初始化时阴影

        _configHelper.MainViewShadowOperate(_configHelper.CurrentConfig?.MainViewShadow ?? false);

        #endregion 初始化时阴影

        #region 开启时隐藏主界面

        // 初始化动画标记
        AnimationHelper.Init();

        if (_configHelper.CurrentConfig?.IsHideOnStart ?? false)
        {
            Hide();

            var isAdmin = CommonUtil.IsUserAdministrator();

            var toolTipFormat = isAdmin ? "STranslate[Admin] {0} started" : "STranslate {0} started";

            var msg = string.Format(toolTipFormat, ConstStr.AppVersion);

            // 显示信息
            Singleton<NotifyIconViewModel>.Instance.ShowBalloonTip(msg);
        }
        else
        {
            // 尝试移动窗口到屏幕最上层
            WindowHelper.SetWindowInForeground(this);
            // 第一次加载页面激活输入框
            (InputView.FindName("InputTB") as TextBox)?.Focus();
        }

        #endregion 开启时隐藏主界面

        base.OnSourceInitialized(e);

        var windowInteropHelper = new WindowInteropHelper(this);
        var hwnd = windowInteropHelper.Handle;

        var windowLong = GetWindowLong(hwnd, GWL_STYLE);
        windowLong &= ~WS_SYSMENU;

        SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(windowLong));
    }

    public const int WS_SYSMENU = 0x00080000;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    public const int GWL_STYLE = -16;

    public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (Environment.Is64BitProcess) return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);

        return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    #endregion 隐藏系统窗口菜单
}