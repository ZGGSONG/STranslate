using STranslate.Model;
using STranslate.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STranslate.ViewModel
{
    public class MainVM : BaseVM
    {
        private string Text;
        public MainVM()
        {
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

                var appId = "";
                var secretKey = "";
                var translateResp = await TranslateUtil.TranslateBaiduAsync(appId, secretKey, Text, LanguageEnum.EN, LanguageEnum.auto);

                if (translateResp == string.Empty)
                {
                    OutputTxt = "翻译出错，请稍候再试...";
                    return;
                }
                OutputTxt = translateResp;
            });
        }

        public ICommand TranslateCmd { get; private set; }

        private string _InputTxt;
        public string InputTxt { get => _InputTxt; set => UpdateProperty(ref _InputTxt, value); }
        private string _OutputTxt;
        public string OutputTxt { get => _OutputTxt; set => UpdateProperty(ref _OutputTxt, value); }

    }
}
