using System.Windows;
using System.Windows.Media.Animation;

namespace STranslate.Style.Controls;

public partial class LoadingUc : IDisposable
{
    public static readonly DependencyProperty IsAnimationPlayingProperty = DependencyProperty.Register(
        nameof(IsAnimationPlaying),
        typeof(bool),
        typeof(LoadingUc),
        new PropertyMetadata(false, OnIsAnimationPlayingChanged)
    );

    private readonly Storyboard _opacityStoryboard;

    public LoadingUc()
    {
        InitializeComponent();
        _opacityStoryboard = (Storyboard)FindResource("OpacityAnimation");
    }

    public bool IsAnimationPlaying
    {
        get => (bool)GetValue(IsAnimationPlayingProperty);
        set => SetValue(IsAnimationPlayingProperty, value);
    }

    public void Dispose()
    {
        _opacityStoryboard.Stop();
        _opacityStoryboard.Remove(); // 移除动画，释放资源
    }

    private static void OnIsAnimationPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var uc = (LoadingUc)d;
        if (uc.IsAnimationPlaying)
            uc._opacityStoryboard.Begin();
        else
            uc._opacityStoryboard.Stop();
    }
}