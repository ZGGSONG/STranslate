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
using STranslate.Model;
using System.Windows.Media.Animation;
using System.Linq;

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
            vm.CommonSettingVM.OnViewWidthChanged += Vm_OnWidthChanged;

            InitializeComponent();

            vm.CommonSettingVM.TriggerMaxHeight();
            vm.CommonSettingVM.TriggerWidth();

            LoadPosition();

            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        }

        /// <summary>
        /// 退出前取消事件订阅
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            vm.NotifyIconVM.OnExit -= UnLoadPosition;
            vm.CommonSettingVM.OnViewMaxHeightChanged -= Vm_OnMaxHeightChanged;
            vm.CommonSettingVM.OnViewWidthChanged -= Vm_OnWidthChanged;
            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;

            base.OnClosing(e);
        }

        /// <summary>
        /// 副屏不生效
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemParameters_StaticPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.WorkArea))
            {
                Dispatcher.Invoke(() =>
                {
                    // 如果当前最大高度值不在enum中则说明为工作区最大高度
                    // 工作区变动=>更新MaxHeight
                    if (!Enum.IsDefined(typeof(MaxHeight), Convert.ToInt32(MaxHeight)))
                    {
                        MaxHeight = SystemParameters.WorkArea.Height;
                        Width = SystemParameters.WorkArea.Width;
                        Mwin_SizeChanged(null, null);
                    }
                });
            }
        }

        private void Vm_OnMaxHeightChanged(int height)
        {
            // 更新最大高度
            MaxHeight = height;
            HAnimation(Height, height);
        }

        private void Vm_OnWidthChanged(int width)
        {
            // 更新宽度
            Width = width;
            WAnimation(Width, width);
        }

        internal void WAnimation(double oValue, double nValue)
        {
            var wAnimation = FindResource("WAnimation") as Storyboard;
            var doubleAnimation = wAnimation?.Children.FirstOrDefault() as DoubleAnimation;
            doubleAnimation!.From = double.IsNaN(oValue) ? 480 : oValue;
            doubleAnimation!.To = nValue;
            wAnimation?.Begin();
        }

        internal void HAnimation(double oValue, double nValue)
        {
            var hAnimation = FindResource("HAnimation") as Storyboard;
            var doubleAnimation = hAnimation?.Children.FirstOrDefault() as DoubleAnimation;
            doubleAnimation!.From = double.IsNaN(oValue) ? 800 : oValue;
            doubleAnimation!.To = nValue;
            hAnimation?.Begin();
        }

        public void ViewAnimation(bool show = true)
        {
            var viewAnimation = (FindResource("ViewAnimation") as Storyboard)!;
            var doubleAnimation = (viewAnimation.Children.FirstOrDefault() as DoubleAnimation)!;
            // 注销之前可能添加的Completed事件
            viewAnimation.Completed -= AnimationCompleted;
            if (show)
            {
                // 如果当前已经显示了，则不执行动画
                if (Mwin.Visibility == Visibility.Visible)
                    return;

                // 在开始动画之前，确保窗口是可见的
                Mwin.Visibility = Visibility.Visible;
                doubleAnimation.From = 0;
                doubleAnimation.To = 1;
            }
            else
            {
                doubleAnimation.From = 1;
                doubleAnimation.To = 0;
                // 在动画完成时注册一个事件，隐藏窗口
                viewAnimation.Completed += AnimationCompleted;
            }

            viewAnimation?.Begin();
        }

        private void AnimationCompleted(object? sender, EventArgs e)
        {
            // 动画完成后隐藏窗口
            Mwin.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 刷新最大高度并刷新界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Mwin_SizeChanged(object? sender, SizeChangedEventArgs? e)
        {
            SizeToContent = SizeToContent.Height;
            Mwin.UpdateDefaultStyle();
        }

        /// <summary>
        /// 保存退出前位置
        /// </summary>
        private void UnLoadPosition()
        {
            //写入配置
            if (!Singleton<ConfigHelper>.Instance.WriteConfig(Left, Top))
            {
                LogService.Logger.Warn($"保存位置({Left},{Top})失败...");
            }
        }

        /// <summary>
        /// 加载上次退出前位置
        /// </summary>
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

            // 首次加载时是否隐藏界面
            if (!(Singleton<ConfigHelper>.Instance.CurrentConfig?.IsHideOnStart ?? false))
            {
                // 第一次加载页面激活输入框
                (InputView.FindName("InputTB") as TextBox)?.Focus();
            }
            else
            {
                Mwin.Visibility = Visibility.Hidden;
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
                ViewAnimation(false);
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
