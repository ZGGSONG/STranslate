using STranslate.Model;
using STranslate.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using STranslate.Util;

namespace STranslate.ViewModel
{
    public class MainVM : BaseVM
    {

        public MainVM()
        {
            InputCombo = LanguageEnumDict.Keys.ToList();
            InputComboSelected = LanguageEnum.AUTO.GetDescription();
            OutputCombo = LanguageEnumDict.Keys.ToList();
            OutputComboSelected = LanguageEnum.AUTO.GetDescription();

            //初始化接口
            SelectedTranslationInterface = TranslationInterface[1];

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
        }

        #region handle
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
            if (ratio > 0.5)
            {
                return new Tuple<string, string>(LanguageEnum.EN.GetDescription(), LanguageEnum.ZH.GetDescription());
            }
            else
            {
                return new Tuple<string, string>(LanguageEnum.ZH.GetDescription(), LanguageEnum.EN.GetDescription());
            }
#if false
            //如果输入是中文
            if (Regex.IsMatch(text, @"^[\u4e00-\u9fa5]+$"))
            {
                return new Tuple<string, string>(LanguageEnum.ZH.GetDescription(), LanguageEnum.EN.GetDescription());
            }
            else
            {
                return new Tuple<string, string>(LanguageEnum.EN.GetDescription(), LanguageEnum.ZH.GetDescription());
            }
#endif
        }
        /// <summary>
        /// 翻译
        /// </summary>
        /// <returns></returns>
        public async Task Translate()
        {
            try
            {
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
        public ICommand TranslateCmd { get; private set; }
        public ICommand CopyInputCmd { get; private set; }
        public ICommand CopyResultCmd { get; private set; }
        public ICommand CopySnakeResultCmd { get; private set; }
        public ICommand CopySmallHumpResultCmd { get; private set; }
        public ICommand CopyLargeHumpResultCmd { get; private set; }
        public ICommand ThemeConvertCmd { get; private set; }

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
        private List<TranslationInterface> _TranslationInterface = new List<TranslationInterface>
        {
            new TranslationInterface
            {
                Name = "zu1k",
                Api = "https://deepl.deno.dev/translate"
            },
            new TranslationInterface
            {
                Name = "zggsong",
                Api = "https://zggsong.cn/tt"
            },
            new TranslationInterface
            {
                Name = "local",
                Api = "http://127.0.0.1:8000/translate"
            }
        };
        public List<TranslationInterface> TranslationInterface { get => _TranslationInterface; set => UpdateProperty(ref _TranslationInterface, value); }
        private TranslationInterface _SelectedTranslationInterface;
        public TranslationInterface SelectedTranslationInterface { get => _SelectedTranslationInterface; set => UpdateProperty(ref _SelectedTranslationInterface, value); }
        private static Dictionary<string, LanguageEnum> LanguageEnumDict { get => Util.Util.GetEnumList<LanguageEnum>(); }
        private static readonly string _ThemeDark = "pack://application:,,,/STranslate;component/Style/Dark.xaml";
        private static readonly string _ThemeDefault = "pack://application:,,,/STranslate;component/Style/Default.xaml";
        #endregion Params
    }
}