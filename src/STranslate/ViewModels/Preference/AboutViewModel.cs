using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Log;
using STranslate.Style.Controls;
using System;
using System.Diagnostics;

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
                
            }
            catch (Exception ex)
            {
                MessageBox_S.Show($"更新程序已打开或无法正确启动检查更新程序");
                LogService.Logger.Warn($"更新程序已打开或无法正确启动检查更新程序, {ex.Message}");
            }
        }
    }
}