using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using STranslate.Model;
using STranslate.Views;

namespace STranslate.Helper;

public class ToastHelper
{
    /// <summary>
    ///     自动收回超时时间
    /// </summary>
    private const int Timeout = 2;

    /// <summary>
    ///     存储每个窗口类型对应的通知信息
    /// </summary>
    private static readonly Dictionary<WindowType, ToastInfo> ToastDictionary = [];

    /// <summary>
    ///     显示通知
    /// </summary>
    /// <param name="message">通知消息</param>
    /// <param name="type">窗口类型</param>
    public static void Show(string message, WindowType type = WindowType.Main)
    {
        EnsureInitialized(type);

        if (!ToastDictionary.TryGetValue(type, out var value)) return;
        var toastInfo = value;
        toastInfo.ToastControl.ToastText = message;
        toastInfo.ToastControl.Visibility = Visibility.Visible;

        // 复制滑入动画并监听完成事件
        var slideInStoryboard = (toastInfo.ToastControl.Resources["SlideInStoryboard"] as Storyboard)!.Clone();
        slideInStoryboard.Completed += (_, _) => SlideInStoryboard_Completed(type);
        toastInfo.ToastControl.BeginStoryboard(slideInStoryboard);

        // 重置计时器
        toastInfo.Timer.Stop();
        toastInfo.Timer.Start();
    }

    /// <summary>
    ///     确保窗口初始化
    /// </summary>
    /// <param name="type">窗口类型</param>
    private static void EnsureInitialized(WindowType type)
    {
        DispatcherTimer t;
        ToastView? tv;
        if (!ToastDictionary.TryGetValue(type, out var value))
        {
            t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Timeout) };
            t.Tick += (_, _) => Timer_Tick(type);

            // 根据窗口类型将通知控件添加到对应的窗口
            tv = GetNotifyControlForWindowType(type);

            if (tv is null) return;

            // 将通知信息添加到字典中
            ToastDictionary.Add(type, new ToastInfo(tv, t));
        }
        else
        {
            t = value.Timer;
            // 根据窗口类型将通知控件添加到对应的窗口
            tv = GetNotifyControlForWindowType(type);

            if (tv is null) return;

            ToastDictionary[type] = new ToastInfo(tv, t);
        }
    }

    /// <summary>
    ///     获取ToastView
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static ToastView? GetNotifyControlForWindowType(WindowType type)
    {
        return type switch
        {
            WindowType.Preference => Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault()?.Notify,
            WindowType.OCR => Application.Current.Windows.OfType<OCRView>().FirstOrDefault()?.Notify,
            _ => Application.Current.Windows.OfType<MainView>().FirstOrDefault()?.Notify
        };
    }

    /// <summary>
    ///     滑入动画完成事件处理
    /// </summary>
    /// <param name="type">窗口类型</param>
    private static void SlideInStoryboard_Completed(WindowType type)
    {
        // 开始计时器
        ToastDictionary[type].Timer.Start();
    }

    /// <summary>
    ///     计时器触发事件处理
    /// </summary>
    /// <param name="type">窗口类型</param>
    private static void Timer_Tick(WindowType type)
    {
        var toastInfo = ToastDictionary[type];
        toastInfo.Timer.Stop();

        // 复制滑出动画并监听完成事件
        var slideOutStoryboard = (toastInfo.ToastControl.Resources["SlideOutStoryboard"] as Storyboard)!.Clone();
        slideOutStoryboard.Completed += (_, _) => SlideOutStoryboard_Completed(type);
        toastInfo.ToastControl.BeginStoryboard(slideOutStoryboard);
    }

    /// <summary>
    ///     滑出动画完成事件处理
    /// </summary>
    /// <param name="type">窗口类型</param>
    private static void SlideOutStoryboard_Completed(WindowType type)
    {
        // 隐藏通知控件
        ToastDictionary[type].ToastControl.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    ///     存储通知信息的内部类
    /// </summary>
    private class ToastInfo(ToastView toastControl, DispatcherTimer timer)
    {
        public ToastView ToastControl { get; } = toastControl;
        public DispatcherTimer Timer { get; } = timer;
    }
}