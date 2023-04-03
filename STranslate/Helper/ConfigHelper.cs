using Newtonsoft.Json;
using STranslate.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STranslate.Helper
{
    public class ConfigHelper : Singleton<ConfigHelper>
    {
        public T ReadConfig<T>()
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(_CnfName));
            }
            catch (Exception)
            {
                throw new Exception("读取配置错误，请检查配置文件");
            }        
        }

        public void WriteConfig(object obj)
        {
            File.WriteAllText(_CnfName, JsonConvert.SerializeObject(obj, Formatting.Indented));
        }

        public ConfigHelper()
        {
            if (!Directory.Exists(_ApplicationData))//判断是否存在
            {
                Directory.CreateDirectory(_ApplicationData);//创建新路径
                ShortcutHelper.SetDesktopShortcut();//创建桌面快捷方式
            }
            if (!File.Exists(_CnfName))//文件不存在
            {
                FileStream fs1 = new FileStream(_CnfName, FileMode.Create, FileAccess.ReadWrite);
                fs1.Close();
                WriteConfig(new ConfigModel().InitialConfig());
            }
        }

        /// <summary>
        /// 配置文件
        /// </summary>
        private static string _CnfName => $"{_ApplicationData}\\{_AppName.ToLower()}.json";

        /// <summary>
        /// C:\Users\user\AppData\Local\STranslate
        /// </summary>
        private static string _ApplicationData => $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{_AppName}";
        private static readonly string _AppName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
    }
}
