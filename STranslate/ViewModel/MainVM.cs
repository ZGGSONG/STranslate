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
        private static string ConfigPath => $"{AppDomain.CurrentDomain.BaseDirectory}STranslate.yml";
        public static ConfigModel config = new ConfigModel();
        private static Dictionary<string, LanguageEnum> LanguageEnumDict { get => TranslateUtil.GetEnumList<LanguageEnum>(); }

        public MainVM()
        {
            //初始化界面参数
            InputCombo = LanguageEnumDict.Keys.ToList();
            InputComboSelected = LanguageEnum.AUTO.GetDescription();
            OutputCombo = LanguageEnumDict.Keys.ToList();
            OutputComboSelected = LanguageEnum.EN.GetDescription();

            //TODO: fix no config
            config = ConfigUtil.ReadConfig(ConfigPath);

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
            //复制驼峰结果
            CopyHumpResultCmd = new RelayCommand((_) =>
            {
                return string.IsNullOrEmpty(HumpRet) ? false : true;
            }, (_) =>
            {
                Clipboard.SetText(HumpRet);
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
                //清空输入框
                OutputTxt = "翻译中...";

                //获取结果
                //var translateResp = await TranslateUtil.TranslateDeepLAsync(config.deepl.url, InputTxt, LanguageEnum.EN, LanguageEnum.AUTO);

                var translateResp = await TranslateUtil.TranslateBaiduAsync(config.baidu.appid, config.baidu.secretKey, InputTxt, LanguageEnumDict[OutputComboSelected], LanguageEnumDict[InputComboSelected]);

                if (translateResp == string.Empty)
                {
                    OutputTxt = "翻译出错，请稍候再试...";
                    return;
                }
                OutputTxt = translateResp;

                var splitList = translateResp.Split(' ').ToList();
                if (splitList.Count > 1)
                {
                    SnakeRet = GenSnakeString(splitList);
                    HumpRet = GenHumpString(splitList);
                }

                System.Diagnostics.Debug.Print(SnakeRet);
                System.Diagnostics.Debug.Print(HumpRet);
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
            //Alarm statistics
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
        /// <returns></returns>
        private string GenHumpString(List<string> req)
        {
            //TODO: I'm your father 出错情况
            var ret = string.Empty;
            var arr = req.ToArray();
            ret += arr[0].Substring(0, 1).ToLower() + arr[0].Substring(1);
            for (int i = 1; i < arr.Length; i++)
            {
                ret += arr[i].Substring(0, 1).ToUpper() + arr[0].Substring(1);
            }
            return ret;
        }

        #endregion handle

        #region Params

        public ICommand TranslateCmd { get; private set; }
        public ICommand CopyResultCmd { get; private set; }
        public ICommand CopySnakeResultCmd { get; private set; }
        public ICommand CopyHumpResultCmd { get; private set; }

        public string SnakeRet { get; set; }
        public string HumpRet { get; set; }

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