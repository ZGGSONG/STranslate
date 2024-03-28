using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels;
using STranslate.ViewModels.Preference;
using STranslate.ViewModels.Preference.Services;
using STranslate.ViewModels.Preference.TTS;

namespace STranslate.Helper;

public class ConfigHelper
{
    #region 公共方法

    public ConfigHelper()
    {
        if (!Directory.Exists(ConstStr.AppData)) //判断是否存在
        {
            Directory.CreateDirectory(ConstStr.AppData); //创建新路径
            ShortcutUtil.SetDesktopShortcut(); //创建桌面快捷方式
        }
        if (!File.Exists(ConstStr.CnfName)) //文件不存在
        {
            FileStream fs = new(ConstStr.CnfName, FileMode.Create, FileAccess.ReadWrite);
            fs.Close();
            WriteConfig(InitialConfig());
        }

        //初始化时将初始值赋给Config属性
        CurrentConfig = ResetConfig;
    }

    /// <summary>
    /// 初始化操作
    /// </summary>
    public void InitialOperate()
    {
        //初始化主题
        ThemeOperate(CurrentConfig?.ThemeType ?? ThemeType.Light);

        //初始化字体
        FontOperate();

        //初始化TTS
        TTSOperate();

        //初始化代理设置
        ProxyOperate(CurrentConfig?.ProxyMethod ?? ProxyMethodEnum.系统代理, CurrentConfig?.ProxyIp ?? "", CurrentConfig?.ProxyPort ?? 0, CurrentConfig?.IsProxyAuthentication ?? false, CurrentConfig?.ProxyUsername ?? "", CurrentConfig?.ProxyPassword ?? "");

        //初始化首页图标
        Singleton<MainViewModel>.Instance.UpdateMainViewIcons();
    }

    /// <summary>
    /// 退出时保存位置
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool WriteConfig(double x, double y)
    {
        bool isSuccess = false;
        if (CurrentConfig is not null)
        {
            CurrentConfig.Position = $"{x},{y}";
            WriteConfig(CurrentConfig);
            isSuccess = true;
        }
        return isSuccess;
    }

    /// <summary>
    /// 写入服务到配置
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public bool WriteConfig(BindingList<ITranslator> services)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        CurrentConfig.Services = services;
        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    /// <summary>
    /// 写入TTS服务到配置
    /// </summary>
    /// <param name="tts"></param>
    /// <returns></returns>
    public bool WriteConfig(TTSCollection<ITTS> tts)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        CurrentConfig.TTSList = tts;
        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    /// <summary>
    /// 写入源语言、目标语言到配置
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool WriteConfig(string source, string target)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        CurrentConfig.SourceLanguage = source;
        CurrentConfig.TargetLanguage = target;
        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    /// <summary>
    /// 写入热键到配置
    /// </summary>
    /// <param name="hotkeys"></param>
    /// <returns></returns>
    public bool WriteConfig(Hotkeys hotkeys)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        CurrentConfig.Hotkeys = hotkeys;
        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    /// <summary>
    /// 写入常规配置项到当前配置
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public bool WriteConfig(CommonViewModel model)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        CurrentConfig.IsStartup = model.IsStartup;
        CurrentConfig.NeedAdministrator = model.NeedAdmin;
        CurrentConfig.HistorySize = model.HistorySize;
        CurrentConfig.AutoScale = model.AutoScale;
        CurrentConfig.ThemeType = model.ThemeType;
        CurrentConfig.IsFollowMouse = model.IsFollowMouse;
        CurrentConfig.CloseUIOcrRetTranslate = model.CloseUIOcrRetTranslate;
        CurrentConfig.UnconventionalScreen = model.UnconventionalScreen;
        CurrentConfig.IsOcrAutoCopyText = model.IsOcrAutoCopyText;
        CurrentConfig.IsAdjustContentTranslate = model.IsAdjustContentTranslate;
        CurrentConfig.IsRemoveLineBreakGettingWords = model.IsRemoveLineBreakGettingWords;
        CurrentConfig.DoubleTapTrayFunc = model.DoubleTapTrayFunc;
        CurrentConfig.CustomFont = model.CustomFont;
        CurrentConfig.IsKeepTopmostAfterMousehook = model.IsKeepTopmostAfterMousehook;
        CurrentConfig.IsShowPreference = model.IsShowPreference;
        CurrentConfig.IsShowConfigureService = model.IsShowConfigureService;
        CurrentConfig.IsShowMousehook = model.IsShowMousehook;
        CurrentConfig.IsShowIncrementalTranslation = model.IsShowIncrementalTranslation;
        CurrentConfig.IsShowScreenshot = model.IsShowScreenshot;
        CurrentConfig.IsShowOCR = model.IsShowOCR;
        CurrentConfig.IsShowSilentOCR = model.IsShowSilentOCR;
        CurrentConfig.IsShowClipboardMonitor = model.IsShowClipboardMonitor;
        CurrentConfig.IsShowQRCode = model.IsShowQRCode;
        CurrentConfig.IsShowHistory = model.IsShowHistory;
        CurrentConfig.WordPickingInterval = model.WordPickingInterval;
        CurrentConfig.IsHideOnStart = model.IsHideOnStart;
        CurrentConfig.ShowCopyOnHeader = model.ShowCopyOnHeader;
        CurrentConfig.IsCaretLast = model.IsCaretLast;
        CurrentConfig.MaxHeight = model.MaxHeight;
        CurrentConfig.Width = model.Width;
        CurrentConfig.ProxyMethod = model.ProxyMethod;
        CurrentConfig.ProxyIp = model.ProxyIp;
        CurrentConfig.ProxyPort = model.ProxyPort;
        CurrentConfig.IsProxyAuthentication = model.IsProxyAuthentication;
        CurrentConfig.ProxyUsername = model.ProxyUsername;
        CurrentConfig.ProxyPassword = model.ProxyPassword;
        CurrentConfig.CopyResultAfterTranslateIndex = model.CopyResultAfterTranslateIndex;
        CurrentConfig.IncrementalTranslation = model.IncrementalTranslation;
        Singleton<MainViewModel>.Instance.UpdateMainViewIcons();
        ThemeOperate(CurrentConfig.ThemeType);
        ProxyOperate(CurrentConfig.ProxyMethod, CurrentConfig.ProxyIp, CurrentConfig.ProxyPort ?? 0, CurrentConfig.IsProxyAuthentication, CurrentConfig.ProxyUsername, CurrentConfig.ProxyPassword);

        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    #endregion 公共方法

    #region 私有方法

    private ConfigModel ReadConfig()
    {
        try
        {
            var settings = new JsonSerializerSettings { Converters = { new TranslatorConverter(), new TTSConverter() } };
            var content = File.ReadAllText(ConstStr.CnfName);
            var config = JsonConvert.DeserializeObject<ConfigModel>(content, settings) ?? throw new Exception("反序列化失败...");
            // 读取时解密AppID、AppKey
            config
                .Services?.ToList()
                .ForEach(service =>
                {
                    service.AppID = string.IsNullOrEmpty(service.AppID) ? service.AppID : DESUtil.DesDecrypt(service.AppID);
                    service.AppKey = string.IsNullOrEmpty(service.AppKey) ? service.AppKey : DESUtil.DesDecrypt(service.AppKey);
                });
            config
                .TTSList?.ToList()
                .ForEach(tts =>
                {
                    tts.AppID = string.IsNullOrEmpty(tts.AppID) ? tts.AppID : DESUtil.DesDecrypt(tts.AppID);
                    tts.AppKey = string.IsNullOrEmpty(tts.AppKey) ? tts.AppKey : DESUtil.DesDecrypt(tts.AppKey);
                });
            return config;
        }
        catch (Exception ex)
        {
            // 备份当前config
            var path = BackupCurrentConfig();

            LogService.Logger.Error($"[READ CONFIG] 读取配置错误，已备份旧配置至: {path} 当前加载初始化配置...", ex);
            return InitialConfig();
        }
    }

    /// <summary>
    /// 备份当前配置文件
    /// </summary>
    private string BackupCurrentConfig()
    {
        var backupFilePath = $"{ConstStr.AppData}\\{ConstStr.AppName.ToLower()}_{DateTime.Now:yyyyMMdd_HHmmssfff}.json";
        File.Copy(ConstStr.CnfName, backupFilePath, true);
        return backupFilePath;
    }

    private void WriteConfig(ConfigModel conf)
    {
        var copy = conf.Clone();
        // Translate Service加密
        copy.Services?.ToList()
            .ForEach(service =>
            {
                service.AppID = string.IsNullOrEmpty(service.AppID) ? service.AppID : DESUtil.DesEncrypt(service.AppID);
                service.AppKey = string.IsNullOrEmpty(service.AppKey) ? service.AppKey : DESUtil.DesEncrypt(service.AppKey);
            });
        // TTS加密
        copy.TTSList?.ToList()
            .ForEach(tts =>
            {
                tts.AppID = string.IsNullOrEmpty(tts.AppID) ? tts.AppID : DESUtil.DesEncrypt(tts.AppID);
                tts.AppKey = string.IsNullOrEmpty(tts.AppKey) ? tts.AppKey : DESUtil.DesEncrypt(tts.AppKey);
            });
        File.WriteAllText(ConstStr.CnfName, JsonConvert.SerializeObject(copy, Formatting.Indented));
    }

    private void ThemeOperate(ThemeType themeType)
    {
        switch (themeType)
        {
            case ThemeType.Auto:
                Singleton<ThemeHelper>.Instance.StartListenRegistry();
                break;
            case ThemeType.Light:
                Singleton<ThemeHelper>.Instance.LightTheme();
                goto default;
            case ThemeType.Dark:
                Singleton<ThemeHelper>.Instance.DarkTheme();
                goto default;
            default:
                Singleton<ThemeHelper>.Instance.StopListenRegistry();
                break;
        }
    }

    //初始化字体
    private void FontOperate()
    {
        try
        {
            Application.Current.Resources[ConstStr.USERDEFINEFONTKEY] = CurrentConfig!.CustomFont.Equals(ConstStr.DEFAULTFONTNAME)
                ? Application.Current.Resources[ConstStr.DEFAULTFONTNAME]
                : new FontFamily(CurrentConfig!.CustomFont);
        }
        catch (Exception)
        {
            Application.Current.Resources[ConstStr.USERDEFINEFONTKEY] = Application.Current.Resources[ConstStr.DEFAULTFONTNAME];
            CurrentConfig!.CustomFont = ConstStr.DEFAULTFONTNAME;
        }
    }

    //初始化文本转语音服务
    private void TTSOperate()
    {
        Singleton<TTSViewModel>.Instance.ActivedTTS = CurrentConfig?.TTSList?.FirstOrDefault(x => x.IsEnabled);
    }

    //代理操作
    private void ProxyOperate(ProxyMethodEnum proxyMethod, string ip, int port, bool isAuth, string username, string pwd)
    {
        ProxyUtil.UpdateProxy(proxyMethod, ip, port, isAuth, username, pwd);
    }

    #endregion 私有方法

    #region 字段 && 属性

    /// <summary>
    /// 重置Config
    /// </summary>
    public ConfigModel ResetConfig => ReadConfig();

    /// <summary>
    /// 初始Config
    /// </summary>
    private ConfigModel InitialConfig()
    {
        var hk = new Hotkeys();
        hk.InputTranslate.Update(KeyModifiers.MOD_ALT, KeyCodes.A, ConstStr.DEFAULTINPUTHOTKEY);
        hk.CrosswordTranslate.Update(KeyModifiers.MOD_ALT, KeyCodes.D, ConstStr.DEFAULTCROSSWORDHOTKEY);
        hk.ScreenShotTranslate.Update(KeyModifiers.MOD_ALT, KeyCodes.S, ConstStr.DEFAULTSCREENSHOTHOTKEY);
        hk.OpenMainWindow.Update(KeyModifiers.MOD_ALT, KeyCodes.G, ConstStr.DEFAULTOPENHOTKEY);
        hk.MousehookTranslate.Update(KeyModifiers.MOD_ALT | KeyModifiers.MOD_SHIFT, KeyCodes.D, ConstStr.DEFAULTMOUSEHOOKHOTKEY);
        hk.OCR.Update(KeyModifiers.MOD_ALT | KeyModifiers.MOD_SHIFT, KeyCodes.S, ConstStr.DEFAULTOCRHOTKEY);
        hk.SilentOCR.Update(KeyModifiers.MOD_ALT | KeyModifiers.MOD_SHIFT, KeyCodes.F, ConstStr.DEFAULTSILENTOCRHOTKEY);
        hk.ClipboardMonitor.Update(KeyModifiers.MOD_ALT | KeyModifiers.MOD_SHIFT, KeyCodes.A, ConstStr.DEFAULTCLIPBOARDMONITORHOTKEY);
        return new ConfigModel
        {
            HistorySize = 100,
            AutoScale = 0.8,
            Hotkeys = hk,
            ThemeType = ThemeType.Light,
            IsStartup = false,
            IsFollowMouse = false,
            IsOcrAutoCopyText = false,
            UnconventionalScreen = false,
            CloseUIOcrRetTranslate = false,
            IsAdjustContentTranslate = false,
            IsRemoveLineBreakGettingWords = false,
            DoubleTapTrayFunc = DoubleTapFuncEnum.InputFunc,
            CustomFont = ConstStr.DEFAULTFONTNAME,
            IsKeepTopmostAfterMousehook = false,
            IsShowPreference = false,
            IsShowMousehook = false,
            IsShowScreenshot = false,
            IsShowOCR = false,
            IsShowSilentOCR = false,
            IsShowClipboardMonitor = false,
            IsShowQRCode = false,
            IsShowHistory = false,
            WordPickingInterval = 100,
            IsHideOnStart = false,
            ShowCopyOnHeader = false,
            IsCaretLast = false,
            MaxHeight = MaxHeight.Maximum,
            Width = WidthEnum.Minimum,
            ProxyMethod = ProxyMethodEnum.系统代理,
            ProxyIp = string.Empty,
            ProxyPort = null,
            IsProxyAuthentication = false,
            ProxyUsername = string.Empty,
            ProxyPassword = string.Empty,
            CopyResultAfterTranslateIndex = 0,
            SourceLanguage = LanguageEnum.AUTO.GetDescription(),
            TargetLanguage = LanguageEnum.AUTO.GetDescription(),
            Services =
            [
                new TranslatorSTranslate(Guid.NewGuid(), "", "STranslate", IconType.STranslate),
                new TranslatorApi(Guid.NewGuid(), "https://googlet.deno.dev/translate", "Google", IconType.Google),
                new TranslatorApi(Guid.NewGuid(), "https://deeplx.deno.dev/translate", "DeepL", IconType.DeepL, isEnabled: false),
                new TranslatorApi(Guid.NewGuid(), "https://iciba.deno.dev/translate", "爱词霸", IconType.Iciba, isEnabled: false)
            ],
            TTSList = [new TTSOffline()]
        };
    }

    public ConfigModel? CurrentConfig { get; private set; }

    #endregion 字段 && 属性
}

#region JsonConvert

public class TTSConverter : JsonConverter<ITTS>
{
    public override ITTS? ReadJson(JsonReader reader, Type objectType, ITTS? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);

        // 根据Type字段的值来决定具体实现类
        var type = jsonObject["Type"]!.Value<int>();
        ITTS tts = type switch
        {
            (int)TTSType.AzureTTS => new TTSAzure(),
            (int)TTSType.OfflineTTS => new TTSOffline(),
            //TODO: 新TTS服务需要适配
            _ => throw new NotSupportedException($"Unsupported TTSServiceType: {type}")
        };

        serializer.Populate(jsonObject.CreateReader(), tts);
        return tts;
    }

    public override void WriteJson(JsonWriter writer, ITTS? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public class TranslatorConverter : JsonConverter<ITranslator>
{
    public override ITranslator ReadJson(JsonReader reader, Type objectType, ITranslator? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);

        // 根据Type字段的值来决定具体实现类
        var type = jsonObject["Type"]!.Value<int>();

        ITranslator translator = type switch
        {
            (int)ServiceType.STranslateService => new TranslatorSTranslate(),
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
            (int)ServiceType.VolcengineService => new TranslatorVolcengine(),
            (int)ServiceType.EcdictService => new TranslatorEcdict(),
            (int)ServiceType.ChatglmService => new TranslatorChatglm(),
            //TODO: 新接口需要适配
            _ => throw new NotSupportedException($"Unsupported ServiceType: {type}")
        };

        translator.UserDefinePrompts.Clear();

        serializer.Populate(jsonObject.CreateReader(), translator);
        return translator;
    }

    public override void WriteJson(JsonWriter writer, ITranslator? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
#endregion JsonConvert
