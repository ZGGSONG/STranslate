using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace STranslate.Style.Controls;

/// <summary>
///     双滑块 Slider
///     * 修改自 <see href="https://www.cnblogs.com/lekko/archive/2012/07/23/2604257.html"/>
///     * 感谢 Claude 3.5 Sonnet
/// </summary>
public partial class SilderArrange : UserControl
{
    // 构造函数
    public SilderArrange()
    {
        InitializeComponent();
    }

    #region 私有变量

    private static readonly int _width = 150; // 拖动条初始宽度
    private static readonly int _height = 30; // 高度
    private static readonly int _min = 0; // 最小值
    private static readonly int _max = 100; // 最大值
    private static readonly int _freq = 10; // 出现刻度的间距

    #endregion

    #region 私有属性

    private RectangleGeometry StartRect
    {
        get => (RectangleGeometry)GetValue(StartRectProperty);
        set => SetValue(StartRectProperty, value);
    }

    private static readonly DependencyProperty StartRectProperty =
        DependencyProperty.Register("StartRect", typeof(RectangleGeometry), typeof(SilderArrange));

    private RectangleGeometry EndRect
    {
        get => (RectangleGeometry)GetValue(EndRectProperty);
        set => SetValue(EndRectProperty, value);
    }

    private static readonly DependencyProperty EndRectProperty =
        DependencyProperty.Register("EndRect", typeof(RectangleGeometry), typeof(SilderArrange));

    #endregion

    #region 公开依赖属性

    /// <summary>
    ///     位置，默认为BottomRight
    /// </summary>
    public TickPlacement SliderTickPlacement
    {
        get => (TickPlacement)GetValue(SliderTickPlacementProperty);
        set => SetValue(SliderTickPlacementProperty, value);
    }

    public static readonly DependencyProperty SliderTickPlacementProperty =
        DependencyProperty.Register("SliderTickPlacement", typeof(TickPlacement), typeof(SilderArrange),
            new PropertyMetadata(TickPlacement.BottomRight));


    /// <summary>
    ///     刻度间距，默认为10
    /// </summary>
    public int SliderTickFrequency
    {
        get => (int)GetValue(SliderTickFrequencyProperty);
        set => SetValue(SliderTickFrequencyProperty, value);
    }

    public static readonly DependencyProperty SliderTickFrequencyProperty =
        DependencyProperty.Register("SliderTickFrequency", typeof(int), typeof(SilderArrange),
            new PropertyMetadata(_freq));

    /// <summary>
    ///     控件高度，默认为30
    /// </summary>
    public int SilderHeight
    {
        get => (int)GetValue(SilderHeightProperty);
        set => SetValue(SilderHeightProperty, value);
    }

    public static readonly DependencyProperty SilderHeightProperty =
        DependencyProperty.Register("SilderHeight", typeof(int), typeof(SilderArrange), new PropertyMetadata(_height));

    /// <summary>
    ///     拖动条宽度，默认为150
    /// </summary>
    public int SilderWidth
    {
        get => (int)GetValue(SilderWidthProperty);
        set => SetValue(SilderWidthProperty, value);
    }

    public static readonly DependencyProperty SilderWidthProperty =
        DependencyProperty.Register("SilderWidth", typeof(int), typeof(SilderArrange), new PropertyMetadata(_width));

    /// <summary>
    ///     最小值，默认为0
    /// </summary>
    public int Minimum
    {
        get => (int)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register("Minimum", typeof(int), typeof(SilderArrange), new PropertyMetadata(_min));

    /// <summary>
    ///     最大值，默认为100
    /// </summary>
    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register("Maximum", typeof(int), typeof(SilderArrange), new PropertyMetadata(_max, OnMaximumChanged));

    private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SilderArrange slider)
        {
            int newMax = (int)e.NewValue;

            // 如果 StartValue 大于新的最大值，更新 StartValue
            if (slider.StartValue > newMax)
            {
                slider.StartValue = newMax;
            }

            // 如果 EndValue 大于新的最大值，更新 EndValue
            if (slider.EndValue > newMax)
            {
                slider.EndValue = newMax;
            }

            // 刷新控件
            slider.ClipSilder();
        }
    }

    /// <summary>
    ///     选中开始值，默认为0
    /// </summary>
    public int StartValue
    {
        get => (int)GetValue(StartValueProperty);
        set => SetValue(StartValueProperty, value);
    }

    public static readonly DependencyProperty StartValueProperty =
        DependencyProperty.Register("StartValue", typeof(int), typeof(SilderArrange),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    ///     选中结束值，默认为100
    /// </summary>
    public int EndValue
    {
        get => (int)GetValue(EndValueProperty);
        set => SetValue(EndValueProperty, value);
    }

    public static readonly DependencyProperty EndValueProperty =
        DependencyProperty.Register("EndValue", typeof(int), typeof(SilderArrange),
            new FrameworkPropertyMetadata(_max, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    #region 前台交互

    /// <summary>
    ///     对两个拖动条进行裁剪
    /// </summary>
    private void ClipSilder()
    {
        var selectedValue = EndValue - StartValue;
        var totalValue = Maximum - Minimum;

        // 添加缓冲区，避免完全重叠
        const double buffer = 4.0;

        // 计算分割点位置
        double sliderClipWidth = SilderWidth * (StartValue - Minimum + selectedValue / 2.0) / totalValue;

        // 调整裁剪区域，添加过渡缓冲
        StartRect = new RectangleGeometry(new Rect(0, 0, sliderClipWidth + buffer, SilderHeight));
        EndRect = new RectangleGeometry(new Rect(Math.Max(0, sliderClipWidth - buffer), 0,
            Math.Max(0, SilderWidth - Math.Max(0, sliderClipWidth - buffer)), SilderHeight));
    }

    /// <summary>
    ///     初始化裁剪
    /// </summary>
    private void UC_Arrange_Loaded(object sender, RoutedEventArgs e)
    {
        ClipSilder();
    }

    private void SL_Bat1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (e.NewValue > EndValue) // 检查值范围
            StartValue = EndValue; // 超出，重设为最大值
        ClipSilder();
    }

    private void SL_Bat2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (e.NewValue < StartValue)
            EndValue = StartValue;
        ClipSilder();
    }

    #endregion
}