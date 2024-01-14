using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Windows;

namespace STranslate.Model
{
    public partial class ConfigModel : ObservableObject
    {
        /// <summary>
        /// 开机自启动
        /// </summary>
        public bool IsStartup { get; set; }

        /// <summary>
        /// 是否管理员启动
        /// </summary>
        public bool NeedAdministrator { get; set; }

        /// <summary>
        /// 历史记录大小
        /// </summary>
        public long HistorySize { get; set; }

        /// <summary>
        /// 自动识别语种标度
        /// </summary>
        public double AutoScale { get; set; }

        /// <summary>
        /// 是否亮色模式
        /// </summary>
        public bool IsBright { get; set; }

        /// <summary>
        /// 是否跟随鼠标
        /// </summary>
        public bool IsFollowMouse { get; set; }

        /// <summary>
        /// OCR结果翻译关闭OCR界面
        /// </summary>
        public bool CloseUIOcrRetTranslate { get; set; }

        /// <summary>
        /// 截图出现问题尝试一下
        /// </summary>
        public bool UnconventionalScreen { get; set; }

        /// <summary>
        /// 禁用系统代理
        /// </summary>
        public bool IsDisableSystemProxy { get; set; }

        /// <summary>
        /// OCR时是否自动复制文本
        /// </summary>
        public bool IsOcrAutoCopyText { get; set; }

        /// <summary>
        /// 是否调整完语句后翻译
        /// </summary>
        public bool IsAdjustContentTranslate { get; set; }

        /// <summary>
        /// 取词时移除换行
        /// </summary>
        public bool IsRemoveLineBreakGettingWords { get; set; }

        /// <summary>
        /// 鼠标双击托盘程序功能
        /// </summary>
        public DoubleTapFuncEnum DoubleTapTrayFunc { get; set; } = DoubleTapFuncEnum.InputFunc;

        /// <summary>
        /// 原始语言
        /// </summary>
        public string SourceLanguage { get; set; } = string.Empty;

        /// <summary>
        /// 目标语言
        /// </summary>
        public string TargetLanguage { get; set; } = string.Empty;

        /// <summary>
        /// 退出时的位置
        /// </summary>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// 服务
        /// </summary>
        [JsonIgnore]
        [ObservableProperty]
        public BindingList<ITranslator>? _services;

        /// <summary>
        /// 热键
        /// </summary>
        public Hotkeys? Hotkeys { get; set; }
    }
}
