namespace STranslate.Model;

public class Hotkeys
{
    public InputTranslate InputTranslate { get; set; } = new();
    public CrosswordTranslate CrosswordTranslate { get; set; } = new();
    public ScreenShotTranslate ScreenShotTranslate { get; set; } = new();
    public OpenMainWindow OpenMainWindow { get; set; } = new();
    public MousehookTranslate MousehookTranslate { get; set; } = new();
    public OCR OCR { get; set; } = new();
    public SilentOCR SilentOCR { get; set; } = new();
    public ClipboardMonitor ClipboardMonitor { get; set; } = new();
    public ReplaceTranslate ReplaceTranslate { get; set; } = new();

    public Hotkeys Clone()
    {
        return new Hotkeys
        {
            InputTranslate = InputTranslate.Clone(),
            CrosswordTranslate = CrosswordTranslate.Clone(),
            ScreenShotTranslate = ScreenShotTranslate.Clone(),
            OpenMainWindow = OpenMainWindow.Clone(),
            MousehookTranslate = MousehookTranslate.Clone(),
            OCR = OCR.Clone(),
            SilentOCR = SilentOCR.Clone(),
            ClipboardMonitor = ClipboardMonitor.Clone(),
            ReplaceTranslate = ReplaceTranslate.Clone()
        };
    }
}

public class InputTranslate : HotkeyBase
{
}

public class CrosswordTranslate : HotkeyBase
{
}

public class ScreenShotTranslate : HotkeyBase
{
}

public class OpenMainWindow : HotkeyBase
{
}

public class MousehookTranslate : HotkeyBase
{
}

public class OCR : HotkeyBase
{
}

public class SilentOCR : HotkeyBase
{
}

public class ClipboardMonitor : HotkeyBase
{
}

public class ReplaceTranslate : HotkeyBase
{
}

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
        t.Key = key;
        t.Modifiers = modifiers;
        t.Text = text;
        t.Conflict = conflict;
        return t;
    }

    public static T Clone<T>(this T t)
        where T : HotkeyBase, new()
    {
        return new T
        {
            Modifiers = t.Modifiers,
            Key = t.Key,
            Text = t.Text,
            Conflict = t.Conflict
        };
    }
}