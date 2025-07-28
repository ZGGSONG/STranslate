using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using System.ComponentModel;
using System.Net.Http;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorQwenMt : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorQwenMt()
        : this(Guid.NewGuid(), "https://dashscope.aliyuncs.com", "Qwen-MT")
    {
    }

    public TranslatorQwenMt(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Bailian,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.QwenMtService
    )
    {
        Identify = guid;
        Url = url;
        Name = name;
        Icon = icon;
        AppID = appID;
        AppKey = appKey;
        IsEnabled = isEnabled;
        Type = type;
    }

    #endregion Constructor

    #region Properties

    [JsonIgnore] private string _model = "qwen-mt-turbo";
    public string Model
    {
        get => _model;
        set => SetProperty(ref _model, value);
    }

    [JsonIgnore]
    private BindingList<string> _models =
    [
        "qwen-mt-turbo",
        "qwen-mt-plus"
    ];
    public BindingList<string> Models
    {
        get => _models;
        set => SetProperty(ref _models, value);
    }

    #endregion

    #region Translator Test

    [property: JsonIgnore]
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task TestAsync(CancellationToken token)
    {
        var result = "";
        var isCancel = false;
        try
        {
            IsTesting = true;
            var reqModel = new RequestModel("你好", LangEnum.zh_cn, LangEnum.en);
            await TranslateAsync(reqModel, _ => result = AppLanguageManager.GetString("Toast.VerifySuccess"), token);
        }
        catch (OperationCanceledException)
        {
            isCancel = true;
        }
        catch (Exception)
        {
            result = AppLanguageManager.GetString("Toast.VerifyFailed");
        }
        finally
        {
            IsTesting = false;
            if (!isCancel)
                ToastHelper.Show(result, WindowType.Preference);
        }
    }

    #endregion Translator Test

    #region Interface Implementation

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
    {
        if (string.IsNullOrEmpty(Url) /* || string.IsNullOrEmpty(AppKey)*/)
            throw new Exception("请先完善配置");

        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");

        //检查语种
        var source = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var target = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

        UriBuilder uriBuilder = new(Url);

        // 如果路径不是有效的API路径结尾，使用默认路径
        if (uriBuilder.Path == "/")
            uriBuilder.Path = "/compatible-mode/v1/chat/completions";

        // 选择模型
        var a_model = Model.Trim();
        a_model = string.IsNullOrEmpty(a_model) ? "qwen-mt-turbo" : a_model;

        var a_messages = new[]
        {
            new
            {
                role = "user",
                content = req.Text,
            }
        };

        // 构建请求数据
        var reqData = new
        {
            model = a_model,
            messages = a_messages,
            translation_options = new
            {
                source_lang = source,
                target_lang = target,
            },
        };

        var header = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {AppKey}" }
        };

        var jsonData = JsonConvert.SerializeObject(reqData);

        try
        {
            var resp = await HttpUtil.PostAsync(uriBuilder.Uri.ToString(), jsonData, null, header, token).ConfigureAwait(false);
            var data = JsonConvert.DeserializeObject<JObject>(resp)?["choices"]?.FirstOrDefault()?["message"]?["content"]?.ToString() ?? throw new Exception(resp);
            return TranslationResult.Success(data);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == null)
        {
            var msg = $"请检查服务是否可以正常访问: {Name} ({Url}).\n{ex.Message}";
            throw new HttpRequestException(msg);
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException is { } innEx)
            {
                var innMsg = JsonConvert.DeserializeObject<JObject>(innEx.Message);
                msg += $" {innMsg?["error"]?["message"]}";
                LogService.Logger.Error($"({Name})({Identify}) raw content:\n{innEx.Message}");
            }

            msg = msg.Trim();

            throw new Exception(msg);
        }
    }

    public ITranslator Clone()
    {
        return new TranslatorQwenMt
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            Data = TranslationResult.Reset,
            AppID = AppID,
            AppKey = AppKey,
            AutoExecute = AutoExecute,
            KeyHide = KeyHide,
            Model = Model,
            Models = Models,
            IsExecuting = IsExecuting,
            IsTranslateBackExecuting = IsTranslateBackExecuting,
            AutoExecuteTranslateBack = AutoExecuteTranslateBack,
        };
    }

    /// <summary>
    ///     https://help.aliyun.com/zh/model-studio/machine-translation#14735a54e0rwb
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto", // 自动检测
            LangEnum.zh_cn => "Chinese", // 简体中文
            LangEnum.zh_tw => "Traditional Chinese", // 繁体中文
            LangEnum.yue => "Cantonese", // 粤语
            LangEnum.ja => "Japanese", // 日语
            LangEnum.en => "English", // 英语
            LangEnum.ko => "Korean", // 韩语
            LangEnum.fr => "French", // 法语
            LangEnum.es => "Spanish", // 西班牙语
            LangEnum.ru => "Russian", // 俄语
            LangEnum.de => "German", // 德语
            LangEnum.it => "Italian", // 意大利语
            LangEnum.tr => "Turkish", // 土耳其语
            LangEnum.pt_pt => "Portuguese", // 葡萄牙语（葡萄牙）
            LangEnum.pt_br => "Portuguese", // 葡萄牙语（巴西）
            LangEnum.vi => "Vietnamese", // 越南语
            LangEnum.id => "Indonesian", // 印度尼西亚语
            LangEnum.th => "Thai", // 泰语
            LangEnum.ms => "Malay", // 马来语
            LangEnum.ar => "Arabic", // 阿拉伯语
            LangEnum.hi => "Hindi", // 印地语
            LangEnum.mn_cy => null, // 不支持（蒙古语-西里尔）
            LangEnum.mn_mo => null, // 不支持（蒙古语-蒙文）
            LangEnum.km => "Khmer", // 高棉语
            LangEnum.nb_no => "Norwegian Bokmål", // 书面挪威语
            LangEnum.nn_no => "Norwegian Nynorsk", // 新挪威语
            LangEnum.fa => "Western Persian", // 西波斯语
            LangEnum.sv => "Swedish", // 瑞典语
            LangEnum.pl => "Polish", // 波兰语
            LangEnum.nl => "Dutch", // 荷兰语
            LangEnum.uk => "Ukrainian", // 乌克兰语
            _ => null
        };
    }

    #endregion Interface Implementation
}