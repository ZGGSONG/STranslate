using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.Helper
{
    public class ProcessHelper
    {
        public static bool Run(string filename, string[] args)
        {
            try
            {
                string arguments = "";
                foreach (string arg in args)
                {
                    arguments += $"\"{arg}\" ";
                }
                arguments = arguments.Trim();
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo(filename, arguments);
                process.StartInfo = startInfo;
                process.Start();
                return true;
            }
            catch (Exception)
            {
                //Logger.Error(ex.ToString());
                return false;
            }

        }
    }
}
