using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using STranslate.Util;
using STranslate.ViewModels.Preference;

namespace STranslate.Views.Preference;

public partial class HistoryPage : UserControl
{
    private readonly HistoryViewModel vm = Singleton<HistoryViewModel>.Instance;

    public HistoryPage()
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void HistoryPageUC_Loaded(object sender, RoutedEventArgs e)
    {
        SearchTb.Focus();
    }

    private void HistoryListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // 按住Ctrl滚动时不将滚动冒泡给上一层级的控件
        if (!e.Handled && !Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
        {
            // ListBox拦截鼠标滚轮事件
            e.Handled = true;

            // 激发一个鼠标滚轮事件，冒泡给外层ListBox接收到
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                { RoutedEvent = MouseWheelEvent, Source = sender };
            var parent = ((Control)sender).Parent as UIElement;
            parent!.RaiseEvent(eventArg);
        }
    }

    private void HistoryListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // 检查是否滚动到底部
        var scrollViewer = (ScrollViewer)e.OriginalSource;

        // 老是触发ScrollableHeight==0，规避一下
        var atBottom = scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight &&
                       scrollViewer.ScrollableHeight != 0;

        //System.Diagnostics.Debug.WriteLine(
        //    $"{DateTime.Now:HH:mm:ss.fff}\t"
        //        + $"{((ScrollViewer)sender).Name}\t"
        //        + $"VerticalOffset: {scrollViewer.VerticalOffset}\t"
        //        + $"ScrollableHeight: {scrollViewer.ScrollableHeight}\t"
        //        + $"bottom: {atBottom}"
        //);

        // 滚动到底 && command执行条件允许才能执行
        if (!atBottom || !vm.CanLoadHistory)
            return;

        // 已经到达底部，执行刷新操作
        vm.LoadMoreHistoryCommand.Execute(null);
    }
}