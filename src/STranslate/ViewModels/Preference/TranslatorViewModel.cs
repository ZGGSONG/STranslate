﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Style.Controls;
using STranslate.Util;
using STranslate.ViewModels.Preference.Translator;
using STranslate.Views.Preference;
using STranslate.Views.Preference.Translator;

namespace STranslate.ViewModels.Preference;

public partial class TranslatorViewModel : ObservableObject
{
    /// <summary>
    ///     导航 UI 缓存
    /// </summary>
    private readonly Dictionary<Type, UIElement?> ContentCache = [];

    /// <summary>
    ///     当前已添加的服务列表
    /// </summary>
    [ObservableProperty] private BindingList<ITranslator> _curTransServiceList = [..Singleton<ConfigHelper>.Instance.CurrentConfig?.Services ?? []];

    [ObservableProperty] private int _selectedIndex;

    public Action? OnSelectedServiceChanged;

    [ObservableProperty] private UIElement? _serviceContent;

    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private int _serviceCounter;

    /// <summary>
    ///     支持的服务
    /// </summary>
    [ObservableProperty] private BindingList<ITranslator> _transServices = [];

    private int tmpIndex;

    public TranslatorViewModel()
    {
        //添加默认支持服务
        TransServices.Add(new TranslatorDeepLX());
        TransServices.Add(new TranslatorSTranslate());
        TransServices.Add(new TranslatorGoogleBuiltin());
        TransServices.Add(new TranslatorMicrosoftBuiltin());
        TransServices.Add(new TranslatorYandexBuiltIn());
        TransServices.Add(new TranslatorTransmartBuiltIn());
        TransServices.Add(new TranslatorKingSoftDict());
        TransServices.Add(new TranslatorBingDict());
        TransServices.Add(new TranslatorEcdict());
        TransServices.Add(new TranslatorDeepL());
        TransServices.Add(new TranslatorOpenAI());
        TransServices.Add(new TranslatorDeerAPI());
        TransServices.Add(new TranslatorClaude());
        TransServices.Add(new TranslatorChatglm());
        TransServices.Add(new TranslatorDeepSeek());
        TransServices.Add(new TranslatorAzureOpenAI());
        TransServices.Add(new TranslatorOllama());
        TransServices.Add(new TranslatorBaiduBce());
        TransServices.Add(new TranslatorGemini());
        TransServices.Add(new TranslatorAli());
        TransServices.Add(new TranslatorBaidu());
        TransServices.Add(new TranslatorTencent());
        TransServices.Add(new TranslatorMicrosoft());
        TransServices.Add(new TranslatorYoudao());
        TransServices.Add(new TranslatorVolcengine());
        TransServices.Add(new TranslatorNiutrans());
        TransServices.Add(new TranslatorCaiyun());
        //TODO: 新接口需要适配

        ResetView();
    }

    private bool CanDelete => ServiceCounter > 1;

    /// <summary>
    ///     重置选中项
    /// </summary>
    /// <param name="type"></param>
    private void ResetView(ActionType type = ActionType.Initialize)
    {
        ServiceCounter = CurTransServiceList.Count;

        if (ServiceCounter < 1)
            return;

        switch (type)
        {
            case ActionType.Delete:
            {
                //不允许小于0
                SelectedIndex = Math.Max(tmpIndex - 1, 0);
                TogglePageCommand.Execute(CurTransServiceList[SelectedIndex]);
                break;
            }
            case ActionType.Add:
            {
                //选中最后一项
                SelectedIndex = ServiceCounter - 1;
                TogglePageCommand.Execute(CurTransServiceList[SelectedIndex]);
                break;
            }
            case ActionType.Next:
            {
                //选中当前下一项
                SelectedIndex += 1;
                TogglePageCommand.Execute(CurTransServiceList[SelectedIndex]);
                break;
            }
            default:
            {
                //初始化默认执行选中第一条
                SelectedIndex = 0;
                TogglePageCommand.Execute(CurTransServiceList.First());
                break;
            }
        }

        // 刷新当前服务列表位置放后面才能刷新
        OnSelectedServiceChanged?.Invoke();
    }

    public void ExternalTogglePage(ITranslator? service)
    {
        if (service is null) return;
        SelectedIndex = CurTransServiceList.IndexOf(service);
        TogglePage(service);
        OnSelectedServiceChanged?.Invoke();
    }
    
    [RelayCommand]
    private void TogglePage(ITranslator? service)
    {
        if (service is null) return;

        if (SelectedIndex != -1)
            tmpIndex = SelectedIndex;

        const string head = "STranslate.Views.Preference.Translator.";
        var name = service.Type switch
        {
            ServiceType.STranslateService => $"{head}{nameof(TranslatorSTranslatePage)}",
            ServiceType.GoogleBuiltinService => $"{head}{nameof(TranslatorGoogleBuiltinPage)}",
            ServiceType.BaiduService => $"{head}{nameof(TranslatorBaiduPage)}",
            ServiceType.MicrosoftService => $"{head}{nameof(TranslatorMicrosoftPage)}",
            ServiceType.OpenAIService => $"{head}{nameof(TranslatorOpenAIPage)}",
            ServiceType.GeminiService => $"{head}{nameof(TranslatorGeminiPage)}",
            ServiceType.TencentService => $"{head}{nameof(TranslatorTencentPage)}",
            ServiceType.AliService => $"{head}{nameof(TranslatorAliPage)}",
            ServiceType.YoudaoService => $"{head}{nameof(TranslatorYoudaoPage)}",
            ServiceType.NiutransService => $"{head}{nameof(TranslatorNiutransPage)}",
            ServiceType.CaiyunService => $"{head}{nameof(TranslatorCaiyunPage)}",
            ServiceType.VolcengineService => $"{head}{nameof(TranslatorVolcenginePage)}",
            ServiceType.EcdictService => $"{head}{nameof(TranslatorEcdictPage)}",
            ServiceType.ChatglmService => $"{head}{nameof(TranslatorChatglmPage)}",
            ServiceType.OllamaService => $"{head}{nameof(TranslatorOllamaPage)}",
            ServiceType.BaiduBceService => $"{head}{nameof(TranslatorBaiduBcePage)}",
            ServiceType.DeepLService => $"{head}{nameof(TranslatorDeepLPage)}",
            ServiceType.AzureOpenAIService => $"{head}{nameof(TranslatorAzureOpenAIPage)}",
            ServiceType.ClaudeService => $"{head}{nameof(TranslatorClaudePage)}",
            ServiceType.DeepSeekService => $"{head}{nameof(TranslatorDeepSeekPage)}",
            ServiceType.KingSoftDictService => $"{head}{nameof(TranslatorKingSoftDictPage)}",
            ServiceType.BingDictService => $"{head}{nameof(TranslatorBingDictPage)}",
            ServiceType.DeepLXService => $"{head}{nameof(TranslatorDeepLXPage)}",
            ServiceType.YandexBuiltInService => $"{head}{nameof(TranslatorYandexBuiltInPage)}",
            ServiceType.MicrosoftBuiltinService => $"{head}{nameof(TranslatorMicrosoftBuiltinPage)}",
            ServiceType.DeerAPIService => $"{head}{nameof(TranslatorDeerAPIPage)}",
            ServiceType.TransmartBuiltInService => $"{head}{nameof(TranslatorTransmartBuiltInPage)}",
            //TODO: 新接口需要适配
            _ => $"{head}{nameof(TranslatorSTranslatePage)}"
        };

        NavigationPage(name, service);
    }

    [Obsolete("使用专门的弹窗进行服务添加")]
    [RelayCommand]
    private void Add(List<object> list)
    {
        if (list?.Count == 2)
        {
            var service = list.First();

            CurTransServiceList.Add(service switch
            {
                TranslatorSTranslate stranslate => stranslate.Clone(),
                TranslatorGoogleBuiltin api => api.Clone(),
                TranslatorBaidu baidu => baidu.Clone(),
                TranslatorMicrosoft bing => bing.Clone(),
                TranslatorOpenAI openAi => openAi.Clone(),
                TranslatorGemini gemini => gemini.Clone(),
                TranslatorTencent tencent => tencent.Clone(),
                TranslatorAli ali => ali.Clone(),
                TranslatorYoudao youdao => youdao.Clone(),
                TranslatorNiutrans niutrans => niutrans.Clone(),
                TranslatorCaiyun caiyun => caiyun.Clone(),
                TranslatorVolcengine volcengine => volcengine.Clone(),
                TranslatorEcdict ecdict => ecdict.Clone(),
                TranslatorChatglm chatglm => chatglm.Clone(),
                TranslatorOllama ollama => ollama.Clone(),
                TranslatorBaiduBce baiduBce => baiduBce.Clone(),
                TranslatorDeepL deepl => deepl.Clone(),
                TranslatorAzureOpenAI azureopenai => azureopenai.Clone(),
                TranslatorClaude claude => claude.Clone(),
                TranslatorDeepSeek deepSeek => deepSeek.Clone(),
                TranslatorKingSoftDict kingsoftdict => kingsoftdict.Clone(),
                TranslatorBingDict bingdict => bingdict.Clone(),
                TranslatorDeepLX deeplx => deeplx.Clone(),
                TranslatorYandexBuiltIn yandex => yandex.Clone(),
                TranslatorMicrosoftBuiltin microsoftBuiltin => microsoftBuiltin.Clone(),
                TranslatorDeerAPI deerapi => deerapi.Clone(),
                TranslatorTransmartBuiltIn transmartbuiltin => transmartbuiltin.Clone(),
                //TODO: 新接口需要适配
                _ => throw new InvalidOperationException($"Unsupported service type: {service.GetType().Name}")
            });

            (list.Last() as ToggleButton)!.IsChecked = false;

            ResetView(ActionType.Add);
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete(ITranslator service)
    {
        CurTransServiceList.Remove(service);

        ResetView(ActionType.Delete);
    }

    [RelayCommand]
    private void Save()
    {
        foreach (var item in CurTransServiceList)
        {
            if (item is not ITranslatorLLM llm) continue;
            if (llm.Models.Contains(llm.Model)) continue;
            llm.Models.Add(llm.Model);
        }

        if (!Singleton<ConfigHelper>.Instance.WriteConfig([.. CurTransServiceList]))
        {
            LogService.Logger.Warn($"保存服务失败，{JsonConvert.SerializeObject(CurTransServiceList)}");

            ToastHelper.Show(AppLanguageManager.GetString("Toast.SaveFailed"), WindowType.Preference);
        }

        ToastHelper.Show(AppLanguageManager.GetString("Toast.SaveSuccess"), WindowType.Preference);
    }

    [RelayCommand]
    private void Reset()
    {
        var list = Singleton<ConfigHelper>.Instance.CurrentConfig?.Services ?? [];

        // 不直接替换对象
        CurTransServiceList.Clear();
        foreach (var item in list)
        {
            CurTransServiceList.Add(item);
        }
        ResetView();
        ToastHelper.Show(AppLanguageManager.GetString("Toast.ResetConf"), WindowType.Preference);

        if (MessageBox_S.Show(AppLanguageManager.GetString("MessageBox.ContinueReset"), AppLanguageManager.GetString("MessageBox.Tip"), MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        {
            Singleton<ReplaceViewModel>.Instance.ResetCommand.Execute(null);
        }
    }

    /// <summary>
    ///     导航页面
    /// </summary>
    /// <param name="name"></param>
    /// <param name="translator"></param>
    public void NavigationPage(string name, ITranslator translator)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("param name is null or empty", nameof(name));

            ArgumentNullException.ThrowIfNull(translator);

            var type = Type.GetType(name) ?? throw new Exception($"{nameof(NavigationPage)} get {name} exception");

            //读取缓存是否存在，存在则从缓存中获取View实例并通过UpdateVM刷新ViewModel
            UIElement? content = null;
            if (ContentCache.ContainsKey(type))
            {
                content = ContentCache[type];
                if (content is UserControl uc)
                {
                    var method = type.GetMethod("UpdateVM");
                    method?.Invoke(uc, new[] { translator });
                }
            }
            else //不存在则创建并通过构造函数传递ViewModel
            {
                content = (UIElement?)Activator.CreateInstance(type, translator);
                ContentCache.Add(type, content);
            }

            ServiceContent = content;
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("服务导航出错", ex);
        }
    }

    [RelayCommand]
    private void OpenSelectPage(BindingList<ITranslator> translators)
    {
        var dialog = new TranslatorSelectDialog(translators);
        if (dialog.ShowDialog() != true)
            return;

        CurTransServiceList.Add(dialog.SelectedTranslator!);

        ResetView(ActionType.Add);
    }

    [RelayCommand]
    private void DuplicateSvc(ITranslator svc)
    {
        var duplicateSvc = svc.Clone();
        duplicateSvc.Identify = Guid.NewGuid();
        duplicateSvc.Name += "_副本";
        
        var index = CurTransServiceList.IndexOf(svc);
        CurTransServiceList.Insert(index + 1, duplicateSvc);
        
        ResetView(ActionType.Next);
    }

    [RelayCommand]
    private void Sort()
    {
        // 根据启用状态排序
        var sortedList = CurTransServiceList.Where(svc => svc.IsEnabled).Reverse();
        foreach (var svc in sortedList)
        {
            CurTransServiceList.Remove(svc);
            CurTransServiceList.Insert(0, svc);
        }

        ResetView();
    }
}