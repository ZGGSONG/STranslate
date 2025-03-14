using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STranslate.Style.Controls;

public partial class LoadingUc2 : UserControl
{
    private readonly Storyboard[] _storyboards;

    public LoadingUc2()
    {
        InitializeComponent();

        // 初始化三个故事板数组
        _storyboards = new Storyboard[3];

        // 创建并配置动画
        CreateAnimations();
    }

    #region IsLoading 依赖属性

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoadingUc2),
            new PropertyMetadata(false, OnIsLoadingChanged));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingUc2 control)
        {
            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                control.StartAnimations();
            }
            else
            {
                control.StopAnimations();
            }
        }
    }

    #endregion

    #region DotSize 依赖属性

    public static readonly DependencyProperty DotSizeProperty =
        DependencyProperty.Register(nameof(DotSize), typeof(double), typeof(LoadingUc2),
            new PropertyMetadata(7.0, OnDotSizeChanged));

    public double DotSize
    {
        get => (double)GetValue(DotSizeProperty);
        set => SetValue(DotSizeProperty, value);
    }

    private static void OnDotSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingUc2 control)
        {
            control.UpdateDotSizes();
        }
    }

    private void UpdateDotSizes()
    {
        Dot1.Width = Dot2.Width = Dot3.Width = DotSize;
        Dot1.Height = Dot2.Height = Dot3.Height = DotSize;
    }

    #endregion

    #region DotColor 依赖属性

    public static readonly DependencyProperty DotColorProperty =
        DependencyProperty.Register(nameof(DotColor), typeof(Brush), typeof(LoadingUc2),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498db")), OnDotColorChanged));

    public Brush DotColor
    {
        get => (Brush)GetValue(DotColorProperty);
        set => SetValue(DotColorProperty, value);
    }

    private static void OnDotColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingUc2 control)
        {
            control.UpdateDotColors();
        }
    }

    private void UpdateDotColors()
    {
        Dot1.Fill = Dot2.Fill = Dot3.Fill = DotColor;
    }

    #endregion

    private void CreateAnimations()
    {
        // 为每个小球创建动画
        for (int i = 0; i < 3; i++)
        {
            var storyboard = new Storyboard();
            var dot = i switch
            {
                0 => Dot1,
                1 => Dot2,
                2 => Dot3,
                _ => throw new ArgumentException("Invalid dot index")
            };

            // 宽度动画
            var widthAnimation = new DoubleAnimation
            {
                From = DotSize,
                To = DotSize + 5,
                Duration = TimeSpan.FromSeconds(0.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(0.15 * i)
            };
            Storyboard.SetTarget(widthAnimation, dot);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(FrameworkElement.WidthProperty));
            storyboard.Children.Add(widthAnimation);

            // 高度动画
            var heightAnimation = new DoubleAnimation
            {
                From = DotSize,
                To = DotSize + 5,
                Duration = TimeSpan.FromSeconds(0.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(0.15 * i)
            };
            Storyboard.SetTarget(heightAnimation, dot);
            Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(FrameworkElement.HeightProperty));
            storyboard.Children.Add(heightAnimation);

            _storyboards[i] = storyboard;
        }
    }

    private void StartAnimations()
    {
        foreach (var storyboard in _storyboards)
        {
            storyboard.Begin();
        }
    }

    private void StopAnimations()
    {
        foreach (var storyboard in _storyboards)
        {
            storyboard.Stop();
        }
    }
}