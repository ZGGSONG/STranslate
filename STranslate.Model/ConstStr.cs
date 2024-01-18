
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

        public const string USERDEFINEFONTKEY = "UserFont";
        public const string DEFAULTFONTNAME = "LXGW WenKai";
    }
}