using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.TTS;
using STranslate.Views.Preference.TTS;

namespace STranslate.ViewModels.Preference;

public partial class TTSViewModel : ObservableObject
{
    /// <summary>
    ///     导航 UI 缓存
    /// </summary>
    private readonly Dictionary<Type, UIElement?> ContentCache = [];

    [ObservableProperty]
    private TTSCollection<ITTS> _curTTSServiceList = Singleton<ConfigHelper>.Instance.CurrentConfig?.TTSList ?? [];

    private bool _isSpeaking;

    [ObservableProperty] private int _selectedIndex;

    [ObservableProperty] private int _ttsCounter;

    [ObservableProperty] private UIElement? _ttsServiceContent;

    [ObservableProperty] private BindingList<ITTS> _ttsServices = [];

    private int tmpIndex;

    public TTSViewModel()
    {
        //添加默认支持TTS
        TtsServices.Add(new TTSOffline());
        TtsServices.Add(new TTSEdge());
        TtsServices.Add(new TTSAzure());
        TtsServices.Add(new TTSLingva());
        //TODO: 新TTS服务需要适配

        ResetView();
    }

    private ITTS? ActivedTTS => CurTTSServiceList.FirstOrDefault(x => x.IsEnabled);

    private CancellationTokenSource? _silentTtsCts;

    public async Task SilentSpeakTextAsync(string content)
    {
        if (_silentTtsCts != null)
        {
            _silentTtsCts.Cancel();
            LogService.Logger.Debug("取消静默TTS");
            return;
        }

        _silentTtsCts ??= new CancellationTokenSource();
        try
        {
            CursorManager.Instance.Execute();

            LogService.Logger.Debug($"<Begin> 静默TTS\tcontent: [{content.Replace("\r", @"\r").Replace("\n", @"\n").Replace("\t", @"\t")}]");

            await SpeakTextAsync(content, WindowType.Main, _silentTtsCts.Token);
        }
        catch (Exception ex)
        {
            Singleton<NotifyIconViewModel>.Instance.ShowBalloonTip("静默TTS失败, 请检查网络或日志");
            LogService.Logger.Warn("静默TTS Error: " + ex.Message);
            CursorManager.Instance.Error();
            await Task.Delay(2000);
        }
        finally
        {
            LogService.Logger.Debug("<End> 静默TTS");
            CursorManager.Instance.Restore();
            _silentTtsCts = null;
        }
    }

    public async Task SpeakTextAsync(string content, WindowType type, CancellationToken token)
    {
        if (ActivedTTS is null)
        {
            ToastHelper.Show("未启用TTS服务", type);
            return;
        }

        if (_isSpeaking)
        {
            ToastHelper.Show("当前语音未结束", type);
            return;
        }

        _isSpeaking = true;
        await ActivedTTS.SpeakTextAsync(content, token);
        _isSpeaking = false;
    }

    /// <summary>
    ///     重置选中项
    /// </summary>
    /// <param name="type"></param>
    private void ResetView(ActionType type = ActionType.Initialize)
    {
        TtsCounter = CurTTSServiceList.Count;

        //当全部删除时则清空view绑定属性
        if (TtsCounter < 1)
        {
            SelectedIndex = 0;
            TtsServiceContent = null;
            return;
        }

        switch (type)
        {
            case ActionType.Delete:
            {
                //不允许小于0
                SelectedIndex = Math.Max(tmpIndex - 1, 0);
                TogglePage(CurTTSServiceList[SelectedIndex]);
                break;
            }
            case ActionType.Add:
            {
                //选中最后一项
                SelectedIndex = TtsCounter - 1;
                TogglePage(CurTTSServiceList[SelectedIndex]);
                break;
            }
            default:
            {
                //初始化默认执行选中第一条
                SelectedIndex = 0;
                TogglePage(CurTTSServiceList.First());
                break;
            }
        }
    }

    [RelayCommand]
    private void TogglePage(ITTS tts)
    {
        if (tts != null)
        {
            if (SelectedIndex != -1)
                tmpIndex = SelectedIndex;

            var head = "STranslate.Views.Preference.TTS.";
            var name = tts.Type switch
            {
                TTSType.OfflineTTS => $"{head}{nameof(TTSOfflinePage)}",
                TTSType.AzureTTS => $"{head}{nameof(TTSAzurePage)}",
                TTSType.LingvaTTS => $"{head}{nameof(TTSLingvaPage)}",
                TTSType.EdgeTTS => $"{head}{nameof(TTSEdgePage)}",
                //TODO: 新TTS服务需要适配
                _ => $"{head}{nameof(TTSOfflinePage)}"
            };

            NavigationPage(name, tts);
        }
    }

    [RelayCommand]
    private void Add(List<object> list)
    {
        if (list?.Count == 2)
        {
            var tts = list.First();

            CurTTSServiceList.Add(tts switch
            {
                TTSAzure azure => azure.Clone(),
                TTSOffline offline => offline.Clone(),
                TTSLingva lingva => lingva.Clone(),
                TTSEdge edge => edge.Clone(),
                //TODO: 新TTS服务需要适配
                _ => throw new InvalidOperationException($"Unsupported tts type: {tts.GetType().Name}")
            });

            (list.Last() as ToggleButton)!.IsChecked = false;

            ResetView(ActionType.Add);
        }
    }

    [RelayCommand]
    private void Delete(ITTS tts)
    {
        if (tts != null)
        {
            CurTTSServiceList.Remove(tts);

            ResetView(ActionType.Delete);

            ToastHelper.Show("删除成功", WindowType.Preference);
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (!Singleton<ConfigHelper>.Instance.WriteConfig(CurTTSServiceList))
        {
            LogService.Logger.Warn($"保存TTS失败，{JsonConvert.SerializeObject(CurTTSServiceList)}");

            ToastHelper.Show("保存失败", WindowType.Preference);
        }

        ToastHelper.Show("保存成功", WindowType.Preference);
    }

    [RelayCommand]
    private void Reset()
    {
        CurTTSServiceList = Singleton<ConfigHelper>.Instance.ResetConfig.TTSList ?? [];
        ResetView();
        ToastHelper.Show("重置配置", WindowType.Preference);
    }

    /// <summary>
    ///     导航页面
    /// </summary>
    /// <param name="name"></param>
    /// <param name="tts"></param>
    public void NavigationPage(string name, ITTS tts)
    {
        UIElement? content = null;

        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("param name is null or empty", nameof(name));

            if (tts == null)
                throw new ArgumentNullException(nameof(tts));

            var type = Type.GetType(name) ?? throw new Exception($"{nameof(NavigationPage)} get {name} exception");

            //读取缓存是否存在，存在则从缓存中获取View实例并通过UpdateVM刷新ViewModel
            if (ContentCache.ContainsKey(type))
            {
                content = ContentCache[type];
                if (content is UserControl uc)
                {
                    var method = type.GetMethod("UpdateVM");
                    method?.Invoke(uc, new[] { tts });
                }
            }
            else //不存在则创建并通过构造函数传递ViewModel
            {
                content = (UIElement?)Activator.CreateInstance(type, tts);
                ContentCache.Add(type, content);
            }

            TtsServiceContent = content;
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("TTS导航出错", ex);
        }
    }
}