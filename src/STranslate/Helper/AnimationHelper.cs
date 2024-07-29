using System.Windows;
using System.Windows.Media.Animation;
using STranslate.Util;
using STranslate.Views;

namespace STranslate.Helper;

/// <summary>
///     Helper class for handling animations in the application.
/// </summary>
public class AnimationHelper
{
    private static readonly Window MainView =
        Application.Current.MainWindow ?? Application.Current.Windows.OfType<MainView>().First();

    private static bool _previousAction;

    public static void Init()
    {
        _previousAction = !(Singleton<ConfigHelper>.Instance.CurrentConfig?.IsHideOnStart ?? false);
    }

    /// <summary>
    ///     Performs the main view animation.
    /// </summary>
    /// <param name="show">Indicates whether to show or hide the main view.</param>
    public static void MainViewAnimation(bool show = true)
    {
        // 记录上一次的状态，避免相同的动画重复触发
        // 显示界面重复触发显/隐 与 失焦功能叠加了
        if (_previousAction == show)
            //Log.LogService.Logger.Debug($"相同的状态: {show}");
            return;
        var viewAnimation = (Storyboard)MainView.FindResource("MainViewAnimation");
        var doubleAnimation = (DoubleAnimation)viewAnimation.Children.First();

        // Unsubscribe from any previously added Completed event
        viewAnimation.Completed -= AnimationCompleted;
        if (show)
        {
            // If the main view is already visible, do not perform the animation
            if (MainView.Visibility == Visibility.Visible)
                return;

            // Ensure that the window is visible before starting the animation
            MainView.Visibility = Visibility.Visible;
            doubleAnimation.From = 0;
            doubleAnimation.To = 1;
        }
        else
        {
            doubleAnimation.From = 1;
            doubleAnimation.To = 0;
            // Register an event to hide the window when the animation is completed
            viewAnimation.Completed += AnimationCompleted;
        }

        viewAnimation.Begin();

        _previousAction = show;
    }

    public static async Task MainViewAnimationAsync(bool show = true)
    {
        if (_previousAction == show)
            return;

        var viewAnimation = (Storyboard)MainView.FindResource("MainViewAnimation");
        var doubleAnimation = (DoubleAnimation)viewAnimation.Children.First();

        viewAnimation.Completed -= AnimationCompleted; // Unsubscribe to ensure no memory leaks

        var tcs = new TaskCompletionSource<bool>();

        if (show)
        {
            if (MainView.Visibility == Visibility.Visible)
                return;

            MainView.Visibility = Visibility.Visible;
            doubleAnimation.From = 0;
            doubleAnimation.To = 1;
        }
        else
        {
            doubleAnimation.From = 1;
            doubleAnimation.To = 0;
            viewAnimation.Completed += (s, e) => tcs.TrySetResult(true); // Use lambda to complete the task
        }

        viewAnimation.Begin();

        _previousAction = show;

        if (!show)
        {
            await tcs.Task; // Wait for the animation to complete
            AnimationCompleted(null, EventArgs.Empty); // Manually call the completion method
        }
    }

    private static void AnimationCompleted(object? sender, EventArgs e)
    {
        MainView.Visibility = Visibility.Hidden;
    }
}