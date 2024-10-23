using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference;

public partial class HotkeyViewModel : ObservableObject
{
    private readonly ConfigHelper _conf = Singleton<ConfigHelper>.Instance;
    private KeyCodes _hotkeysKey;
    private KeyModifiers _hotkeysModifiers;
    private string _hotkeysText = string.Empty;

    public HotkeyViewModel()
    {
        InputHk.Content = _conf.CurrentConfig?.Hotkeys?.InputTranslate?.Text ?? "";
        CrosswordHk.Content = _conf.CurrentConfig?.Hotkeys?.CrosswordTranslate?.Text ?? "";
        ScreenshotHk.Content = _conf.CurrentConfig?.Hotkeys?.ScreenShotTranslate?.Text ?? "";
        OpenHk.Content = _conf.CurrentConfig?.Hotkeys?.OpenMainWindow?.Text ?? "";
        ReplaceHk.Content = _conf.CurrentConfig?.Hotkeys?.ReplaceTranslate?.Text ?? "";
        MousehookHk.Content = _conf.CurrentConfig?.Hotkeys?.MousehookTranslate?.Text ?? "";
        OcrHk.Content = _conf.CurrentConfig?.Hotkeys?.OCR?.Text ?? "";
        SilentOcrHk.Content = _conf.CurrentConfig?.Hotkeys?.SilentOCR?.Text ?? "";
        ClipboardMonitorHk.Content = _conf.CurrentConfig?.Hotkeys?.ClipboardMonitor?.Text ?? "";
        HotKeyConflictCheck();
        HotkeyHelper.OnUpdateConflict += HotKeyConflictCheck;
    }

    #region Property

    public List<SoftHotkey> SoftHotkeyList { get; set; } =
    [
        new SoftHotkey("ESC", "隐藏/退出界面(若有请求则同时取消该请求: 翻译、OCR、TTS)"),
        new SoftHotkey("Ctrl + ,", "打开设置"),
        new SoftHotkey("Ctrl + /", "打开历史记录"),
        new SoftHotkey("Ctrl + B", "打开/关闭自动翻译"),
        new SoftHotkey("Ctrl + E", "打开/关闭增量翻译"),
        new SoftHotkey("Ctrl + Shift + A", "隐藏/显示输入框"),
        new SoftHotkey("Ctrl + Shift + M", "重置窗口至主显示器中央"),
        new SoftHotkey("Ctrl + Shift + T", "置顶/取消置顶"),
        new SoftHotkey("Ctrl + Shift + R", "切换主题(自动/明亮/黑暗主题切换)"),
        new SoftHotkey("Ctrl + Shift + S", "保存至生词本"),
        new SoftHotkey("Ctrl + Shift + Q", "退出程序"),
        new SoftHotkey("Ctrl + 滚轮上", "在输入输出文本框配合Ctrl滚动放大文字"),
        new SoftHotkey("Ctrl + 滚轮下", "在输入输出文本框配合Ctrl滚动缩小文字"),
        new SoftHotkey("Ctrl + `", "文本框恢复默认文字大小"),
        new SoftHotkey("Ctrl + 1...8", "按服务顺序复制翻译结果"),
        new SoftHotkey("Ctrl + 9", "复制最后一条服务翻译结果"),
        new SoftHotkey("Alt + `", "语音播报输入内容"),
        new SoftHotkey("Alt + 1...8", "按服务顺序语音播报翻译结果"),
        new SoftHotkey("Alt + 9", "语音播报最后一条服务翻译结果"),
        new SoftHotkey("Ctrl + +", "放大界面(宽度、最大高度)"),
        new SoftHotkey("Ctrl + -", "缩小界面(宽度、最大高度)"),
        new SoftHotkey("Ctrl + 0", "恢复默认界面(宽度、最大高度)"),
        new SoftHotkey("Ctrl + Alt + +", "宽度增加"),
        new SoftHotkey("Ctrl + Alt + -", "宽度减少"),
        new SoftHotkey("Ctrl + Shift + +", "最大高度增加"),
        new SoftHotkey("Ctrl + Shift + -", "最大高度减少"),
        new SoftHotkey("Ctrl + [", "全局字体减小"),
        new SoftHotkey("Ctrl + ]", "全局字体增大"),
        new SoftHotkey("Ctrl + '", "全局字体恢复默认"),
        new SoftHotkey("Ctrl + 换行", "打开/关闭始终换行"),
        new SoftHotkey("Alt + 换行", "打开/关闭OCR净化"),
        new SoftHotkey("Alt + 回译", "打开/关闭回译功能"),
    ];

    [ObservableProperty] private HotkeyContentVisibilityModel _inputHk = new();

    [ObservableProperty] private HotkeyContentVisibilityModel _crosswordHk = new();

    [ObservableProperty] private HotkeyContentVisibilityModel _screenshotHk = new();

    [ObservableProperty] private HotkeyContentVisibilityModel _openHk = new();

    [ObservableProperty] private HotkeyContentVisibilityModel _replaceHk = new();

    [ObservableProperty] private HotkeyContentVisibilityModel _mousehookHk = new();

    [ObservableProperty] private HotkeyContentVisibilityModel _ocrHk = new();

    [ObservableProperty] private HotkeyContentVisibilityModel _silentOcrHk = new();

    [ObservableProperty] private HotkeyContentVisibilityModel _clipboardMonitorHk = new();

    #endregion Property

    #region Command

    [RelayCommand]
    private void Save()
    {
        if (!_conf.WriteConfig(_conf.CurrentConfig!.Hotkeys!))
        {
            LogService.Logger.Debug($"保存全局热键配置失败，{JsonConvert.SerializeObject(_conf.CurrentConfig.Hotkeys)}");

            ToastHelper.Show("保存热键失败", WindowType.Preference);
        }
        else
        {
            RefreshNotifyToolTip();
            ToastHelper.Show("保存热键成功", WindowType.Preference);
        }
    }

    [RelayCommand]
    private void Reset()
    {
        var cHotkeys = _conf.ResetConfig.Hotkeys!;

        InputHk.Content = cHotkeys.InputTranslate.Text ?? "";
        CrosswordHk.Content = cHotkeys.CrosswordTranslate.Text ?? "";
        ScreenshotHk.Content = cHotkeys.ScreenShotTranslate.Text ?? "";
        OpenHk.Content = cHotkeys.OpenMainWindow.Text ?? "";
        ReplaceHk.Content = cHotkeys.ReplaceTranslate.Text ?? "";
        MousehookHk.Content = cHotkeys.MousehookTranslate.Text ?? "";
        OcrHk.Content = cHotkeys.OCR.Text ?? "";
        SilentOcrHk.Content = cHotkeys.SilentOCR.Text ?? "";
        ClipboardMonitorHk.Content = cHotkeys.ClipboardMonitor.Text ?? "";

        _conf.CurrentConfig!.Hotkeys!.InputTranslate.Modifiers = cHotkeys.InputTranslate.Modifiers;
        _conf.CurrentConfig!.Hotkeys!.InputTranslate.Key = cHotkeys.InputTranslate.Key;
        _conf.CurrentConfig!.Hotkeys!.InputTranslate.Text = cHotkeys.InputTranslate.Text;

        _conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Modifiers = cHotkeys.CrosswordTranslate.Modifiers;
        _conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Key = cHotkeys.CrosswordTranslate.Key;
        _conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Text = cHotkeys.CrosswordTranslate.Text;

        _conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Modifiers = cHotkeys.ScreenShotTranslate.Modifiers;
        _conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Key = cHotkeys.ScreenShotTranslate.Key;
        _conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Text = cHotkeys.ScreenShotTranslate.Text;

        _conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Modifiers = cHotkeys.OpenMainWindow.Modifiers;
        _conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Key = cHotkeys.OpenMainWindow.Key;
        _conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Text = cHotkeys.OpenMainWindow.Text;

        _conf.CurrentConfig!.Hotkeys!.ReplaceTranslate.Modifiers = cHotkeys.ReplaceTranslate.Modifiers;
        _conf.CurrentConfig!.Hotkeys!.ReplaceTranslate.Key = cHotkeys.ReplaceTranslate.Key;
        _conf.CurrentConfig!.Hotkeys!.ReplaceTranslate.Text = cHotkeys.ReplaceTranslate.Text;

        _conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Modifiers = cHotkeys.MousehookTranslate.Modifiers;
        _conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Key = cHotkeys.MousehookTranslate.Key;
        _conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Text = cHotkeys.MousehookTranslate.Text;

        _conf.CurrentConfig!.Hotkeys!.OCR.Modifiers = cHotkeys.OCR.Modifiers;
        _conf.CurrentConfig!.Hotkeys!.OCR.Key = cHotkeys.OCR.Key;
        _conf.CurrentConfig!.Hotkeys!.OCR.Text = cHotkeys.OCR.Text;

        _conf.CurrentConfig!.Hotkeys!.SilentOCR.Modifiers = cHotkeys.SilentOCR.Modifiers;
        _conf.CurrentConfig!.Hotkeys!.SilentOCR.Key = cHotkeys.SilentOCR.Key;
        _conf.CurrentConfig!.Hotkeys!.SilentOCR.Text = cHotkeys.SilentOCR.Text;

        _conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Modifiers = cHotkeys.ClipboardMonitor.Modifiers;
        _conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Key = cHotkeys.ClipboardMonitor.Key;
        _conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Text = cHotkeys.ClipboardMonitor.Text;

        HotkeyHelper.Hotkeys!.InputTranslate.Modifiers = cHotkeys.InputTranslate.Modifiers;
        HotkeyHelper.Hotkeys!.InputTranslate.Key = cHotkeys.InputTranslate.Key;
        HotkeyHelper.Hotkeys!.InputTranslate.Text = cHotkeys.InputTranslate.Text;

        HotkeyHelper.Hotkeys!.CrosswordTranslate.Modifiers = cHotkeys.CrosswordTranslate.Modifiers;
        HotkeyHelper.Hotkeys!.CrosswordTranslate.Key = cHotkeys.CrosswordTranslate.Key;
        HotkeyHelper.Hotkeys!.CrosswordTranslate.Text = cHotkeys.CrosswordTranslate.Text;

        HotkeyHelper.Hotkeys!.ScreenShotTranslate.Modifiers = cHotkeys.ScreenShotTranslate.Modifiers;
        HotkeyHelper.Hotkeys!.ScreenShotTranslate.Key = cHotkeys.ScreenShotTranslate.Key;
        HotkeyHelper.Hotkeys!.ScreenShotTranslate.Text = cHotkeys.ScreenShotTranslate.Text;

        HotkeyHelper.Hotkeys!.OpenMainWindow.Modifiers = cHotkeys.OpenMainWindow.Modifiers;
        HotkeyHelper.Hotkeys!.OpenMainWindow.Key = cHotkeys.OpenMainWindow.Key;
        HotkeyHelper.Hotkeys!.OpenMainWindow.Text = cHotkeys.OpenMainWindow.Text;

        HotkeyHelper.Hotkeys!.ReplaceTranslate.Modifiers = cHotkeys.ReplaceTranslate.Modifiers;
        HotkeyHelper.Hotkeys!.ReplaceTranslate.Key = cHotkeys.ReplaceTranslate.Key;
        HotkeyHelper.Hotkeys!.ReplaceTranslate.Text = cHotkeys.ReplaceTranslate.Text;

        HotkeyHelper.Hotkeys!.MousehookTranslate.Modifiers = cHotkeys.MousehookTranslate.Modifiers;
        HotkeyHelper.Hotkeys!.MousehookTranslate.Key = cHotkeys.MousehookTranslate.Key;
        HotkeyHelper.Hotkeys!.MousehookTranslate.Text = cHotkeys.MousehookTranslate.Text;

        HotkeyHelper.Hotkeys!.OCR.Modifiers = cHotkeys.OCR.Modifiers;
        HotkeyHelper.Hotkeys!.OCR.Key = cHotkeys.OCR.Key;
        HotkeyHelper.Hotkeys!.OCR.Text = cHotkeys.OCR.Text;

        HotkeyHelper.Hotkeys!.SilentOCR.Modifiers = cHotkeys.SilentOCR.Modifiers;
        HotkeyHelper.Hotkeys!.SilentOCR.Key = cHotkeys.SilentOCR.Key;
        HotkeyHelper.Hotkeys!.SilentOCR.Text = cHotkeys.SilentOCR.Text;

        HotkeyHelper.Hotkeys!.ClipboardMonitor.Modifiers = cHotkeys.ClipboardMonitor.Modifiers;
        HotkeyHelper.Hotkeys!.ClipboardMonitor.Key = cHotkeys.ClipboardMonitor.Key;
        HotkeyHelper.Hotkeys!.ClipboardMonitor.Text = cHotkeys.ClipboardMonitor.Text;

        HotkeyHelper.ReRegisterHotKey();
        HotKeyConflictCheck();
        RefreshNotifyToolTip();

        ToastHelper.Show("重置成功", WindowType.Preference);
    }

    [RelayCommand]
    private void Keyup(UserdefineKeyArgsModel model)
    {
        var e = model.KeyEventArgs!;
        var hke = (HotkeyEnum)model.Obj!;

        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (
            key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin
        )
            return;
        UpdateCnf(hke);
        HotkeyHelper.ReRegisterHotKey();
        HotKeyConflictCheck();
        RefreshNotifyToolTip();
    }

    [RelayCommand]
    public void Keydown(UserdefineKeyArgsModel model)
    {
        var e = model.KeyEventArgs!;
        var control = (TextBox)model.Obj!;

        e.Handled = true;
        _hotkeysModifiers = KeyModifiers.MOD_NONE;
        _hotkeysKey = KeyCodes.None;
        _hotkeysText = string.Empty;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (
            key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin
        )
            return;
        StringBuilder shortcutText = new();
        if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
        {
            _hotkeysModifiers += 2;
            shortcutText.Append("Ctrl + ");
        }

        if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
        {
            _hotkeysModifiers += 1;
            shortcutText.Append("Alt + ");
        }

        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
        {
            _hotkeysModifiers += 4;
            shortcutText.Append("Shift + ");
        }

        //backspace and delete 
        if (_hotkeysModifiers == 0 && (key == Key.Back || key == Key.Delete))
        {
            _hotkeysKey = 0;
            shortcutText.Clear();
            control.Text = _hotkeysText = shortcutText.ToString();
            return;
        }

        if (_hotkeysModifiers == 0 && (key < Key.F1 || key > Key.F12))
            ToastHelper.Show("单字符可能会影响使用", WindowType.Preference);

        _hotkeysKey = (KeyCodes)KeyInterop.VirtualKeyFromKey(key);
        shortcutText.Append(key.ToString());
        control.Text = _hotkeysText = shortcutText.ToString();
    }

    #endregion Command

    #region 私有方法

    private void UpdateCnf(HotkeyEnum hke)
    {
        switch (hke)
        {
            case HotkeyEnum.InputHk:
                _conf.CurrentConfig!.Hotkeys!.InputTranslate.Modifiers = _hotkeysModifiers;
                _conf.CurrentConfig!.Hotkeys!.InputTranslate.Key = _hotkeysKey;
                _conf.CurrentConfig!.Hotkeys!.InputTranslate.Text = _hotkeysText;
                break;

            case HotkeyEnum.CrosswordHk:
                _conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Modifiers = _hotkeysModifiers;
                _conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Key = _hotkeysKey;
                _conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Text = _hotkeysText;
                break;

            case HotkeyEnum.ScreenshotHk:
                _conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Modifiers = _hotkeysModifiers;
                _conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Key = _hotkeysKey;
                _conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Text = _hotkeysText;
                break;

            case HotkeyEnum.OpenHk:
                _conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Modifiers = _hotkeysModifiers;
                _conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Key = _hotkeysKey;
                _conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Text = _hotkeysText;
                break;

            case HotkeyEnum.ReplaceHk:
                _conf.CurrentConfig!.Hotkeys!.ReplaceTranslate.Modifiers = _hotkeysModifiers;
                _conf.CurrentConfig!.Hotkeys!.ReplaceTranslate.Key = _hotkeysKey;
                _conf.CurrentConfig!.Hotkeys!.ReplaceTranslate.Text = _hotkeysText;
                break;

            case HotkeyEnum.MousehookHk:
                _conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Modifiers = _hotkeysModifiers;
                _conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Key = _hotkeysKey;
                _conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Text = _hotkeysText;
                break;

            case HotkeyEnum.OcrHk:
                _conf.CurrentConfig!.Hotkeys!.OCR.Modifiers = _hotkeysModifiers;
                _conf.CurrentConfig!.Hotkeys!.OCR.Key = _hotkeysKey;
                _conf.CurrentConfig!.Hotkeys!.OCR.Text = _hotkeysText;
                break;

            case HotkeyEnum.SilentOcrHk:
                _conf.CurrentConfig!.Hotkeys!.SilentOCR.Modifiers = _hotkeysModifiers;
                _conf.CurrentConfig!.Hotkeys!.SilentOCR.Key = _hotkeysKey;
                _conf.CurrentConfig!.Hotkeys!.SilentOCR.Text = _hotkeysText;
                break;

            case HotkeyEnum.ClipboardMonitorHk:
                _conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Modifiers = _hotkeysModifiers;
                _conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Key = _hotkeysKey;
                _conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Text = _hotkeysText;
                break;
        }
    }

    private void HotKeyConflictCheck()
    {
        InputHk.ContentVisible = _conf.CurrentConfig!.Hotkeys!.InputTranslate.Conflict;
        CrosswordHk.ContentVisible = _conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Conflict;
        ScreenshotHk.ContentVisible = _conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Conflict;
        OpenHk.ContentVisible = _conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Conflict;
        ReplaceHk.ContentVisible = _conf.CurrentConfig!.Hotkeys!.ReplaceTranslate.Conflict;
        MousehookHk.ContentVisible = _conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Conflict;
        OcrHk.ContentVisible = _conf.CurrentConfig!.Hotkeys!.OCR.Conflict;
        SilentOcrHk.ContentVisible = _conf.CurrentConfig!.Hotkeys!.SilentOCR.Conflict;
        ClipboardMonitorHk.ContentVisible = _conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Conflict;
    }

    private void RefreshNotifyToolTip()
    {
        var msg = "";
        if (!_conf.CurrentConfig!.Hotkeys!.InputTranslate.Conflict &&
            !string.IsNullOrEmpty(_conf.CurrentConfig!.Hotkeys!.InputTranslate.Text))
            msg += $"输入: {_conf.CurrentConfig!.Hotkeys!.InputTranslate.Text}\n";
        if (!_conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Conflict &&
            !string.IsNullOrEmpty(_conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Text))
            msg += $"划词: {_conf.CurrentConfig!.Hotkeys!.CrosswordTranslate.Text}\n";
        if (!_conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Conflict &&
            !string.IsNullOrEmpty(_conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Text))
            msg += $"截图: {_conf.CurrentConfig!.Hotkeys!.ScreenShotTranslate.Text}\n";
        if (!_conf.CurrentConfig!.Hotkeys!.ReplaceTranslate.Conflict &&
            !string.IsNullOrEmpty(_conf.CurrentConfig!.Hotkeys!.ReplaceTranslate.Text))
            msg += $"替换: {_conf.CurrentConfig!.Hotkeys!.ReplaceTranslate.Text}\n";
        if (!_conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Conflict &&
            !string.IsNullOrEmpty(_conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Text))
            msg += $"显示: {_conf.CurrentConfig!.Hotkeys!.OpenMainWindow.Text}\n";
        if (!_conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Conflict &&
            !string.IsNullOrEmpty(_conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Text))
            msg += $"鼠标: {_conf.CurrentConfig!.Hotkeys!.MousehookTranslate.Text}\n";
        if (!_conf.CurrentConfig!.Hotkeys!.OCR.Conflict &&
            !string.IsNullOrEmpty(_conf.CurrentConfig!.Hotkeys!.OCR.Text))
            msg += $"识字: {_conf.CurrentConfig!.Hotkeys!.OCR.Text}\n";
        if (!_conf.CurrentConfig!.Hotkeys!.SilentOCR.Conflict &&
            !string.IsNullOrEmpty(_conf.CurrentConfig!.Hotkeys!.SilentOCR.Text))
            msg += $"静默: {_conf.CurrentConfig!.Hotkeys!.SilentOCR.Text}\n";
        if (!_conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Conflict &&
            !string.IsNullOrEmpty(_conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Text))
            msg += $"剪贴板: {_conf.CurrentConfig!.Hotkeys!.ClipboardMonitor.Text}\n";
        Singleton<NotifyIconViewModel>.Instance.UpdateToolTip(msg.TrimEnd(['\r', '\n']));
    }

    #endregion 私有方法
}

public record SoftHotkey(string Hotkey, string Name);