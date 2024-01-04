using STranslate.Model;
using STranslate.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace STranslate.Helper
{
    public class ToastHelper
    {
        /// <summary>
        /// 存储每个窗口类型对应的通知信息
        /// </summary>
        private static readonly Dictionary<WindowType, ToastInfo> toastDictionary = [];

        /// <summary>
        /// 自动收回超时时间
        /// </summary>
        private const int TIMEOUT = 2;

        /// <summary>
        /// 显示通知
        /// </summary>
        /// <param name="message">通知消息</param>
        /// <param name="type">窗口类型</param>
        public static void Show(string message, WindowType type = WindowType.Main)
        {
            EnsureInitialized(type);

            var toastInfo = toastDictionary[type];
            toastInfo.ToastControl.ToastText = message;
            toastInfo.ToastControl.Visibility = Visibility.Visible;

            // 复制滑入动画并监听完成事件
            var slideInStoryboard = (toastInfo.ToastControl.Resources["SlideInStoryboard"] as Storyboard)!.Clone();
            slideInStoryboard.Completed += (sender, e) => SlideInStoryboard_Completed(type);
            toastInfo.ToastControl.BeginStoryboard(slideInStoryboard);

            // 重置计时器
            toastInfo.Timer.Stop();
            toastInfo.Timer.Start();
        }

        /// <summary>
        /// 确保窗口初始化
        /// </summary>
        /// <param name="type">窗口类型</param>
        private static void EnsureInitialized(WindowType type)
        {
            DispatcherTimer t;
            ToastView tv;
            if (!toastDictionary.ContainsKey(type))
            {
                t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(TIMEOUT) };
                t.Tick += (sender, e) => Timer_Tick(type);

                // 根据窗口类型将通知控件添加到对应的窗口
                tv = GetNotifyControlForWindowType(type);

                // 将通知信息添加到字典中
                toastDictionary.Add(type, new ToastInfo(tv, t));
            }
            else
            {
                t = toastDictionary[type].Timer;
                // 根据窗口类型将通知控件添加到对应的窗口
                tv = GetNotifyControlForWindowType(type);

                toastDictionary[type] = new ToastInfo(tv, t);
            }
        }

        /// <summary>
        /// 获取ToastView
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ToastView GetNotifyControlForWindowType(WindowType type) => type switch
        {
            WindowType.Preference => Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault()!.notify,
            WindowType.OCR => Application.Current.Windows.OfType<OCRView>().FirstOrDefault()!.notify,
            _ => Application.Current.Windows.OfType<MainView>().FirstOrDefault()!.notify
        };

        /// <summary>
        /// 滑入动画完成事件处理
        /// </summary>
        /// <param name="type">窗口类型</param>
        private static void SlideInStoryboard_Completed(WindowType type)
        {
            // 开始计时器
            toastDictionary[type].Timer.Start();
        }

        /// <summary>
        /// 计时器触发事件处理
        /// </summary>
        /// <param name="type">窗口类型</param>
        private static void Timer_Tick(WindowType type)
        {
            var toastInfo = toastDictionary[type];
            toastInfo.Timer.Stop();

            // 复制滑出动画并监听完成事件
            var slideOutStoryboard = (toastInfo.ToastControl.Resources["SlideOutStoryboard"] as Storyboard)!.Clone();
            slideOutStoryboard.Completed += (sender, e) => SlideOutStoryboard_Completed(type);
            toastInfo.ToastControl.BeginStoryboard(slideOutStoryboard);
        }

        /// <summary>
        /// 滑出动画完成事件处理
        /// </summary>
        /// <param name="type">窗口类型</param>
        private static void SlideOutStoryboard_Completed(WindowType type)
        {
            // 隐藏通知控件
            toastDictionary[type].ToastControl.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 存储通知信息的内部类
        /// </summary>
        private class ToastInfo
        {
            public ToastView ToastControl { get; }
            public DispatcherTimer Timer { get; }

            public ToastInfo(ToastView toastControl, DispatcherTimer timer)
            {
                ToastControl = toastControl;
                Timer = timer;
            }
        }
    }
}