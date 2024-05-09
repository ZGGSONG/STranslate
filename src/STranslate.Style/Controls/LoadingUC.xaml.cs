using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace STranslate.Style.Controls;

public partial class LoadingUC : UserControl, IDisposable
{
    private readonly Storyboard OpacityStoryboard;

    public static readonly DependencyProperty IsAnimationPlayingProperty = DependencyProperty.Register(
        nameof(IsAnimationPlaying),
        typeof(bool),
        typeof(LoadingUC),
        new PropertyMetadata(false, OnIsAnimationPlayingChanged)
    );

    public bool IsAnimationPlaying
    {
        get => (bool)GetValue(IsAnimationPlayingProperty);
        set => SetValue(IsAnimationPlayingProperty, value);
    }

    private static void OnIsAnimationPlayingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var uc = (LoadingUC)d;
        if (uc.IsAnimationPlaying)
        {
            uc.OpacityStoryboard.Begin();
        }
        else
        {
            uc.OpacityStoryboard.Stop();
        }
    }

    public LoadingUC()
    {
        InitializeComponent();
        OpacityStoryboard = (Storyboard)FindResource("OpacityAnimation");
    }

    public void Dispose()
    {
        OpacityStoryboard.Stop();
        OpacityStoryboard.Remove(); // 移除动画，释放资源
    }
}
