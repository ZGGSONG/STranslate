using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference
{
    public partial class CommonViewModel : ObservableObject
    {
        [RelayCommand]
        private void Save()
        {
            if (Singleton<ConfigHelper>.Instance.WriteConfig(this))
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
            IsStartup = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsStartup ?? false;
            NeedAdmin = Singleton<ConfigHelper>.Instance.CurrentConfig?.NeedAdministrator ?? false;
            HistorySize = Singleton<ConfigHelper>.Instance.CurrentConfig?.HistorySize ?? 100;
            AutoScale = Singleton<ConfigHelper>.Instance.CurrentConfig?.AutoScale ?? 0.8;
            IsBright = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsBright ?? false;
            IsFollowMouse = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsFollowMouse ?? false;
            CloseUIOcrRetTranslate = Singleton<ConfigHelper>.Instance.CurrentConfig?.CloseUIOcrRetTranslate ?? false;
            UnconventionalScreen = Singleton<ConfigHelper>.Instance.CurrentConfig?.UnconventionalScreen ?? false;
            IsOcrAutoCopyText = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsOcrAutoCopyText ?? false;
            IsAdjustContentTranslate = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsAdjustContentTranslate ?? false;
            IsRemoveLineBreakGettingWords = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false;
            DoubleTapTrayFunc = Singleton<ConfigHelper>.Instance.CurrentConfig?.DoubleTapTrayFunc ?? DoubleTapFuncEnum.InputFunc;
            CustomFont = Singleton<ConfigHelper>.Instance.CurrentConfig?.CustomFont ?? ConstStr.DEFAULTFONTNAME;
            IsKeepTopmostAfterMousehook = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsKeepTopmostAfterMousehook ?? false;
            IsShowPreference = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowPreference ?? false;
            IsShowSwitchTheme = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowSwitchTheme ?? false;
            IsShowMousehook = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowMousehook ?? false;
            IsShowScreenshot = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowScreenshot ?? false;
            IsShowOCR = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowOCR ?? false;
            IsShowSilentOCR = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowSilentOCR ?? false;
            IsShowQRCode = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowQRCode ?? false;
            WordPickingInterval = Singleton<ConfigHelper>.Instance.CurrentConfig?.WordPickingInterval ?? 200;
            IsHideOnStart = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsHideOnStart ?? false;
            ShowCopyOnHeader = Singleton<ConfigHelper>.Instance.CurrentConfig?.ShowCopyOnHeader ?? false;

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

            // 加载历史记录类型
            LoadHistorySizeType();
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
                _ => 1
            };
        }

        [ObservableProperty]
        private bool isStartup = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsStartup ?? false;

        [ObservableProperty]
        private bool needAdmin = Singleton<ConfigHelper>.Instance.CurrentConfig?.NeedAdministrator ?? false;

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
                        _ => 100
                    };

                    OnPropertyChanged(nameof(HistorySizeType));
                }
            }
        }

        public long HistorySize = Singleton<ConfigHelper>.Instance.CurrentConfig?.HistorySize ?? 100;

        [ObservableProperty]
        private double autoScale = Singleton<ConfigHelper>.Instance.CurrentConfig?.AutoScale ?? 0.8;

        private bool isBright = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsBright ?? false;

        public bool IsBright
        {
            get => isBright;
            set
            {
                if (isBright != value)
                {
                    OnPropertyChanging(nameof(IsBright));
                    isBright = value;

                    // 切换主题
                    Application.Current.Resources.MergedDictionaries.First().Source = value ? ConstStr.LIGHTURI : ConstStr.DARKURI;

                    OnPropertyChanged(nameof(IsBright));
                }
            }
        }

        [ObservableProperty]
        private bool isFollowMouse = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsFollowMouse ?? false;

        [ObservableProperty]
        private bool closeUIOcrRetTranslate = Singleton<ConfigHelper>.Instance.CurrentConfig?.CloseUIOcrRetTranslate ?? false;

        [ObservableProperty]
        private bool unconventionalScreen = Singleton<ConfigHelper>.Instance.CurrentConfig?.UnconventionalScreen ?? false;

        private bool isDisableSystemProxy = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsDisableSystemProxy ?? false;

        public bool IsDisableSystemProxy
        {
            get => isDisableSystemProxy;
            set
            {
                if (isDisableSystemProxy != value)
                {
                    OnPropertyChanging(nameof(IsDisableSystemProxy));
                    isDisableSystemProxy = value;
                    ProxyUtil.UpdateDynamicProxy(value);
                    OnPropertyChanged(nameof(IsDisableSystemProxy));
                }
            }
        }

        [ObservableProperty]
        private bool isOcrAutoCopyText = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsOcrAutoCopyText ?? false;

        [ObservableProperty]
        private bool isAdjustContentTranslate = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsAdjustContentTranslate ?? false;

        [ObservableProperty]
        private bool isRemoveLineBreakGettingWords = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsRemoveLineBreakGettingWords ?? false;

        public Dictionary<string, DoubleTapFuncEnum> FuncDict
        {
            get => CommonUtil.GetEnumList<DoubleTapFuncEnum>();
        }

        [ObservableProperty]
        private DoubleTapFuncEnum doubleTapTrayFunc = Singleton<ConfigHelper>.Instance.CurrentConfig?.DoubleTapTrayFunc ?? DoubleTapFuncEnum.InputFunc;

        [ObservableProperty]
        private List<string> _getFontFamilys;

        private string _customFont = Singleton<ConfigHelper>.Instance.CurrentConfig?.CustomFont ?? ConstStr.DEFAULTFONTNAME;

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
        private bool isKeepTopmostAfterMousehook = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsKeepTopmostAfterMousehook ?? false;

        /// <summary>
        /// 是否显示设置图标
        /// </summary>
        [ObservableProperty]
        private bool isShowPreference = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowPreference ?? false;

        /// <summary>
        /// 是否显示切换主题图标
        /// </summary>
        [ObservableProperty]
        private bool isShowSwitchTheme = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowSwitchTheme ?? false;

        /// <summary>
        /// 是否显示打开鼠标划词图标
        /// </summary>
        [ObservableProperty]
        private bool isShowMousehook = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowMousehook ?? false;

        /// <summary>
        /// 是否显示截图翻译图标
        /// </summary>
        [ObservableProperty]
        private bool isShowScreenshot = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowScreenshot ?? false;

        /// <summary>
        /// 是否显示OCR图标
        /// </summary>
        [ObservableProperty]
        private bool isShowOCR = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowOCR ?? false;

        /// <summary>
        /// 是否显示静默OCR图标
        /// </summary>
        [ObservableProperty]
        private bool isShowSilentOCR = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowSilentOCR ?? false;

        /// <summary>
        /// 是否显示识别二维码图标
        /// </summary>
        [ObservableProperty]
        private bool isShowQRCode = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsShowQRCode ?? false;

        /// <summary>
        /// 取词时间间隔
        /// </summary>
        [ObservableProperty]
        private int wordPickingInterval = Singleton<ConfigHelper>.Instance.CurrentConfig?.WordPickingInterval ?? 100;

        /// <summary>
        /// 启动时隐藏主界面
        /// </summary>
        [ObservableProperty]
        private bool isHideOnStart = Singleton<ConfigHelper>.Instance.CurrentConfig?.IsHideOnStart ?? false;

        /// <summary>
        /// 收缩框是否显示复制按钮
        /// </summary>
        [ObservableProperty]
        private bool showCopyOnHeader = Singleton<ConfigHelper>.Instance.CurrentConfig?.ShowCopyOnHeader ?? false;
    }
}
