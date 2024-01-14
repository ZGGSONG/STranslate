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

namespace STranslate.ViewModels.Preference.History
{
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

            outputContents = outputs?.Select(x => new Tuple<string, IconType, string>(x.Name, x.Icon, x.Data?.ToString() ?? ""))?.ToList();
        }

        [RelayCommand]
        private void Delete()
        {
            Singleton<HistoryViewModel>.Instance.DeleteHistoryCommand.Execute(null);
        }

        [RelayCommand]
        private void CopyResult(object obj)
        {
            if (obj is string str && !string.IsNullOrEmpty(str))
            {
                Clipboard.SetDataObject(str);
            }
        }

        [ObservableProperty]
        private string inputContent = "";

        [ObservableProperty]
        private DateTime time;

        [ObservableProperty]
        private string sourceLang = "";

        [ObservableProperty]
        private string targetLang = "";

        [ObservableProperty]
        private List<Tuple<string, IconType, string>>? outputContents;
    }

    public class HistoryTranslatorConverter : JsonConverter<ITranslator>
    {
        public override ITranslator ReadJson(
            JsonReader reader,
            Type objectType,
            ITranslator? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            // 从 JSON 数据中加载一个 JObject
            JObject jsonObject = JObject.Load(reader);

            // 根据Type字段的值来决定具体实现类
            var type = jsonObject["Type"]!.Value<int>();
            ITranslator translator;

            translator = type switch
            {
                (int)ServiceType.ApiService => new TranslatorApi(),
                (int)ServiceType.BaiduService => new TranslatorBaidu(),
                (int)ServiceType.BingService => new TranslatorBing(),
                //TODO: 新接口需要适配
                _ => throw new NotSupportedException($"Unsupported ServiceType: {type}")
            };

            serializer.Populate(jsonObject.CreateReader(), translator);

            // 从 JSON 中提取 Data 字段的值，设置到 translator 的 Data 属性中
            translator.Data = jsonObject["Data"]!.Value<string>()!;

            // 返回构建好的 translator 对象
            return translator;
        }

        public override void WriteJson(JsonWriter writer, ITranslator? value, JsonSerializer serializer)
        {
            // WriteJson 方法在此处未实现，因为当前转换器主要用于反序列化
            throw new NotImplementedException();
        }
    }
}
