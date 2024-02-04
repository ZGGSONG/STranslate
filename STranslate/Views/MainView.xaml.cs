using STranslate.Log;
using STranslate.Util;
using STranslate.ViewModels;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using STranslate.Helper;
using System.ComponentModel;

namespace STranslate.Views
{
    public partial class MainView : Window
    {
        private readonly MainViewModel vm = Singleton<MainViewModel>.Instance;

        public MainView()
        {
            DataContext = vm;

            vm.NotifyIconVM.OnExit += UnLoadPosition;

            vm.CommonSettingVM.OnViewMaxHeightChanged += Vm_OnMaxHeightChanged;
            vm.CommonSettingVM.TriggerMaxHeight();

            InitializeComponent();

            LoadPosition();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            vm.NotifyIconVM.OnExit -= UnLoadPosition;
            vm.CommonSettingVM.OnViewMaxHeightChanged -= Vm_OnMaxHeightChanged;

            base.OnClosing(e);
        }

        private void Vm_OnMaxHeightChanged(int height)
        {
            MaxHeight = height;
        }
        private void Mwin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SizeToContent = SizeToContent.Height;
            Mwin.UpdateDefaultStyle();
        }

        private void UnLoadPosition()
        {
            //写入配置
            if (!Singleton<ConfigHelper>.Instance.WriteConfig(Left, Top))
            {
                LogService.Logger.Error($"保存位置({Left},{Top})失败...");
            }
        }

        private void LoadPosition()
        {
            var position = Singleton<ConfigHelper>.Instance.CurrentConfig?.Position;
            try
            {
                var args = position?.Split(',');
                if (string.IsNullOrEmpty(position) || args?.Length != 2)
                {
                    throw new Exception();
                }

                bool ret = true;
                ret &= double.TryParse(args[0], out var left);
                ret &= double.TryParse(args[1], out var top);
                if (!ret || left > SystemParameters.WorkArea.Width || top > SystemParameters.WorkArea.Height)
                {
                    throw new Exception($"当前({SystemParameters.WorkArea.Width}x{SystemParameters.WorkArea.Height})");
                }

                Left = left;
                Top = top;
            }
            catch (Exception)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner;

                LogService.Logger.Warn($"加载上次窗口位置({position})失败，启用默认位置");
            }

            if (!(Singleton<ConfigHelper>.Instance.CurrentConfig?.IsHideOnStart ?? false))
            {
                // 第一次加载页面激活输入框
                (InputView.FindName("InputTb") as TextBox)?.Focus();
            }
        }

        private void Mwin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 开始拖动窗体
            DragMove();
        }

        private void Mwin_Deactivated(object sender, EventArgs e)
        {
            if (!Topmost)
            {
                Hide();
            }
        }

        #region 隐藏系统窗口菜单

        //方法来自于 Lindexi
        //https://blog.lindexi.com/post/WPF-%E9%9A%90%E8%97%8F%E7%B3%BB%E7%BB%9F%E7%AA%97%E5%8F%A3%E8%8F%9C%E5%8D%95.html

        protected override void OnSourceInitialized(EventArgs e)
        {
            #region 开启时隐藏主界面

            if (Singleton<ConfigHelper>.Instance.CurrentConfig?.IsHideOnStart ?? false)
            {
                Hide();

                bool isAdmin = CommonUtil.IsUserAdministrator();

                string toolTipFormat = isAdmin ? "STranslate[Admin] {0} started" : "STranslate {0} started";

                var msg = string.Format(toolTipFormat, Application.ResourceAssembly.GetName().Version!);

                // 显示信息
                Singleton<NotifyIconViewModel>.Instance.ShowBalloonTip(msg);
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
            if (Environment.Is64BitProcess)
            {
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            }

            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        #endregion 隐藏系统窗口菜单
    }
}
