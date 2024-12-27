using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference;
using STranslate.Views;

namespace STranslate.ViewModels;

public partial class OutputViewModel : ObservableObject, IDropTarget
{
    private static readonly ConfigModel? CurConfig = Singleton<ConfigHelper>.Instance.CurrentConfig;
    private readonly InputViewModel _inputVm = Singleton<InputViewModel>.Instance;
    private readonly MainViewModel _mainVm = Singleton<MainViewModel>.Instance;

    [ObservableProperty] private bool _isPromptToggleVisible = CurConfig?.IsPromptToggleVisible ?? true;

    [ObservableProperty] private bool _isShowLargeHumpCopyBtn = CurConfig?.IsShowLargeHumpCopyBtn ?? false;

    [ObservableProperty] private bool _isShowSmallHumpCopyBtn = CurConfig?.IsShowSmallHumpCopyBtn ?? false;

    [ObservableProperty] private bool _isShowTranslateBackBtn = CurConfig?.IsShowTranslateBackBtn ?? false;

    [ObservableProperty] private bool _isShowSnakeCopyBtn = CurConfig?.IsShowSnakeCopyBtn ?? false;

    [ObservableProperty]
    private BindingList<ITranslator> _translators = Singleton<TranslatorViewModel>.Instance.CurTransServiceList ?? [];

    [RelayCommand]
    private void ExpanderHeader(List<object> e)
    {
        // 使用模式匹配来简化类型检查和转换
        if (e.FirstOrDefault() is not Expander ep || e.LastOrDefault() is not ITranslator service) return;

        // 检查输入框内容是否为空，如果是，则不展开并直接返回
        if (string.IsNullOrEmpty(_inputVm.InputContent))
        {
            ep.IsExpanded = false;
            return;
        }

        // 检查服务是否正在执行或者结果是否已存在，如果是，则直接返回
        if (service.IsExecuting || !string.IsNullOrEmpty(service.Data.Result?.ToString())) return;

        // 执行翻译服务
        SingleTranslateCommand.Execute(service);
        // 采用新动画避免直接展开，通过结果更新展开状态
        ep.IsExpanded = false;
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task SingleTranslateAsync(ITranslator service, CancellationToken token)
    {
        if (_inputVm is { GetSourceLang: LangEnum.auto, GetTargetLang: LangEnum.auto })
            (_inputVm.GetSourceLang, _inputVm.GetTargetLang) =
                await _inputVm.GetLangInfoAsync(null, null, _mainVm.SourceLang, _mainVm.TargetLang, token);

        await _inputVm.DoTranslateSingleAsync(null, service, null, _inputVm.GetSourceLang, _inputVm.GetTargetLang, token, 0);
        if (service.AutoExecuteTranslateBack)
            await _inputVm.DoTranslateBackSingleAsync(service, _inputVm.GetSourceLang, _inputVm.GetTargetLang, token);

        await PostSingleTranslateAsync(_inputVm.InputContent, _mainVm.SourceLang, _mainVm.TargetLang);
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task SingleTranslateBackAsync(ITranslator service, CancellationToken token)
    {
        // Alt+LeftClick 开关
        if ((Keyboard.Modifiers & ModifierKeys.Alt) > 0)
        {
            var newResult = !service.AutoExecuteTranslateBack;
            service.AutoExecuteTranslateBack = newResult;
            Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);
            ToastHelper.Show($"{(newResult ? "打开" : "关闭")}自动回译");
            return;
        }
        // 执行回译
        if (_inputVm is { GetSourceLang: LangEnum.auto, GetTargetLang: LangEnum.auto })
            (_inputVm.GetSourceLang, _inputVm.GetTargetLang) =
                await _inputVm.GetLangInfoAsync(null, null, _mainVm.SourceLang, _mainVm.TargetLang, token);

        await _inputVm.DoTranslateBackSingleAsync(service, _inputVm.GetSourceLang, _inputVm.GetTargetLang, token);

        await PostSingleTranslateAsync(_inputVm.InputContent, _mainVm.SourceLang, _mainVm.TargetLang);
    }

    private async Task PostSingleTranslateAsync(string content, LangEnum source, LangEnum target)
    {
        var enableServices = Singleton<TranslatorViewModel>.Instance.CurTransServiceList.Where(x => x.IsEnabled);
        var jsonSerializerSettings = new JsonSerializerSettings
            { ContractResolver = new CustomizeContractResolver() };

        var data = new HistoryModel
        {
            Time = DateTime.Now,
            SourceLang = source.GetDescription(),
            TargetLang = target.GetDescription(),
            SourceText = content,
            Data = JsonConvert.SerializeObject(enableServices, jsonSerializerSettings)
        };
        //翻译结果插入数据库
        await SqlHelper.UpdateAsync(data);
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task TTSAsync(string text, CancellationToken token)
    {
        await Singleton<TTSViewModel>.Instance.SpeakTextAsync(text, WindowType.Main, token);
    }

    [RelayCommand]
    private void CopyResult(object obj)
    {
        if (obj is not string str || string.IsNullOrEmpty(str)) return;
        ClipboardHelper.Copy(str);

        ToastHelper.Show("复制成功");
    }

    [RelayCommand]
    private void CopySnakeResult(object obj)
    {
        if (obj is not string str || string.IsNullOrEmpty(str)) return;
        var snakeRet = StringUtil.GenSnakeString(str);
        ClipboardHelper.Copy(snakeRet);

        ToastHelper.Show("蛇形复制成功");
    }

    [RelayCommand]
    private void CopySmallHumpResult(object obj)
    {
        if (obj is not string str || string.IsNullOrEmpty(str)) return;
        var snakeRet = StringUtil.GenHumpString(str, true);
        ClipboardHelper.Copy(snakeRet);

        ToastHelper.Show("小驼峰复制成功");
    }

    [RelayCommand]
    private void CopyLargeHumpResult(object obj)
    {
        if (obj is not string str || string.IsNullOrEmpty(str)) return;
        var snakeRet = StringUtil.GenHumpString(str);
        ClipboardHelper.Copy(snakeRet);

        ToastHelper.Show("大驼峰复制成功");
    }

    /// <summary>
    ///     软件热键复制翻译结果
    /// </summary>
    /// <param name="param">1-9</param>
    [RelayCommand]
    private void HotkeyCopy(string param)
    {
        if (!int.TryParse(param, out var index)) return;

        Singleton<MainViewModel>.Instance.IsHotkeyCopy = true;

        var enabledTranslators = Translators.Where(x => x.IsEnabled).ToList();
        var translator = index == 9
            ? enabledTranslators.LastOrDefault()
            : enabledTranslators.ElementAtOrDefault(index - 1);

        if (translator == null) return;
        
        var result = GetWord(translator.Type, translator.Data?.Result);
        if (string.IsNullOrEmpty(result)) return;

        ClipboardHelper.Copy(result);
        if (CurConfig?.HotkeyCopySuccessToast ?? true)
            ToastHelper.Show($"复制{translator.Name}结果");
    }

    /// <summary>
    ///     软件热键语音播报翻译结果
    /// </summary>
    /// <param name="param">1-9</param>
    [RelayCommand]
    private async Task HotkeyTtsAsync(string param)
    {
        if (!int.TryParse(param, out var index)) return;

        string? result;

        if (index == 0)
        {
            result = _inputVm.InputContent;
            ToastHelper.Show($"播报输入内容");
        }
        else
        {
            var enabledTranslators = Translators.Where(x => x.IsEnabled).ToList();
            var translator = index == 9
                ? enabledTranslators.LastOrDefault()
                : enabledTranslators.ElementAtOrDefault(index - 1);

            if (translator == null) return;

            result = GetWord(translator.Type, translator.Data?.Result);
            if (string.IsNullOrEmpty(result)) return;

            ToastHelper.Show($"播报{translator.Name}结果");
        }

        await TTSCommand.ExecuteAsync(result);
    }

    /// <summary>
    ///     获取单词
    /// </summary>
    /// <param name="serviceType"></param>
    /// <param name="str"></param>
    /// <returns></returns>
    private string? GetWord(ServiceType serviceType, string? str)
    {
        return serviceType switch
        {
            ServiceType.EcdictService => InternalGetWord(),
            ServiceType.BingDictService => InternalGetWord(),
            ServiceType.KingSoftDictService => InternalGetWord(),
            //TODO: 词典
            _ => str
        };

        string InternalGetWord() => str?.Trim().Split(['\r', '\n']).FirstOrDefault() ?? string.Empty;
    }

    public void Clear()
    {
        foreach (var service in Translators)
            TranslationResult.CopyFrom(TranslationResult.Reset, service.Data);
    }

    [RelayCommand]
    private void SelectedPrompt(List<object> list)
    {
        if (list is not [ITranslator service, UserDefinePrompt ud, ToggleButton tb])
            return;

        foreach (var item in service.UserDefinePrompts) item.Enabled = false;
        ud.Enabled = true;
        service.ManualPropChanged(nameof(service.UserDefinePrompts));
        tb.IsChecked = false;

        // 输入内容不为空时才进行翻译
        if (!string.IsNullOrEmpty(_inputVm.InputContent))
            SingleTranslateCommand.Execute(service);
    }

    [RelayCommand]
    private void CanAutoExecute(List<object> list)
    {
        if (list.Count != 2 || list.FirstOrDefault() is not ITranslator service ||
            list.LastOrDefault() is not ToggleButton tb)
            return;

        service.AutoExecute = !service.AutoExecute;
        Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);
        tb.IsChecked = false;
    }

    [RelayCommand]
    private void CanAutoExecuteTranslateBack(List<object> list)
    {
        if (list.Count != 2 || list.FirstOrDefault() is not ITranslator service ||
            list.LastOrDefault() is not ToggleButton tb)
            return;

        service.AutoExecuteTranslateBack = !service.AutoExecuteTranslateBack;
        Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);
        tb.IsChecked = false;
    }

    [RelayCommand]
    private void ExecuteTranslate(List<object> list)
    {
        if (list.Count != 2 || list.FirstOrDefault() is not ITranslator service ||
            list.LastOrDefault() is not ToggleButton tb)
            return;

        SingleTranslateCommand.Execute(service);
        tb.IsChecked = false;
    }

    [RelayCommand]
    private void ExecuteTranslateBack(List<object> list)
    {
        if (list.Count != 2 || list.FirstOrDefault() is not ITranslator service ||
            list.LastOrDefault() is not ToggleButton tb)
            return;

        SingleTranslateBackCommand.Execute(service);
        tb.IsChecked = false;
    }

    [RelayCommand]
    private void NavigateToService(List<object> list)
    {
        if (list.Count != 2 || list.FirstOrDefault() is not ITranslator service ||
            list.LastOrDefault() is not ToggleButton tb)
            return;
        tb.IsChecked = false;
        
        var view = Application.Current.Windows.OfType<PreferenceView>().FirstOrDefault();
        view ??= new PreferenceView();
        view.UpdateNavigation(PerferenceType.Translator);
        view.Show();
        if (view.WindowState == WindowState.Minimized)
            view.WindowState = WindowState.Normal;
        view.Activate();
        
        Singleton<TranslatorViewModel>.Instance.ExternalTogglePage(service);
    }

    [RelayCommand]
    private void CloseService(List<object> list)
    {
        if (list.Count != 2 || list.FirstOrDefault() is not ITranslator service ||
            list.LastOrDefault() is not ToggleButton tb)
            return;

        if (Singleton<TranslatorViewModel>.Instance.CurTransServiceList.Where(x => x.IsEnabled)?.Count() < 2)
        {
            ToastHelper.Show("至少保留一个服务");
            return;
        }

        service.IsEnabled = false;
        Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);
        tb.IsChecked = false;
    }

    [RelayCommand]
    private async Task InsertResultAsync(List<object> obj)
    {
        if (obj?.Count != 2 || obj.First() is not string str || obj.Last() is not MainView win)
            return;

        win.Topmost = false;
        _mainVm.IsTopMost = Constant.TagFalse;
        _mainVm.TopMostContent = Constant.UnTopmostContent;

        win.WindowAnimation(false);

        // 额外主线程等待一段时间，避免动画未完成时执行输入操作
        await Task.Delay(150);
        if (CurConfig?.UsePasteOutput ?? false)
            InputSimulatorHelper.PrintTextWithClipboard(str);
        else
            InputSimulatorHelper.PrintText(str);
    }

    [RelayCommand]
    private void CloseTranslationBackUi(ITranslator translator)
    {
        translator.Data.TranslateBackResult = string.Empty;
    }

    #region gong-wpf-dragdrop interface implementation

    public void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is not ITranslator || dropInfo.TargetItem is not ITranslator) return;
        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
        dropInfo.Effects = DragDropEffects.Copy;
    }

    public void Drop(IDropInfo dropInfo)
    {
        var sourceItem = (ITranslator)dropInfo.Data;
        var targetIndex = dropInfo.InsertIndex > 0 ? dropInfo.InsertIndex - 1 : 0;
        var replaceVm = Singleton<ReplaceViewModel>.Instance;
        var tmp = replaceVm.ReplaceProp.ActiveService?.Identify;
        Translators.Remove(sourceItem);
        Translators.Insert(targetIndex, sourceItem);
        // 检查替换翻译
        if (tmp == sourceItem.Identify) replaceVm.ReplaceProp.ActiveService = sourceItem;

        // Save Configuration
        Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);
    }

    #endregion gong-wpf-dragdrop interface implementation
}