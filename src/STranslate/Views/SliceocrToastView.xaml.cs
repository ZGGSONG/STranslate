using System.Windows;
using System.Windows.Media.Animation;

namespace STranslate.Views;

public partial class SliceocrToastView : Window
{
    private readonly string FailIcon = "\ue60a";
    private readonly string SuccessIcon = "\ue66a";

    /// <summary>
    ///     显示图标
    /// </summary>
    /// <param name="x">截图结束X坐标</param>
    /// <param name="y">截图结束Y坐标</param>
    /// <param name="isSuccess"></param>
    public SliceocrToastView(double x, double y, bool isSuccess = true)
    {
        InitializeComponent();

        if (x == y && y == 0)
            return;

        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = x + 6;
        Top = y + 6;

        IconTB.Text = isSuccess ? SuccessIcon : FailIcon;

        // Create a DoubleAnimation to animate the Opacity property
        DoubleAnimation opacityAnimation =
            new()
            {
                From = 0,
                To = 0.7,
                Duration = TimeSpan.FromSeconds(0.5)
            };

        // Create a DoubleAnimation to animate the Opacity property to 0
        DoubleAnimation fadeOutAnimation =
            new()
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5),
                BeginTime = TimeSpan.FromSeconds(1)
            };

        // Create a Storyboard to combine the animations
        Storyboard storyboard = new();
        storyboard.Children.Add(opacityAnimation);
        storyboard.Children.Add(fadeOutAnimation);

        // Set the target of the animations to the window's Opacity property
        Storyboard.SetTarget(opacityAnimation, this);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(OpacityProperty));
        Storyboard.SetTarget(fadeOutAnimation, this);
        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(OpacityProperty));

        // Start the storyboard when the window is loaded
        Loaded += (sender, e) => storyboard.Begin();

        // Close the window when the storyboard has finished
        storyboard.Completed += (sender, e) => Close();
    }
}