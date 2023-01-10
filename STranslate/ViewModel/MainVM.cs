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

namespace STranslate.ViewModel
{
    public class MainVM : BaseVM
    {
        public MainVM()
        {
            if (!ReadConfig())
            {
                ExitApp(-1);
            }

            InputCombo = LanguageEnumDict.Keys.ToList();
            OutputCombo = LanguageEnumDict.Keys.ToList();

            #region 托盘程序
            //运行前检查是否开机自启
            IsStartup = StartupHelper.IsStartup();

            //输入翻译
            InputTranslateCmd = new RelayCommand((_) => true, (_) =>
            {
                InputTranslate();
            });

            //截图翻译
            ScreenShotTranslateCmd = new RelayCommand((_) => true, (_) =>
            {
                ScreenShotTranslate();
            });

            //显示主界面
            ShowMainWinCmd = new RelayCommand((_) => true, (_) =>
            {
                OpenMainWin();
            });

            //开机自启
            StartupCmd = new RelayCommand((_) => true, (_) =>
            {
                if (StartupHelper.IsStartup()) StartupHelper.UnSetStartup();
                else StartupHelper.SetStartup();
                IsStartup = StartupHelper.IsStartup();
            });
            
            //退出App
            ExitCmd = new RelayCommand((_) => true, (_) =>
            {
                ExitApp(0);
            });

            //置顶
            TopmostCmd = new RelayCommand((_) => true, (o) =>
            {
                var button = o as System.Windows.Controls.Button;
                if (IsTopmost)
                {
                    button.SetResourceReference(System.Windows.Controls.Control.TemplateProperty, _UnTopmostTemplateName);
                }
                else
                {
                    button.SetResourceReference(System.Windows.Controls.Control.TemplateProperty, _TopmostTemplateName);
                }
                IsTopmost = !IsTopmost;
            });

            //ESC
            EscCmd = new RelayCommand((_) => true, (o) =>
            {
                //取消置顶
                if (IsTopmost)
                {
                    (o as System.Windows.Controls.Button)
                    .SetResourceReference(System.Windows.Controls.Control.TemplateProperty, _UnTopmostTemplateName);
                    IsTopmost = !IsTopmost;
                }
                Mainwin.Hide();
            });

            //切换语言
            SelectLangChangedCmd = new RelayCommand((_) => true, (_) =>
            {
                if (!string.IsNullOrEmpty(InputTxt))
                {
                    IdentifyLanguage = string.Empty;
                    _ = Translate();
                }
            });

            #endregion

            #region Common
            //移动
            MouseLeftDownCmd = new RelayCommand((_) => true, (_) =>
            {
                Mainwin.DragMove();
            });
            //失去焦点
            DeactivatedCmd = new RelayCommand((_) => true, (_) =>
            {
                if (!IsTopmost)
                {
                    Speech.SpeakAsyncCancelAll();
                    Mainwin.Hide();
                }
            });
            //source speak
            SourceSpeakCmd = new RelayCommand((_) => true, (_) =>
            {
                Speech.SpeakAsync(InputTxt);
            });
            //target speak
            TargetSpeakCmd = new RelayCommand((_) => true, (_) =>
            {
                Speech.SpeakAsync(OutputTxt);
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
                .ToString() == _ThemeDark ? new Uri(_ThemeDefault) : new Uri(_ThemeDark);
            });

            //翻译
            TranslateCmd = new RelayCommand((_) =>
            {
                return string.IsNullOrEmpty(InputTxt) ? false : true;
            }, async (_) =>
            {
                await Translate();
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
            (Mainwin.FindName("TextBoxInput") as System.Windows.Controls.TextBox).Focus();
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
            InputTxt = sentence.Trim();
            _ = Translate();
        }
        /// <summary>
        /// 截屏翻译
        /// </summary>
        public void ScreenShotTranslate()
        {
            var screen = new ScreenShotWindow();
            screen.Show();
            screen.Activate();
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
            //隐藏icon
            IsVisibility = false;
            //语音合成销毁
            Speech.Dispose();
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
                _GlobalConfig = ConfigHelper.Instance.ReadConfig<ConfigModel>();

                //配置读取主题
                Application.Current.Resources.MergedDictionaries[0].Source = _GlobalConfig.IsBright ? new Uri(_ThemeDefault) : new Uri(_ThemeDark);

                //更新服务
                TranslationInterface = _GlobalConfig.Servers.ToList();

                if (TranslationInterface.Count < 1) throw new Exception("尚未配置任何翻译接口服务");

                try
                {
                    //配置读取接口
                    SelectedTranslationInterface = TranslationInterface[_GlobalConfig.SelectServer];
                }
                catch (Exception ex)
                {
                    throw new Exception($"配置文件选择服务索引出错, 请修改配置文件后重试", ex);
                }

                //从配置读取source target
                InputComboSelected = _GlobalConfig.SourceLanguage;
                OutputComboSelected = _GlobalConfig.TargetLanguage;

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
                    IsBright = Application.Current.Resources.MergedDictionaries[0].Source.ToString() == _ThemeDefault ? true : false,
                    SourceLanguage = InputComboSelected,
                    TargetLanguage = OutputComboSelected,
                    SelectServer = TranslationInterface.FindIndex(x => x == SelectedTranslationInterface),
                    Servers = _GlobalConfig.Servers,
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
                string.Empty);

            //2. 取出上一步中所有英文字符
            var engStr = Util.Util.ExtractEngString(text);

            var ratio = (double)engStr.Length / text.Length;
            
            //3. 判断英文字符个数占第一步所有字符个数比例，若超过一半则判定原字符串为英文字符串，否则为中文字符串
            //TODO: 配置项
            if (ratio > 0.8)
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
        public async Task Translate()
        {
            try
            {
                if (string.IsNullOrEmpty(InputTxt.Trim())) throw new Exception("输入值为空!");
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
                    var autoRet = AutomaticLanguageRecognition(InputTxt);
                    IdentifyLanguage = autoRet.Item1;
                    isEng = autoRet.Item2;
                    translateResp = await Util.Util.TranslateDeepLAsync(SelectedTranslationInterface.Api, InputTxt, LanguageEnumDict[autoRet.Item2], LanguageEnumDict[InputComboSelected]);
                }
                else
                {
                    translateResp = await Util.Util.TranslateDeepLAsync(SelectedTranslationInterface.Api, InputTxt, LanguageEnumDict[OutputComboSelected], LanguageEnumDict[InputComboSelected]);
                }

                //百度 Api
                //var translateResp = await TranslateUtil.TranslateBaiduAsync(config.baidu.appid, config.baidu.secretKey, InputTxt, LanguageEnumDict[OutputComboSelected], LanguageEnumDict[InputComboSelected]);
                if (translateResp == string.Empty)
                {
                    OutputTxt = "翻译出错，请稍候再试...";
                    return;
                }
                OutputTxt = translateResp;

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
                    SnakeRet = Util.Util.GenSnakeString(splitList);
                    SmallHumpRet = Util.Util.GenHumpString(splitList, true);  //小驼峰
                    LargeHumpRet = Util.Util.GenHumpString(splitList, false); //大驼峰
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
        private string translateResp;
        public ICommand MouseLeftDownCmd { get; private set; }
        public ICommand DeactivatedCmd { get; private set; }
        public ICommand SourceSpeakCmd { get; private set; }
        public ICommand TargetSpeakCmd { get; private set; }
        public ICommand TranslateCmd { get; private set; }
        public ICommand CopyInputCmd { get; private set; }
        public ICommand CopyResultCmd { get; private set; }
        public ICommand CopySnakeResultCmd { get; private set; }
        public ICommand CopySmallHumpResultCmd { get; private set; }
        public ICommand CopyLargeHumpResultCmd { get; private set; }
        public ICommand ThemeConvertCmd { get; private set; }
        //托盘程序
        public ICommand InputTranslateCmd { get; private set; }
        public ICommand ScreenShotTranslateCmd { get; private set; }
        public ICommand ShowMainWinCmd { get; private set; }
        public ICommand StartupCmd { get; private set; }
        public ICommand ExitCmd { get; private set; }
        public ICommand TopmostCmd { get; private set; }
        public ICommand EscCmd { get; private set; }
        public ICommand SelectLangChangedCmd { get; private set; }

        /// <summary>
        /// 是否开机自启
        /// </summary>
        private bool _IsStartup;
        public bool IsStartup { get => _IsStartup; set => UpdateProperty(ref _IsStartup, value); }
        /// <summary>
        /// 托盘图标可见
        /// </summary>
        private bool _IsVisibility = true;
        public bool IsVisibility { get => _IsVisibility; set => UpdateProperty(ref _IsVisibility, value); }

        /// <summary>
        /// view传递至viewmodel
        /// </summary>
        public MainWindow Mainwin;

        private static MainVM _Instance;
        public static MainVM Instance => _Instance ?? (_Instance = new MainVM());

        public bool IsTopmost { get; set; }
        private readonly string _TopmostTemplateName = "ButtonTemplateTopmost";
        private readonly string _UnTopmostTemplateName = "ButtonTemplateUnTopmost";

        /// <summary>
        /// 全局配置文件
        /// </summary>
        private ConfigModel _GlobalConfig;

        /// <summary>
        /// 识别语种
        /// </summary>
        private string _IdentifyLanguage;
        public string IdentifyLanguage { get => _IdentifyLanguage; set => UpdateProperty(ref _IdentifyLanguage, value); }
        /// <summary>
        /// 构造蛇形结果
        /// </summary>
        private string _SnakeRet;
        public string SnakeRet { get => _SnakeRet; set => UpdateProperty(ref _SnakeRet, value); }
        /// <summary>
        /// 构造驼峰结果
        /// </summary>
        private string _SmallHumpRet;
        public string SmallHumpRet { get => _SmallHumpRet; set => UpdateProperty(ref _SmallHumpRet, value); }
        /// <summary>
        /// 构造驼峰结果
        /// </summary>
        private string _LargeHumpRet;
        public string LargeHumpRet { get => _LargeHumpRet; set => UpdateProperty(ref _LargeHumpRet, value); }

        private string _InputTxt;
        public string InputTxt { get => _InputTxt; set => UpdateProperty(ref _InputTxt, value); }

        private string _OutputTxt;
        public string OutputTxt { get => _OutputTxt; set => UpdateProperty(ref _OutputTxt, value); }

        private List<string> _InputCombo;
        public List<string> InputCombo { get => _InputCombo; set => UpdateProperty(ref _InputCombo, value); }

        private string _InputComboSelected;
        public string InputComboSelected { get => _InputComboSelected; set => UpdateProperty(ref _InputComboSelected, value); }

        private List<string> _OutputCombo;
        public List<string> OutputCombo { get => _OutputCombo; set => UpdateProperty(ref _OutputCombo, value); }

        private string _OutputComboSelected;
        public string OutputComboSelected { get => _OutputComboSelected; set => UpdateProperty(ref _OutputComboSelected, value); }

        /// <summary>
        /// 目标接口
        /// </summary>
        private List<Server> _TranslationInterface;
        public List<Server> TranslationInterface { get => _TranslationInterface; set => UpdateProperty(ref _TranslationInterface, value); }

        private Server _SelectedTranslationInterface;
        public Server SelectedTranslationInterface { get => _SelectedTranslationInterface; set => UpdateProperty(ref _SelectedTranslationInterface, value); }
        private static Dictionary<string, LanguageEnum> LanguageEnumDict { get => Util.Util.GetEnumList<LanguageEnum>(); }

        /// <summary>
        /// 语音
        /// </summary>
        public readonly SpeechSynthesizer Speech = new SpeechSynthesizer();

        private static readonly string _ThemeDark = "pack://application:,,,/STranslate;component/Style/Dark.xaml";
        private static readonly string _ThemeDefault = "pack://application:,,,/STranslate;component/Style/Default.xaml";
        #endregion Params
    }
}