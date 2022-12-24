using STranslate.Model;
using STranslate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STranslate.ViewModel
{
    public class MainVM : BaseVM
    {
        /// <summary>
        /// 引入Go项目库
        /// </summary>
        /// <returns></returns>
        //[System.Runtime.InteropServices.DllImport("deepl.dll", EntryPoint = "run")]
        //extern static void run();

        public string defaultApi = "http://127.0.0.1:8000/translate";
        private string ConfigPath => $"{AppDomain.CurrentDomain.BaseDirectory}STranslate.yml";
        public ConfigModel config = new ConfigModel();
        private static Dictionary<string, LanguageEnum> LanguageEnumDict { get => TranslateUtil.GetEnumList<LanguageEnum>(); }

        public MainVM()
        {
            //启动deepl
            //Task.Run(() => RunCmdUtil.RunCmd(AppDomain.CurrentDomain.BaseDirectory + "\\deepl-x86_64-pc-windows-gnu.exe"));

            //初始化界面参数
            InputCombo = LanguageEnumDict.Keys.ToList();
            InputComboSelected = LanguageEnum.AUTO.GetDescription();
            OutputCombo = LanguageEnumDict.Keys.ToList();
            OutputComboSelected = LanguageEnum.EN.GetDescription();

            config = ConfigUtil.ReadConfig(ConfigPath);

            //复制输入
            CopyInputCmd = new RelayCommand((_) =>
            {
                return string.IsNullOrEmpty(InputTxt) ? false : true;
            }, (_) =>
            {
                Clipboard.SetText(InputTxt);
            });
            //复制翻译结果
            CopyResultCmd = new RelayCommand((_) =>
            {
                return string.IsNullOrEmpty(OutputTxt) ? false : true;
            }, (_) =>
            {
                Clipboard.SetText(OutputTxt);
            });
            //复制蛇形结果
            CopySnakeResultCmd = new RelayCommand((_) =>
            {
                return string.IsNullOrEmpty(SnakeRet) ? false : true;
            }, (_) =>
            {
                Clipboard.SetText(SnakeRet);
            });
            //复制小驼峰结果
            CopySmallHumpResultCmd = new RelayCommand((_) =>
            {
                return string.IsNullOrEmpty(SmallHumpRet) ? false : true;
            }, (_) =>
            {
                Clipboard.SetText(SmallHumpRet);
            });
            //复制大驼峰结果
            CopyLargeHumpResultCmd = new RelayCommand((_) =>
            {
                return string.IsNullOrEmpty(LargeHumpRet) ? false : true;
            }, (_) =>
            {
                Clipboard.SetText(LargeHumpRet);
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

        public async Task Translate()
        {
            try
            {
                OutputTxt = "翻译中...";

                //获取结果

                //DeepL Api
                var translateResp = await TranslateUtil.TranslateDeepLAsync(config.deepl?.url ?? defaultApi, InputTxt, LanguageEnumDict[OutputComboSelected], LanguageEnumDict[InputComboSelected]);

                //百度 Api
                //var translateResp = await TranslateUtil.TranslateBaiduAsync(config.baidu.appid, config.baidu.secretKey, InputTxt, LanguageEnumDict[OutputComboSelected], LanguageEnumDict[InputComboSelected]);

                if (translateResp == string.Empty)
                {
                    OutputTxt = "翻译出错，请稍候再试...";
                    return;
                }
                OutputTxt = translateResp;

                //如果不是英文则不进行转换
                if (LanguageEnumDict[OutputComboSelected] != LanguageEnum.EN) return;

                var splitList = OutputTxt.Split(' ').ToList();
                if (splitList.Count > 1)
                {
                    SnakeRet = GenSnakeString(splitList);
                    SmallHumpRet = GenHumpString(splitList, true);  //小驼峰
                    LargeHumpRet = GenHumpString(splitList, false); //大驼峰
                }
                //System.Diagnostics.Debug.Print(SnakeRet + "\n" + SmallHumpRet + "\n" + LargeHumpRet);
            }
            catch (Exception ex)
            {
                OutputTxt = ex.Message;
            }
        }
        /// <summary>
        /// 构造蛇形结果
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private string GenSnakeString(List<string> req)
        {
            //TODO: 构造时间过长
            var ret = string.Empty;
            
            req.ForEach(x =>
            {
                ret += "_" + x.ToLower();
            });
            return ret.Substring(1);
        }

        /// <summary>
        /// 构造驼峰结果
        /// </summary>
        /// <param name="req"></param>
        /// <param name="isSmallHump">是否为小驼峰</param>
        /// <returns></returns>
        private string GenHumpString(List<string> req, bool isSmallHump)
        {
            string ret = string.Empty;
            var array = req.ToArray();
            for (var j = 0; j < array.Length; j++)
            {
                char[] chars = array[j].ToCharArray();
                if (j == 0 && isSmallHump) chars[0] = char.ToLower(chars[0]);
                else chars[0] = char.ToUpper(chars[0]);
                for (int i = 1; i < chars.Length; i++)
                {
                    chars[i] = char.ToLower(chars[i]);
                }
                ret += new string(chars);
            }
            return ret;

        }

        #endregion handle

        #region Params

        public ICommand TranslateCmd { get; private set; }
        public ICommand CopyInputCmd { get; private set; }
        public ICommand CopyResultCmd { get; private set; }
        public ICommand CopySnakeResultCmd { get; private set; }
        public ICommand CopySmallHumpResultCmd { get; private set; }
        public ICommand CopyLargeHumpResultCmd { get; private set; }

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

        #endregion Params
    }
}