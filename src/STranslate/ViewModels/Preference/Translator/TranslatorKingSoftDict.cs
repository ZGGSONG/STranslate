﻿using System.Net.Http;
using System.Text;
using System.Web;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorKingSoftDict : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorKingSoftDict()
        : this(Guid.NewGuid(), "http://dict-co.iciba.com/api/dictionary.php", "金山词霸")
    {
    }

    public TranslatorKingSoftDict(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Iciba,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        ServiceType type = ServiceType.KingSoftDictService
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
            if (!isCancel)
                ToastHelper.Show(result, WindowType.Preference);
        }
    }

    #endregion Translator Test

    #region Interface Implementation

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
    {
        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");

        //英文似乎只接收小写
        var content = req.Text.ToLower();

        //金山词霸可查中文无需检查是否为英文单词
        //var isWord = StringUtil.IsWord(content);
        //if (!isWord)
        //    goto Empty;

        var uriBuilder = new UriBuilder(Url);
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["type"] = "json";
        query["w"] = content;
        query["key"] = "54A9DE969E911BC5294B70DA8ED5C9C4";
        uriBuilder.Query = query.ToString();

        var resp = "";
        JObject parseData;
        try
        {
            resp = await HttpUtil.GetAsync(uriBuilder.Uri.AbsoluteUri, token).ConfigureAwait(false);
            parseData = JsonConvert.DeserializeObject<JObject>(resp) ?? throw new Exception($"反序列化失败: {resp}");
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
        catch
        {
            LogService.Logger.Error($"金山词霸服务失效,访问结果: {resp}");
            return TranslationResult.Success("");
        }

        var wordName = parseData["word_name"]?.ToString();
        if (string.IsNullOrEmpty(wordName)) goto Empty;

        StringBuilder sb = new();
        sb.AppendLine(wordName);

        var isChinese = StringUtil.IsChinese(content);
        if (isChinese)
        {
            var symbol = parseData["symbols"]?.FirstOrDefault()?["word_symbol"]?.ToString();
            if (!string.IsNullOrEmpty(symbol))
                sb.AppendLine($"\r\nzh · [{symbol}]");
        }
        else
        {
            var symbolEn = parseData["symbols"]?.FirstOrDefault()?["ph_en"]?.ToString();
            if (!string.IsNullOrEmpty(symbolEn))
                sb.Append($"\r\nuk · [{symbolEn}]\n");

            var symbolAm = parseData["symbols"]?.FirstOrDefault()?["ph_am"]?.ToString();
            if (!string.IsNullOrEmpty(symbolAm))
                sb.AppendLine($"us · [{symbolAm}]");
        }

        var means = parseData["symbols"]?.FirstOrDefault()?["parts"];
        if (means == null)
            return TranslationResult.Success(sb.ToString());

        foreach (var meanItem in means)
        {
            var meansContent = meanItem["means"];
            if (meansContent == null) continue;

            var meanList = meansContent.Select(item => isChinese ? item["word_mean"]?.ToString() : item?.ToString())
                .Where(wordMean => !string.IsNullOrEmpty(wordMean)).ToList();
            var meanType = meanItem[isChinese ? "part_name" : "part"]?.ToString();
            if (!string.IsNullOrEmpty(meanType))
                sb.Append($"\n[{meanType}] ");
            sb.Append(string.Join("、", meanList));
        }

        var wordPls = parseData["exchange"]?["word_pl"];
        if (wordPls != null && wordPls.Count() != 0)
        {
            sb.Append($"\r\n\r\n复数形式 {string.Join("、", wordPls)}");
        }

        return TranslationResult.Success(sb.ToString());

        Empty:
        return TranslationResult.Success("");
    }

    public ITranslator Clone()
    {
        return new TranslatorKingSoftDict
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
            IdHide = IdHide,
            KeyHide = KeyHide,
            IsExecuting = IsExecuting,
            IsTranslateBackExecuting = IsTranslateBackExecuting,
            AutoExecuteTranslateBack = AutoExecuteTranslateBack
        };
    }

    public string? LangConverter(LangEnum lang)
    {
        return null;
    }

    #endregion Interface Implementation
}