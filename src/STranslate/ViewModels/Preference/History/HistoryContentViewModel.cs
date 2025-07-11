﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.Translator;

namespace STranslate.ViewModels.Preference.History;

public partial class HistoryContentViewModel : ObservableObject
{
    [ObservableProperty] private string _inputContent = "";

    [ObservableProperty] private List<Tuple<string, IconType, TranslationResult>>? _outputContents;

    [ObservableProperty] private string _sourceLang = string.Empty;

    [ObservableProperty] private string _targetLang = string.Empty;

    [ObservableProperty] private DateTime _time;

    public HistoryContentViewModel(HistoryModel? history)
    {
        if (history == null) return;

        var settings = new JsonSerializerSettings { Converters = { new HistoryTranslatorConverter() } };

        var outputs = JsonConvert.DeserializeObject<List<ITranslator>>(history.Data, settings);

        InputContent = history.SourceText;
        Time = history.Time;
        SourceLang = history.SourceLang;
        TargetLang = history.TargetLang;

        OutputContents = outputs?.Select(x => new Tuple<string, IconType, TranslationResult>(x.Name, x.Icon, x.Data))
            .ToList();
    }

    [RelayCommand]
    private void Delete()
    {
        Singleton<HistoryViewModel>.Instance.DeleteHistoryCommand.Execute(null);

        ToastHelper.Show(AppLanguageManager.GetString("Toast.DeleteSuccess"), WindowType.Preference);
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task TTS(object obj, CancellationToken token)
    {
        if (obj is string text && !string.IsNullOrEmpty(text))
            await Singleton<TTSViewModel>.Instance.SpeakTextAsync(text, WindowType.Preference, token);
    }

    [RelayCommand]
    private void CopyResult(object obj)
    {
        if (obj is string str && !string.IsNullOrEmpty(str))
        {
            ClipboardHelper.Copy(str);

            ToastHelper.Show(AppLanguageManager.GetString("Toast.CopySuccess"), WindowType.Preference);
        }
    }

    [RelayCommand]
    private void CopySnakeResult(object obj)
    {
        if (obj is not string str || string.IsNullOrEmpty(str)) return;
        var snakeRet = StringUtil.GenSnakeString(str);
        ClipboardHelper.Copy(snakeRet);

        ToastHelper.Show(AppLanguageManager.GetString("Toast.CopySnakeSuccess"), WindowType.Preference);
    }

    [RelayCommand]
    private void CopySmallHumpResult(object obj)
    {
        if (obj is not string str || string.IsNullOrEmpty(str)) return;
        var snakeRet = StringUtil.GenHumpString(str, true);
        ClipboardHelper.Copy(snakeRet);

        ToastHelper.Show(AppLanguageManager.GetString("Toast.CopySmallHumpSuccess"), WindowType.Preference);
    }

    [RelayCommand]
    private void CopyLargeHumpResult(object obj)
    {
        if (obj is not string str || string.IsNullOrEmpty(str)) return;
        var snakeRet = StringUtil.GenHumpString(str);
        ClipboardHelper.Copy(snakeRet);

        ToastHelper.Show(AppLanguageManager.GetString("Toast.CopyLargeHumpSuccess"), WindowType.Preference);
    }
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
            (int)ServiceType.STranslateService => new TranslatorSTranslate(),
            (int)ServiceType.GoogleBuiltinService => new TranslatorGoogleBuiltin(),
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
            (int)ServiceType.DeepLXService => new TranslatorDeepLX(),
            (int)ServiceType.YandexBuiltInService => new TranslatorYandexBuiltIn(),
            (int)ServiceType.MicrosoftBuiltinService => new TranslatorMicrosoftBuiltin(),
            (int)ServiceType.DeerAPIService => new TranslatorDeerAPI(),
            (int)ServiceType.TransmartBuiltInService => new TranslatorTransmartBuiltIn(),
            //TODO: 新接口需要适配
            _ => throw new NotSupportedException($"Unsupported ServiceType: {type}")
        };

        serializer.Populate(jsonObject.CreateReader(), translator);

        try
        {
            // 从 JSON 中提取 Data 字段的值，设置到 translator 的 Data 属性中
            var dataToken = jsonObject["Data"];
            var data = dataToken?.ToObject<TranslationResult>();
            if (data is { IsSuccess: true })
            {
                TranslationResult.CopyFrom(data, translator.Data);
            }
            else
            {
                translator.Data.IsSuccess = false;
                translator.Data.Result = AppLanguageManager.GetString("Constant.HistoryErrorContent");
            }
        }
        catch (Exception)
        {
            //兼容旧版结果
            var data = jsonObject["Data"]?.Value<string>();
            translator.Data.Result = data ?? AppLanguageManager.GetString("Constant.HistoryErrorContent");
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