using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace STranslate.ViewModels.Preference
{
    public partial class ServiceViewModel : ObservableObject
    {
        public ServiceViewModel()
        {
            //添加默认支持服务
            //TODO: 新接口需要适配
            TransServices.Add(new TranslatorSTranslate());
            TransServices.Add(new TranslatorEcdict());
            TransServices.Add(new TranslatorApi());
            TransServices.Add(new TranslatorOpenAI());
            TransServices.Add(new TranslatorGemini());
            TransServices.Add(new TranslatorAli());
            TransServices.Add(new TranslatorBaidu());
            TransServices.Add(new TranslatorTencent());
            TransServices.Add(new TranslatorNiutrans());
            TransServices.Add(new TranslatorMicrosoft());
            TransServices.Add(new TranslatorYoudao());
            TransServices.Add(new TranslatorCaiyun());
            TransServices.Add(new TranslatorVolcengine());
            TransServices.Add(new TranslatorChatglm());
            TransServices.Add(new TranslatorOllama());

            ResetView();
        }

        /// <summary>
        /// 重置选中项
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
                default:
                    {
                        //初始化默认执行选中第一条
                        SelectedIndex = 0;
                        TogglePageCommand.Execute(CurTransServiceList.First());
                        break;
                    }
            }
        }

        private int tmpIndex;

        [RelayCommand]
        private void TogglePage(ITranslator service)
        {
            if (service != null)
            {
                if (SelectedIndex != -1)
                    tmpIndex = SelectedIndex;

                string head = "STranslate.Views.Preference.Service.";
                //TODO: 新接口需要适配
                var name = service.Type switch
                {
                    ServiceType.STranslateService => string.Format("{0}TextSTranslateServicesPage", head),
                    ServiceType.ApiService => string.Format("{0}TextApiServicePage", head),
                    ServiceType.BaiduService => string.Format("{0}TextBaiduServicesPage", head),
                    ServiceType.MicrosoftService => string.Format("{0}TextMicrosoftServicesPage", head),
                    ServiceType.OpenAIService => string.Format("{0}TextOpenAIServicesPage", head),
                    ServiceType.GeminiService => string.Format("{0}TextGeminiServicesPage", head),
                    ServiceType.TencentService => string.Format("{0}TextTencentServicesPage", head),
                    ServiceType.AliService => string.Format("{0}TextAliServicesPage", head),
                    ServiceType.YoudaoService => string.Format("{0}TextYoudaoServicesPage", head),
                    ServiceType.NiutransService => string.Format("{0}TextNiutransServicesPage", head),
                    ServiceType.CaiyunService => string.Format("{0}TextCaiyunServicesPage", head),
                    ServiceType.VolcengineService => string.Format("{0}TextVolcengineServicesPage", head),
                    ServiceType.EcdictService => string.Format("{0}TextEcdictServicesPage", head),
                    ServiceType.ChatglmService => string.Format("{0}TextChatglmServicesPage", head),
                    ServiceType.OllamaService => string.Format("{0}TextOllamaServicesPage", head),
                    _ => string.Format("{0}TextApiServicePage", head)
                };

                NavigationPage(name, service);
            }
        }

        [RelayCommand]
        private void Add(List<object> list)
        {
            if (list?.Count == 2)
            {
                var service = list.First();

                //TODO: 新接口需要适配
                CurTransServiceList.Add(service switch
                {
                    TranslatorSTranslate stranslate => stranslate.Clone(),
                    TranslatorApi api => api.Clone(),
                    TranslatorBaidu baidu => baidu.Clone(),
                    TranslatorMicrosoft bing => bing.Clone(),
                    TranslatorOpenAI openAI => openAI.Clone(),
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
                    _ => throw new InvalidOperationException($"Unsupported service type: {service.GetType().Name}")
                });

                (list.Last() as ToggleButton)!.IsChecked = false;

                ResetView(ActionType.Add);
            }
        }

        private bool CanDelete => ServiceCounter > 1;

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private void Delete(ITranslator service)
        {
            if (service != null)
            {
                CurTransServiceList.Remove(service);

                ResetView(ActionType.Delete);

                ToastHelper.Show("删除成功", WindowType.Preference);
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (!Singleton<ConfigHelper>.Instance.WriteConfig(CurTransServiceList))
            {
                LogService.Logger.Debug($"保存服务失败，{JsonConvert.SerializeObject(CurTransServiceList)}");

                ToastHelper.Show("保存失败", WindowType.Preference);
            }
            ToastHelper.Show("保存成功", WindowType.Preference);
        }

        [RelayCommand]
        private void Reset()
        {
            CurTransServiceList.Clear();
            foreach (var item in Singleton<ConfigHelper>.Instance.ResetConfig.Services ?? [])
            {
                CurTransServiceList.Add(item);
            }
            ResetView(ActionType.Initialize);
            ToastHelper.Show("重置配置", WindowType.Preference);
        }

        /// <summary>
        /// 导航 UI 缓存
        /// </summary>
        private readonly Dictionary<Type, UIElement?> ContentCache = [];

        /// <summary>
        /// 导航页面
        /// </summary>
        /// <param name="name"></param>
        /// <param name="translator"></param>
        public void NavigationPage(string name, ITranslator translator)
        {
            UIElement? content = null;

            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("param name is null or empty", nameof(name));

                ArgumentNullException.ThrowIfNull(translator);

                Type? type = Type.GetType(name) ?? throw new Exception($"{nameof(NavigationPage)} get {name} exception");

                //读取缓存是否存在，存在则从缓存中获取View实例并通过UpdateVM刷新ViewModel
                if (ContentCache.ContainsKey(type))
                {
                    content = ContentCache[type];
                    if (content is UserControl uc)
                    {
                        var method = type.GetMethod("UpdateVM");
                        method?.Invoke(uc, new[] { translator });
                    }
                }
                else//不存在则创建并通过构造函数传递ViewModel
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

        [ObservableProperty]
        private int _selectedIndex = 0;

        [ObservableProperty]
        private UIElement? _serviceContent;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
        private int _serviceCounter;

        /// <summary>
        /// 支持的服务
        /// </summary>
        [ObservableProperty]
        private BindingList<ITranslator> _transServices = [];

        /// <summary>
        /// 当前已添加的服务列表
        /// </summary>
        [ObservableProperty]
        private BindingList<ITranslator> _curTransServiceList = Singleton<ConfigHelper>.Instance.CurrentConfig?.Services ?? [];
    }
}