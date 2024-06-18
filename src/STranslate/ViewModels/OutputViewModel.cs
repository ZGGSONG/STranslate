using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    private readonly InputViewModel _inputVm = Singleton<InputViewModel>.Instance;
    private readonly MainViewModel _mainVm = Singleton<MainViewModel>.Instance;

    [ObservableProperty]
    private BindingList<ITranslator> _translators = Singleton<TranslatorViewModel>.Instance.CurTransServiceList ?? [];

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ExpanderHeaderAsync(List<object> e, CancellationToken token)
    {
        if (e.First() is not Expander ep || e.Last() is not ITranslator service) return;

        if (string.IsNullOrEmpty(_inputVm.InputContent))
        {
            ep.IsExpanded = false;
            return;
        }

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
                identify = await LangDetectHelper.DetectAsync(content, detectType, token);
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
            //TODO: 新接口需要适配
            switch (service.Type)
            {
                case ServiceType.GeminiService:
                case ServiceType.OpenAIService:
                case ServiceType.ChatglmService:
                case ServiceType.OllamaService:
                case ServiceType.BaiduBceService:
                {
                    //流式处理目前给AI使用，所以可以传递识别语言给AI做更多处理
                    //Auto则转换为识别语种
                    sourceLang = sourceLang == LangEnum.auto ? identify : sourceLang;
                    await _inputVm.StreamHandlerAsync(service, _inputVm.InputContent, sourceLang, targetLang, token);
                    break;
                }

                default:
                    await _inputVm.NonStreamHandlerAsync(service, _inputVm.InputContent, sourceLang, targetLang, token);
                    break;
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
        if (obj is string str && !string.IsNullOrEmpty(str))
        {
            ClipboardHelper.Copy(str);

            ToastHelper.Show("复制成功");
        }
    }

    [RelayCommand]
    private void CopySnakeResult(object obj)
    {
        if (obj is not string str || string.IsNullOrEmpty(str)) return;
        var snakeRet = StringUtil.GenSnakeString([.. str.Split(' ')]);
        ClipboardHelper.Copy(snakeRet);

        ToastHelper.Show("蛇形复制成功");
    }

    [RelayCommand]
    private void CopySmallHumpResult(object obj)
    {
        if (obj is string str && !string.IsNullOrEmpty(str))
        {
            var snakeRet = StringUtil.GenHumpString([.. str.Split(' ')], true);
            ClipboardHelper.Copy(snakeRet);

            ToastHelper.Show("小驼峰复制成功");
        }
    }

    [RelayCommand]
    private void CopyLargeHumpResult(object obj)
    {
        if (obj is string str && !string.IsNullOrEmpty(str))
        {
            var snakeRet = StringUtil.GenHumpString([.. str.Split(' ')], false);
            ClipboardHelper.Copy(snakeRet);

            ToastHelper.Show("大驼峰复制成功");
        }
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
        ToastHelper.Show($"复制{translator.Name}结果");
    }

    [RelayCommand]
    private void UpdateAutoExpander(ITranslator param)
    {
        param.AutoExpander = !param.AutoExpander;

        Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);
    }

    [RelayCommand]
    private void CloseService(ITranslator param)
    {
        if (Singleton<TranslatorViewModel>.Instance.CurTransServiceList.Where(x => x.IsEnabled).Count() < 2)
        {
            ToastHelper.Show("至少保留一个服务");
            return;
        }

        param.IsEnabled = false;

        Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);
    }

    public void Clear()
    {
        foreach (var item in Translators) item.Data = TranslationResult.Reset;
    }

    #region gong-wpf-dragdrop interface implementation

    public void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is ITranslator && dropInfo.TargetItem is ITranslator)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = DragDropEffects.Copy;
        }
    }

    public void Drop(IDropInfo dropInfo)
    {
        var sourceItem = (ITranslator)dropInfo.Data;
        var targetIndex = dropInfo.InsertIndex > 0 ? dropInfo.InsertIndex - 1 : 0;

        Translators.Remove(sourceItem);
        Translators.Insert(targetIndex, sourceItem);

        // Save Configuration
        Singleton<TranslatorViewModel>.Instance.SaveCommand.Execute(null);
    }

    #endregion gong-wpf-dragdrop interface implementation
}