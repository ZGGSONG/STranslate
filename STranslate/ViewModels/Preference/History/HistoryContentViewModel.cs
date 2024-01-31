using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace STranslate.ViewModels.Preference.History;

public partial class HistoryContentViewModel : ObservableObject
{
    public HistoryContentViewModel(HistoryModel? history)
    {
        if (history == null)
        {
            return;
        }

        var settings = new JsonSerializerSettings { Converters = { new HistoryTranslatorConverter() } };

        var outputs = JsonConvert.DeserializeObject<List<ITranslator>>(history.Data, settings);

        InputContent = history.SourceText;
        Time = history.Time;
        SourceLang = history.SourceLang;
        TargetLang = history.TargetLang;

        OutputContents = outputs
            ?.Select(x => new Tuple<string, IconType, TranslationResult>(x.Name, x.Icon, x.Data)).ToList();
    }

    [RelayCommand]
    private void Delete()
    {
        Singleton<HistoryViewModel>.Instance.DeleteHistoryCommand.Execute(null);

        ToastHelper.Show("删除成功", WindowType.Preference);
    }

    [RelayCommand]
    private void CopyResult(object obj)
    {
        if (obj is string str && !string.IsNullOrEmpty(str))
        {
            Clipboard.SetDataObject(str);

            ToastHelper.Show("复制成功", WindowType.Preference);
        }
    }

    [ObservableProperty] private string _inputContent = "";

    [ObservableProperty] private DateTime _time;

    [ObservableProperty] private string _sourceLang = "";

    [ObservableProperty] private string _targetLang = "";

    [ObservableProperty] private List<Tuple<string, IconType, TranslationResult>>? _outputContents;
}

public class HistoryTranslatorConverter : JsonConverter<ITranslator>
{
    public override ITranslator ReadJson(JsonReader reader, Type objectType, ITranslator? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        // 从 JSON 数据中加载一个 JObject
        var jsonObject = JObject.Load(reader);

        // 根据Type字段的值来决定具体实现类
        var type = jsonObject["Type"]!.Value<int>();

        ITranslator translator = type switch
        {
            (int)ServiceType.ApiService => new TranslatorApi(),
            (int)ServiceType.BaiduService => new TranslatorBaidu(),
            (int)ServiceType.MicrosoftService => new TranslatorMicrosoft(),
            (int)ServiceType.OpenAIService => new TranslatorOpenAI(),
            (int)ServiceType.GeminiService => new TranslatorGemini(),
            (int)ServiceType.TencentService => new TranslatorTencent(),
            (int)ServiceType.AliService => new TranslatorAli(),
            (int)ServiceType.YoudaoService => new TranslatorYoudao(),
            (int)ServiceType.NiutransService => new TranslatorNiutrans(),
            (int)ServiceType.CaiyunService => new TranslatorCaiyun(),
            //TODO: 新接口需要适配
            _ => throw new NotSupportedException($"Unsupported ServiceType: {type}")
        };

        serializer.Populate(jsonObject.CreateReader(), translator);

        try
        {
            // 从 JSON 中提取 Data 字段的值，设置到 translator 的 Data 属性中
            var dataToken = jsonObject["Data"];
            var data = dataToken?.ToObject<TranslationResult>();
            translator.Data = string.IsNullOrEmpty(data?.Result)
                ? TranslationResult.Fail(ConstStr.HISTORYERRORCONTENT)
                : data;
        }
        catch (Exception)
        {
            //兼容旧版结果
            var data = jsonObject["Data"]?.Value<string>();
            translator.Data.Result = string.IsNullOrEmpty(data) ? ConstStr.HISTORYERRORCONTENT : data;
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