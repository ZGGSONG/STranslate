using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Style.Commons;
using STranslate.Util;
using STranslate.ViewModels.Preference;
using STranslate.ViewModels.Preference.Translator;

namespace STranslate.ViewModels;

public partial class InputViewModel : ObservableObject
{
    #region 文本转语音

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task TTS(string text, CancellationToken token)
    {
        await Singleton<TTSViewModel>.Instance.SpeakTextAsync(text, WindowType.Main, token);
    }

    #endregion

    #region 属性、字段

    public NotifyIconViewModel NotifyIconVM => Singleton<NotifyIconViewModel>.Instance;
    public CommonViewModel CommonVm => Singleton<CommonViewModel>.Instance;
    public OutputViewModel OutputVm => Singleton<OutputViewModel>.Instance;
    public ConfigHelper CnfHelper => Singleton<ConfigHelper>.Instance;
    public VocabularyBookViewModel VocabularyBookVm => Singleton<VocabularyBookViewModel>.Instance;

    /// <summary>
    ///     是否正在执行自动翻译
    /// </summary>
    [ObservableProperty] private bool _isAutoTranslateExecuting;

    /// <summary>
    ///     自动识别的语言
    /// </summary>
    [ObservableProperty] private string _identifyLanguage = string.Empty;

    /// <summary>
    ///     常用语言
    /// </summary>
    [ObservableProperty] private string _oftenUsedLang = string.Empty;

    /// <summary>
    ///     手动指定识别语种
    /// </summary>
    private LangEnum? _userSelectedLang;

    public LangEnum GetSourceLang { get; set; }
    public LangEnum GetTargetLang { get; set; }

    /// <summary>
    ///     判断是否完成翻译，避免自动翻译重复执行
    /// </summary>
    private bool _hasDown;

    /// <summary>
    ///     输入内容
    /// </summary>
    private string _inputContent = string.Empty;

    public string InputContent
    {
        get => _inputContent;
        set
        {
            var previousValue = _inputContent;
            if (!SetProperty(ref _inputContent, value))
                return;

            _hasDown = false;
            //清空识别语种
            if (!string.IsNullOrEmpty(IdentifyLanguage))
                IdentifyLanguage = string.Empty;

            CanTranslate = !string.IsNullOrWhiteSpace(value);

            //如果增加的为非空格字符则进行自动翻译
            if (previousValue.Trim() != value.Trim()) _autoTranslateTimer?.Change(AutoTranslateDelay, Timeout.Infinite);

            GetSourceLang = LangEnum.auto;
            GetTargetLang = LangEnum.auto;
        }
    }

    private bool _canTranslate = true;

    private bool CanTranslate
    {
        get => _canTranslate;
        set
        {
            _canTranslate = value;
            TranslateCommand.NotifyCanExecuteChanged();
        }
    }

    [ObservableProperty] private string _placeholder = Constant.PlaceHolderContent;

    [ObservableProperty] private bool _mainOcrLangVisibile;


    private const int AutoTranslateDelay = 1000; // 设置延迟时间

    private Timer? _autoTranslateTimer; // 延时搜索定时器

    #endregion 属性、字段

    #region 自动翻译

    internal void InvokeTimer(bool value)
    {
        _autoTranslateTimer =
            value ? new Timer(AutoTranslate, null, Timeout.Infinite, Timeout.Infinite) : null;
    }

    private async void AutoTranslate(object? _)
    {
        if (string.IsNullOrEmpty(InputContent) || !CanTranslate || _hasDown) return;
        IsAutoTranslateExecuting = true;
        //如果重复执行先取消上一步操作
        OutputVm.SingleTranslateCancelCommand.Execute(null);
        OutputVm.SingleTranslateBackCancelCommand.Execute(null);
        TranslateCancelCommand.Execute(null);
        Save2VocabularyBookCancelCommand.Execute(null);
        await TranslateCommand.ExecuteAsync(null);
        IsAutoTranslateExecuting = false;
    }

    #endregion

    #region 文本翻译

    [RelayCommand(CanExecute = nameof(CanTranslate), IncludeCancelCommand = true)]
    private async Task TranslateAsync(object? obj, CancellationToken token)
    {
        CanTranslate = false;

        try
        {
            //翻译前清空旧数据
            OutputVm.Clear();
            var sourceLang = Singleton<MainViewModel>.Instance.SourceLang;
            var targetLang = Singleton<MainViewModel>.Instance.TargetLang;
            var size = CnfHelper.CurrentConfig?.HistorySize ?? 100;

            if (!PreTranslate())
                return;

            GetSourceLang = sourceLang;
            GetTargetLang = targetLang;
            var history = await DoTranslateAsync(obj, size, token);

            // 正常进行则记录历史记录，如果出现异常(eg. 取消任务)则不记录
            await PostTranslateAsync(history, sourceLang, targetLang, size);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("[TranslateAsync]", ex);
        }
        finally
        {
            _hasDown = true;
            CanTranslate = true;
        }
    }

    #region 翻译前操作

    private bool PreTranslate()
    {
        if (!string.IsNullOrWhiteSpace(InputContent))
            return true;

        Parallel.ForEach(Singleton<TranslatorViewModel>.Instance.CurTransServiceList,
            (service) =>
            {
                service.Data.IsSuccess = false;
                service.Data.Result = "请输入有效内容";
            });
        return false;
    }

    #endregion

    #region 翻译操作

    #region 回译操作

    public async Task DoTranslateBackSingleAsync(ITranslator service, LangEnum source, LangEnum target,
        CancellationToken cancellationToken)
    {
        try
        {
            service.Data.TranslateBackResult = string.Empty;
            service.IsTranslateBackExecuting = true;

            //替换源和目标语言
            (source, target) = (target, source);

            if (service is ITranslatorLlm)
                await TranslateBackStreamHandlerAsync(service, source, target, cancellationToken);
            else
                await TranslateBackNonStreamHandlerAsync(service, source, target, cancellationToken);
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            service.Data.IsTranslateBackSuccess = false;
            service.Data.TranslateBackResult = ex.Message;
        }
        finally
        {
            if (service.IsTranslateBackExecuting) service.IsTranslateBackExecuting = false;
        }
    }

    public async Task TranslateBackNonStreamHandlerAsync(ITranslator service, LangEnum source, LangEnum target,
        CancellationToken token)
    {
        var data = await service.TranslateAsync(new RequestModel(service.Data.Result, source, target), token);
        service.Data.IsTranslateBackSuccess = true;
        service.Data.TranslateBackResult = data.Result;
    }

    public async Task TranslateBackStreamHandlerAsync(ITranslator service, LangEnum source, LangEnum target,
        CancellationToken token)
    {
        //先清空
        service.Data.IsTranslateBackSuccess = true;
        service.Data.TranslateBackResult = string.Empty;

        await service.TranslateAsync(
            new RequestModel(service.Data.Result, source, target),
            msg =>
            {
                //开始有数据就停止加载动画
                if (service.IsTranslateBackExecuting)
                    service.IsTranslateBackExecuting = false;
                service.Data.IsTranslateBackSuccess = true;
                service.Data.TranslateBackResult += msg;
            },
            token
        );
    }

    #endregion

    /// <summary>
    ///     单个服务翻译
    /// </summary>
    /// <param name="services">null: 不检查缓存</param>
    /// <param name="service"></param>
    /// <param name="servicesCache"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="copyIndex">0: 不进行翻译后复制操作</param>
    /// <returns></returns>
    public async Task<bool> DoTranslateSingleAsync(List<ITranslator>? services, ITranslator service,
        List<ITranslator>? servicesCache, LangEnum source, LangEnum target, CancellationToken cancellationToken,
        int copyIndex)
    {
        var hasCache = false;
        try
        {
            TranslationResult.CopyFrom(TranslationResult.Reset, service.Data);
            service.IsExecuting = true;

            if (GetCache(service, servicesCache))
            {
                hasCache = true;
                goto copy;
            }

            // 避免从缓存获取结果失败后翻译触发时语种全为自动
            if (GetSourceLang == LangEnum.auto && GetTargetLang == LangEnum.auto)
                (GetSourceLang, GetTargetLang) = await GetLangInfoAsync(null, null, GetSourceLang, GetTargetLang, cancellationToken);
            
            if (service is ITranslatorLlm)
                await StreamHandlerAsync(service, InputContent, source, target, cancellationToken);
            else
                await NonStreamHandlerAsync(service, InputContent, source, target, cancellationToken);

            copy:
            var currentServiceIndex = services?.IndexOf(service) + 1;
            if (currentServiceIndex == copyIndex)
                CommonUtil.InvokeOnUIThread(() =>
                    OutputVm.HotkeyCopyCommand.Execute(copyIndex.ToString()));
        }
        catch (TaskCanceledException ex)
        {
            HandleTranslationException(service, "请求取消", ex, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            HandleTranslationException(service, "请求出错", ex, cancellationToken);
        }
        catch (Exception ex)
        {
            HandleTranslationException(service, "翻译出错", ex, cancellationToken);
        }
        finally
        {
            if (service.IsExecuting) service.IsExecuting = false;
        }

        return hasCache;
    }

    private async Task<HistoryModel?> DoTranslateAsync(object? obj, long size, CancellationToken token)
    {
        HistoryModel? history = null;
        List<ITranslator>? servicesCache = null;
        // 过滤非启用的翻译服务
        var services = Singleton<TranslatorViewModel>.Instance.CurTransServiceList.Where(x => x.IsEnabled).ToList();
        // 读取配置翻译后复制服务索引
        var copyIndex = CnfHelper.CurrentConfig?.CopyResultAfterTranslateIndex ?? 0;

        // 读取缓存
        var isCheckCacheFirst = obj == null;
        if (size != 0 && isCheckCacheFirst)
        {
            history = await SqlHelper.GetDataAsync(InputContent, GetSourceLang.GetDescription(), GetTargetLang.GetDescription());
            if (history != null)
            {
                IdentifyLanguage = "缓存";
                var settings = new JsonSerializerSettings { Converters = { new CurrentTranslatorConverter() } };
                servicesCache = JsonConvert.DeserializeObject<List<ITranslator>>(history.Data, settings);
            }
        }

        // 获取识别语种
        (GetSourceLang, GetTargetLang) = await GetLangInfoAsync(services, servicesCache, GetSourceLang, GetTargetLang, token);

        
        var allCache = true;
        await Parallel.ForEachAsync(
            services.Where(x => x.AutoExecute),
            token,
            async (service, cancellationToken) =>
            {
                allCache &= await DoTranslateSingleAsync(services, service, servicesCache, GetSourceLang, GetTargetLang,
                    cancellationToken, copyIndex);

                // 开启自动回译并且未获取到缓存在执行回译
                if (!service.AutoExecuteTranslateBack || !string.IsNullOrEmpty(service.Data.TranslateBackResult)) return;

                // 在识别全部为缓存的情况下均为自动识别，此时回译则需要重新获取语种了
                if (GetSourceLang == LangEnum.auto && GetTargetLang == LangEnum.auto)
                    (GetSourceLang, GetTargetLang) = await GetLangInfoAsync(null, null, GetSourceLang, GetTargetLang, token);

                await DoTranslateBackSingleAsync(service, GetSourceLang, GetTargetLang, cancellationToken);
            }
        );

        // 只要有一个服务未获取到缓存则更新缓存
        if (!allCache) history = null;

        return history;
    }

    /// <summary>
    ///     获取语种信息
    /// </summary>
    /// <param name="services">null: 不检查缓存</param>
    /// <param name="servicesCache"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="token"></param>
    /// <returns>
    ///     * Item1: source
    ///     * Item2: target
    /// </returns>
    public async Task<(LangEnum, LangEnum)> GetLangInfoAsync(List<ITranslator>? services, List<ITranslator>? servicesCache, LangEnum source, LangEnum target, CancellationToken token)
    {
        var identify = LangEnum.auto;
        //手动指定识别语种优先级最高
        if (_userSelectedLang != null)
        {
            identify = (LangEnum)_userSelectedLang;
            IdentifyLanguage = identify.GetDescription();
        }
        else if (source == LangEnum.auto)
        {
            // 如果当前启用服务为null(逐个启动)或者与缓存服务一致则无需进行语种识别
            var servicesMatch = services != null
                                && servicesCache != null
                                && services.All(s => servicesCache.Any(c => c.Identify == s.Identify));
            if (servicesMatch)
            {
                return (source, target);
            }

            var detectType = CnfHelper.CurrentConfig?.DetectType ?? LangDetectType.Local;
            var rate = CnfHelper.CurrentConfig?.AutoScale ?? 0.8;

            // 显示识别内容为...
            IdentifyLanguage = "...";

            identify = await LangDetectHelper.DetectAsync(InputContent, detectType, rate, token);

            //如果identify也是自动（只有服务识别服务出错的情况下才是auto）
            identify = identify == LangEnum.auto
                ? CnfHelper.CurrentConfig?.SourceLangIfAuto ?? LangEnum.en
                : identify;

            // 获取最终的识别语种
            IdentifyLanguage = identify.GetDescription();
            source = identify;
        }

        if (target == LangEnum.auto)
            target = identify is LangEnum.zh_cn or LangEnum.zh_tw or LangEnum.yue
                ? CnfHelper.CurrentConfig?.TargetLangIfSourceZh ?? LangEnum.en
                : CnfHelper.CurrentConfig?.TargetLangIfSourceNotZh ?? LangEnum.zh_cn;

        return (source, target);
    }

    private bool GetCache(ITranslator? service, List<ITranslator>? servicesCache)
    {
        //传入当前启用服务列表为空时不获取缓存
        if (service == null)
            return false;

        var cachedTranslator = servicesCache?.FirstOrDefault(x => x.Identify == service.Identify);
        if (cachedTranslator?.Data is null || cachedTranslator.Data.IsSuccess == false)
            return false;

        TranslationResult.CopyFrom(cachedTranslator.Data, service.Data);

        // 如果未开启清理输出的回译结果
        if (!service.AutoExecuteTranslateBack)
            service.Data.TranslateBackResult = string.Empty;

        return true;
    }

    private void HandleTranslationException(ITranslator service, string errorMessage, Exception exception,
        CancellationToken token)
    {
        var isCancelMsg = false;
        switch (exception)
        {
            case TaskCanceledException:
                errorMessage = token.IsCancellationRequested ? "请求取消" : "请求超时(请检查网络环境是否正常或服务是否可用)\n";
                isCancelMsg = token.IsCancellationRequested;
                break;
            case HttpRequestException:
                if (exception.InnerException != null) exception = exception.InnerException;
                errorMessage = "请求出错";
                break;
        }

        service.Data.IsSuccess = false;
        service.Data.Result = $"{errorMessage}: {exception.Message}";
        service.Data.Exception = exception;

        if (isCancelMsg)
            LogService.Logger.Debug(
                $"[{service.Name}({service.Identify})] {errorMessage}, 请求API: {service.Url}, 异常信息: {exception.Message}");
        else
            LogService.Logger.Error(
                $"[{service.Name}({service.Identify})] {errorMessage}, 请求API: {service.Url}, 异常信息: {exception.Message}");
    }

    /// <summary>
    ///     非流数据处理
    /// </summary>
    /// <param name="service"></param>
    /// <param name="content"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task NonStreamHandlerAsync(ITranslator service, string content, LangEnum source, LangEnum target,
        CancellationToken token)
    {
        var data = await service.TranslateAsync(new RequestModel(content, source, target), token);
        TranslationResult.CopyFrom(data, service.Data);
    }

    /// <summary>
    ///     流式数据处理
    /// </summary>
    /// <param name="service"></param>
    /// <param name="content"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task StreamHandlerAsync(ITranslator service, string content, LangEnum source, LangEnum target,
        CancellationToken token)
    {
        //先清空
        TranslationResult.CopyFrom(TranslationResult.Reset, service.Data);

        await service.TranslateAsync(
            new RequestModel(content, source, target),
            msg =>
            {
                //开始有数据就停止加载动画
                if (service.IsExecuting)
                    service.IsExecuting = false;
                service.Data.IsSuccess = true;
                service.Data.Result += msg;
            },
            token
        );
    }

    #endregion

    #region 翻译后操作

    private async Task PostTranslateAsync(HistoryModel? history, LangEnum source, LangEnum dbTarget,
        long size)
    {
        if (history is null && size > 0)
        {
            var enableServices = Singleton<TranslatorViewModel>.Instance.CurTransServiceList.Where(x => x.IsEnabled);
            var jsonSerializerSettings = new JsonSerializerSettings
                { ContractResolver = new CustomizeContractResolver() };

            var data = new HistoryModel
            {
                Time = DateTime.Now,
                SourceLang = source.GetDescription(),
                TargetLang = dbTarget.GetDescription(),
                SourceText = InputContent,
                Data = JsonConvert.SerializeObject(enableServices, jsonSerializerSettings)
            };
            //翻译结果插入数据库
            await SqlHelper.InsertDataAsync(data, size);
        }
    }

    #endregion

    #endregion

    #region 其他操作

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task Save2VocabularyBookAsync(string content, CancellationToken token)
    {
        var ret = await VocabularyBookVm.ExecuteAsync(content, token);
        ToastHelper.Show($"保存至{VocabularyBookVm.ActiveVocabularyBook?.Name ?? "生词本"}{(ret ? "成功" : "失败")}");
    }

    [RelayCommand]
    private void CopyContent(string content)
    {
        if (string.IsNullOrEmpty(content)) return;
        ClipboardHelper.Copy(content);
        ToastHelper.Show("复制成功");
    }

    [RelayCommand]
    private void RemoveSpace(PlaceholderTextBox textBox)
    {
        var oldTxt = textBox.Text;
        var newTxt = StringUtil.RemoveSpace(oldTxt);
        if (string.Equals(oldTxt, newTxt))
            return;

        ToastHelper.Show("移除空格");

        RemoveHandler(textBox, newTxt);
    }

    [RelayCommand]
    private void RemoveLineBreaks(PlaceholderTextBox textBox)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            TogglePurify();
        else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            ToggleRemoveLineBreak();
        else
            RemoveLineBreaksFromTextBox(textBox);
    }


    [RelayCommand]
    private async Task SelectedLanguageAsync(List<object> list)
    {
        if (list.Count != 2 || list.First() is not EnumerationExtension.EnumerationMember member ||
            list.Last() is not ToggleButton tb)
            return;

        tb.IsChecked = false;

        if (!Enum.TryParse(typeof(LangEnum), member.Value?.ToString() ?? "", out var obj) ||
            obj is not LangEnum lang) return;

        _userSelectedLang = lang;

        // 选择语言后自动翻译
        OutputVm.SingleTranslateCancelCommand.Execute(null);
        OutputVm.SingleTranslateBackCancelCommand.Execute(null);
        TranslateCancelCommand.Execute(null);
        await TranslateCommand.ExecuteAsync(string.Empty);//填充参数=>不读缓存

        _userSelectedLang = null;
    }

    [RelayCommand]
    private async Task SelectedLangDetectTypeAsync(List<object> list)
    {
        if (list.Count != 2 || list.First() is not EnumerationExtension.EnumerationMember member ||
            list.Last() is not ToggleButton tb)
            return;

        tb.IsChecked = false;

        if (!Enum.TryParse(typeof(LangDetectType), member.Value?.ToString() ?? "", out var obj) ||
            obj is not LangDetectType detectType) return;

        CommonVm.SaveCommand.Execute(null);

        // 选择语言后自动翻译
        OutputVm.SingleTranslateCancelCommand.Execute(null);
        OutputVm.SingleTranslateBackCancelCommand.Execute(null);
        TranslateCancelCommand.Execute(null);
        await TranslateCommand.ExecuteAsync(string.Empty);
    }


    internal void RemoveHandler(PlaceholderTextBox textBox, string newTxt)
    {
        //https://stackoverflow.com/questions/4476282/how-can-i-undo-a-textboxs-text-changes-caused-by-a-binding
        textBox.SelectAll();
        textBox.SelectedText = newTxt;

        if (!CnfHelper.CurrentConfig?.IsAdjustContentTranslate ?? false)
            return;

        OutputVm.SingleTranslateCancelCommand.Execute(null);
        OutputVm.SingleTranslateBackCancelCommand.Execute(null);
        TranslateCancelCommand.Execute(null);
        Save2VocabularyBookCancelCommand.Execute(null);
        TranslateCommand.Execute(null);
    }

    private void TogglePurify()
    {
        CommonVm.IsPurify = !CommonVm.IsPurify;
        CommonVm.SaveCommand.Execute(null);
        ToastHelper.Show($"{(CommonVm.IsPurify ? "打开" : "关闭")}OCR净化结果");
    }

    private void ToggleRemoveLineBreak()
    {
        CommonVm.IsRemoveLineBreakGettingWords = !CommonVm.IsRemoveLineBreakGettingWords;
        CommonVm.SaveCommand.Execute(null);
        ToastHelper.Show($"{(CommonVm.IsRemoveLineBreakGettingWords ? "打开" : "关闭")}始终移除换行");
    }

    private void RemoveLineBreaksFromTextBox(PlaceholderTextBox textBox)
    {
        var oldTxt = textBox.Text;
        var newTxt = StringUtil.RemoveLineBreaks(oldTxt);
        if (string.Equals(oldTxt, newTxt))
            return;

        ToastHelper.Show("移除换行");
        RemoveHandler(textBox, newTxt);
    }


    internal void UpdateOftenUsedLang()
    {
        OftenUsedLang = CnfHelper.CurrentConfig?.OftenUsedLang ?? string.Empty;
    }

    public void Clear()
    {
        InputContent = string.Empty;
    }

    #endregion

    #region ContextMenu

    [RelayCommand]
    private void TbSelectAll(object obj)
    {
        if (obj is TextBox tb) tb.SelectAll();
    }

    [RelayCommand]
    private void TbCopy(object obj)
    {
        if (obj is not TextBox tb) return;
        var text = tb.SelectedText;
        if (!string.IsNullOrEmpty(text))
            ClipboardHelper.Copy(text);
    }

    [RelayCommand]
    private void TbPaste(object obj)
    {
        if (obj is not TextBox tb) return;
        var getText = Clipboard.GetText();

        //剪贴板内容为空或者为非字符串则不处理
        if (string.IsNullOrEmpty(getText))
            return;
        var index = tb.SelectionStart;
        //处理选中字符串
        var selectLength = tb.SelectionLength;
        //删除选中文本再粘贴
        var preHandleStr = tb.Text.Remove(index, selectLength);

        var newText = preHandleStr.Insert(index, getText);
        tb.Text = newText;

        // 重新定位光标索引
        tb.SelectionStart = index + getText.Length;

        // 聚焦光标
        tb.Focus();
    }

    [RelayCommand]
    private void TbClear(object obj)
    {
        if (obj is TextBox tb) tb.Clear();
    }

    #endregion ContextMenu
}

#region JsonConvert

/// <summary>
///     自定义属性构造器
///     1、可以通过构造方法，传入bool动态控制，主要用于外面有统一封装的时候
///     2、可以通过构造方法，传入需要显示的属性名称，然后基于list做linq过滤
/// </summary>
public class CustomizeContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);

        return properties
            .Select(property =>
            {
                // 设置不忽略 Data 属性
                if (property is { Ignored: true, PropertyName: "Data" }) property.Ignored = false;

                switch (property.PropertyName)
                {
                    // 忽略 AppID 和 AppKey 属性
                    case "AppID"
                        or "AppKey":
                        property.Ignored = true;
                        break;
                    // 特殊处理 TranslationResult 类型的 Data 属性
                    case "Data" when property.PropertyType == typeof(TranslationResult):
                        property.ShouldSerialize = instance =>
                        {
                            var data = property.ValueProvider?.GetValue(instance) as TranslationResult;

                            // 过滤翻译失败数据
                            return data?.IsSuccess ?? false;
                        };
                        break;
                }

                return property;
            })
            .ToList();
    }
}

/// <summary>
///     获取当前翻译服务的缓存结果
/// </summary>
public class CurrentTranslatorConverter : JsonConverter<ITranslator>
{
    public override ITranslator ReadJson(JsonReader reader, Type objectType, ITranslator? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        // 从 JSON 数据中加载一个 JObject
        var jsonObject = JObject.Load(reader);

        // 获取当前可用的翻译服务列表
        var translators = Singleton<TranslatorViewModel>.Instance.CurTransServiceList.Clone();

        // 从 JSON 中提取 Identify 字段的值，用于确定具体实现类
        var identify = jsonObject["Identify"]!.Value<string>();
        var type = jsonObject["Type"]!.Value<int>();

        var translator =
            // 根据 Identify 查找匹配的翻译服务
            translators.FirstOrDefault(x => x.Identify.ToString() == identify)
            ?? type switch
            {
                (int)ServiceType.STranslateService => new TranslatorSTranslate(),
                (int)ServiceType.BaiduService => new TranslatorBaidu(),
                (int)ServiceType.MicrosoftService => new TranslatorMicrosoft(),
                (int)ServiceType.OpenAIService => new TranslatorOpenAI(),
                (int)ServiceType.GeminiService => new TranslatorGemini(),
                (int)ServiceType.TencentService => new TranslatorTencent(),
                (int)ServiceType.AliService => new TranslatorAli(),
                (int)ServiceType.YoudaoService => new TranslatorYoudao(),
                (int)ServiceType.NiutransService => new TranslatorNiutrans(),
                (int)ServiceType.CaiyunService => new TranslatorCaiyun(),
                (int)ServiceType.VolcengineService => new TranslatorVolcengine(),
                (int)ServiceType.EcdictService => new TranslatorEcdict(),
                (int)ServiceType.ChatglmService => new TranslatorChatglm(),
                (int)ServiceType.OllamaService => new TranslatorOllama(),
                (int)ServiceType.BaiduBceService => new TranslatorBaiduBce(),
                (int)ServiceType.DeepLService => new TranslatorDeepL(),
                (int)ServiceType.AzureOpenAIService => new TranslatorAzureOpenAI(),
                (int)ServiceType.ClaudeService => new TranslatorClaude(),
                (int)ServiceType.DeepSeekService => new TranslatorDeepSeek(),
                (int)ServiceType.KingSoftDictService => new TranslatorKingSoftDict(),
                (int)ServiceType.BingDictService => new TranslatorBingDict(),
                //TODO: 新接口需要适配
                _ => new TranslatorApi()
            };

        // 从 JSON 中提取 Data 字段的值，设置到 translator 的 Data 属性中
        try
        {
            var dataToken = jsonObject["Data"];
            var data = dataToken?.ToObject<TranslationResult>();
            // 如果结果为空则设置为失败，移除提示信息，当前直接访问服务获取新结果
            if (data is {IsSuccess: true})
            {
                TranslationResult.CopyFrom(data, translator.Data);
            }
            else
            {
                translator.Data.IsSuccess = false;
                translator.Data.Result = string.Empty;
            }
        }
        catch (Exception)
        {
            //兼容旧版结果
            var data = jsonObject["Data"]?.Value<string>();
            translator.Data.Result = data ?? Constant.InputErrorContent;
        }

        // 返回构建好的 translator 对象
        return translator;
    }

    public override void WriteJson(JsonWriter writer, ITranslator? value, JsonSerializer serializer)
    {
        // WriteJson 方法在此处未实现，因为当前转换器主要用于反序列化
        throw new NotImplementedException();
    }
}

#endregion JsonConvert