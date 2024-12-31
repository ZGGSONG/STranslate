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
    private Guid? deletedSvc;
    public ReplaceViewModel()
    {
        // View 上绑定结果从List中获取
        ReplaceProp.ActiveService = AllServices.FirstOrDefault(x => x.Identify == ReplaceProp.ActiveService?.Identify);

        _translateVm.CurTransServiceList.ListChanged += (sender, args) =>
        {
            switch (args.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    var svc = _translateVm.CurTransServiceList[args.NewIndex];
                    if (svc.Identify == deletedSvc)
                    {
                        ReplaceProp.ActiveService = svc;
                        deletedSvc = null;
                    }
                    break;
                case ListChangedType.ItemDeleted:
                    deletedSvc = ReplaceProp.ActiveService?.Identify;
                    break;
            }
        };
    }


    private CancellationTokenSource? _replaceCts;

    public async Task ExecuteAsync(string content)
    {
        if (ReplaceProp.ActiveService is null)
        {
            Singleton<NotifyIconViewModel>.Instance.ShowBalloonTip("请先选择替换翻译服务后重试");
            return;
        }

        if (_replaceCts != null)
        {
            _replaceCts.Cancel();
            LogService.Logger.Debug("取消替换翻译");
            return;
        }

        _replaceCts ??= new CancellationTokenSource();
        var token = _replaceCts.Token;

        try
        {
            CursorManager.Instance.Execute();
            // Determine target language
            var (sourceLang, targetLang) = await DetectLanguageAsync(content, token);

            LogService.Logger.Debug(
                $"<Begin> 替换翻译\tservice: [{ReplaceProp.ActiveService.Type}]\tcontent: [{content.Replace("\r", @"\r").Replace("\n", @"\n").Replace("\t", @"\t")}]\ttarget: [{targetLang.GetDescription()}]");

            // Perform translation
            var req = new RequestModel(content, sourceLang, targetLang);


            if (ReplaceProp.ActiveService is ITranslatorLlm)
                await TranslateLlmAsync(req, token);
            else
                await TranslateRegularAsync(req,token);
        }
        catch (Exception ex)
        {
            Singleton<NotifyIconViewModel>.Instance.ShowBalloonTip("替换翻译失败, 请检查网络或日志");
            LogService.Logger.Warn("替换翻译 Error: " + ex.Message);
            CursorManager.Instance.Error();
            await Task.Delay(2000);
        }
        finally
        {
            LogService.Logger.Debug("<End> 替换翻译");
            CursorManager.Instance.Restore();
            _replaceCts = null;
        }
    }

    private async Task<(LangEnum, LangEnum)> DetectLanguageAsync(string content, CancellationToken token)
    {
        var sourceLang = ReplaceProp.SourceLang;
            
        if (sourceLang == LangEnum.auto)
        {
            sourceLang = await LangDetectHelper.DetectAsync(content, ReplaceProp.DetectType, ReplaceProp.AutoScale, token);
            if (sourceLang == LangEnum.auto)
            {
                sourceLang = ReplaceProp.SourceLangIfAuto;
                LogService.Logger.Error($"ReplaceViewModel|DetectLanguageAsync 识别语种出错: {ReplaceProp.DetectType.GetDescription()}");
            }
        }

        var targetLang = ReplaceProp.TargetLang;
        if (targetLang != LangEnum.auto)
            return (sourceLang, targetLang);

        targetLang = sourceLang is LangEnum.zh_cn or LangEnum.zh_tw or LangEnum.yue
            ? ReplaceProp.TargetLangIfSourceZh
            : ReplaceProp.TargetLangIfSourceNotZh;
        LogService.Logger.Debug($"ReplaceViewModel|DetectLanguageAsync 目标语种 自动 => {targetLang.GetDescription()}");

        return (sourceLang, targetLang);
    }

    private async Task TranslateRegularAsync(RequestModel req, CancellationToken token)
    {
        var ret = await ReplaceProp.ActiveService!.TranslateAsync(req, CancellationToken.None);

        if (!ret.IsSuccess) throw new Exception(ret.Result);


        // 判断是否使用粘贴输出
        if (_configHelper.CurrentConfig?.UsePasteOutput ?? false)
            InputSimulatorHelper.PrintTextWithClipboard(ret.Result);
        else
            InputSimulatorHelper.PrintText(ret.Result);
    }

    private async Task TranslateLlmAsync(RequestModel req, CancellationToken token)
    {
        var result = "";
        try
        {
            await ReplaceProp.ActiveService!.TranslateAsync(req,
                msg =>
                {
                    result += msg;
                    if (_configHelper.CurrentConfig?.UsePasteOutput ?? false)
                        return;

                    InputSimulatorHelper.PrintText(msg);
                }, token);

            // 回调结束后判断是否需要使用剪贴板输出
            if (_configHelper.CurrentConfig?.UsePasteOutput ?? false)
                InputSimulatorHelper.PrintTextWithClipboard(result);
        }
        catch (Exception)
        {
            // 出错则移除已输出内容
            InputSimulatorHelper.Backspace(result.Length);
            throw;
        }
    }

    #region Property

    [ObservableProperty] private BindingList<ITranslator> _allServices = Singleton<TranslatorViewModel>.Instance.CurTransServiceList;

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