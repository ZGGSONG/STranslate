using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Log;
using STranslate.Style.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace STranslate.ViewModels.Preference
{
    public partial class AboutViewModel : ObservableObject
    {
        [ObservableProperty]
        private string version = "";

        public AboutViewModel()
        {
            Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0101";
        }

        [RelayCommand]
        private void OpenLink(string url) => Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });

        [RelayCommand]
        private void CheckUpdate()
        {
            try
            {
                const string updateFolder = "Update";

                string GetPath(string fileName) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                string GetCachePath(string fileName) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, updateFolder, fileName);

                string[] requiredFiles =
                {
                    "Updater.exe",
                    "Updater.dll",
                    "Newtonsoft.Json.dll",
                    "Updater.deps.json",
                    "Updater.runtimeconfig.json"
                };

                if (!Directory.Exists(GetPath(updateFolder)))
                {
                    Directory.CreateDirectory(GetPath(updateFolder));
                }

                if (requiredFiles.All(file => File.Exists(GetPath(file))))
                {
                    foreach (var file in requiredFiles)
                    {
                        File.Copy(GetPath(file), GetCachePath(file), true);
                    }

                    Util.CommonUtil.ExecuteProgram(GetCachePath("Updater.exe"), [Version]);
                }
                else
                {
                    MessageBox_S.Show("升级程序似乎遭到破坏，请手动前往发布页查看新版本");
                    LogService.Logger.Warn("升级程序似乎遭到破坏，请手动前往发布页查看新版本");
                }
            }
            catch (Exception ex)
            {
                MessageBox_S.Show($"更新程序已打开或无法正确启动检查更新程序");
                LogService.Logger.Warn($"更新程序已打开或无法正确启动检查更新程序, {ex.Message}");
            }
        }

        /// <summary>
        /// 同步Github版本命名
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        [Obsolete]
        private static string HandleVersion(string version)
        {
            string? ret = version[..^2];
            var location = ret.LastIndexOf('.');
            ret = ret.Remove(location, 1);
            return ret;
        }
    }
}