
using Microsoft.Win32;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace STranslate.Model
{
    public static class ConstStr
    {
        public const string THEMELIGHT = "pack://application:,,,/STranslate.Style;component/Styles/Themes/ColorLight.xaml";
        public const string THEMEDARK = "pack://application:,,,/STranslate.Style;component/Styles/Themes/ColorDark.xaml";

        public static System.Uri LIGHTURI = new(THEMELIGHT);
        public static System.Uri DARKURI = new(THEMEDARK);

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

        public static string AppName = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()!.Location);
        public static string AppData = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{AppName}";
        public static string CnfName = $"{AppData}\\{AppName.ToLower()}.json";
        public static string ECDICTPath = System.IO.Path.Combine(AppData, "stardict.db");

        public static Dictionary<IconType, string> ICONDICT = System.Windows.Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(x => x.Source == new Uri("pack://application:,,,/STranslate.Style;component/Styles/IconStyle.xaml", UriKind.Absolute))
                !.OfType<System.Collections.DictionaryEntry>()
                .ToDictionary(
                    entry => (IconType)Enum.Parse(typeof(IconType), entry.Key.ToString() ?? "STranslate"),
                    entry => entry.Value!.ToString() ?? ICON
                ) ?? [];

        public const string MAINVIEWPLACEHOLDER = "Enter 翻译/缓存\nCtrl+Enter 强制翻译\nShift+Enter 换行";
    }
}