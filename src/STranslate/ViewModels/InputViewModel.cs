using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    #region 属性、字段
    public NotifyIconViewModel NotifyIconVM => Singleton<NotifyIconViewModel>.Instance;

    /// <summary>
    /// 自动识别的语言
    /// </summary>
    [ObservableProperty]
    private string _identifyLanguage = string.Empty;

    /// <summary>
    /// 输入内容
    /// </summary>
    private string _inputContent = string.Empty;

    public string InputContent
    {
        get => _inputContent;
        set
        {
            if (_inputContent == value)
                return;
            OnPropertyChanging();
            _inputContent = value;
            OnPropertyChanged();

            //清空识别语种
            if (!string.IsNullOrEmpty(IdentifyLanguage))
                IdentifyLanguage = string.Empty;

            //输入框中有值时才可以执行翻译
            if (value != "")
                TranslateCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanTranslate => !string.IsNullOrEmpty(InputContent);

    [ObservableProperty]
    private string _placeholder = ConstStr.MAINVIEWPLACEHOLDER;

    #endregion 属性、字段

    #region 命令
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task TTS(string text, CancellationToken token)
    {
        await Singleton<TTSViewModel>.Instance.SpeakTextAsync(text, WindowType.Main, token);
    }

    #region Translatehandle

    [RelayCommand(CanExecute = nameof(CanTranslate), IncludeCancelCommand = true)]
    private async Task TranslateAsync(object? obj, CancellationToken token)
    {
        //翻译前清空旧数据
        Singleton<OutputViewModel>.Instance.Clear();
        var sourceLang = Singleton<MainViewModel>.Instance.SourceLang;
        var targetLang = Singleton<MainViewModel>.Instance.TargetLang;
        var size = Singleton<ConfigHelper>.Instance.CurrentConfig?.HistorySize ?? 100;
        var dbTarget = targetLang;

        if (!PreviousHandle())
            return;

        try
        {
            var history = await TranslateServiceAsync(obj, sourceLang, dbTarget, targetLang, size, token);

            // 正常进行则记录历史记录，如果出现异常(eg. 取消任务)则不记录
            await HandleHistoryAsync(obj, history, sourceLang, dbTarget, size);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            LogService.Logger.Error("[TranslateAsync]", ex);
        }
    }

    /// <summary>
    /// 前置处理
    /// </summary>
    /// <returns></returns>
    private bool PreviousHandle()
    {
        if (!string.IsNullOrWhiteSpace(InputContent))
            return true;

        Parallel.ForEach(Singleton<TranslatorViewModel>.Instance.CurTransServiceList, (service, cancellationToken) => service.Data = TranslationResult.Fail("请输入有效内容"));
        return false;
    }

    private async Task<HistoryModel?> TranslateServiceAsync(object? obj, LangEnum source, LangEnum dbTarget, LangEnum target, long size, CancellationToken token)
    {
        var services = Singleton<TranslatorViewModel>.Instance.CurTransServiceList.Where(x => x.IsEnabled).ToList();
        HistoryModel? history = null;
        List<ITranslator>? translatorList = null;
        //读取配置翻译后复制服务索引
        var copyIndex = Singleton<ConfigHelper>.Instance.CurrentConfig?.CopyResultAfterTranslateIndex ?? 0;

        //如果数据库限制大小为0则跳过检查数据库
        if (size == 0)
            goto execute;

        //是否强制翻译
        var isCheckCacheFirst = obj == null;
        if (isCheckCacheFirst)
        {
            history = await SqlHelper.GetDataAsync(InputContent, source.GetDescription(), dbTarget.GetDescription());

            if (history != null)
            {
                var settings = new JsonSerializerSettings { Converters = { new CurrentTranslatorConverter() } };

                translatorList = JsonConvert.DeserializeObject<List<ITranslator>>(history.Data, settings);
            }
        }

    execute:
        await Parallel.ForEachAsync(
            services,
            token,
            async (service, cancellationToken) =>
            {
                try
                {
                    service.IsExecuting = true;

                    if (translatorList != null)
                    {
                        IdentifyLanguage = "缓存";
                        service.Data = translatorList.FirstOrDefault(x => x.Identify == service.Identify)?.Data ?? TranslationResult.Fail("该服务未获取到缓存Ctrl+Enter更新");
                        goto copy;
                    }

                    var identify = LangEnum.auto;

                    // 原始语种自动识别
                    if (source == LangEnum.auto)
                    {
                        var detectType = Singleton<ConfigHelper>.Instance.CurrentConfig?.DetectType ?? LangDetectType.Local;
                        identify = await LangDetectHelper.DetectAsync(InputContent, detectType, cancellationToken);
                        IdentifyLanguage = identify.GetDescription();
                    }

                    //如果是自动则获取自动识别后的目标语种
                    if (target == LangEnum.auto)
                    {
                        //目标语言
                        //1. 识别语种为中文系则目标语言为英文
                        //2. 识别语种为自动或其他语系则目标语言为中文
                        target = (identify == LangEnum.zh_cn || identify == LangEnum.zh_tw || identify == LangEnum.yue) ? LangEnum.en : LangEnum.zh_cn;
                    }

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
                                source = source == LangEnum.auto ? identify : source;
                                await StreamHandlerAsync(service, InputContent, source, target, cancellationToken);
                                break;
                            }

                        default:
                            await NonStreamHandlerAsync(service, InputContent, source, target, cancellationToken);
                            break;
                    }

                copy:
                    //翻译后复制结果
                    var currentServiceIndex = services.IndexOf(service) + 1;
                    if (currentServiceIndex == copyIndex)
                    {
                        CommonUtil.InvokeOnUIThread(() => Singleton<OutputViewModel>.Instance.HotkeyCopyCommand.Execute(copyIndex.ToString()));
                    }
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
                    if (service.IsExecuting)
                    {
                        service.IsExecuting = false;
                    }
                }
            }
        );

        return history;
    }

    private void HandleTranslationException(ITranslator service, string errorMessage, Exception exception, CancellationToken token)
    {
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
        }

        service.Data = TranslationResult.Fail($"{errorMessage}: {exception.Message}", exception);

        if (isCancelMsg)
            LogService.Logger.Debug($"[{service.Name}({service.Identify})] {errorMessage}, 请求API: {service.Url}, 异常信息: {exception.Message}");
        else
            LogService.Logger.Error($"[{service.Name}({service.Identify})] {errorMessage}, 请求API: {service.Url}, 异常信息: {exception.Message}");
    }

    /// <summary>
    /// 插入数据库
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="history"></param>
    /// <param name="source"></param>
    /// <param name="dbTarget"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    private async Task HandleHistoryAsync(object? obj, HistoryModel? history, LangEnum source, LangEnum dbTarget, long size)
    {
        if (history is null && size > 0)
        {
            var enableServices = Singleton<TranslatorViewModel>.Instance.CurTransServiceList.Where(x => x.IsEnabled);
            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CustomizeContractResolver() };

            var data = new HistoryModel
            {
                Time = DateTime.Now,
                SourceLang = source.GetDescription(),
                TargetLang = dbTarget.GetDescription(),
                SourceText = InputContent,
                Data = JsonConvert.SerializeObject(enableServices, jsonSerializerSettings)
            };
            var isForceWrite = obj != null;
            //翻译结果插入数据库
            await SqlHelper.InsertDataAsync(data, size, isForceWrite);
        }
    }

    /// <summary>
    /// 非流数据处理
    /// </summary>
    /// <param name="service"></param>
    /// <param name="content"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task NonStreamHandlerAsync(ITranslator service, string content, LangEnum source, LangEnum target, CancellationToken token) =>
        service.Data = await service.TranslateAsync(new RequestModel(content, source, target), token);

    /// <summary>
    /// 流式数据处理
    /// </summary>
    /// <param name="service"></param>
    /// <param name="content"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task StreamHandlerAsync(ITranslator service, string content, LangEnum source, LangEnum target, CancellationToken token)
    {
        //先清空
        service.Data = TranslationResult.Reset;

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

    #endregion Translatehandle

    public void Clear()
    {
        InputContent = string.Empty;
    }

    [RelayCommand]
    private void CopyContent(string content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            ClipboardHelper.Copy(content);
            ToastHelper.Show("复制成功");
        }
    }

    [RelayCommand]
    private void RemoveLineBreaks(PlaceholderTextBox textBox)
    {
        //根据Ctrl+LeftClick
        if ((Keyboard.Modifiers & ModifierKeys.Control) <= 0)
        {
            var oldTxt = textBox.Text;
            var newTxt = StringUtil.RemoveLineBreaks(oldTxt);
            if (string.Equals(oldTxt, newTxt))
                return;

            ToastHelper.Show("移除换行");

            RemoveHandler(textBox, newTxt);
            return;
        }

        var vm = Singleton<CommonViewModel>.Instance;
        vm.IsRemoveLineBreakGettingWords = !vm.IsRemoveLineBreakGettingWords;
        vm.SaveCommand.Execute(null);
        ToastHelper.Show($"{(vm.IsRemoveLineBreakGettingWords ? "打开" : "关闭")}始终移除换行");
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

    internal void RemoveHandler(PlaceholderTextBox textBox, string newTxt)
    {
        //https://stackoverflow.com/questions/4476282/how-can-i-undo-a-textboxs-text-changes-caused-by-a-binding
        textBox.SelectAll();
        textBox.SelectedText = newTxt;

        if (!Singleton<ConfigHelper>.Instance.CurrentConfig?.IsAdjustContentTranslate ?? false)
            return;

        Singleton<OutputViewModel>.Instance.SingleTranslateCancelCommand.Execute(null);
        TranslateCancelCommand.Execute(null);
        TranslateCommand.Execute(null);
    }

    #region ContextMenu

    [RelayCommand]
    private void TbSelectAll(object obj)
    {
        if (obj is TextBox tb)
        {
            tb.SelectAll();
        }
    }

    [RelayCommand]
    private void TbCopy(object obj)
    {
        if (obj is TextBox tb)
        {
            var text = tb.SelectedText;
            if (!string.IsNullOrEmpty(text))
                ClipboardHelper.Copy(text);
        }
    }

    [RelayCommand]
    private void TbPaste(object obj)
    {
        if (obj is TextBox tb)
        {
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
    }

    [RelayCommand]
    private void TbClear(object obj)
    {
        if (obj is TextBox tb)
        {
            tb.Clear();
        }
    }

    #endregion ContextMenu

    #endregion 命令
}

#region JsonConvert

/// <summary>
/// 自定义属性构造器
/// 1、可以通过构造方法，传入bool动态控制，主要用于外面有统一封装的时候
/// 2、可以通过构造方法，传入需要显示的属性名称，然后基于list做linq过滤
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
                if (property is { Ignored: true, PropertyName: "Data" })
                {
                    property.Ignored = false;
                }

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
/// 获取当前翻译服务的缓存结果
/// </summary>
public class CurrentTranslatorConverter : JsonConverter<ITranslator>
{
    public override ITranslator ReadJson(JsonReader reader, Type objectType, ITranslator? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // 从 JSON 数据中加载一个 JObject
        var jsonObject = JObject.Load(reader);

        // 获取当前可用的翻译服务列表
        var translators = Singleton<TranslatorViewModel>.Instance.CurTransServiceList;

        // 从 JSON 中提取 Identify 字段的值，用于确定具体实现类
        var identify = jsonObject["Identify"]!.Value<string>();
        var type = jsonObject["Type"]!.Value<int>();

        var translator =
            // 根据 Identify 查找匹配的翻译服务
            //TODO: 新接口需要适配
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
                _ => new TranslatorApi()
            };

        // 从 JSON 中提取 Data 字段的值，设置到 translator 的 Data 属性中
        try
        {
            var dataToken = jsonObject["Data"];
            var data = dataToken?.ToObject<TranslationResult>();
            translator.Data = data?.Result is null ? TranslationResult.Fail(ConstStr.INPUTERRORCONTENT) : data;
        }
        catch (Exception)
        {
            //兼容旧版结果
            var data = jsonObject["Data"]?.Value<string>();
            translator.Data.Result = data is null ? ConstStr.INPUTERRORCONTENT : data;
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
