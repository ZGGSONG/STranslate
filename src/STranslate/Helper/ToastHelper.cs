using System.Windows;
using System.Windows.Threading;
using STranslate.Model;
using STranslate.Views;

namespace STranslate.Helper;

public class ToastHelper
{
    private const int Timeout = 2;
    private static readonly Dictionary<WindowType, ToastInfo> ToastDictionary = new();

    public static void Show(string message, WindowType type = WindowType.Main)
    {
        if (!EnsureInitialized(type))
            return;

        var toastInfo = ToastDictionary[type];
        toastInfo.Toast.CreateStoryboards();
        toastInfo.Toast.ToastText = message;
        toastInfo.Toast.Visibility = Visibility.Visible;
        toastInfo.Toast.OnClicked = () =>
        {
            toastInfo.Timer.Stop();
            toastInfo.Toast.SlideOutStoryboard.Completed += (_, _) => SlideOutStoryboard_Completed(type);
            toastInfo.Toast.PlaySlideOutAnimation();
        };

        toastInfo.Toast.SlideInStoryboard.Completed += (_, _) => SlideInStoryboard_Completed(type);
        toastInfo.Toast.PlaySlideInAnimation();
    }

    private static bool EnsureInitialized(WindowType type)
    {
        var tv = GetNotifyControlForWindowType(type);
        if (tv is null) return false;

        if (ToastDictionary.TryGetValue(type, out var value))
        {
            value.UpdateToast(tv);
            value.Timer.Stop();
            return true;
        }

        var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Timeout) };
        t.Tick += (_, _) => Timer_Tick(type);

        ToastDictionary[type] = new ToastInfo(tv, t);
        return true;
    }

    private static ToastView? GetNotifyControlForWindowType(WindowType type)
    {
        return type switch
        {
            WindowType.Preference => Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault()?.Notify,
            WindowType.OCR => Application.Current.Windows.OfType<OCRView>().FirstOrDefault()?.Notify,
            _ => Application.Current.Windows.OfType<MainView>().FirstOrDefault()?.Notify
        };
    }

    private static void SlideInStoryboard_Completed(WindowType type)
    {
        ToastDictionary[type].Timer.Start();
    }

    private static void Timer_Tick(WindowType type)
    {
        var toastInfo = ToastDictionary[type];
        toastInfo.Timer.Stop();

        toastInfo.Toast.SlideOutStoryboard.Completed += (_, _) => SlideOutStoryboard_Completed(type);
        toastInfo.Toast.PlaySlideOutAnimation();
    }

    private static void SlideOutStoryboard_Completed(WindowType type)
    {
        var toastInfo = ToastDictionary[type];
        toastInfo.Toast.Visibility = Visibility.Collapsed;
    }

    private class ToastInfo(ToastView toast, DispatcherTimer timer)
    {
        public ToastView Toast { get; private set; } = toast;
        public DispatcherTimer Timer { get; } = timer;

        public void UpdateToast(ToastView newToastView)
        {
            Toast = newToastView;
        }
    }
}