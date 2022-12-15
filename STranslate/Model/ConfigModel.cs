using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.Model
{
    public class ConfigModel
    {
        public string service { get; set; }
        public Baidu baidu { get; set; }
        public DeepL deepl { get; set; }
    }

    public class Baidu
    {
        public string appid { get; set; }
        public string secretKey { get; set; }
    }
    public class DeepL
    {
        public string url { get; set; }
    }
}