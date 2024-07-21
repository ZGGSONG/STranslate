using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Style.Controls;
using STranslate.Util;

namespace STranslate.ViewModels.Preference;

public partial class AboutViewModel : ObservableObject
{
    [ObservableProperty] private bool _isChecking;

    [ObservableProperty] private string _version = "";

    [ObservableProperty] private string _fileSize = "";

    private readonly DirectoryInfo _logInfo;

    public AboutViewModel()
    {
        Version = ConstStr.AppVersion;

        if (!Directory.Exists(ConstStr.LogPath))
        {
            Directory.CreateDirectory(ConstStr.LogPath);
        }

        _logInfo = new DirectoryInfo(ConstStr.LogPath);
    }

    [RelayCommand]
    private void CheckLog()
    {
        var length = _logInfo.GetFiles().Sum(f => f.Length);
        FileSize = CommonUtil.CountSize(length);
    }

    [RelayCommand]
    private void CleanLog()
    {
        if (MessageBox_S.Show("确定要清理所有日志吗?", "警告", MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
            return;

        LogService.UnRegister();

        foreach (var file in _logInfo.GetFiles())
        {
            try
            {
                File.Delete(file.FullName);
            }
            catch (Exception e)
            {
                LogService.Logger.Error($"Delete Log File Failed: {file.Name}", e);
            }
        }

        CheckLog();

        LogService.Register();

        ToastHelper.Show("清理成功", WindowType.Preference);
    }

    [RelayCommand]
    private void OpenLog()
    {
        Process.Start("explorer.exe", ConstStr.LogPath);
    }

    [RelayCommand] private void OpenConfig()
    {
        Process.Start("explorer.exe", ConstStr.AppData);
    }

    [RelayCommand]
    private void OpenLink(string url)
    {
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task CheckUpdateAsync(CancellationToken token)
    {
        try
        {
            const string updateFolder = "Update";

            string GetPath(string fileName)
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            }

            string GetCachePath(string fileName)
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, updateFolder, fileName);
            }

            string[] requiredFiles = ["Updater.exe"];

            if (requiredFiles.All(file => File.Exists(GetPath(file))))
            {
                Directory.CreateDirectory(GetPath(updateFolder));

                foreach (var file in requiredFiles) File.Copy(GetPath(file), GetCachePath(file), true);

                CommonUtil.ExecuteProgram(GetCachePath("Updater.exe"), [Version]);
            }
            else
            {
                throw new Exception("升级程序似乎遭到破坏，请手动前往发布页查看新版本");
            }
        }
        catch (Exception ex)
        {
            try
            {
                IsChecking = true;
                var resp = await HttpUtil.GetAsync(ConstStr.GITHUBRELEASEURL, token);
                var parseData = JsonConvert.DeserializeObject<JObject>(resp);
                var remoteVer = parseData?["tag_name"]?.ToString() ?? ConstStr.DEFAULTVERSION;
                var canUpdate = StringUtil.IsCanUpdate(remoteVer, Version);
                MessageBox_S.Show(canUpdate ? $"检测到最新版本: {remoteVer}\n当前版本: {Version}" : "恭喜您, 当前为最新版本!");
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                MessageBox_S.Show("检查更新出错, 请检查网络情况");
                LogService.Logger.Warn($"检查更新出错, 请检查网络情况, {e.Message}");
            }
            finally
            {
                IsChecking = false;
            }

            LogService.Logger.Warn($"更新程序已打开或无法正确启动检查更新程序, {ex.Message}");
        }
    }
}