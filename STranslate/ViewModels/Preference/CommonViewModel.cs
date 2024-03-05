using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace STranslate.ViewModels.Preference
{
    public partial class CommonViewModel : ObservableObject
    {
        public Action<int>? OnViewMaxHeightChanged;
        public Action<int>? OnViewWidthChanged;

        [RelayCommand]
        private void Save()
        {
            if (configHelper.WriteConfig(this))
            {
                ToastHelper.Show("保存常规配置成功", WindowType.Preference);

                if (IsStartup)
                {
                    if (!ShortcutUtil.IsStartup())
                        ShortcutUtil.SetStartup();
                }
                else
                {
                    ShortcutUtil.UnSetStartup();
                }
            }
            else
            {
                LogService.Logger.Debug($"保存常规配置失败，{JsonConvert.SerializeObject(this)}");
                ToastHelper.Show("保存常规配置失败", WindowType.Preference);
            }
        }

        [RelayCommand]
        private void Reset()
        {
            IsStartup = curConfig?.IsStartup ?? false;
            NeedAdmin = curConfig?.NeedAdministrator ?? false;
            HistorySize = curConfig?.HistorySize ?? 100;
            AutoScale = curConfig?.AutoScale ?? 0.8;
            ThemeType = curConfig?.ThemeType ?? ThemeType.Light;
            IsFollowMouse = curConfig?.IsFollowMouse ?? false;
            CloseUIOcrRetTranslate = curConfig?.CloseUIOcrRetTranslate ?? false;
            UnconventionalScreen = curConfig?.UnconventionalScreen ?? false;
            IsOcrAutoCopyText = curConfig?.IsOcrAutoCopyText ?? false;
            IsAdjustContentTranslate = curConfig?.IsAdjustContentTranslate ?? false;
            IsRemoveLineBreakGettingWords = curConfig?.IsRemoveLineBreakGettingWords ?? false;
            DoubleTapTrayFunc = curConfig?.DoubleTapTrayFunc ?? DoubleTapFuncEnum.InputFunc;
            CustomFont = curConfig?.CustomFont ?? ConstStr.DEFAULTFONTNAME;
            IsKeepTopmostAfterMousehook = curConfig?.IsKeepTopmostAfterMousehook ?? false;
            IsShowPreference = curConfig?.IsShowPreference ?? false;
            IsShowMousehook = curConfig?.IsShowMousehook ?? false;
            IsShowScreenshot = curConfig?.IsShowScreenshot ?? false;
            IsShowOCR = curConfig?.IsShowOCR ?? false;
            IsShowSilentOCR = curConfig?.IsShowSilentOCR ?? false;
            IsShowClipboardMonitor = curConfig?.IsShowClipboardMonitor ?? false;
            IsShowQRCode = curConfig?.IsShowQRCode ?? false;
            IsShowHistory = curConfig?.IsShowHistory ?? false;
            WordPickingInterval = curConfig?.WordPickingInterval ?? 200;
            IsHideOnStart = curConfig?.IsHideOnStart ?? false;
            ShowCopyOnHeader = curConfig?.ShowCopyOnHeader ?? false;
            IsCaretLast = curConfig?.IsCaretLast ?? false;
            MaxHeight = curConfig?.MaxHeight ?? MaxHeight.Maximum;
            Width = curConfig?.Width ?? WidthEnum.Minimum;
            ProxyMethod = curConfig?.ProxyMethod ?? ProxyMethodEnum.系统代理;
            ProxyIp = curConfig?.ProxyIp ?? string.Empty;
            ProxyPort = curConfig?.ProxyPort ?? 0;
            ProxyUsername = curConfig?.ProxyUsername ?? string.Empty;
            ProxyPassword = curConfig?.ProxyPassword ?? string.Empty;

            LoadHistorySizeType();
            ToastHelper.Show("重置配置", WindowType.Preference);
            if (IsStartup)
            {
                if (!ShortcutUtil.IsStartup())
                    ShortcutUtil.SetStartup();
            }
            else
            {
                ShortcutUtil.UnSetStartup();
            }
        }

        public CommonViewModel()
        {
            // 获取系统已安装字体
            GetFontFamilys = Fonts.SystemFontFamilies.Select(font => font.Source).ToList();
            // 判断是否已安装软件字体，没有则插入到列表中
            if (!GetFontFamilys.Contains(ConstStr.DEFAULTFONTNAME))
            {
                GetFontFamilys.Insert(0, ConstStr.DEFAULTFONTNAME);
            }

            // 加载最大高度集合
            MaxHeightList = EnumExtensions.GetEnumList<MaxHeight>();
            WidthList = EnumExtensions.GetEnumList<WidthEnum>();

            // 加载主题集合
            ThemeList = EnumExtensions.GetEnumList<ThemeType>();

            // 代理方式集合
            ProxyMethodList = EnumExtensions.GetEnumList<ProxyMethodEnum>();

            // 加载历史记录类型
            LoadHistorySizeType();
        }

        /// <summary>
        /// 触发最大高度信息到View
        /// </summary>
        public void TriggerMaxHeight()
        {
            var workAreaHeight = Convert.ToInt32(SystemParameters.WorkArea.Height);
            // 只要设定最大高度超过工作区高度,则设定最大高度为工作区高度
            var height = MaxHeight.GetHashCode() > workAreaHeight ? workAreaHeight : MaxHeight.ToInt();
            OnViewMaxHeightChanged?.Invoke(height);
        }

        /// <summary>
        /// 触发最大宽度信息到View
        /// </summary>
        public void TriggerWidth()
        {
            var workAreaWidth = Convert.ToInt32(SystemParameters.WorkArea.Width);
            // 只要设定最大高度超过工作区高度,则设定最大高度为工作区高度
            var width = Width.GetHashCode() > workAreaWidth ? workAreaWidth : Width.ToInt();
            OnViewWidthChanged?.Invoke(width);
        }

        private void LoadHistorySizeType()
        {
            HistorySizeType = HistorySize switch
            {
                50 => 0,
                100 => 1,
                200 => 2,
                500 => 3,
                1000 => 4,
                long.MaxValue => 5,
                0 => 6,
                _ => 1
            };
        }

        private static ConfigHelper configHelper = Singleton<ConfigHelper>.Instance;
        private static ConfigModel? curConfig = configHelper.CurrentConfig;

        [ObservableProperty]
        private bool isStartup = curConfig?.IsStartup ?? false;

        [ObservableProperty]
        private bool needAdmin = curConfig?.NeedAdministrator ?? false;

        private long historySizeType = 1;

        public long HistorySizeType
        {
            get => historySizeType;
            set
            {
                if (historySizeType != value)
                {
                    OnPropertyChanging(nameof(HistorySizeType));
                    historySizeType = value;

                    HistorySize = value switch
                    {
                        0 => 50,
                        1 => 100,
                        2 => 200,
                        3 => 500,
                        4 => 1000,
                        5 => long.MaxValue,
                        6 => 0,
                        _ => 100
                    };

                    OnPropertyChanged(nameof(HistorySizeType));
                }
            }
        }

        public long HistorySize = curConfig?.HistorySize ?? 100;

        [ObservableProperty]
        private double autoScale = curConfig?.AutoScale ?? 0.8;

        public List<ThemeType> ThemeList { get; set; }

        [ObservableProperty]
        private ThemeType themeType = curConfig?.ThemeType ?? ThemeType.Light;

        [ObservableProperty]
        private bool isFollowMouse = curConfig?.IsFollowMouse ?? false;

        [ObservableProperty]
        private bool closeUIOcrRetTranslate = curConfig?.CloseUIOcrRetTranslate ?? false;

        [ObservableProperty]
        private bool unconventionalScreen = curConfig?.UnconventionalScreen ?? false;

        [ObservableProperty]
        private bool isOcrAutoCopyText = curConfig?.IsOcrAutoCopyText ?? false;

        [ObservableProperty]
        private bool isAdjustContentTranslate = curConfig?.IsAdjustContentTranslate ?? false;

        [ObservableProperty]
        private bool isRemoveLineBreakGettingWords = curConfig?.IsRemoveLineBreakGettingWords ?? false;

        public Dictionary<string, DoubleTapFuncEnum> FuncDict
        {
            get => CommonUtil.GetEnumList<DoubleTapFuncEnum>();
        }

        [ObservableProperty]
        private DoubleTapFuncEnum doubleTapTrayFunc = curConfig?.DoubleTapTrayFunc ?? DoubleTapFuncEnum.InputFunc;

        [ObservableProperty]
        private List<string> _getFontFamilys;

        private string _customFont = curConfig?.CustomFont ?? ConstStr.DEFAULTFONTNAME;

        public string CustomFont
        {
            get => _customFont;
            set
            {
                if (_customFont != value)
                {
                    OnPropertyChanging(nameof(CustomFont));

                    try
                    {
                        // 切换字体
                        Application.Current.Resources[ConstStr.USERDEFINEFONTKEY] = value.Equals(ConstStr.DEFAULTFONTNAME)
                            ? Application.Current.Resources[ConstStr.DEFAULTFONTNAME]
                            : new FontFamily(value);
                        _customFont = value;
                    }
                    catch (Exception)
                    {
                        Application.Current.Resources[ConstStr.USERDEFINEFONTKEY] = Application.Current.Resources[ConstStr.DEFAULTFONTNAME];
                        _customFont = ConstStr.DEFAULTFONTNAME;
                    }

                    OnPropertyChanged(nameof(CustomFont));
                }
            }
        }

        [ObservableProperty]
        private bool isKeepTopmostAfterMousehook = curConfig?.IsKeepTopmostAfterMousehook ?? false;

        /// <summary>
        /// 是否显示设置图标
        /// </summary>
        [ObservableProperty]
        private bool isShowPreference = curConfig?.IsShowPreference ?? false;

        /// <summary>
        /// 是否显示打开鼠标划词图标
        /// </summary>
        [ObservableProperty]
        private bool isShowMousehook = curConfig?.IsShowMousehook ?? false;

        /// <summary>
        /// 是否显示截图翻译图标
        /// </summary>
        [ObservableProperty]
        private bool isShowScreenshot = curConfig?.IsShowScreenshot ?? false;

        /// <summary>
        /// 是否显示OCR图标
        /// </summary>
        [ObservableProperty]
        private bool isShowOCR = curConfig?.IsShowOCR ?? false;

        /// <summary>
        /// 是否显示静默OCR图标
        /// </summary>
        [ObservableProperty]
        private bool isShowSilentOCR = curConfig?.IsShowSilentOCR ?? false;

        /// <summary>
        /// 是否显示监听剪贴板
        /// </summary>
        [ObservableProperty]
        private bool isShowClipboardMonitor = curConfig?.IsShowClipboardMonitor ?? false;

        /// <summary>
        /// 是否显示识别二维码图标
        /// </summary>
        [ObservableProperty]
        private bool isShowQRCode = curConfig?.IsShowQRCode ?? false;

        /// <summary>
        /// 是否显示历史记录图标
        /// </summary>
        [ObservableProperty]
        private bool isShowHistory = curConfig?.IsShowHistory ?? false;

        /// <summary>
        /// 取词时间间隔
        /// </summary>
        [ObservableProperty]
        private int wordPickingInterval = curConfig?.WordPickingInterval ?? 100;

        /// <summary>
        /// 启动时隐藏主界面
        /// </summary>
        [ObservableProperty]
        private bool isHideOnStart = curConfig?.IsHideOnStart ?? false;

        /// <summary>
        /// 收缩框是否显示复制按钮
        /// </summary>
        [ObservableProperty]
        private bool showCopyOnHeader = curConfig?.ShowCopyOnHeader ?? false;

        /// <summary>
        /// 激活窗口时光标移动至末尾
        /// </summary>
        [ObservableProperty]
        private bool isCaretLast = curConfig?.IsCaretLast ?? false;

        /// <summary>
        /// View 最大高度
        /// </summary>
        private MaxHeight _maxHeight = curConfig?.MaxHeight ?? MaxHeight.Maximum;

        public MaxHeight MaxHeight
        {
            get => _maxHeight;
            set
            {
                if (_maxHeight != value)
                {
                    OnPropertyChanging();
                    _maxHeight = value;
                    TriggerMaxHeight();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// View 最大宽度
        /// </summary>
        private WidthEnum _width = curConfig?.Width ?? WidthEnum.Minimum;

        public WidthEnum Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    OnPropertyChanging();
                    _width = value;
                    TriggerWidth();
                    OnPropertyChanged();
                }
            }
        }

        public List<MaxHeight> MaxHeightList { get; set; }

        public List<WidthEnum> WidthList { get; set; }

        [ObservableProperty]
        private List<ProxyMethodEnum> _proxyMethodList;

        [ObservableProperty]
        private ProxyMethodEnum _proxyMethod = curConfig?.ProxyMethod ?? ProxyMethodEnum.系统代理;

        [ObservableProperty]
        private string _proxyIp = curConfig?.ProxyIp ?? string.Empty;

        [ObservableProperty]
        private int? _proxyPort = curConfig?.ProxyPort;

        [ObservableProperty]
        private bool _isProxyAuthentication = curConfig?.IsProxyAuthentication ?? false;

        [ObservableProperty]
        private string _proxyUsername = curConfig?.ProxyUsername ?? string.Empty;

        [ObservableProperty]
        private string _proxyPassword = curConfig?.ProxyPassword ?? string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        private bool _isProxyPasswordHide = true;

        private RelayCommand<string>? showEncryptInfoCommand;

        [JsonIgnore]
        public IRelayCommand<string> ShowEncryptInfoCommand => showEncryptInfoCommand ??= new RelayCommand<string>(new Action<string?>(ShowEncryptInfo));

        private void ShowEncryptInfo(string? obj)
        {
            if (obj != null && obj.Equals(nameof(ProxyPassword)))
            {
                IsProxyPasswordHide = !IsProxyPasswordHide;
            }
        }
    }
}
