using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using STranslate.Views;

namespace STranslate.Helper;

/// <summary>
///     Helper class for handling animations in the application.
/// </summary>
public class AnimationHelper
{
    private static readonly Window MainView = Application.Current.Windows.Cast<MainView>().First();

    /// <summary>
    ///     Performs a width animation on the MainView.
    /// </summary>
    /// <param name="oldValue">The original width value.</param>
    /// <param name="newValue">The new width value.</param>
    public static void WidthAnimation(double oldValue, double newValue)
    {
        var widthAnimation = MainView.FindResource("WidthAnimation") as Storyboard;
        var doubleAnimation = widthAnimation?.Children.FirstOrDefault() as DoubleAnimation;
        doubleAnimation!.From = double.IsNaN(oldValue) ? 460 : oldValue;
        doubleAnimation!.To = newValue;
        widthAnimation?.Begin();
    }

    /// <summary>
    ///     Performs a max height animation on the MainView.
    /// </summary>
    /// <param name="oldValue">The original max height value.</param>
    /// <param name="newValue">The new max height value.</param>
    public static void MaxHeightAnimation(double oldValue, double newValue)
    {
        var maxHeightAnimation = MainView.FindResource("MaxHeightAnimation") as Storyboard;
        var doubleAnimation = maxHeightAnimation?.Children.FirstOrDefault() as DoubleAnimation;
        doubleAnimation!.From = double.IsNaN(oldValue) ? 840 : oldValue;
        doubleAnimation!.To = newValue;
        maxHeightAnimation?.Begin();
    }

    /// <summary>
    ///     Performs the main view animation.
    /// </summary>
    /// <param name="show">Indicates whether to show or hide the main view.</param>
    public static void MainViewAnimation(bool show = true)
    {
        var viewAnimation = (MainView.FindResource("MainViewAnimation") as Storyboard)!;
        var doubleAnimation = (viewAnimation.Children.FirstOrDefault() as DoubleAnimation)!;
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

        viewAnimation?.Begin();
    }

    /// <summary>
    ///     Event handler for the animation completed event.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    internal static void AnimationCompleted(object? sender, EventArgs e)
    {
        // Hide the window after the animation is completed
        MainView.Visibility = Visibility.Hidden;
    }
}