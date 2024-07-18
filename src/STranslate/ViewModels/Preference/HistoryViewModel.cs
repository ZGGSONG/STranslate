using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.History;
using STranslate.Views.Preference.History;

namespace STranslate.ViewModels.Preference;

public partial class HistoryViewModel : ObservableObject
{
    private const int searchDelayMilliseconds = 500; // 设置延迟时间

    private readonly Timer _searchTimer; // 延时搜索定时器

    /// <summary>
    ///     每页大小
    /// </summary>
    private readonly int pageSize = 30;

    private readonly ScrollViewer viewer = new();

    [ObservableProperty] private int _count;

    [ObservableProperty] private UIElement? _historyDetailContent;

    [ObservableProperty] private BindingList<HistoryModel> _historyList = [];

    [ObservableProperty] private bool _isLoading;

    /// <summary>
    ///     SelectedChanged 可能会先触发 "SelectionChanged" 事件
    ///     然后再触发 "LostFocus" 事件，导致 Command 被调用两次
    /// </summary>
    private bool _isSelectionChanging;

    /// <summary>
    ///     初始游标时间
    /// </summary>
    private DateTime _lastCursorTime = DateTime.Now;

    private CancellationTokenSource? _searchCancellationTokenSource;

    private string _searchContent = string.Empty;

    [ObservableProperty] private int _selectedIndex;

    [ObservableProperty] private List<string> eventList = ["清空全部"];

    public HistoryViewModel()
    {
        _searchTimer = new Timer(async _ => await SearchAsync(), null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    ///     1. 加载中
    ///     2. 在搜索过程中
    ///     3. view列表总数等于数据库总数
    /// </summary>
    public bool CanLoadHistory =>
        !IsLoading && string.IsNullOrEmpty(SearchContent) && (Count == 0 || HistoryList.Count != Count);

    public string SearchContent
    {
        get => _searchContent;
        set
        {
            if (_searchContent != value)
            {
                OnPropertyChanging();
                _searchContent = value;
                OnPropertyChanged();
                ResetSearchTimer();
            }
        }
    }

    [RelayCommand]
    private async Task LoadMoreHistoryAsync(ScrollViewer? scroll)
    {
        IsLoading = true;
        try
        {
            var totalCount = 0;
            if (scroll is not null)
            {
                // 重置游标
                _lastCursorTime = DateTime.Now;

                // 计算总数
                totalCount = await SqlHelper.GetCountAsync();
            }

            var historyData = await SqlHelper.GetDataCursorPagedAsync(pageSize, _lastCursorTime);

            // 未获取到结果则返回
            if (!historyData.Any()) return;

            CommonUtil.InvokeOnUIThread(() =>
            {
                // 如果是手动刷新，清空并重新刷新历史记录
                if (scroll is not null)
                {
                    HistoryList.Clear();
                    Count = totalCount;
                    HistoryDetailContent = null;
                }

                // 更新游标
                _lastCursorTime = historyData.Last().Time;

                // 检查是否已经加载过相同的记录
                var uniqueHistoryData = historyData.Where(h => !HistoryList.Any(existing => existing.Id == h.Id));

                // 插入记录
                HistoryList.AddRange(uniqueHistoryData);

                // 刷新右侧面板
                UpdateHistoryIndex();
            });
        }
        finally
        {
            IsLoading = false;

            scroll?.ScrollToTop();
            //CommonUtil.InvokeOnUIThread(() => ToastHelper.Show("刷新历史记录", WindowType.Preference));
        }
    }

    /// <summary>
    ///     刷新选中历史记录
    /// </summary>
    private void UpdateHistoryIndex(int index = 0)
    {
        index = HistoryList?.Count > index ? index : 0;

        SelectedIndex = index;

        HistoryDetailContent = HistoryList is null || HistoryList?.Count == 0
            ? null
            : new HistoryContentPage(new HistoryContentViewModel(HistoryList?[index]));
    }

    [RelayCommand]
    private void Popup(Popup control)
    {
        control.IsOpen = true;
    }

    /// <summary>
    ///     删除某条记录
    /// </summary>
    [RelayCommand]
    private async Task DeleteHistoryAsync()
    {
        if (HistoryList == null || HistoryList.Count < 1) return;

        var tmpIndex = SelectedIndex;
        var history = HistoryList.ElementAtOrDefault(SelectedIndex);
        if (history == null || !await SqlHelper.DeleteDataAsync(history))
        {
            LogService.Logger.Warn($"删除失败，{history}");

            ToastHelper.Show("删除失败", WindowType.Preference);
            return;
        }

        HistoryList.RemoveAt(SelectedIndex);
        SelectedIndex = tmpIndex < HistoryList.Count ? tmpIndex : tmpIndex - 1;
        Count--;

        UpdateHistoryDetailContent();

        ToastHelper.Show("删除成功", WindowType.Preference);
    }

    /// <summary>
    ///     删除所有记录
    /// </summary>
    [RelayCommand]
    private async Task DeleteAllHistoryAsync(Popup control)
    {
        if (await SqlHelper.DeleteAllDataAsync())
        {
            HistoryList?.Clear();
            Count = 0;
            SelectedIndex = -1;
            HistoryDetailContent = null;

            ToastHelper.Show("删除全部成功", WindowType.Preference);
        }

        control.IsOpen = false;
    }

    /// <summary>
    ///     更新历史详情UI
    /// </summary>
    private void UpdateHistoryDetailContent()
    {
        if (SelectedIndex != -1)
            HistoryDetailContent = new HistoryContentPage(new HistoryContentViewModel(HistoryList![SelectedIndex]));
        else
            HistoryDetailContent = null;
    }

    /// <summary>
    ///     导航页面
    /// </summary>
    /// <param name="model"></param>
    [RelayCommand]
    private void TogglePage(HistoryModel? model)
    {
        // 防止重入
        if (model == null || _isSelectionChanging || HistoryDetailContent is null)
            return;

        _isSelectionChanging = true;

        // 取消TTS
        ((HistoryDetailContent as UserControl)?.DataContext as HistoryContentViewModel)?.TTSCancelCommand
            ?.Execute(null);

        try
        {
            var method = typeof(HistoryContentPage).GetMethod("UpdateVM");
            method?.Invoke(HistoryDetailContent, new[] { new HistoryContentViewModel(model) });
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("历史记录导航出错", ex);
        }
        finally
        {
            _isSelectionChanging = false;
        }
    }

    private void ResetSearchTimer()
    {
        _searchTimer.Change(searchDelayMilliseconds, Timeout.Infinite);
    }

    private async Task SearchAsync()
    {
        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource = new CancellationTokenSource();

        if (string.IsNullOrEmpty(SearchContent))
        {
            await LoadMoreHistoryAsync(viewer);
            return;
        }

        // 执行搜索逻辑，传递 cancellationToken: searchCancellationTokenSource.Token
        var searchRet = await SqlHelper.GetDataAsync(SearchContent, _searchCancellationTokenSource.Token);
        CommonUtil.InvokeOnUIThread(() =>
        {
            // 清空
            HistoryList.Clear();

            HistoryList.AddRange(searchRet);

            UpdateHistoryIndex();
        });
    }
}