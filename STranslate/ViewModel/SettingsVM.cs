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
    public class SettingsVM : BaseMainVM
    {
        public SettingsVM()
        {
            LoadedCmd = new RelayCommand((_) => true, (_) =>
              {
                  Console.WriteLine("123");
              });

            ClosedCmd = new RelayCommand((_) => true, (_) =>
              {
                  Console.WriteLine("123");
              });

            //重置快捷键
            ResetHotKeysCmd = new RelayCommand((_) => true, (_) =>
              {
                  Console.WriteLine("123");
              });

            //重置取词间隔
            ResetWordPickupIntervalCmd = new RelayCommand((_) => true, (_) =>
              {
                  System.Diagnostics.Debug.Print(WordPickupInterval.ToString());
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
        }
        public ICommand LoadedCmd { get; private set; }
        public ICommand ClosedCmd { get; private set; }
        public ICommand UpdateCmd { get; private set; }

        public ICommand ResetHotKeysCmd { get; private set; }
        public ICommand ResetWordPickupIntervalCmd { get; private set; }

        private double _wordPickupInterval = 200;
        public double WordPickupInterval { get => _wordPickupInterval; set => UpdateProperty(ref _wordPickupInterval, value); }

        private static SettingsVM _instance;
        public static SettingsVM Instance => _instance ?? (_instance = new SettingsVM());
    }
}
