using System.ComponentModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference;

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

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ExpanderHeaderAsync(List<object> e, CancellationToken token)
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
        await SingleTranslateAsync(service, token);
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task SingleTranslateAsync(ITranslator service, CancellationToken token)
    {
        var content = _inputVm.InputContent;
        var sourceLang = _mainVm.SourceLang;
        var targetLang = _mainVm.TargetLang;
        // 选中的目标语种
        var selectTargetLang = targetLang;
        try
        {
            service.Data = TranslationResult.Reset;
            service.IsExecuting = true;

            var identify = LangEnum.auto;

            // 原始语种自动识别
            if (sourceLang == LangEnum.auto)
            {
                var detectType = Singleton<ConfigHelper>.Instance.CurrentConfig?.DetectType ?? LangDetectType.Local;
                var rate = Singleton<ConfigHelper>.Instance.CurrentConfig?.AutoScale ?? 0.8;
                identify = await LangDetectHelper.DetectAsync(content, detectType, rate, token);
            }

            //如果是自动则获取自动识别后的目标语种
            if (targetLang == LangEnum.auto)
                //目标语言
                //1. 识别语种为中文系则目标语言为英文
                //2. 识别语种为自动或其他语系则目标语言为中文
                targetLang = identify is LangEnum.zh_cn or LangEnum.zh_tw or LangEnum.yue
                    ? LangEnum.en
                    : LangEnum.zh_cn;

            //根据不同服务类型区分-默认非流式请求数据，若走此种方式请求则无需添加
            if (service is ITranslatorLlm)
            {
                //流式处理目前给AI使用，所以可以传递识别语言给AI做更多处理
                //Auto则转换为识别语种
                sourceLang = sourceLang == LangEnum.auto ? identify : sourceLang;
                await _inputVm.StreamHandlerAsync(service, _inputVm.InputContent, sourceLang, targetLang, token);
            }
            else
            {
                await _inputVm.NonStreamHandlerAsync(service, _inputVm.InputContent, sourceLang, targetLang, token);
            }
        }
        catch (Exception exception)
        {
            var errorMessage = "";
            var isCancelMsg = false;
            switch (exception)
            {
                case TaskCanceledException:
                    errorMessage = token.IsCancellationRequested ? "请求取消" : "请求超时";
                    isCancelMsg = token.IsCancellationRequested;
                    break;

                case HttpRequestException:
                    errorMessage = "请求出错";
                    break;

                default:
                    errorMessage = "翻译出错";
                    break;
            }

            service.Data = TranslationResult.Fail($"{errorMessage}: {exception.Message}", exception);

            if (isCancelMsg)
                LogService.Logger.Debug(
                    $"[{service.Name}({service.Identify})] {errorMessage}, 请求API: {service.Url}, 异常信息: {exception.Message}");
            else
                LogService.Logger.Error(
                    $"[{service.Name}({service.Identify})] {errorMessage}, 请求API: {service.Url}, 异常信息: {exception.Message}");
        }
        finally
        {
            if (service.IsExecuting) service.IsExecuting = false;

            await HandleHistoryAsync(content, sourceLang, selectTargetLang);
        }
    }

    private async Task HandleHistoryAsync(string content, LangEnum source, LangEnum dbTarget)
    {
        var enableServices = Singleton<TranslatorViewModel>.Instance.CurTransServiceList.Where(x => x.IsEnabled);
        var jsonSerializerSettings = new JsonSerializerSettings
            { ContractResolver = new CustomizeContractResolver() };

        var data = new HistoryModel
        {
            Time = DateTime.Now,
            SourceLang = source.GetDescription(),
            TargetLang = dbTarget.GetDescription(),
            SourceText = content,
            Data = JsonConvert.SerializeObject(enableServices, jsonSerializerSettings)
        };
        //翻译结果插入数据库
        await SqlHelper.UpdateAsync(data);
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task TTS(string text, CancellationToken token)
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

        var result = translator.Data?.Result?.ToString();
        if (string.IsNullOrEmpty(result)) return;

        ClipboardHelper.Copy(result);
        if (CurConfig?.HotkeyCopySuccessToast ?? true)
            ToastHelper.Show($"复制{translator.Name}结果");
    }

    public void Clear()
    {
        foreach (var item in Translators) item.Data = TranslationResult.Reset;
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task SelectedPromptAsync(List<object> list, CancellationToken token)
    {
        if (list is not [ITranslator service, UserDefinePrompt ud, ToggleButton tb])
            return;

        foreach (var item in service.UserDefinePrompts) item.Enabled = false;
        ud.Enabled = true;
        service.ManualPropChanged(nameof(service.UserDefinePrompts));
        tb.IsChecked = false;

        // 输入内容不为空时才进行翻译
        if (!string.IsNullOrEmpty(_inputVm.InputContent))
            await SingleTranslateAsync(service, token);
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
        if (obj?.Count != 2 || obj.First() is not string str || obj.Last() is not Window win)
            return;

        win.Topmost = false;
        _mainVm.IsTopMost = ConstStr.TAGFALSE;
        _mainVm.TopMostContent = ConstStr.UNTOPMOSTCONTENT;

        LogService.Logger.Debug("<Start> [Animation]");
        await AnimationHelper.MainViewAnimationAsync(false);
        LogService.Logger.Debug("<End> [Animation]");

        // 额外主线程等待一段时间，避免动画未完成时执行输入操作
        await Task.Delay(100);
        LogService.Logger.Debug("<Start> [Output]");
        InputSimulatorHelper.PrintText(str);
        LogService.Logger.Debug("<End> [Output]");
    }
    
    [RelayCommand]
    private void TranslateBack(object obj)
    {
        if (obj is not string str || string.IsNullOrEmpty(str)) return;
        //根据Ctrl+LeftClick null is check database first
        var dbFirst = (Keyboard.Modifiers & ModifierKeys.Control) <= 0 ? null : "";
        _inputVm.InputContent = str;
        CancelAndTranslate(dbFirst);
    }

    private void CancelAndTranslate(string? dbFirst)
    {
        ExpanderHeaderCancelCommand.Execute(null);
        SelectedPromptCancelCommand.Execute(null);
        SingleTranslateCancelCommand.Execute(null);
        _inputVm.TranslateCancelCommand.Execute(null);
        _inputVm.TranslateCommand.Execute(dbFirst);
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