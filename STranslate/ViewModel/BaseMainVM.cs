using STranslate.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.ViewModel
{
    public class BaseMainVM : BaseVM
    {
        public BaseMainVM()
        {
            Version = HandleVersion(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() ?? "1.0.0.0");
        }


        /// <summary>
        /// 同步Github版本命名
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        private static string HandleVersion(string version)
        {
            var ret = string.Empty;
            ret = version.Substring(0, version.Length - 2);
            var location = ret.LastIndexOf('.');
            ret = ret.Remove(location, 1);
            return ret;
        }

        private string _version;
        public string Version { get => _version; set => UpdateProperty(ref _version, value); }

    }
}
