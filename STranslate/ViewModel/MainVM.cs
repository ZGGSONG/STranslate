using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using STranslate.View;
using STranslate.Model;
using STranslate.Helper;
using System.Windows.Controls;

namespace STranslate.ViewModel
{
    public class MainVM : BaseVM
    {
        public MainVM()
        {
            #region Initial
            if (!ReadConfig())
            {
                ExitApp(-1);
            }

            InputCombo = LanguageEnumDict.Keys.ToList();
            OutputCombo = LanguageEnumDict.Keys.ToList();
            _sqlHelper = new SqliteHelper();
            #endregion

            #region Common
            //退出
            ExitCmd = new RelayCommand((_) => true, (_) => ExitApp(0));
            //置顶
            TopmostCmd = new RelayCommand((_) => true, (o) =>
            {
                ((Button)o).SetResourceReference(Control.TemplateProperty,
                    IsTopmost ? UnTopmostTemplateName : TopmostTemplateName);
                IsTopmost = !IsTopmost;
            });

            //ESC
            EscCmd = new RelayCommand((_) => true, (o) =>
            {
                //取消置顶
                if (IsTopmost)
                {
                    ((Button)o).SetResourceReference(Control.TemplateProperty, UnTopmostTemplateName);
                    IsTopmost = !IsTopmost;
                }
                Mainwin.Hide();
            });

            //切换语言
            SelectLangChangedCmd = new RelayCommand((_) => true, (_) =>
            {
                if (string.IsNullOrEmpty(InputTxt)) return;
                IdentifyLanguage = string.Empty;
                _ = Translate(true);
            });
            //移动
            MouseLeftDownCmd = new RelayCommand((_) => true, (_) =>
            {
                Mainwin.DragMove();
            });
            //失去焦点
            DeactivatedCmd = new RelayCommand((_) => true, (_) =>
            {
                if (IsTopmost) return;
                _speech.SpeakAsyncCancelAll();
                Mainwin.Hide();
                
                Util.FlushMemory();
            });
            //source speak
            SourceSpeakCmd = new RelayCommand((_) => true, (_) =>
            {
                _speech.SpeakAsync(InputTxt);
            });
            //target speak
            TargetSpeakCmd = new RelayCommand((_) => true, (_) =>
            {
                _speech.SpeakAsync(OutputTxt);
            });
            //复制输入
            CopyInputCmd = new RelayCommand((_) => true, (_) =>
            {
                Clipboard.SetText(InputTxt);
            });
            //复制翻译结果
            CopyResultCmd = new RelayCommand((_) => true, (_) =>
            {
                Clipboard.SetText(OutputTxt);
            });
            //复制蛇形结果
            CopySnakeResultCmd = new RelayCommand((_) => true, (_) =>
            {
                Clipboard.SetText(SnakeRet);
            });
            //复制小驼峰结果
            CopySmallHumpResultCmd = new RelayCommand((_) => true, (_) =>
            {
                Clipboard.SetText(SmallHumpRet);
            });
            //复制大驼峰结果
            CopyLargeHumpResultCmd = new RelayCommand((_) => true, (_) =>
            {
                Clipboard.SetText(LargeHumpRet);
            });

            //主题切换
            ThemeConvertCmd = new RelayCommand((_) => true, (o) =>
            {
                Application.Current.Resources.MergedDictionaries[0].Source =
                Application.Current.Resources.MergedDictionaries[0].Source
                .ToString() == ThemeDark ? new Uri(ThemeDefault) : new Uri(ThemeDark);
            });

            //翻译
            TranslateCmd = new RelayCommand((_) => !string.IsNullOrEmpty(InputTxt), async (o) =>
            {
                var forceTranslate = o is null ? false : true;
                await Translate(forceTranslate);
            });
            #endregion
        }

        #region handle
        /// <summary>
        /// 清空所有
        /// </summary>
        private void ClearAll()
        {
            InputTxt = string.Empty;
            OutputTxt = string.Empty;
            SnakeRet = string.Empty;
            SmallHumpRet = string.Empty;
            LargeHumpRet = string.Empty;
            IdentifyLanguage = string.Empty;
            
            Util.FlushMemory();
        }
        /// <summary>
        /// 打开主窗口
        /// </summary>
        public void OpenMainWin()
        {
            Mainwin.Show();
            Mainwin.Activate();
            //TODO: need to deal with this
            //TextBoxInput.Focus();
            ((TextBox)Mainwin.FindName("TextBoxInput"))?.Focus();
        }
        /// <summary>
        /// 输入翻译
        /// </summary>
        public void InputTranslate()
        {
            ClearAll();
            OpenMainWin();
        }
        /// <summary>
        /// 划词翻译
        /// </summary>
        public void CrossWordTranslate()
        {
            ClearAll();
            var sentence = GetWordsHelper.Get();
            OpenMainWin();
            sentence = Util.PreProcessTexts(sentence);
            InputTxt = sentence;
            _ = Translate();
        }

        /// <summary>
        /// 截屏翻译
        /// </summary>
        public void ScreenShotTranslate()
        {
            ScreenShotWindow window = null;
            foreach (Window item in Application.Current.Windows)
            {
                if (item is ScreenShotWindow)
                {
                    window = (ScreenShotWindow)item;
                    window.WindowState = WindowState.Normal;
                    window.Activate();
                    break;
                }
            }
            if (window == null)
            {
                window = new ScreenShotWindow();
                window.Show();
                window.Activate();
            }
        }
        /// <summary>
        /// 截屏翻译Ex
        /// </summary>
        public void ScreenShotTranslateEx(string text)
        {
            InputTranslate();
            InputTxt = text;
            _ = Translate();
        }

        /// <summary>
        /// 退出App
        /// </summary>
        public void ExitApp(int id)
        {
            Util.FlushMemory();
            Mainwin.NotifyIcon.Dispose();
            Mainwin.Close();
            //关闭数据库
            _sqlHelper.Dispose();
            //语音合成销毁
            _speech.Dispose();
            //注销快捷键
            HotkeysHelper.UnRegisterHotKey();
            if (id == 0)
            {
                //写入配置
                WriteConfig();
            }
            Environment.Exit(id);
        }

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        /// <returns></returns>
        private bool ReadConfig()
        {
            try
            {
                _globalConfig = ConfigHelper.Instance.ReadConfig<ConfigModel>();

                #region 读取热键
                NHotkeys = _globalConfig.Hotkeys ?? new Hotkeys
                {
                    InputTranslate = new InputTranslate
                    {
                        Modifiers = 1,
                        Key = 65,
                        Text = "Alt + A",
                        Conflict = false,
                    },
                    CrosswordTranslate = new CrosswordTranslate
                    {
                        Modifiers = 1,
                        Key = 68,
                        Text = "Alt + D",
                        Conflict = false,
                    },
                    ScreenShotTranslate = new ScreenShotTranslate
                    {
                        Modifiers = 1,
                        Key = 83,
                        Text = "Alt + S",
                        Conflict = false,
                    },
                    OpenMainWindow = new OpenMainWindow
                    {
                        Modifiers = 1,
                        Key = 71,
                        Text = "Alt + G",
                        Conflict = false,
                    },
                };
                #endregion

                //读取历史记录数量
                var count = _globalConfig.MaxHistoryCount;
                SettingsVM.Instance.MaxHistoryCount = (count <= 0 || count >= 1000) ? 100 : count;

                //读取自动识别语种比例
                var scale = _globalConfig.AutoScale;
                SettingsVM.Instance.AutoScale = (scale <= 0 || scale >= 1) ? 0.8 : scale;

                //读取间隔
                var tmp = _globalConfig.WordPickupInterval;
                SettingsVM.Instance.WordPickupInterval = (tmp == 0 || tmp > 1000 || tmp < 100) ? 200 : tmp;

                //配置读取主题
                Application.Current.Resources.MergedDictionaries[0].Source = _globalConfig.IsBright ? new Uri(ThemeDefault) : new Uri(ThemeDark);

                //更新服务
                TranslationInterface = _globalConfig.Servers.ToList();

                if (TranslationInterface.Count < 1) throw new Exception("尚未配置任何翻译接口服务");

                try
                {
                    //配置读取接口
                    SelectedTranslationInterface = TranslationInterface[_globalConfig.SelectServer];
                }
                catch (Exception ex)
                {
                    throw new Exception($"配置文件选择服务索引出错, 请修改配置文件后重试", ex);
                }

                //从配置读取source target
                InputComboSelected = _globalConfig.SourceLanguage;
                OutputComboSelected = _globalConfig.TargetLanguage;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private void WriteConfig()
        {
            try
            {
                ConfigHelper.Instance.WriteConfig(new ConfigModel
                {
                    MaxHistoryCount = SettingsVM.Instance.MaxHistoryCount,
                    AutoScale = SettingsVM.Instance.AutoScale,
                    WordPickupInterval = SettingsVM.Instance.WordPickupInterval,
                    IsBright = Application.Current.Resources.MergedDictionaries[0].Source.ToString() == ThemeDefault ? true : false,
                    SourceLanguage = InputComboSelected,
                    TargetLanguage = OutputComboSelected,
                    SelectServer = TranslationInterface.FindIndex(x => x == SelectedTranslationInterface),
                    Servers = _globalConfig.Servers,
                    Hotkeys = NHotkeys,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 自动识别语种
        /// </summary>
        /// <param name="text">输入语言</param>
        /// <returns>
        ///     Item1: SourceLang
        ///     Item2: TargetLang
        /// </returns>
        private Tuple<string, string> AutomaticLanguageRecognition(string text)
        {
            //1. 首先去除所有数字、标点及特殊符号
            //https://www.techiedelight.com/zh/strip-punctuations-from-a-string-in-csharp/
            text = Regex.Replace(text,
                "[1234567890!\"#$%&'()*+,-./:;<=>?@\\[\\]^_`{|}~，。、《》？；‘’：“”【】、{}|·！@#￥%……&*（）——+~\\\\]",
                string.Empty).Replace(Environment.NewLine, "").Replace(" ", "");

            //2. 取出上一步中所有英文字符
            var engStr = Util.ExtractEngString(text);

            var ratio = (double)engStr.Length / text.Length;
            
            //3. 判断英文字符个数占第一步所有字符个数比例，若超过一定比例则判定原字符串为英文字符串，否则为中文字符串
            if (ratio > SettingsVM.Instance.AutoScale)
            {
                return new Tuple<string, string>(LanguageEnum.EN.GetDescription(), LanguageEnum.ZH.GetDescription());
            }
            else
            {
                return new Tuple<string, string>(LanguageEnum.ZH.GetDescription(), LanguageEnum.EN.GetDescription());
            }
        }

        /// <summary>
        /// 翻译
        /// </summary>
        /// <returns></returns>
        private async Task Translate(bool forceTranslate = false)
        {
            try
            {
                if (string.IsNullOrEmpty(InputTxt.Trim()))
                    throw new Exception("输入值为空!");
                var isEng = string.Empty;
                IdentifyLanguage = string.Empty;
                OutputTxt = "翻译中...";
                //清空多种复制
                SnakeRet = string.Empty;
                SmallHumpRet = string.Empty;
                LargeHumpRet = string.Empty;

                //自动选择目标语言
                if (OutputComboSelected == LanguageEnum.AUTO.GetDescription())
                {
                    //只有在自动的模式下读取
                    var resp = _sqlHelper.Query(InputTxt);
                    if (!string.IsNullOrEmpty(resp) && !forceTranslate)
                    {
                        OutputTxt = resp;
                        return;
                    }
                    var autoRet = AutomaticLanguageRecognition(InputTxt);
                    IdentifyLanguage = autoRet.Item1;
                    isEng = autoRet.Item2;
                    _translateResp = await Util.TranslateDeepLAsync(SelectedTranslationInterface.Api, InputTxt, LanguageEnumDict[autoRet.Item2], LanguageEnumDict[InputComboSelected]);
                }
                else
                {
                    _translateResp = await Util.TranslateDeepLAsync(SelectedTranslationInterface.Api, InputTxt, LanguageEnumDict[OutputComboSelected], LanguageEnumDict[InputComboSelected]);
                }

                //百度 Api
                //var translateResp = await TranslateUtil.TranslateBaiduAsync(config.baidu.appid, config.baidu.secretKey, InputTxt, LanguageEnumDict[OutputComboSelected], LanguageEnumDict[InputComboSelected]);
                if (_translateResp == string.Empty)
                {
                    OutputTxt = "翻译出错，请稍候再试...";
                    return;
                }
                OutputTxt = _translateResp;

                await Task.Run(() =>
                {
                    _sqlHelper.Insert(DateTime.Now,
                       InputTxt,
                       OutputTxt,
                       LanguageEnumDict[string.IsNullOrEmpty(IdentifyLanguage) ? InputComboSelected : IdentifyLanguage],
                       LanguageEnumDict[string.IsNullOrEmpty(isEng) ? OutputComboSelected : isEng],
                       SelectedTranslationInterface.Api);
                });

                //如果目标语言不是英文则不进行转换
                //1. 自动判断语种：Tuple item2 不为 EN
                //2. 非自动判断语种，目标语种不为 EN
                if (!string.IsNullOrEmpty(isEng))
                {
                    if (isEng != LanguageEnum.EN.GetDescription()) return;
                }
                else
                {
                    if (LanguageEnumDict[OutputComboSelected] != LanguageEnum.EN) return;
                }

                var splitList = OutputTxt.Split(' ').ToList();
                if (splitList.Count > 1)
                {
                    SnakeRet = Util.GenSnakeString(splitList);
                    SmallHumpRet = Util.GenHumpString(splitList, true);  //小驼峰
                    LargeHumpRet = Util.GenHumpString(splitList, false); //大驼峰
                }
                //System.Diagnostics.Debug.Print(SnakeRet + "\n" + SmallHumpRet + "\n" + LargeHumpRet);
            }
            catch (Exception ex)
            {
                OutputTxt = ex.Message;
            }
        }
        #endregion handle

        #region Params
        private readonly SqliteHelper _sqlHelper;
        private string _translateResp;
        public ICommand MouseLeftDownCmd { get; set; }
        public ICommand DeactivatedCmd { get; set; }
        public ICommand SourceSpeakCmd { get; set; }
        public ICommand TargetSpeakCmd { get; set; }
        public ICommand TranslateCmd { get; set; }
        public ICommand CopyInputCmd { get; set; }
        public ICommand CopyResultCmd { get; set; }
        public ICommand CopySnakeResultCmd { get; set; }
        public ICommand CopySmallHumpResultCmd { get; set; }
        public ICommand CopyLargeHumpResultCmd { get; set; }
        public ICommand ThemeConvertCmd { get; set; }
        public ICommand TopmostCmd { get; set; }
        public ICommand EscCmd { get; set; }
        public ICommand ExitCmd { get; set; }
        public ICommand SelectLangChangedCmd { get; set; }

        /// <summary>
        /// view传递至viewmodel
        /// </summary>
        public MainWindow Mainwin;

        private static Lazy<MainVM> _instance = new Lazy<MainVM>(() => new MainVM());
        public static MainVM Instance => _instance.Value;

        public Hotkeys NHotkeys;
        private bool IsTopmost { get; set; }
        private const string TopmostTemplateName = "ButtonTemplateTopmost";
        private const string UnTopmostTemplateName = "ButtonTemplateUnTopmost";

        /// <summary>
        /// 全局配置文件
        /// </summary>
        private ConfigModel _globalConfig;

        /// <summary>
        /// 识别语种
        /// </summary>
        private string _identifyLanguage;
        public string IdentifyLanguage { get => _identifyLanguage; set => UpdateProperty(ref _identifyLanguage, value); }
        /// <summary>
        /// 构造蛇形结果
        /// </summary>
        private string _snakeRet;
        public string SnakeRet { get => _snakeRet; set => UpdateProperty(ref _snakeRet, value); }
        /// <summary>
        /// 构造驼峰结果
        /// </summary>
        private string _smallHumpRet;
        public string SmallHumpRet { get => _smallHumpRet; set => UpdateProperty(ref _smallHumpRet, value); }
        /// <summary>
        /// 构造驼峰结果
        /// </summary>
        private string _largeHumpRet;
        public string LargeHumpRet { get => _largeHumpRet; set => UpdateProperty(ref _largeHumpRet, value); }

        private string _inputTxt;
        public string InputTxt { get => _inputTxt; set => UpdateProperty(ref _inputTxt, value); }

        private string _outputTxt;
        public string OutputTxt { get => _outputTxt; set => UpdateProperty(ref _outputTxt, value); }

        private List<string> _inputCombo;
        public List<string> InputCombo { get => _inputCombo; set => UpdateProperty(ref _inputCombo, value); }

        private string _inputComboSelected;
        public string InputComboSelected { get => _inputComboSelected; set => UpdateProperty(ref _inputComboSelected, value); }

        private List<string> _outputCombo;
        public List<string> OutputCombo { get => _outputCombo; set => UpdateProperty(ref _outputCombo, value); }

        private string _outputComboSelected;
        public string OutputComboSelected { get => _outputComboSelected; set => UpdateProperty(ref _outputComboSelected, value); }

        /// <summary>
        /// 目标接口
        /// </summary>
        private List<Server> _translationInterface;
        public List<Server> TranslationInterface { get => _translationInterface; set => UpdateProperty(ref _translationInterface, value); }

        private Server _selectedTranslationInterface;
        public Server SelectedTranslationInterface { get => _selectedTranslationInterface; set => UpdateProperty(ref _selectedTranslationInterface, value); }
        private static Dictionary<string, LanguageEnum> LanguageEnumDict { get => Util.GetEnumList<LanguageEnum>(); }

        /// <summary>
        /// 语音
        /// </summary>
        private readonly SpeechSynthesizer _speech = new SpeechSynthesizer();

        private const string ThemeDark = "pack://application:,,,/STranslate.Style;component/Styles/Dark.xaml";
        private const string ThemeDefault = "pack://application:,,,/STranslate.Style;component/Styles/Default.xaml";

        #endregion Params
    }
}