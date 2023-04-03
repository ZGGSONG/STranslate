using STranslate.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STranslate.ViewModel
{
    public class SettingsVM : BaseVM
    {
        public SettingsVM()
        {
            IsStartup = ShortcutHelper.IsStartup();
            
            Version = HandleVersion(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() ?? "1.0.0.0");

            ClosedCmd = new RelayCommand((_) => true, (_) =>
              {
                  Util.Util.FlushMemory();
              });

            //更新
            UpdateCmd = new RelayCommand((_) => true, (_) =>
              {
                  try
                  {
                      var updaterExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                          "Updater.exe");
                      var updaterCacheExePath = Path.Combine(
                          AppDomain.CurrentDomain.BaseDirectory,
                          "Updater",
                          "Updater.exe");
                      var updateDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater");
                      if (!Directory.Exists(updateDirPath))
                      {
                          Directory.CreateDirectory(updateDirPath);
                      }

                      if (!File.Exists(updaterExePath))
                      {
                          MessageBox.Show("升级程序似乎已被删除，请手动前往发布页查看新版本");
                          return;
                      }
                      File.Copy(updaterExePath, updaterCacheExePath, true);

                      File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Newtonsoft.Json.dll"), Path.Combine(
                          AppDomain.CurrentDomain.BaseDirectory,
                          "Updater",
                          "Newtonsoft.Json.dll"), true);

                      ProcessHelper.Run(updaterCacheExePath, new string[] { Version });
                  }
                  catch (Exception ex)
                  {

                      MessageBox.Show($"无法正确启动检查更新程序\n{ex.Message}");
                  }
              });

            StartupCmd = new RelayCommand((_) => true, (_) =>
              {
                  if (ShortcutHelper.IsStartup()) ShortcutHelper.UnSetStartup();
                  else ShortcutHelper.SetStartup();
                  IsStartup = ShortcutHelper.IsStartup();
              });
            EscCmd = new RelayCommand((_) => true, (o) =>
              {
                  (o as Window)?.Close();
              });

            OpenUrlCmd = new RelayCommand((_) => true, (o) =>
              {
                  try
                  {
                      System.Diagnostics.Process proc = new System.Diagnostics.Process();
                      proc.StartInfo.FileName = o.ToString();
                      proc.Start();
                  }
                  catch (Exception ex)
                  {
                      MessageBox.Show($"未找到默认应用\n{ex.Message}");
                  }
              });
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


        public ICommand ClosedCmd { get; private set; }
        public ICommand OpenUrlCmd { get; private set; }
        public ICommand UpdateCmd { get; private set; }
        public ICommand StartupCmd { get; private set; }
        public ICommand EscCmd { get; private set; }


        private static Lazy<SettingsVM> _instance = new Lazy<SettingsVM>(() => new SettingsVM());
        public static SettingsVM Instance => _instance.Value;

        /// <summary>
        /// 是否开机自启
        /// </summary>
        private bool _isStartup;
        public bool IsStartup { get => _isStartup; set => UpdateProperty(ref _isStartup, value); }

        /// <summary>
        /// 版本
        /// </summary>
        private string _version;
        public string Version { get => _version; set => UpdateProperty(ref _version, value); }

        /// <summary>
        /// 语种识别比例
        /// </summary>
        private double _autoScale;
        public double AutoScale { get => _autoScale; set => UpdateProperty(ref _autoScale, value); }

        /// <summary>
        /// 取词间隔
        /// </summary>
        private double _wordPickupInterval;
        public double WordPickupInterval { get => _wordPickupInterval; set => UpdateProperty(ref _wordPickupInterval, value); }

        /// <summary>
        /// 最大历史记录数量
        /// </summary>
        private int _maxHistoryCount;
        public int MaxHistoryCount { get => _maxHistoryCount; set => UpdateProperty(ref _maxHistoryCount, value); }

    }
}
