using STranslate.Model;
using STranslate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STranslate.ViewModel
{
    public class MainVM : BaseVM
    {
        private static readonly string ConfigPath = @"D:\STranslate.yml";
        public static ConfigModel config = new ConfigModel();
        private static Dictionary<string, LanguageEnum> LanguageEnumDict { get => TranslateUtil.GetEnumList<LanguageEnum>(); }

        private string Text;
        public MainVM()
        {
            try
            {
                //初始化界面参数
                InputCombo = LanguageEnumDict.Keys.ToList();
                InputComboSelected = LanguageEnum.AUTO.GetDescription();
                OutputCombo = LanguageEnumDict.Keys.ToList();
                OutputComboSelected = LanguageEnum.EN.GetDescription();

                config = ConfigUtil.ReadConfig(ConfigPath);

                //手动复制翻译结果
                CopyTranslateResultCmd = new RelayCommand((_) =>
                {
                    return string.IsNullOrEmpty(OutputTxt) ? false : true;
                }, (_) =>
                {
                    System.Diagnostics.Debug.Print("手动复制翻译结果: " + OutputTxt);
                    Clipboard.SetText(OutputTxt);
                });


                TranslateCmd = new RelayCommand((_) =>
                {
                    return string.IsNullOrEmpty(InputTxt) ? false : true;
                }, async (_) =>
                {
                    Text = InputTxt;

                    //清空输入框
                    InputTxt = "";

                    OutputTxt = "翻译中...";

                    //获取结果
                    //var translateResp = await TranslateUtil.TranslateDeepLAsync(InputTxt, LanguageEnum.EN, LanguageEnum.auto);

                    var translateResp = await TranslateUtil.TranslateBaiduAsync(config.baidu.appid, config.baidu.secretKey, Text, LanguageEnumDict[OutputComboSelected], LanguageEnumDict[InputComboSelected]);

                    if (translateResp == string.Empty)
                    {
                        OutputTxt = "翻译出错，请稍候再试...";
                        return;
                    }
                    OutputTxt = translateResp;
                    Clipboard.SetText(OutputTxt);
                });

            }
            catch (Exception ex)
            {
                OutputTxt = ex.Message;
            }
        }

        public ICommand TranslateCmd { get; private set; }
        public ICommand CopyTranslateResultCmd { get; private set; }

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

    }
}
