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
        public MainVM()
        {
            TranslateCmd = new RelayCommand((_) =>
            {
                return string.IsNullOrEmpty(InputTxt) ? false : true;
            }, (_) =>
            {
                OutputTxt = TranslateUtil.Translate(InputTxt, LanguageEnum.ZH, LanguageEnum.EN);
                //清空输入框
                InputTxt = "";

            });
        }

        public ICommand TranslateCmd { get; private set; }

        private string _InputTxt;
        public string InputTxt { get => _InputTxt; set => UpdateProperty(ref _InputTxt, value); }
        private string _OutputTxt;
        public string OutputTxt { get => _OutputTxt; set => UpdateProperty(ref _OutputTxt, value); }

    }
}
