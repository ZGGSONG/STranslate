﻿using System.Net.Http;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorGoogleBuiltin : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorGoogleBuiltin() : this(Guid.NewGuid(), "https://googlet.deno.dev/translate", "Google")
    {
    }

    public TranslatorGoogleBuiltin(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Google,
        string appId = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.GoogleBuiltinService
    )
    {
        Identify = guid;
        Url = url;
        Name = name;
        Icon = icon;
        AppID = appId;
        AppKey = appKey;
        IsEnabled = isEnabled;
        Type = type;
    }

    #endregion Constructor

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
            var ret = await TranslateAsync(reqModel, token);

            result = ret.IsSuccess ? AppLanguageManager.GetString("Toast.VerifySuccess") : AppLanguageManager.GetString("Toast.VerifyFailed");
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
            if (!isCancel) ToastHelper.Show(result, WindowType.Preference);
        }
    }

    #endregion Translator Test

    #region Interface Implementation

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken canceltoken)
    {
        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");

        var convSource = LangConverter(req.SourceLang) ?? throw new Exception($"该服务不支持{req.SourceLang.GetDescription()}");
        var convTarget = LangConverter(req.TargetLang) ?? throw new Exception($"该服务不支持{req.TargetLang.GetDescription()}");

        var reqStr = JsonConvert.SerializeObject(new
        {
            text = req.Text,
            source_lang = convSource,
            target_lang = convTarget
        });

        try
        {
            var resp = await HttpUtil.PostAsync(Url, reqStr, canceltoken).ConfigureAwait(false) ?? throw new Exception("请求结果为空");
            var data = JsonConvert.DeserializeObject<JObject>(resp)?["data"]?.ToString() ?? throw new Exception(resp);
            return TranslationResult.Success(data);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == null)
        {
            var msg = $"请检查服务是否可以正常访问: {Name} ({Url}).";
            throw new HttpRequestException(msg);
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException is Exception innEx)
            {
                var innMsg = JsonConvert.DeserializeObject<JObject>(innEx.Message);
                msg += $" {innMsg?["message"]}";
                LogService.Logger.Error($"({Name})({Identify}) raw content:\n{innEx.Message}");
            }

            msg = msg.Trim();

            throw new Exception(msg);
        }
    }

    public ITranslator Clone()
    {
        return new TranslatorGoogleBuiltin
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
            IsExecuting = IsExecuting,
            IsTranslateBackExecuting = IsTranslateBackExecuting,
            AutoExecuteTranslateBack = AutoExecuteTranslateBack,
        };
    }

    /// <summary>
    ///     https://blog.julym.com/original/74.html
    /// </summary>
    /// <param name="lang"></param>
    /// <returns></returns>
    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "auto",
            LangEnum.zh_cn => "zh-CN",
            LangEnum.zh_tw => "zh-TW",
            LangEnum.yue => "yue",
            LangEnum.en => "en",
            LangEnum.ja => "ja",
            LangEnum.ko => "ko",
            LangEnum.fr => "fr",
            LangEnum.es => "es",
            LangEnum.ru => "ru",
            LangEnum.de => "de",
            LangEnum.it => "it",
            LangEnum.tr => "tr",
            LangEnum.pt_pt => "pt",
            LangEnum.pt_br => "pt",
            LangEnum.vi => "vi",
            LangEnum.id => "id",
            LangEnum.th => "th",
            LangEnum.ms => "ms",
            LangEnum.ar => "ar",
            LangEnum.hi => "hi",
            LangEnum.mn_cy => "mn",
            LangEnum.mn_mo => "mn",
            LangEnum.km => "km",
            LangEnum.nb_no => "no",
            LangEnum.nn_no => "no",
            LangEnum.fa => "fa",
            LangEnum.sv => "sv",
            LangEnum.pl => "pl",
            LangEnum.nl => "nl",
            LangEnum.uk => "uk",
            _ => "auto"
        };
    }

    #endregion Interface Implementation
}