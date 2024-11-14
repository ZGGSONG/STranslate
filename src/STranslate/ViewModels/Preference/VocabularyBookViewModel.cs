using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.VocabularyBook;
using STranslate.Views.Preference.VocabularyBook;

namespace STranslate.ViewModels.Preference;

public partial class VocabularyBookViewModel : ObservableObject
{
    /// <summary>
    ///     导航 UI 缓存
    /// </summary>
    private readonly Dictionary<Type, UIElement?> _contentCache = [];

    [ObservableProperty]
    private VocabularyBookCollection<IVocabularyBook> _curServiceList = Singleton<ConfigHelper>.Instance.CurrentConfig?.VocabularyBookList ?? [];

    [ObservableProperty] private int _selectedIndex;

    [ObservableProperty] private int _serviceCounter;

    [ObservableProperty] private UIElement? _serviceContent;

    [ObservableProperty] private BindingList<IVocabularyBook> _services = [];

    [ObservableProperty] private IVocabularyBook? _activeVocabularyBook;

    private int _tmpIndex;

    public VocabularyBookViewModel()
    {
        ActiveVocabularyBook = CurServiceList.FirstOrDefault(x => x.IsEnabled);
        //添加默认支持生词本
        Services.Add(new VocabularyBookEuDict());
        //TODO: 新生词本服务需要适配

        ResetView();

        CurServiceList.ListChanged += (sender, e) =>
        {
            if (e.ListChangedType == ListChangedType.ItemChanged)
                ActiveVocabularyBook = CurServiceList.FirstOrDefault(x => x.IsEnabled);
        };
    }

    //public IVocabularyBook? ActiveVocabularyBook => CurServiceList.FirstOrDefault(x => x.IsEnabled);

    public async Task<bool> ExecuteAsync(string content, CancellationToken token)
    {
        if (ActiveVocabularyBook is not null)
            return await ActiveVocabularyBook.ExecuteAsync(content, token);

        LogService.Logger.Warn("未启用生词本服务");
        return false;
    }

    /// <summary>
    ///     重置选中项
    /// </summary>
    /// <param name="type"></param>
    private void ResetView(ActionType type = ActionType.Initialize)
    {
        ServiceCounter = CurServiceList.Count;

        //当全部删除时则清空view绑定属性
        if (ServiceCounter < 1)
        {
            SelectedIndex = 0;
            ServiceContent = null;
            return;
        }

        switch (type)
        {
            case ActionType.Delete:
            {
                //不允许小于0
                SelectedIndex = Math.Max(_tmpIndex - 1, 0);
                TogglePage(CurServiceList[SelectedIndex]);
                break;
            }
            case ActionType.Add:
            {
                //选中最后一项
                SelectedIndex = ServiceCounter - 1;
                TogglePage(CurServiceList[SelectedIndex]);
                break;
            }
            default:
            {
                //初始化默认执行选中第一条
                SelectedIndex = 0;
                TogglePage(CurServiceList.First());
                break;
            }
        }
    }

    [RelayCommand]
    private void TogglePage(IVocabularyBook? service)
    {
        if (service == null) return;

        if (SelectedIndex != -1)
            _tmpIndex = SelectedIndex;

        const string head = "STranslate.Views.Preference.VocabularyBook.";
        var name = service.Type switch
        {
            VocabularyBookType.EuDictVocabularyBook => $"{head}{nameof(VocabularyBookEuDictPage)}",
            //TODO: 新生词本服务需要适配
            _ => $"{head}{nameof(VocabularyBookEuDictPage)}"
        };

        NavigationPage(name, service);
    }

    [RelayCommand]
    private void Add(List<object> list)
    {
        if (list?.Count != 2) return;
        var service = list.First();

        CurServiceList.Add(service switch
        {
            VocabularyBookEuDict euDict => euDict.Clone(),
            //TODO: 新生词本服务需要适配
            _ => throw new InvalidOperationException($"Unsupported VocabularyBook type: {service.GetType().Name}")
        });

        (list.Last() as ToggleButton)!.IsChecked = false;

        ResetView(ActionType.Add);
    }

    [RelayCommand]
    private void Delete(IVocabularyBook? service)
    {
        if (service == null) return;
        CurServiceList.Remove(service);

        ResetView(ActionType.Delete);

        ToastHelper.Show("删除成功", WindowType.Preference);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (ActiveVocabularyBook != null)
        {
            var checkResult = await ActiveVocabularyBook.CheckAsync(CancellationToken.None);
            if (!checkResult)
            {
                ToastHelper.Show("请检查生词本配置", WindowType.Preference);
                return;
            }
        }

        if (!Singleton<ConfigHelper>.Instance.WriteConfig(CurServiceList))
        {
            LogService.Logger.Warn($"保存生词本失败，{JsonConvert.SerializeObject(CurServiceList)}");

            ToastHelper.Show("保存失败", WindowType.Preference);
        }

        ToastHelper.Show("保存成功", WindowType.Preference);
    }

    [RelayCommand]
    private void Reset()
    {
        CurServiceList = Singleton<ConfigHelper>.Instance.ResetConfig.VocabularyBookList ?? [];
        ResetView();
        ToastHelper.Show("重置配置", WindowType.Preference);
    }

    /// <summary>
    ///     导航页面
    /// </summary>
    /// <param name="name"></param>
    /// <param name="service"></param>
    public void NavigationPage(string name, IVocabularyBook? service)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(service);

            var type = Type.GetType(name) ?? throw new Exception($"{nameof(NavigationPage)} get {name} exception");

            //读取缓存是否存在，存在则从缓存中获取View实例并通过UpdateVM刷新ViewModel
            UIElement? content = null;
            if (_contentCache.TryGetValue(type, out var value))
            {
                content = value;
                if (content is UserControl uc)
                {
                    var method = type.GetMethod("UpdateVM");
                    method?.Invoke(uc, new object?[] { service });
                }
            }
            else //不存在则创建并通过构造函数传递ViewModel
            {
                content = (UIElement?)Activator.CreateInstance(type, service);
                _contentCache.Add(type, content);
            }

            ServiceContent = content;
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("生词本导航出错", ex);
        }
    }
}