using STranslate.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.Utils
{
    public static class TranslateUtil
    {
        private static readonly string _url = "http://172.17.209.47:8080/translate";
        public static string Translate(string input, LanguageEnum source, LanguageEnum target)
        {
            var req = new DeeplReq()
            {
                Text = input,
                SourceLang = source.ToString(),
                TargetLang = target.ToString(),
            };
            var resp = HttpUtil.Post(_url, req);
            if (resp.Code == 200)
            {
                return resp.Data;
            }
            return string.Empty;
        }
    }
}
