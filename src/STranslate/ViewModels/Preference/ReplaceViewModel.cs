using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference;

public partial class ReplaceViewModel : ObservableObject
{
    private readonly ConfigHelper _configHelper = Singleton<ConfigHelper>.Instance;
    private readonly TranslatorViewModel _translateVm = Singleton<TranslatorViewModel>.Instance;
    public InputViewModel InputVm => Singleton<InputViewModel>.Instance;
    public ReplaceViewModel()
    {
        // View 上绑定结果从List中获取
        ReplaceProp.ActiveService = AllServices.FirstOrDefault(x => x.Identify == ReplaceProp.ActiveService?.Identify);

        _translateVm.PropertyChanged += (sender, args) =>
        {
            // 检查是否被删除
            if (sender is not BindingList<ITranslator> services || ReplaceProp.ActiveService is null) return;

            // 当服务列表中有增删时检查当前活动服务是否被删除
            if (services.All(x => x.Identify != ReplaceProp.ActiveService?.Identify))
                ReplaceProp.ActiveService = null;
        };
    }

    public async Task ExecuteAsync(string content, CancellationToken token)
    {
        if (ReplaceProp.ActiveService is null)
        {
            Singleton<NotifyIconViewModel>.Instance.ShowBalloonTip("请先选择替换翻译服务后重试");
            return;
        }

        try
        {
            const string translating = "翻译中...";
            var transLength = translating.Length;
            InputSimulatorHelper.PrintText(translating);

            // Determine target language
            var targetLang = ReplaceProp.TargetLang;
            if (targetLang == LangEnum.auto) targetLang = await DetectLanguageAsync(content, token);

            LogService.Logger.Debug(
                $"<Begin> Replace Translator\tcontent: [{content.Replace("\r", @"\r").Replace("\n", @"\n").Replace("\t", @"\t")}]\ttarget: [{targetLang.GetDescription()}]");

            // Perform translation
            var req = new RequestModel(content, LangEnum.auto, targetLang);


            if (ReplaceProp.ActiveService is ITranslatorLlm)
                await TranslateLlmAsync(req, transLength, token);
            else
                await TranslateRegularAsync(req, transLength, token);

            await SuccessAsync(token);
        }
        catch (Exception ex)
        {
            LogService.Logger.Warn("Replace Translator Error: " + ex.Message);
            await FailAsync(token);
            // 还原原始内容
            InputSimulatorHelper.PrintText(content);
        }
        finally
        {
            LogService.Logger.Debug("<End> Replace Translator");
        }
    }

    private async Task<LangEnum> DetectLanguageAsync(string content, CancellationToken token)
    {
        var identify =
            await LangDetectHelper.DetectAsync(content, ReplaceProp.DetectType, ReplaceProp.AutoScale, token);
        return identify is LangEnum.zh_cn or LangEnum.zh_tw ? LangEnum.en : LangEnum.zh_cn;
    }

    private async Task TranslateRegularAsync(RequestModel req, int length, CancellationToken token)
    {
        TranslationResult ret;
        try
        {
            ret = await ReplaceProp.ActiveService!.TranslateAsync(req, CancellationToken.None);
        }
        catch (Exception)
        {
            InputSimulatorHelper.Backspace(length);
            throw;
        }

        InputSimulatorHelper.Backspace(length);

        if (!ret.IsSuccess) throw new Exception(ret.Result?.ToString());
        InputSimulatorHelper.PrintText(ret.Result);
    }

    private async Task TranslateLlmAsync(RequestModel req, int length, CancellationToken token)
    {
        var isStart = false;
        var count = 0;
        try
        {
            await ReplaceProp.ActiveService!.TranslateAsync(req,
                msg =>
                {
                    // 如果开始移除等待标记
                    if (!isStart)
                        InputSimulatorHelper.Backspace(length);

                    isStart = true;
                    count += msg.Length; // 计算已输出长度
                    InputSimulatorHelper.PrintText(msg);
                }, token);
        }
        catch (Exception)
        {
            // 出错判断是否已经开始 未开始则移除等待标记
            if (!isStart)
                InputSimulatorHelper.Backspace(length);

            // 出错则移除已输出内容
            InputSimulatorHelper.Backspace(count);
            throw;
        }
    }

    private async Task SuccessAsync(CancellationToken token)
    {
        const string successMark = "√";
        InputSimulatorHelper.PrintText(successMark);
        await Task.Delay(300, token).ConfigureAwait(false);
        InputSimulatorHelper.Backspace(successMark.Length);
    }

    private async Task FailAsync(CancellationToken cancellationToken)
    {
        const string errorMsg = "翻译出错...";
        InputSimulatorHelper.PrintText(errorMsg);
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        InputSimulatorHelper.Backspace(errorMsg.Length);
    }

    #region Property

    [ObservableProperty] private BindingList<ITranslator> _allServices = Singleton<TranslatorViewModel>.Instance.CurTransServiceList.Clone();

    [ObservableProperty] private ReplaceProp _replaceProp = Singleton<ConfigHelper>.Instance.CurrentConfig?.ReplaceProp ?? new ReplaceProp();

    #endregion

    #region Command

    [RelayCommand]
    private void Save()
    {
        if (_configHelper.WriteConfig(this))
        {
            ToastHelper.Show("保存替换翻译成功", WindowType.Preference);
        }
        else
        {
            LogService.Logger.Debug($"保存替换翻译失败，{JsonConvert.SerializeObject(this)}");
            ToastHelper.Show("保存替换翻译失败", WindowType.Preference);
        }
    }

    [RelayCommand]
    private void Reset()
    {
        AllServices.Clear();

        foreach (var service in _translateVm.CurTransServiceList) AllServices.Add(service);

        ReplaceProp = _configHelper.CurrentConfig?.ReplaceProp ?? new ReplaceProp();

        // View 上绑定结果从List中获取
        ReplaceProp.ActiveService = AllServices.FirstOrDefault(x => x.Identify == ReplaceProp.ActiveService?.Identify);
        
        ToastHelper.Show("重置配置", WindowType.Preference);
    }

    #endregion
}