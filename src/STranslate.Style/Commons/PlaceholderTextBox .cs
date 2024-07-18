using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace STranslate.Style.Commons;

/// <summary>
///     带占位符的文本输入控件
/// </summary>
public class PlaceholderTextBox : TextBox
{
    #region Public Methods

    public PlaceholderTextBox()
    {
        var plcaeholderBinding = new Binding { Source = this, Path = new PropertyPath("Placeholder") };
        _placeholderTextBlock.SetBinding(TextProperty, plcaeholderBinding);
        _placeholderTextBlock.FontWeight = FontWeights.Thin;
        _placeholderTextBlock.Padding = new Thickness(3, 0, 3, 0);
        _placeholderTextBlock.BorderThickness = new Thickness(0);
        _placeholderTextBlock.Foreground = Brushes.Gray;

        _placeholderVisualBrush.AlignmentX = AlignmentX.Left;
        _placeholderVisualBrush.AlignmentY = AlignmentY.Top;
        _placeholderVisualBrush.Stretch = Stretch.None;
        _placeholderVisualBrush.Visual = _placeholderTextBlock;

        Background = _placeholderVisualBrush;
        TextChanged += PlaceholderTextBox_TextChanged;
    }

    #endregion Public Methods

    #region Events Handling

    /// <summary>
    ///     文本变化的响应
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PlaceholderTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        Background = string.IsNullOrEmpty(Text) ? _placeholderVisualBrush : null;
    }

    #endregion Events Handling

    #region Fields

    /// <summary>
    ///     占位符的文本框
    /// </summary>
    private readonly TextBox _placeholderTextBlock = new();

    /// <summary>
    ///     占位符的画刷
    /// </summary>
    private readonly VisualBrush _placeholderVisualBrush = new();

    #endregion Fields

    #region Properties

    /// <summary>
    ///     占位符的依赖属性
    /// </summary>
    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        "Placeholder",
        typeof(string),
        typeof(PlaceholderTextBox),
        new FrameworkPropertyMetadata("请在此输入", FrameworkPropertyMetadataOptions.AffectsRender)
    );

    /// <summary>
    ///     占位符
    /// </summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    #endregion Properties
}