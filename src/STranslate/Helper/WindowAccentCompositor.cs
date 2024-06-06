using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using Newtonsoft.Json.Linq;
using STranslate.Views;

namespace STranslate.Helper;

public class WindowAccentCompositor
{
    #region 调用

    private bool _isEnabled;
    private readonly Window _window;

    private static readonly Lazy<WindowAccentCompositor> _instance = new(() => new(Application.Current.Windows.OfType<MainView>().First()));
    public static WindowAccentCompositor Instance => _instance.Value;

    public WindowAccentCompositor(Window window) => _window = window;

    /// <summary>
    /// 获取或设置此窗口模糊特效是否生效的一个状态。
    /// 默认为 false，即不生效。
    /// </summary>
    [DefaultValue(false)]
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            OnIsEnabledChanged(value);
        }
    }

    private void OnIsEnabledChanged(bool isEnabled)
    {
        var handle = new WindowInteropHelper(_window).EnsureHandle();
        Composite(handle, isEnabled);
    }

    /// <summary>
    /// 获取或设置此窗口模糊特效叠加的颜色。
    /// </summary>
    public Color DefineColor { get; set; } = Color.FromArgb(180, 0, 0, 0);
    private int BlurColor => DefineColor.R << 0 | DefineColor.G << 8 | DefineColor.B << 16 | DefineColor.A << 24;

    private void Composite(IntPtr handle, bool isEnabled)
    {
        // 操作系统版本判定。
        var osVersion = Environment.OSVersion.Version;
        var windows11 = new Version(10, 0, 22621);
        var windows10_1809 = new Version(10, 0, 17763);
        var windows10 = new Version(10, 0);
        if (osVersion >= windows11)
        {
            if (!isEnabled)
            {
                SetWindowBlur(handle, 0, BlurMode.None);
                return;
            }
            //针对Win11的特殊处理
            WindowChrome.SetWindowChrome(_window, new WindowChrome() { GlassFrameThickness = new Thickness(-1), ResizeBorderThickness = new Thickness(0, 0, 0, 0) });
            //_window.Background = new SolidColorBrush(_color);
            SetWindowBlur(handle, DarkMode.Dark, BlurMode.Auto);
        }
        else
        {
            // 创建 AccentPolicy 对象。
            var accent = new AccentPolicy();
            WindowChrome.SetWindowChrome(_window, new WindowChrome() { GlassFrameThickness = new Thickness(0.1), ResizeBorderThickness = new Thickness(0, 0, 0, 0) });
            _window.Background = new SolidColorBrush(Colors.Transparent);

            // 设置特效。
            if (!isEnabled)
            {
                accent.AccentState = AccentState.ACCENT_DISABLED;
            }
            else if (osVersion >= windows10_1809)
            {
                //1803能用但是兼容性不好哇----  1903完美支持

                // 如果系统在 Windows 10 (1809) 以上，则启用亚克力效果，并组合已设置的叠加颜色和透明度。
                //  请参见《在 WPF 程序中应用 Windows 10 真•亚克力效果》
                //  https://blog.walterlv.com/post/using-acrylic-in-wpf-application.html
                accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
                accent.GradientColor = BlurColor;
            }
            else if (osVersion >= windows10)
            {
                // 如果系统在 Windows 10 以上，则启用 Windows 10 早期的模糊特效。
                //  请参见《在 Windows 10 上为 WPF 窗口添加模糊特效》
                //  https://blog.walterlv.com/post/win10/2017/10/02/wpf-transparent-blur-in-windows-10.html
                accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;
            }
            else
            {
                // 暂时不处理其他操作系统：
                //  - Windows 8/8.1 不支持任何模糊特效
                //  - Windows Vista/7 支持 Aero 毛玻璃效果
                return;
            }

            // 将托管结构转换为非托管对象。
            var accentPolicySize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentPolicySize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            // 设置窗口组合特性。
            try
            {
                // 设置模糊特效。
                var data = new WindowCompositionAttributeData
                {
                    Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                    SizeOfData = accentPolicySize,
                    Data = accentPtr,
                };
                _ = SetWindowCompositionAttribute(handle, ref data);
            }
            finally
            {
                // 释放非托管对象。
                Marshal.FreeHGlobal(accentPtr);
            }
        }
    }

    #endregion 调用

    #region Win10 生效

    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    private enum AccentState
    {
        ACCENT_DISABLED = 0,                    //完全禁用 DWM 的叠加特效。
        ACCENT_ENABLE_GRADIENT = 1,             // 渐变 (实测没什么用)
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,  // 透明 (实测没什么用)
        ACCENT_ENABLE_BLURBEHIND = 3,           // 背景模糊 (有用)
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,    // 背景亚克力模糊 (有用)
        ACCENT_ENABLE_HOSTBACKDROP = 5,         // 没啥用
        ACCENT_INVALID_STATE = 6,
    }

    [Flags]
    public enum AccentFlags
    {
        None = 0,
        ExtendSize = 0x4, // 启用此 flag 会导致窗体大小拓展至屏幕大小
        LeftBorder = 0x20, // 启用窗口左侧边框 (当 WindowStyle 为 None 时可以看出来)
        TopBorder = 0x40, // 启用窗口顶部边框 (同上)
        RightBorder = 0x80, // 启用窗口右侧边框 (同上)
        BottomBorder = 0x100, // 启用窗口底部边框

        // 合起来, 启用窗口所有边框
        AllBorder = LeftBorder | TopBorder | RightBorder | BottomBorder,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public AccentState AccentState;
        public AccentFlags AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    private enum WindowCompositionAttribute
    {
        // 省略其他未使用的字段
        WCA_ACCENT_POLICY = 19,

        // 省略其他未使用的字段
    }

    #endregion Win10 生效

    #region Win11 生效

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);

    public static int SetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, int parameter) => DwmSetWindowAttribute(hwnd, attribute, ref parameter, Marshal.SizeOf<int>());

    [Flags]
    public enum DWMWINDOWATTRIBUTE
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20, // 表示是否使用暗色模式, 它会将窗体的模糊背景调整为暗色
        DWMWA_SYSTEMBACKDROP_TYPE = 38 // 背景类型, 值可以是: 自动, 无, 云母, 或者亚克力
    }

    public enum BlurMode
    {
        Auto = 0, //自动
        None = 1, //无
        Mica = 2, //云母
        Acrylic = 3, //亚克力
        Tabbed = 4
    }

    public enum DarkMode
    {
        Light = 0,
        Dark = 1,
    }

    /// <summary>
    /// 应用模糊特效 for Win11
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="darkMode"></param>
    /// <param name="blurMode"></param>
    public static void SetWindowBlur(IntPtr handle, DarkMode darkMode, BlurMode blurMode)
    {
        SetWindowAttribute(handle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, (int)darkMode);
        SetWindowAttribute(handle, DWMWINDOWATTRIBUTE.DWMWA_SYSTEMBACKDROP_TYPE, (int)blurMode);
    }

    #endregion Win11 生效
}
