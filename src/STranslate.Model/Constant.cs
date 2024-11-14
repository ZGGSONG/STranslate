using System.Collections;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace STranslate.Model;

public static class Constant
{
    #region Path
    public const string GithubReleaseUrl = "https://api.github.com/repos/zggsong/stranslate/releases/latest";
    public const string DefaultVersion = "1.0.0.0";

    public static readonly string AppVersion =
        Application.ResourceAssembly.GetName().Version?.ToString() ?? DefaultVersion;

    public const string CnfExtension = ".json";
    public const string CnfName = "stranslate";
    public const string AppName = "STranslate";
    private const string PortableConfig = "portable_config";

    /// <summary>
    ///     用户软件根目录
    /// </summary>
    /// <remarks>
    ///     <see cref="Environment.CurrentDirectory"/>
    ///     * 使用批处理时获取路径为批处理文件所在目录
    /// </remarks>
    public static readonly string ExecutePath = AppDomain.CurrentDomain.BaseDirectory;

    private static readonly string PortableCnfPath = $"{ExecutePath}{PortableConfig}";

    /// <summary>
    ///     是否为便携模式
    /// </summary>
    private static readonly bool IsPortable = Directory.Exists(PortableCnfPath);

    /// <summary>
    ///     用户配置目录
    /// </summary>
    public static readonly string CnfPath = IsPortable
            ? PortableCnfPath
            : $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{AppName}";

    /// <summary>
    ///     配置路径
    /// </summary>
    public static readonly string CnfFullName = $"{CnfPath}\\{CnfName}.json";

    public static readonly string DbConnectionString = $"Data Source={CnfPath}\\{CnfName}.db";
    public static readonly string EcDictPath = Path.Combine(CnfPath, "stardict.db");

    public static readonly string LogPath = $"{ExecutePath}logs";

    public static readonly string PaddleOcrModelPath = $@"{ExecutePath}inference\";

    public static readonly List<string> PaddleOcrDlls =
    [
        "common.dll",
        "libiomp5md.dll",
        "mkldnn.dll",
        "mklml.dll",
        "opencv_world470.dll",
        "PaddleOCR.dll",
        "paddle_inference.dll",
        "tbb12.dll",
        "tbbmalloc.dll",
        "tbbmalloc_proxy.dll",
        "vcruntime140.dll",
        "vcruntime140_1.dll"
    ];
    #endregion

    #region Theme

    private const string ThemeLight = "pack://application:,,,/STranslate.Style;component/Styles/Themes/ColorLight.xaml";
    private const string ThemeDark = "pack://application:,,,/STranslate.Style;component/Styles/Themes/ColorDark.xaml";

    public const RegistryHive ThemeRegistryHive = RegistryHive.CurrentUser;
    public const string ThemeRegistry = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    public const string SystemThemeRegistryKey = "SystemUsesLightTheme";
    public const string AppThemeRegistryKey = "AppsUseLightTheme";
    public static readonly Uri LightUri = new(ThemeLight);
    public static readonly Uri DarkUri = new(ThemeDark);

    #endregion

    #region Resources

    public const string Icon = "pack://application:,,,/STranslate.Style;component/Resources/favicon.ico";
    public const string IconForbidden = "pack://application:,,,/STranslate.Style;component/Resources/forbidden.ico";

    public const string WindowResourcePath =
        "pack://application:,,,/STranslate.Style;component/Styles/WindowStyle.xaml";

    public const string WindowResourceName = "WindowStyle";

    public const string TopmostContent = "\xe637";
    public const string UnTopmostContent = "\xe9ba";

    public const string MaximizeContent = "\xe651";
    public const string MaximizeBackContent = "\xe693";

    public const string ShowIcon = "\xe608";
    public const string HideIcon = "\xe725";

    public const string TagTrue = "True";
    public const string TagFalse = "False";

    public const string Loading = "加载中...";
    public const string Unloading = "加载结束...";

    public const string UserDefineFontKey = "UserFont";
    public const string DefaultFontName = "LXGW WenKai";
    public const string PingFangFontName = "PingFang SC";

    public const string FontSize18 = "FontSize18";
    public const string FontSize18TextBox = "FontSize18TextBox";   //输入框文字大小可独立调整

    public static readonly List<Tuple<string, double>> GlobalFontSizeList =
    [
        new Tuple<string, double>("FontSize30", 30),
        new Tuple<string, double>("FontSize24", 24),
        new Tuple<string, double>("FontSize22", 22),
        new Tuple<string, double>("FontSize21", 21),
        new Tuple<string, double>("FontSize20", 20),
        new Tuple<string, double>("FontSize19", 19),
        new Tuple<string, double>(FontSize18, 18),
        new Tuple<string, double>("FontSize17", 17),
        new Tuple<string, double>("FontSize16", 16),
        new Tuple<string, double>("FontSize14", 14),
        new Tuple<string, double>("FontSize12", 12),
        new Tuple<string, double>("FontSize10", 10),
        new Tuple<string, double>(FontSize18TextBox, 18),
    ];

    public const string PlaceHolderContent = "Enter 翻译/缓存\nCtrl+Enter 强制翻译\nShift+Enter 换行";
    public const string InputErrorContent = "该服务未获取到缓存Ctrl+Enter更新";
    public const string HistoryErrorContent = "该服务翻译时未正确返回Ctrl+Enter以更新";

    public static readonly Dictionary<IconType, string> IconDict =
        Application.Current.Resources.MergedDictionaries.FirstOrDefault(x =>
                x.Source == new Uri("pack://application:,,,/STranslate.Style;component/Styles/IconStyle.xaml",
                    UriKind.Absolute)
            )!
            .OfType<DictionaryEntry>()
            .ToDictionary(entry => (IconType)Enum.Parse(typeof(IconType), entry.Key.ToString() ?? "STranslate"),
                entry => entry.Value!.ToString() ?? Icon);

    #endregion

    #region Hotkeys

    public const string DefaultInputHotkey = "Alt + A";
    public const string DefaultCrosswordHotkey = "Alt + D";
    public const string DefaultScreenshotHotkey = "Alt + S";
    public const string DefaultOpenHotkey = "Alt + G";
    public const string DefaultReplaceHotkey = "Alt + F";
    public const string DefaultMouseHookHotkey = "Alt + Shift + D";
    public const string DefaultOcrHotkey = "Alt + Shift + S";
    public const string DefaultSilentOcrHotkey = "Alt + Shift + F";
    public const string DefaultSilentTtsHotkey = "Alt + Shift + G";
    public const string DefaultClipboardMonitorHotkey = "Alt + Shift + A";

    #endregion
}