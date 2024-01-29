using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.History;
using STranslate.Views.Preference.History;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace STranslate.ViewModels.Preference
{
    public partial class HistoryViewModel : ObservableObject
    {

        public HistoryViewModel()
        {
            // 异步加载
            Task.Run(async () => await LoadMoreHistory(null));
        }

        [RelayCommand]
        private async Task LoadMoreHistory(object? obj)
        {
            // 清空搜索框
            SearchContent = string.Empty;

            if (IsLoading) return;

            IsLoading = true;

            try
            {
                _lastCursorTime = obj is null ? DateTime.Now : _lastCursorTime;

                var historyData = await SqlHelper.GetDataCursorPagedAsync(pageSize, _lastCursorTime);

                if (!historyData.Any()) return;

                CommonUtil.InvokeOnUIThread(() =>
                {
                    // 更新游标
                    _lastCursorTime = historyData.Last().Time;

                    // 检查是否已经加载过相同的记录
                    var uniqueHistoryData = historyData.Where(h => !HistoryList.Any(existing => existing.Id == h.Id));
                    
                    // 当前添加的数据的数量
                    var curCount = uniqueHistoryData.Count();
                   
                    if (obj is null)
                    {
                        HistoryList.Insert(0, uniqueHistoryData);
                    }
                    else
                    {
                        HistoryList.AddRange(uniqueHistoryData);
                    }

                    // 缓存一份
                    tmpList = HistoryList;

                    // 刷新历史记录数量
                    Count = HistoryList.Count;

                    if (Count > 0)
                    {
                        // 刷新右侧面板
                        UpdateHistoryIndex(obj is null ? 0 : Count - curCount);
                    }
                    else
                    {
                        HistoryDetailContent = null;
                    }
                });
            }
            finally
            {
                IsLoading = false;

                if (obj is null) ToastHelper.Show("刷新历史记录", WindowType.Preference);
            }
        }

        /// <summary>
        /// 刷新选中历史记录
        /// </summary>
        private void UpdateHistoryIndex(int index = 0)
        {
            index = HistoryList?.Count > index ? index : 0;

            SelectedIndex = index;

            HistoryDetailContent = HistoryList is null || HistoryList?.Count == 0 ? null : new HistoryContentPage(new HistoryContentViewModel(HistoryList?[index]));
        }

        [RelayCommand]
        private void Popup(Popup control) => control.IsOpen = true;

        /// <summary>
        /// 删除某条记录
        /// </summary>
        [RelayCommand]
        private async Task DeleteHistoryAsync()
        {
            if (HistoryList == null || HistoryList.Count < 1)
            {
                return;
            }

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
        /// 删除所有记录
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
        /// 更新历史详情UI
        /// </summary>
        private void UpdateHistoryDetailContent()
        {
            if (SelectedIndex != -1)
            {
                HistoryDetailContent = new HistoryContentPage(new HistoryContentViewModel(HistoryList![SelectedIndex]));
            }
            else
            {
                HistoryDetailContent = null;
            }
        }

        /// <summary>
        /// 导航页面
        /// </summary>
        /// <param name="model"></param>
        [RelayCommand]
        private void TogglePage(HistoryModel? model)
        {
            // 防止重入
            if (model == null || _isSelectionChanging || HistoryDetailContent is null)
                return;

            _isSelectionChanging = true;

            try
            {
                var method = typeof(HistoryContentPage).GetMethod("UpdateVM");
                method?.Invoke(HistoryDetailContent, new[] { new HistoryContentViewModel(model) });
            }
            catch(Exception ex)
            {
                LogService.Logger.Error("历史记录导航出错", ex);
            }
            finally
            {
                _isSelectionChanging = false;
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            await Task.Run(() =>
            {
                CommonUtil.InvokeOnUIThread(() =>
                {
                    HistoryList = string.IsNullOrEmpty(SearchContent) ? tmpList ?? []
                    : new BindingList<HistoryModel>(tmpList?.Where(x => x.SourceText.Contains(SearchContent, StringComparison.CurrentCultureIgnoreCase))?.ToList() ?? []);

                    UpdateHistoryIndex();
                });
            });
        }

        private BindingList<HistoryModel>? tmpList;

        /// <summary>
        /// SelectedChanged 可能会先触发 "SelectionChanged" 事件
        /// 然后再触发 "LostFocus" 事件，导致 Command 被调用两次
        /// </summary>
        private bool _isSelectionChanging = false;

        [ObservableProperty]
        private List<string> eventList = ["清空全部"];

        [ObservableProperty]
        private int _selectedIndex;

        [ObservableProperty]
        private int _count = 0;

        [ObservableProperty]
        private UIElement? _historyDetailContent;

        [ObservableProperty]
        private BindingList<HistoryModel> _historyList = [];

        [ObservableProperty]
        private string _searchContent = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        /// <summary>
        /// 每页大小
        /// </summary>
        private readonly int pageSize = 10;

        /// <summary>
        /// 初始游标时间
        /// </summary>
        private DateTime _lastCursorTime = DateTime.Now;
    }
}