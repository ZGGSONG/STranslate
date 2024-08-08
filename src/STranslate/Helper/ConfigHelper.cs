using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels;
using STranslate.ViewModels.Preference;
using STranslate.ViewModels.Preference.OCR;
using STranslate.ViewModels.Preference.Translator;
using STranslate.ViewModels.Preference.TTS;
using STranslate.Views;

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

        if (!File.Exists(ConstStr.CnfFullName)) //文件不存在
        {
            FileStream fs = new(ConstStr.CnfFullName, FileMode.Create, FileAccess.ReadWrite);
            fs.Close();
            WriteConfig(InitialConfig());
        }

        InitCurrentCnf();
    }

    /// <summary>
    ///     读取配置文件到缓存
    /// </summary>
    public void InitCurrentCnf()
    {
        //初始化时将初始值赋给Config属性
        CurrentConfig = ResetConfig;
    }

    /// <summary>
    ///     初始化操作
    /// </summary>
    public void InitialOperate()
    {
        StartupOperate(CurrentConfig?.IsStartup ?? false);

        //初始化主题
        ThemeOperate(CurrentConfig?.ThemeType ?? ThemeType.Light);

        //初始化字体
        FontOperate();

        //初始化代理设置
        ProxyOperate(
            CurrentConfig?.ProxyMethod ?? ProxyMethodEnum.系统代理,
            CurrentConfig?.ProxyIp ?? "",
            CurrentConfig?.ProxyPort ?? 0,
            CurrentConfig?.IsProxyAuthentication ?? false,
            CurrentConfig?.ProxyUsername ?? "",
            CurrentConfig?.ProxyPassword ?? ""
        );

        //初始化主窗口提示词
        PlaceholderOperate(CurrentConfig?.IsShowMainPlaceholder ?? true);

        //初始化首页图标
        MainViewIconOperate();

        //初始化外部调用服务
        ExternalCallOperate(CurrentConfig?.ExternalCall ?? false, CurrentConfig?.ExternalCallPort ?? 50020);
    }

    /// <summary>
    ///     退出时保存位置
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool WriteConfig(double x, double y)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        CurrentConfig.Position = $"{x},{y}";
        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    /// <summary>
    ///     写入服务到配置
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
    ///     写入OCR服务到配置
    /// </summary>
    /// <param name="ocrList"></param>
    /// <returns></returns>
    public bool WriteConfig(OCRCollection<IOCR> ocrList)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        CurrentConfig.OCRList = ocrList;
        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    /// <summary>
    ///     写入TTS服务到配置
    /// </summary>
    /// <param name="ttsList"></param>
    /// <returns></returns>
    public bool WriteConfig(TTSCollection<ITTS> ttsList)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        CurrentConfig.TTSList = ttsList;
        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    /// <summary>
    ///     写入源语言、目标语言到配置
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool WriteConfig(LangEnum source, LangEnum target)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        CurrentConfig.SourceLang = source;
        CurrentConfig.TargetLang = target;
        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    /// <summary>
    ///     写入热键到配置
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
    ///     写入常规配置项到当前配置
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public bool WriteConfig(CommonViewModel model)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        //判断是否相同,避免重复注册
        var isHotkeyConfSame = CurrentConfig.DisableGlobalHotkeys == model.DisableGlobalHotkeys;
        CurrentConfig.IsStartup = model.IsStartup;
        CurrentConfig.NeedAdministrator = model.NeedAdmin;
        CurrentConfig.HistorySize = model.HistorySize;
        CurrentConfig.AutoScale = model.AutoScale;
        CurrentConfig.ThemeType = model.ThemeType;
        CurrentConfig.IsFollowMouse = model.IsFollowMouse;
        CurrentConfig.CloseUIOcrRetTranslate = model.CloseUIOcrRetTranslate;
        CurrentConfig.IsOcrAutoCopyText = model.IsOcrAutoCopyText;
        CurrentConfig.IsAdjustContentTranslate = model.IsAdjustContentTranslate;
        CurrentConfig.IsRemoveLineBreakGettingWords = model.IsRemoveLineBreakGettingWords;
        CurrentConfig.IsRemoveLineBreakGettingWordsOCR = model.IsRemoveLineBreakGettingWordsOCR;
        CurrentConfig.DoubleTapTrayFunc = model.DoubleTapTrayFunc;
        CurrentConfig.CustomFont = model.CustomFont;
        CurrentConfig.IsKeepTopmostAfterMousehook = model.IsKeepTopmostAfterMousehook;
        CurrentConfig.IsShowClose = model.IsShowClose;
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
        CurrentConfig.ProxyMethod = model.ProxyMethod;
        CurrentConfig.ProxyIp = model.ProxyIp;
        CurrentConfig.ProxyPort = model.ProxyPort;
        CurrentConfig.IsProxyAuthentication = model.IsProxyAuthentication;
        CurrentConfig.ProxyUsername = model.ProxyUsername;
        CurrentConfig.ProxyPassword = model.ProxyPassword;
        CurrentConfig.CopyResultAfterTranslateIndex = model.CopyResultAfterTranslateIndex;
        CurrentConfig.IncrementalTranslation = model.IncrementalTranslation;
        CurrentConfig.IsTriggerShowHide = model.IsTriggerShowHide;
        CurrentConfig.IsShowMainPlaceholder = model.IsShowMainPlaceholder;
        CurrentConfig.ShowAuxiliaryLine = model.ShowAuxiliaryLine;
        CurrentConfig.ChangedLang2Execute = model.ChangedLang2Execute;
        CurrentConfig.OcrChangedLang2Execute = model.OcrChangedLang2Execute;
        CurrentConfig.UseFormsCopy = model.UseFormsCopy;
        CurrentConfig.ExternalCall = model.ExternalCall;
        CurrentConfig.ExternalCallPort = model.ExternalCallPort;
        CurrentConfig.DetectType = model.DetectType;
        CurrentConfig.DisableGlobalHotkeys = model.DisableGlobalHotkeys;
        CurrentConfig.MainViewMaxHeight = model.MainViewMaxHeight;
        CurrentConfig.MainViewWidth = model.MainViewWidth;
        CurrentConfig.MainViewShadow = model.MainViewShadow;
        CurrentConfig.IsPromptToggleVisible = model.IsPromptToggleVisible;
        CurrentConfig.IsShowSnakeCopyBtn = model.IsShowSnakeCopyBtn;
        CurrentConfig.IsShowSmallHumpCopyBtn = model.IsShowSmallHumpCopyBtn;
        CurrentConfig.IsShowLargeHumpCopyBtn = model.IsShowLargeHumpCopyBtn;
        CurrentConfig.IsShowTranslateBackBtn = model.IsShowTranslateBackBtn;
        CurrentConfig.IgnoreHotkeysOnFullscreen = model.IgnoreHotkeysOnFullscreen;
        CurrentConfig.StayMainViewWhenLoseFocus = model.StayMainViewWhenLoseFocus;
        CurrentConfig.MainOcrLang = model.MainOcrLang;
        CurrentConfig.ShowMainOcrLang = model.ShowMainOcrLang;
        CurrentConfig.HotkeyCopySuccessToast = model.HotkeyCopySuccessToast;
        CurrentConfig.OftenUsedLang = model.OftenUsedLang;
        CurrentConfig.UseCacheLocation = model.UseCacheLocation;

        //重新执行必要操作
        StartupOperate(CurrentConfig.IsStartup);
        ThemeOperate(CurrentConfig.ThemeType);
        ProxyOperate(
            CurrentConfig.ProxyMethod,
            CurrentConfig.ProxyIp,
            CurrentConfig.ProxyPort ?? 8089,
            CurrentConfig.IsProxyAuthentication,
            CurrentConfig.ProxyUsername,
            CurrentConfig.ProxyPassword
        );
        PlaceholderOperate(CurrentConfig.IsShowMainPlaceholder);
        MainViewIconOperate();
        MainViewOftenUsedLangOperate();
        ExternalCallOperate(CurrentConfig.ExternalCall, CurrentConfig.ExternalCallPort ?? 50020, true);
        MainViewShadowOperate(CurrentConfig.MainViewShadow);
        MainViewStayOperate(CurrentConfig.StayMainViewWhenLoseFocus);
        //输出界面显示控制
        OutputViewOperate(
            CurrentConfig.IsPromptToggleVisible,
            CurrentConfig.IsShowSnakeCopyBtn,
            CurrentConfig.IsShowSmallHumpCopyBtn,
            CurrentConfig.IsShowLargeHumpCopyBtn,
            CurrentConfig.IsShowTranslateBackBtn);

        if (!isHotkeyConfSame)
            DisableGlobalHotkeysOperate(CurrentConfig.DisableGlobalHotkeys,
                Application.Current.Windows.OfType<MainView>().First());

        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    public bool WriteConfig(ReplaceViewModel model)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;

        CurrentConfig.ReplaceProp = model.ReplaceProp;

        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    /// <summary>
    ///     写入备份到配置
    /// </summary>
    /// <param name="hotkeys"></param>
    /// <returns></returns>
    public bool WriteConfig(BackupViewModel model)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        CurrentConfig.BackupType = model.BackupType;
        CurrentConfig.WebDavUrl = model.WebDavUrl;
        CurrentConfig.WebDavUsername = model.WebDavUsername;
        CurrentConfig.WebDavPassword = model.WebDavPassword;
        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    /// <summary>
    ///     保存ocr页面宽高
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool WriteOCRConfig(double height, double width)
    {
        var isSuccess = false;
        if (CurrentConfig is null)
            return isSuccess;
        CurrentConfig.OcrViewHeight = height;
        CurrentConfig.OcrViewWidth = width;
        WriteConfig(CurrentConfig);
        isSuccess = true;
        return isSuccess;
    }

    /// <summary>
    ///     校验配置
    /// </summary>
    /// <param name="configPath"></param>
    /// <returns></returns>
    public bool VerificateConfig(string configPath)
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                Converters =
                    { new TranslatorConverter(), new OCRConverter(), new TTSConverter(), new ReplaceConverter() }
            };
            var content = File.ReadAllText(configPath);
            var config = JsonConvert.DeserializeObject<ConfigModel>(content, settings) ??
                         throw new Exception("反序列化失败...");
            Decryption(config);
            return true;
        }
        catch (Exception ex)
        {
            LogService.Logger.Warn($"读取配置({configPath})出错，{ex.Message}");
            return false;
        }
    }

    #endregion 公共方法

    #region 私有方法

    private ConfigModel ReadConfig()
    {
        try
        {
            var settings = new JsonSerializerSettings
            {
                Converters =
                    { new TranslatorConverter(), new OCRConverter(), new TTSConverter(), new ReplaceConverter() }
            };
            var content = File.ReadAllText(ConstStr.CnfFullName);
            var config = JsonConvert.DeserializeObject<ConfigModel>(content, settings) ??
                         throw new Exception("反序列化失败...");
            Decryption(config);
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
    ///     备份当前配置文件
    /// </summary>
    private string BackupCurrentConfig()
    {
        var backupFilePath = $"{ConstStr.AppData}\\{ConstStr.AppName.ToLower()}_{DateTime.Now:yyyyMMdd_HHmmssfff}.json";
        File.Copy(ConstStr.CnfFullName, backupFilePath, true);
        return backupFilePath;
    }

    private void WriteConfig(ConfigModel conf)
    {
        var copy = conf.Clone();
        Encryption(copy);
        File.WriteAllText(ConstStr.CnfFullName, JsonConvert.SerializeObject(copy, Formatting.Indented));
    }

    /// <summary>
    ///     加密
    /// </summary>
    /// <param name="conf"></param>
    private void Encryption(ConfigModel conf)
    {
        // proxy pwd
        conf.ProxyPassword = string.IsNullOrEmpty(conf.ProxyPassword)
            ? conf.ProxyPassword
            : DESUtil.DesEncrypt(conf.ProxyPassword);

        // webdav pwd
        conf.WebDavPassword = string.IsNullOrEmpty(conf.WebDavPassword)
            ? conf.WebDavPassword
            : DESUtil.DesEncrypt(conf.WebDavPassword);

        // Translate Service加密
        conf.Services?.ToList()
            .ForEach(service =>
            {
                service.AppID = string.IsNullOrEmpty(service.AppID) ? service.AppID : DESUtil.DesEncrypt(service.AppID);
                service.AppKey = string.IsNullOrEmpty(service.AppKey)
                    ? service.AppKey
                    : DESUtil.DesEncrypt(service.AppKey);
            });
        // OCR加密
        conf.OCRList?.ToList()
            .ForEach(ocr =>
            {
                ocr.AppID = string.IsNullOrEmpty(ocr.AppID) ? ocr.AppID : DESUtil.DesEncrypt(ocr.AppID);
                ocr.AppKey = string.IsNullOrEmpty(ocr.AppKey) ? ocr.AppKey : DESUtil.DesEncrypt(ocr.AppKey);
            });
        // TTS加密
        conf.TTSList?.ToList()
            .ForEach(tts =>
            {
                tts.AppID = string.IsNullOrEmpty(tts.AppID) ? tts.AppID : DESUtil.DesEncrypt(tts.AppID);
                tts.AppKey = string.IsNullOrEmpty(tts.AppKey) ? tts.AppKey : DESUtil.DesEncrypt(tts.AppKey);
            });

        // Replace属性加密
        var rp = conf.ReplaceProp.ActiveService;
        if (rp is null) return;
        rp.AppID = string.IsNullOrEmpty(rp.AppID) ? rp.AppID : DESUtil.DesEncrypt(rp.AppID);
        rp.AppKey = string.IsNullOrEmpty(rp.AppKey) ? rp.AppKey : DESUtil.DesEncrypt(rp.AppKey);
    }

    /// <summary>
    ///     解密
    /// </summary>
    /// <param name="conf"></param>
    private void Decryption(ConfigModel conf)
    {
        // 读取时解密 proxy webdav 密码
        conf.ProxyPassword = string.IsNullOrEmpty(conf.ProxyPassword)
            ? conf.ProxyPassword
            : DESUtil.DesDecrypt(conf.ProxyPassword);
        conf.WebDavPassword = string.IsNullOrEmpty(conf.WebDavPassword)
            ? conf.WebDavPassword
            : DESUtil.DesDecrypt(conf.WebDavPassword);

        // 读取时解密AppID、AppKey
        conf.Services?.ToList()
            .ForEach(service =>
            {
                service.AppID = string.IsNullOrEmpty(service.AppID) ? service.AppID : DESUtil.DesDecrypt(service.AppID);
                service.AppKey = string.IsNullOrEmpty(service.AppKey)
                    ? service.AppKey
                    : DESUtil.DesDecrypt(service.AppKey);
            });
        conf.OCRList?.ToList()
            .ForEach(ocr =>
            {
                ocr.AppID = string.IsNullOrEmpty(ocr.AppID) ? ocr.AppID : DESUtil.DesDecrypt(ocr.AppID);
                ocr.AppKey = string.IsNullOrEmpty(ocr.AppKey) ? ocr.AppKey : DESUtil.DesDecrypt(ocr.AppKey);
            });
        conf.TTSList?.ToList()
            .ForEach(tts =>
            {
                tts.AppID = string.IsNullOrEmpty(tts.AppID) ? tts.AppID : DESUtil.DesDecrypt(tts.AppID);
                tts.AppKey = string.IsNullOrEmpty(tts.AppKey) ? tts.AppKey : DESUtil.DesDecrypt(tts.AppKey);
            });
        var rp = conf.ReplaceProp.ActiveService;
        if (rp is null) return;
        rp.AppID = string.IsNullOrEmpty(rp.AppID) ? rp.AppID : DESUtil.DesDecrypt(rp.AppID);
        rp.AppKey = string.IsNullOrEmpty(rp.AppKey) ? rp.AppKey : DESUtil.DesDecrypt(rp.AppKey);
    }

    /// <summary>
    ///     初始化自启动
    /// </summary>
    /// <param name="isStartup"></param>
    private void StartupOperate(bool isStartup)
    {
        if (isStartup)
        {
            if (!ShortcutUtil.IsStartup())
                ShortcutUtil.SetStartup();
        }
        else
        {
            ShortcutUtil.UnSetStartup();
        }
    }

    /// <summary>
    ///     初始化主题
    /// </summary>
    /// <param name="themeType"></param>
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

    /// <summary>
    ///     初始化字体
    /// </summary>
    private void FontOperate()
    {
        try
        {
            Application.Current.Resources[ConstStr.USERDEFINEFONTKEY] =
                CurrentConfig!.CustomFont.Equals(ConstStr.DEFAULTFONTNAME)
                    ? Application.Current.Resources[ConstStr.DEFAULTFONTNAME]
                    : new FontFamily(CurrentConfig!.CustomFont);
        }
        catch (Exception)
        {
            Application.Current.Resources[ConstStr.USERDEFINEFONTKEY] =
                Application.Current.Resources[ConstStr.DEFAULTFONTNAME];
            CurrentConfig!.CustomFont = ConstStr.DEFAULTFONTNAME;
        }
    }

    /// <summary>
    ///     代理操作
    /// </summary>
    /// <param name="proxyMethod"></param>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="isAuth"></param>
    /// <param name="username"></param>
    /// <param name="pwd"></param>
    private void ProxyOperate(ProxyMethodEnum proxyMethod, string ip, int port, bool isAuth, string username,
        string pwd)
    {
        ProxyUtil.UpdateProxy(proxyMethod, ip, port, isAuth, username, pwd);
    }

    /// <summary>
    ///     主窗口提示词
    /// </summary>
    /// <param name="isShowMainPlaceholder"></param>
    private void PlaceholderOperate(bool isShowMainPlaceholder)
    {
        Singleton<InputViewModel>.Instance.Placeholder =
            isShowMainPlaceholder ? ConstStr.MAINVIEWPLACEHOLDER : string.Empty;
    }

    /// <summary>
    ///     刷新主窗口图标
    /// </summary>
    private void MainViewIconOperate()
    {
        Singleton<MainViewModel>.Instance.UpdateMainViewIcons();
    }

    private void MainViewOftenUsedLangOperate()
    {
        Singleton<MainViewModel>.Instance.InputVM.UpdateOftenUsedLang();
    }

    /// <summary>
    ///     外部调用功能
    /// </summary>
    /// <param name="can"></param>
    /// <param name="port"></param>
    /// <param name="isStop"></param>
    private void ExternalCallOperate(bool can, int port, bool isStop = false)
    {
        if (can)
            Singleton<ExternalCallHelper>.Instance.StartService($"http://127.0.0.1:{port}/", isStop);
        else
            Singleton<ExternalCallHelper>.Instance.StopService();
    }

    /// <summary>
    ///     主窗口阴影
    /// </summary>
    /// <param name="mainViewShadow"></param>
    public void MainViewShadowOperate(bool mainViewShadow)
    {
        ShadowHelper.ShadowEffect(mainViewShadow);
    }

    /// <summary>
    ///     主窗口失焦后保留
    /// </summary>
    /// <param name="isStayView"></param>
    public void MainViewStayOperate(bool isStayView)
    {
        Application.Current.MainWindow!.ShowInTaskbar = isStayView;
    }

    /// <summary>
    ///     初始化时自动进MainViewModel进行处理
    ///     后续修改执行该处理
    /// </summary>
    /// <param name="value"></param>
    /// <param name="view"></param>
    private void DisableGlobalHotkeysOperate(bool value, Window view)
    {
        Singleton<NotifyIconViewModel>.Instance.InvokeForbiddenShotcuts(view, value);
    }

    /// <summary>
    ///     输出界面显示按钮控制
    /// </summary>
    /// <param name="isPromptToggleVisible"></param>
    /// <param name="isShowSnakeCopyBtn"></param>
    /// <param name="isShowSmallHumpCopyBtn"></param>
    /// <param name="isShowLargeHumpCopyBtn"></param>
    /// <param name="isShowTranslateBackBtn"></param>
    private void OutputViewOperate(bool isPromptToggleVisible, bool isShowSnakeCopyBtn, bool isShowSmallHumpCopyBtn, bool isShowLargeHumpCopyBtn, bool isShowTranslateBackBtn)
    {
        Singleton<OutputViewModel>.Instance.IsPromptToggleVisible = isPromptToggleVisible;
        Singleton<OutputViewModel>.Instance.IsShowSnakeCopyBtn = isShowSnakeCopyBtn;
        Singleton<OutputViewModel>.Instance.IsShowSmallHumpCopyBtn = isShowSmallHumpCopyBtn;
        Singleton<OutputViewModel>.Instance.IsShowLargeHumpCopyBtn = isShowLargeHumpCopyBtn;
        Singleton<OutputViewModel>.Instance.IsShowTranslateBackBtn = isShowTranslateBackBtn;
    }

    #endregion 私有方法

    #region 字段 && 属性

    /// <summary>
    ///     重置Config
    /// </summary>
    public ConfigModel ResetConfig => ReadConfig();

    /// <summary>
    ///     初始Config
    /// </summary>
    private ConfigModel InitialConfig()
    {
        var hk = new Hotkeys();
        hk.InputTranslate.Update(KeyModifiers.MOD_ALT, KeyCodes.A, ConstStr.DEFAULTINPUTHOTKEY);
        hk.CrosswordTranslate.Update(KeyModifiers.MOD_ALT, KeyCodes.D, ConstStr.DEFAULTCROSSWORDHOTKEY);
        hk.ScreenShotTranslate.Update(KeyModifiers.MOD_ALT, KeyCodes.S, ConstStr.DEFAULTSCREENSHOTHOTKEY);
        hk.OpenMainWindow.Update(KeyModifiers.MOD_ALT, KeyCodes.G, ConstStr.DEFAULTOPENHOTKEY);
        hk.ReplaceTranslate.Update(KeyModifiers.MOD_ALT, KeyCodes.F, ConstStr.DEFAULTREPLACEHOTKEY);
        hk.MousehookTranslate.Update(KeyModifiers.MOD_ALT | KeyModifiers.MOD_SHIFT, KeyCodes.D,
            ConstStr.DEFAULTMOUSEHOOKHOTKEY);
        hk.OCR.Update(KeyModifiers.MOD_ALT | KeyModifiers.MOD_SHIFT, KeyCodes.S, ConstStr.DEFAULTOCRHOTKEY);
        hk.SilentOCR.Update(KeyModifiers.MOD_ALT | KeyModifiers.MOD_SHIFT, KeyCodes.F, ConstStr.DEFAULTSILENTOCRHOTKEY);
        hk.ClipboardMonitor.Update(KeyModifiers.MOD_ALT | KeyModifiers.MOD_SHIFT, KeyCodes.A,
            ConstStr.DEFAULTCLIPBOARDMONITORHOTKEY);
        return new ConfigModel
        {
            HistorySize = 100,
            AutoScale = 0.8,
            Hotkeys = hk,
            ThemeType = ThemeType.Light,
            IsStartup = false,
            IsFollowMouse = false,
            IsOcrAutoCopyText = false,
            CloseUIOcrRetTranslate = false,
            IsAdjustContentTranslate = false,
            IsRemoveLineBreakGettingWords = false,
            IsRemoveLineBreakGettingWordsOCR = false,
            DoubleTapTrayFunc = DoubleTapFuncEnum.InputFunc,
            CustomFont = ConstStr.DEFAULTFONTNAME,
            IsKeepTopmostAfterMousehook = false,
            IsShowClose = false,
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
            ProxyMethod = ProxyMethodEnum.系统代理,
            ProxyIp = string.Empty,
            ProxyPort = 8089,
            IsProxyAuthentication = false,
            ProxyUsername = string.Empty,
            ProxyPassword = string.Empty,
            CopyResultAfterTranslateIndex = 0,
            IncrementalTranslation = false,
            IsTriggerShowHide = false,
            IsShowMainPlaceholder = true,
            ShowAuxiliaryLine = true,
            WebDavUrl = string.Empty,
            WebDavUsername = string.Empty,
            WebDavPassword = string.Empty,
            SourceLang = LangEnum.auto,
            TargetLang = LangEnum.auto,
            ChangedLang2Execute = false,
            UseFormsCopy = false,
            ExternalCall = false,
            ExternalCallPort = 50020,
            OcrViewHeight = 400,
            OcrViewWidth = 1000,
            DetectType = LangDetectType.Local,
            DisableGlobalHotkeys = false,
            MainViewMaxHeight = 840,
            MainViewWidth = 460,
            MainViewShadow = false,
            IsPromptToggleVisible = true,
            IsShowSnakeCopyBtn = false,
            IsShowSmallHumpCopyBtn = false,
            IsShowLargeHumpCopyBtn = false,
            IsShowTranslateBackBtn = false,
            IgnoreHotkeysOnFullscreen = false,
            MainOcrLang = LangEnum.auto,
            ShowMainOcrLang = false,
            HotkeyCopySuccessToast = true,
            OftenUsedLang = string.Empty,
            UseCacheLocation = false,
            ReplaceProp = new ReplaceProp(),
            Services =
            [
                new TranslatorSTranslate(Guid.NewGuid(), "", "STranslate"),
                new TranslatorApi(Guid.NewGuid(), "https://googlet.deno.dev/translate", "Google", IconType.Google),
                new TranslatorApi(Guid.NewGuid(), "https://deeplx.deno.dev/translate", "DeepL", isEnabled: false)
            ],
            OCRList = [new PaddleOCR()],
            TTSList = [new TTSOffline()]
        };
    }

    public ConfigModel? CurrentConfig { get; private set; }

    #endregion 字段 && 属性
}

#region JsonConvert

public class OCRConverter : JsonConverter<IOCR>
{
    public override IOCR? ReadJson(JsonReader reader, Type objectType, IOCR? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

        // 根据Type字段的值来决定具体实现类
        var type = jsonObject["Type"]!.Value<int>();
        IOCR ocr = type switch
        {
            //TODO: 新OCR服务需要适配
            (int)OCRType.PaddleOCR => new PaddleOCR(),
            (int)OCRType.BaiduOCR => new BaiduOCR(),
            (int)OCRType.TencentOCR => new TencentOCR(),
            (int)OCRType.VolcengineOCR => new VolcengineOCR(),
            _ => throw new NotSupportedException($"Unsupported OCRServiceType: {type}")
        };

        serializer.Populate(jsonObject.CreateReader(), ocr);
        return ocr;
    }

    public override void WriteJson(JsonWriter writer, IOCR? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public class TTSConverter : JsonConverter<ITTS>
{
    public override ITTS? ReadJson(JsonReader reader, Type objectType, ITTS? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

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

public class ReplaceConverter : JsonConverter<ReplaceProp>
{
    public override ReplaceProp? ReadJson(JsonReader reader, Type objectType, ReplaceProp? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var rate = jsonObject["AutoScale"]!.Value<double>();
        var detectType = jsonObject["DetectType"]!.Value<int>();
        var targetLang = jsonObject["TargetLang"]!.Value<int>();
        var model = new ReplaceProp
        {
            //ActiveService = ,
            AutoScale = rate,
            DetectType = (LangDetectType)detectType,
            TargetLang = (LangEnum)targetLang
        };

        var obj = jsonObject["ActiveService"]?.Value<object>();
        if (obj is null) return model;
        var service = JsonConvert.DeserializeObject<ITranslator>(obj.ToString() ?? string.Empty,
            new JsonSerializerSettings { Converters = { new TranslatorConverter() } });
        model.ActiveService = service;

        return model;
    }

    public override void WriteJson(JsonWriter writer, ReplaceProp? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

public class TranslatorConverter : JsonConverter<ITranslator>
{
    public override ITranslator ReadJson(JsonReader reader, Type objectType, ITranslator? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);

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
            (int)ServiceType.OllamaService => new TranslatorOllama(),
            (int)ServiceType.BaiduBceService => new TranslatorBaiduBce(),
            (int)ServiceType.DeepLService => new TranslatorDeepL(),
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