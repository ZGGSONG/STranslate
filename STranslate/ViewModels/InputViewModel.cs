using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference;
using STranslate.ViewModels.Preference.Services;

namespace STranslate.ViewModels
{
    public partial class InputViewModel : ObservableObject
    {
        #region 属性、字段

        /// <summary>
        /// 自动识别的语言
        /// </summary>
        [ObservableProperty]
        private string identifyLanguage = string.Empty;

        private static Dictionary<string, LanguageEnum> LangDict
        {
            get => CommonUtil.GetEnumList<LanguageEnum>();
        }

        /// <summary>
        /// 输入内容
        /// </summary>
        private string inputContent = string.Empty;

        public string InputContent
        {
            get => inputContent;
            set
            {
                if (inputContent != value)
                {
                    OnPropertyChanging(nameof(InputContent));
                    inputContent = value;
                    OnPropertyChanged(nameof(InputContent));

                    //清空识别语种
                    if (!string.IsNullOrEmpty(IdentifyLanguage))
                        IdentifyLanguage = string.Empty;

                    TranslateCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool CanTranslate => !string.IsNullOrEmpty(InputContent);

        #endregion 属性、字段

        #region 命令

        #region Translatehandle

        [RelayCommand(CanExecute = nameof(CanTranslate), IncludeCancelCommand = true)]
        private async Task TranslateAsync(object obj, CancellationToken token)
        {
            //翻译前清空旧数据
            Singleton<OutputViewModel>.Instance.Clear();
            string source = Singleton<MainViewModel>.Instance.SelectedSourceLanguage ?? LanguageEnum.AUTO.GetDescription();
            string target = Singleton<MainViewModel>.Instance.SelectedTargetLanguage ?? LanguageEnum.AUTO.GetDescription();
            long size = Singleton<ConfigHelper>.Instance.CurrentConfig?.HistorySize ?? 100;
            string dbTarget = target;
            HistoryModel? history = null;

            try
            {
                history = await TranslateServiceAsync(obj, source, dbTarget, target, size, token);
            }
            catch (Exception ex)
            {
                LogService.Logger.Error("[TranslateAsync]", ex);
            }
            finally
            {
                await HandleHistoryAsync(obj, history, source, dbTarget, size);
            }
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
        private async Task HandleHistoryAsync(object obj, HistoryModel? history, string source, string dbTarget, long size)
        {
            if (history is null && size > 0)
            {
                var enableServices = Singleton<ServiceViewModel>.Instance.CurTransServiceList.Where(x => x.IsEnabled);
                var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CustomizeContractResolver() };

                var data = new HistoryModel
                {
                    Time = DateTime.Now,
                    SourceLang = source,
                    TargetLang = dbTarget,
                    SourceText = InputContent,
                    Data = JsonConvert.SerializeObject(enableServices, Formatting.None, jsonSerializerSettings)
                };
                var isForceWrite = obj != null;
                //翻译结果插入数据库
                await SqlHelper.InsertDataAsync(data, size, isForceWrite);
            }
        }

        private async Task<HistoryModel?> TranslateServiceAsync(object obj, string source, string dbTarget, string target, long size, CancellationToken token)
        {
            var services = Singleton<ServiceViewModel>.Instance.CurTransServiceList;
            HistoryModel? history = null;
            List<ITranslator>? translatorList = null;

            //如果数据库限制大小为0则跳过检查数据库
            if (size == 0)
                goto excute;

            //是否强制翻译
            bool isCheckCacheFirst = obj == null;
            if (isCheckCacheFirst)
            {
                history = await SqlHelper.GetDataAsync(InputContent, source, dbTarget);

                if (history != null)
                {
                    var settings = new JsonSerializerSettings { Converters = { new CurrentTranslatorConverter() } };

                    translatorList = JsonConvert.DeserializeObject<List<ITranslator>>(history.Data, settings);
                }
            }

            excute:
            await Parallel.ForEachAsync(
                services,
                token,
                async (service, token) =>
                {
                    ///检查是否启用
                    if (!service.IsEnabled)
                        return;

                    try
                    {
                        if (translatorList != null)
                        {
                            IdentifyLanguage = "缓存";
                            service.Data = translatorList?.FirstOrDefault(x => x.Identify == service.Identify)?.Data ?? "该服务未获取到缓存Ctrl+Enter更新";
                            return;
                        }

                        //如果是自动则获取自动识别后的目标语种
                        if (target == LanguageEnum.AUTO.GetDescription())
                        {
                            var autoRet = StringUtil.AutomaticLanguageRecognition(InputContent);
                            IdentifyLanguage = autoRet.Item1;
                            target = autoRet.Item2;
                        }

                        var sourceStr = LangDict[source].ToString();
                        var targetStr = LangDict[target].ToString();

                        //根据不同服务类型区分
                        //TODO: 新接口需要适配
                        service.Data = service.Type switch
                        {
                            ServiceType.ApiService => await ServiceHandler.ApiHandler(service, InputContent, sourceStr, targetStr, token),
                            ServiceType.BaiduService => await ServiceHandler.BaiduHandler(service, InputContent, sourceStr, targetStr, token),
                            ServiceType.BingService => await ServiceHandler.BingHandler(service, InputContent, sourceStr, targetStr, token),
                            _ => throw new NotImplementedException()
                        };
                    }
                    catch (TaskCanceledException ex)
                    {
                        HandleTranslationException(service, "请求取消", ex, token);
                    }
                    catch (HttpRequestException ex)
                    {
                        HandleTranslationException(service, "请求出错", ex, token);
                    }
                    catch (Exception ex)
                    {
                        HandleTranslationException(service, "翻译出错", ex, token);
                    }
                }
            );

            return history;
        }

        private void HandleTranslationException(ITranslator service, string errorMessage, Exception exception, CancellationToken token)
        {
            bool isDebug = false;
            if (exception is TaskCanceledException)
            {
                errorMessage = token.IsCancellationRequested ? "请求取消..." : "请求超时...";
                isDebug = token.IsCancellationRequested;
            }
            else if (exception is HttpRequestException)
            {
                errorMessage = "请求出错...";
            }

            service.Data = errorMessage;

            if (isDebug)
                LogService.Logger.Debug($"[{service.Name}({service.Identify})] {errorMessage}, 请求API: {service.Url}, 异常信息: {exception?.Message}");
            else
                LogService.Logger.Error($"[{service.Name}({service.Identify})] {errorMessage}, 请求API: {service.Url}, 异常信息: {exception?.Message}");
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
                Clipboard.SetDataObject(content);
                ToastHelper.Show("复制成功");
            }
        }

        [RelayCommand]
        private void RemoveLineBreaks()
        {
            var tmp = InputContent;
            InputContent = StringUtil.RemoveLineBreaks(InputContent);
            if (string.Equals(tmp, InputContent))
                return;

            ToastHelper.Show("移除换行");
            if (!Singleton<ConfigHelper>.Instance.CurrentConfig?.IsAdjustContentTranslate ?? false)
                return;

            TranslateCancelCommand.Execute(null);
            TranslateCommand.Execute(null);
        }

        [RelayCommand]
        private void RemoveSpace()
        {
            var tmp = InputContent;
            InputContent = StringUtil.RemoveSpace(InputContent);
            if (string.Equals(tmp, InputContent))
                return;

            ToastHelper.Show("移除空格");

            if (!Singleton<ConfigHelper>.Instance.CurrentConfig?.IsAdjustContentTranslate ?? false)
                return;

            TranslateCancelCommand.Execute(null);
            TranslateCommand.Execute(null);
        }

        #region ContextMenu

        [RelayCommand]
        private void TBSelectAll(object obj)
        {
            if (obj is TextBox tb)
            {
                tb.SelectAll();
            }
        }

        [RelayCommand]
        private void TBCopy(object obj)
        {
            if (obj is TextBox tb)
            {
                var text = tb.SelectedText;
                if (!string.IsNullOrEmpty(text))
                    Clipboard.SetDataObject(text);
            }
        }

        [RelayCommand]
        private void TBPaste(object obj)
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
        private void TBClear(object obj)
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
            var list = base.CreateProperties(type, memberSerialization);

            list!
                .ToList()
                .ForEach(x =>
                {
                    if (x.Ignored && x.PropertyName == "Data")
                        x.Ignored = false; //不忽略
                });
            return list!;
        }
    }

    /// <summary>
    /// 获取当前翻译服务的缓存结果
    /// </summary>
    public class CurrentTranslatorConverter : JsonConverter<ITranslator>
    {
        public override ITranslator? ReadJson(JsonReader reader, Type objectType, ITranslator? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // 从 JSON 数据中加载一个 JObject
            JObject jsonObject = JObject.Load(reader);

            // 获取当前可用的翻译服务列表
            var translators = Singleton<ServiceViewModel>.Instance.CurTransServiceList;

            // 从 JSON 中提取 Identify 字段的值，用于确定具体实现类
            var identify = jsonObject["Identify"]!.Value<string>();
            var type = jsonObject["Type"]!.Value<int>();
            ITranslator translator;

            // 根据 Identify 查找匹配的翻译服务
            //TODO: 新接口需要适配
            translator =
                translators.FirstOrDefault(x => x.Identify.ToString() == identify)
                ?? type switch
                {
                    (int)ServiceType.BaiduService => new TranslatorBaidu(),
                    (int)ServiceType.BingService => new TranslatorBing(),
                    _ => new TranslatorApi(),
                };

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

    #endregion JsonConvert
}
