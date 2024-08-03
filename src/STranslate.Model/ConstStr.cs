using System.Collections;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace STranslate.Model;

public static class ConstStr
{
    public const string THEMELIGHT = "pack://application:,,,/STranslate.Style;component/Styles/Themes/ColorLight.xaml";
    public const string THEMEDARK = "pack://application:,,,/STranslate.Style;component/Styles/Themes/ColorDark.xaml";

    public const string WindowResourcePath =
        "pack://application:,,,/STranslate.Style;component/Styles/WindowStyle.xaml";

    public const string WindowResourceName = "WindowStyle";

    public const string ICON = "pack://application:,,,/STranslate.Style;component/Resources/favicon.ico";
    public const string ICONFORBIDDEN = "pack://application:,,,/STranslate.Style;component/Resources/forbidden.ico";

    public const string TAGTRUE = "True";
    public const string TAGFALSE = "False";

    public const string TOPMOSTCONTENT = "\xe637";
    public const string UNTOPMOSTCONTENT = "\xe9ba";

    public const string MAXIMIZECONTENT = "\xe651";
    public const string MAXIMIZEBACKCONTENT = "\xe693";

    public const string SHOWICON = "\xe608";
    public const string HIDEICON = "\xe725";

    public const string DEFAULTINPUTHOTKEY = "Alt + A";
    public const string DEFAULTCROSSWORDHOTKEY = "Alt + D";
    public const string DEFAULTSCREENSHOTHOTKEY = "Alt + S";
    public const string DEFAULTOPENHOTKEY = "Alt + G";
    public const string DEFAULTREPLACEHOTKEY = "Alt + F";
    public const string DEFAULTMOUSEHOOKHOTKEY = "Alt + Shift + D";
    public const string DEFAULTOCRHOTKEY = "Alt + Shift + S";
    public const string DEFAULTSILENTOCRHOTKEY = "Alt + Shift + F";
    public const string DEFAULTCLIPBOARDMONITORHOTKEY = "Alt + Shift + A";

    public const string USERDEFINEFONTKEY = "UserFont";
    public const string DEFAULTFONTNAME = "LXGW WenKai";

    public const string INPUTERRORCONTENT = "该服务未获取到缓存Ctrl+Enter更新";
    public const string HISTORYERRORCONTENT = "该服务翻译时未正确返回Ctrl+Enter以更新";

    public const string LOADING = "加载中...";
    public const string UNLOADING = "加载结束...";

    public const RegistryHive REGISTRYHIVE = RegistryHive.CurrentUser;
    public const string REGISTRY = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
    public const string REGISTRYKEY = "SystemUsesLightTheme";

    public const string MAINVIEWPLACEHOLDER = "Enter 翻译/缓存\nCtrl+Enter 强制翻译\nShift+Enter 换行";

    public const string CNFEXTENSION = ".json";
    public const string CNFNAME = "stranslate";

    public const string ZIP = ".zip";

    public const string GITHUBRELEASEURL = "https://api.github.com/repos/zggsong/stranslate/releases/latest";
    public const string DEFAULTVERSION = "1.0.0.0";

    public static readonly Uri LIGHTURI = new(THEMELIGHT);
    public static readonly Uri DARKURI = new(THEMEDARK);

    public static readonly string AppVersion =
        Application.ResourceAssembly.GetName().Version?.ToString() ?? DEFAULTVERSION;

    public static readonly string AppName = "STranslate";

    /// <summary>
    ///     用户软件根目录
    /// </summary>
    public static readonly string ExecutePath = AppDomain.CurrentDomain.BaseDirectory; //Environment.CurrentDirectory使用批处理时会出现问题

    /// <summary>
    ///     用户配置目录
    /// </summary>
    public static readonly string AppData =
        $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{AppName}";

    public static readonly string LogPath = $"{ExecutePath}logs";

    public static readonly string CnfFullName = $"{AppData}\\{AppName.ToLower()}.json";
    public static readonly string DbFullName = $"{AppData}\\{AppName.ToLower()}.db";
    public static readonly string DbConnectionString = $"Data Source={DbFullName}";
    public static readonly string ECDICTPath = Path.Combine(AppData, "stardict.db");


    public static readonly Dictionary<IconType, string> ICONDICT =
        Application.Current.Resources.MergedDictionaries.FirstOrDefault(x =>
                x.Source == new Uri("pack://application:,,,/STranslate.Style;component/Styles/IconStyle.xaml",
                    UriKind.Absolute)
            )!
            .OfType<DictionaryEntry>()
            .ToDictionary(entry => (IconType)Enum.Parse(typeof(IconType), entry.Key.ToString() ?? "STranslate"),
                entry => entry.Value!.ToString() ?? ICON) ?? [];

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
}