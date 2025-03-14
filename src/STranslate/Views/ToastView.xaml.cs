using System.Windows;
using System.Windows.Media.Animation;

namespace STranslate.Views;

public partial class ToastView
{
    public Storyboard SlideInStoryboard { get; private set; } = new();
    public Storyboard SlideOutStoryboard { get; private set; } = new();
    public Action? OnClicked { get; set; }

    public ToastView()
    {
        InitializeComponent();
    }

    public void CreateStoryboards()
    {
        SlideInStoryboard = CreateStoryboard(-50, 0);
        SlideOutStoryboard = CreateStoryboard(0, -50);
    }

    private Storyboard CreateStoryboard(double from, double to)
    {
        var storyboard = new Storyboard();
        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = new Duration(TimeSpan.FromSeconds(0.25))
        };
        Storyboard.SetTarget(animation, this);
        Storyboard.SetTargetProperty(animation, new PropertyPath("RenderTransform.(TranslateTransform.Y)"));
        storyboard.Children.Add(animation);
        return storyboard;
    }

    public void PlaySlideInAnimation()
    {
        SlideInStoryboard.Begin();
    }

    public void PlaySlideOutAnimation()
    {
        SlideOutStoryboard.Begin();
    }

    public string ToastText
    {
        get => ToastTb.Text;
        set => ToastTb.Text = value;
    }

    private void MouseClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        OnClicked?.Invoke();
    }
}