namespace STranslate.Model
{
    public class Hotkeys
    {
        public InputTranslate InputTranslate { get; set; } = new InputTranslate();

        public CrosswordTranslate CrosswordTranslate { get; set; } = new CrosswordTranslate();

        public ScreenShotTranslate ScreenShotTranslate { get; set; } = new ScreenShotTranslate();

        public OpenMainWindow OpenMainWindow { get; set; } = new OpenMainWindow();

        public MousehookTranslate MousehookTranslate { get; set; } = new MousehookTranslate();

        public OCR OCR { get; set; } = new OCR();
    }

    public class InputTranslate : HotkeyBase { }

    public class CrosswordTranslate : HotkeyBase { }

    public class ScreenShotTranslate : HotkeyBase { }

    public class OpenMainWindow : HotkeyBase { }

    public class MousehookTranslate : HotkeyBase { }

    public class OCR : HotkeyBase { }

    public class HotkeyBase
    {
        public KeyModifiers Modifiers { get; set; }

        public KeyCodes Key { get; set; }

        public string? Text { get; set; }

        public bool Conflict { get; set; }
    }

    public static class HotkeyExtensions
    {
        public static T Update<T>(this T t, KeyModifiers modifiers, KeyCodes key, string? text, bool conflict = false)
            where T : HotkeyBase
        {
            t.Modifiers = modifiers;
            t.Key = key;
            t.Text = text;
            t.Conflict = conflict;
            return t;
        }
    }
}
