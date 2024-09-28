using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STranslate.Style.Themes;

public class ThemeProps
{
    /// <summary>
    ///     是否使用动画
    /// </summary>
    public static bool IsThemeChangedAnimation { get; set; } = true;

    #region Animation

    private static void AnimateBrushProperty(FrameworkElement element, SolidColorBrush newBrush, string propertyName)
    {
        var property = element.GetType().GetProperty(propertyName);

        if (property == null) return;

        if (property.GetValue(element) is not SolidColorBrush currentBrush || currentBrush.IsFrozen)
        {
            currentBrush = new SolidColorBrush(newBrush.Color);

            property.SetValue(element, currentBrush);
        }

        currentBrush.BeginAnimation(SolidColorBrush.ColorProperty, new ColorAnimation
        {
            To = newBrush.Color,
            Duration = TimeSpan.FromSeconds(0.3)
        });
    }

    #endregion

    #region PropertyChanged

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e, string propertyName)
    {
        if (d is not FrameworkElement element || e.NewValue is not SolidColorBrush newBrush)
            return;

        if (IsThemeChangedAnimation)
        {
            AnimateBrushProperty(element, newBrush, propertyName);
        }
        else
        {
            var property = element.GetType().GetProperty(propertyName);
            property?.SetValue(element, newBrush);
        }
    }

    #endregion

    #region Background

    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached("Background",
        typeof(Brush), typeof(ThemeProps), new PropertyMetadata(null, (d, e) => OnPropertyChanged(d, e, "Background")));

    public static Brush GetBackground(DependencyObject obj)
    {
        return (Brush)obj.GetValue(BackgroundProperty);
    }

    public static void SetBackground(DependencyObject obj, Brush value)
    {
        obj.SetValue(BackgroundProperty, value);
    }

    #endregion

    #region Foreground

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.RegisterAttached("Foreground", typeof(Brush), typeof(ThemeProps),
            new PropertyMetadata(null, (d, e) => OnPropertyChanged(d, e, "Foreground")));

    public static Brush GetForeground(DependencyObject obj)
    {
        return (Brush)obj.GetValue(ForegroundProperty);
    }

    public static void SetForeground(DependencyObject obj, Brush value)
    {
        obj.SetValue(ForegroundProperty, value);
    }

    #endregion

    #region BorderBrush

    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.RegisterAttached("BorderBrush", typeof(Brush), typeof(ThemeProps),
            new PropertyMetadata(null, (d, e) => OnPropertyChanged(d, e, "BorderBrush")));

    public static Brush GetBorderBrush(DependencyObject obj)
    {
        return (Brush)obj.GetValue(BorderBrushProperty);
    }

    public static void SetBorderBrush(DependencyObject obj, Brush value)
    {
        obj.SetValue(BorderBrushProperty, value);
    }

    #endregion

    #region SelectionBrush

    public static readonly DependencyProperty SelectionBrushProperty =
        DependencyProperty.RegisterAttached("SelectionBrush", typeof(Brush), typeof(ThemeProps),
            new PropertyMetadata(null, (d, e) => OnPropertyChanged(d, e, "SelectionBrush")));

    public static Brush GetSelectionBrush(DependencyObject obj)
    {
        return (Brush)obj.GetValue(SelectionBrushProperty);
    }

    public static void SetSelectionBrush(DependencyObject obj, Brush value)
    {
        obj.SetValue(SelectionBrushProperty, value);
    }

    #endregion

    #region CaretBrush

    public static readonly DependencyProperty CaretBrushProperty =
        DependencyProperty.RegisterAttached("CaretBrush", typeof(Brush), typeof(ThemeProps),
            new PropertyMetadata(null, (d, e) => OnPropertyChanged(d, e, "CaretBrush")));

    public static Brush GetCaretBrush(DependencyObject obj)
    {
        return (Brush)obj.GetValue(CaretBrushProperty);
    }

    public static void SetCaretBrush(DependencyObject obj, Brush value)
    {
        obj.SetValue(CaretBrushProperty, value);
    }

    #endregion

    #region Stroke

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.RegisterAttached("Stroke", typeof(Brush), typeof(ThemeProps),
            new PropertyMetadata(null, (d, e) => OnPropertyChanged(d, e, "Stroke")));

    public static Brush GetStroke(DependencyObject obj)
    {
        return (Brush)obj.GetValue(StrokeProperty);
    }

    public static void SetStroke(DependencyObject obj, Brush value)
    {
        obj.SetValue(StrokeProperty, value);
    }

    #endregion
}