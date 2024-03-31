using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.Tests.Utils
{
    public class Utils
    {
        public static string DocumentToBase64Str(string fileName)
        {
            using FileStream filestream = new(fileName, FileMode.Open);

            byte[] bt = new byte[filestream.Length];
            //调用read读取方法
            filestream.Read(bt, 0, bt.Length);
            string base64Str = Convert.ToBase64String(bt);
            return base64Str;
        }
    }
}
