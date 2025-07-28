using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    [ObservableProperty] private bool _isDownloading;

    [ObservableProperty] private double _downloadProgress;

    private readonly DirectoryInfo _logInfo;

    public AboutViewModel()
    {
        Version = Constant.AppVersion;

        if (!Directory.Exists(Constant.LogPath))
        {
            Directory.CreateDirectory(Constant.LogPath);
        }

        _logInfo = new DirectoryInfo(Constant.LogPath);
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
        if (MessageBox_S.Show(AppLanguageManager.GetString("About.ConfirmClearAllLog"), AppLanguageManager.GetString("About.Warning"), MessageBoxButton.YesNo) != MessageBoxResult.Yes)
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
                LogService.Logger.Error($"删除日志失败: {file.Name}", e);
            }
        }

        CheckLog();

        LogService.Register();

        ToastHelper.Show(AppLanguageManager.GetString("Toast.ClearSuccess"), WindowType.Preference);
    }

    [RelayCommand]
    private void OpenLog()
    {
        Process.Start("explorer.exe", Constant.LogPath);
    }

    [RelayCommand]
    private void OpenConfig()
    {
        Process.Start("explorer.exe", Constant.CnfPath);
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
            IsChecking = true;


            var proxy = (Singleton<ConfigHelper>.Instance.CurrentConfig?.DownloadProxy ?? DownloadProxyKind.GhProxy).GetDescription();

            var result = await UpdateUtil.CheckForUpdates(proxy, token);
            if (result == null)
            {
                MessageBox_S.Show(AppLanguageManager.GetString("Constant.NeweastVersionInfo"));
                return;
            }

            var remoteVer = result?.Version ?? Constant.AppVersion;
            var desc = result?.Body ?? "";
            var newVersionInfo = $"# {AppLanguageManager.GetString("About.GetNewer")}: {remoteVer}\n{(string.IsNullOrEmpty(desc) ? "" : $"\n{desc}")}";
            var title = AppLanguageManager.GetString("MessageBox.Tip");
            if (MessageBox_S_MD.Show(newVersionInfo, title, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var downloadInfo = result!.Downloads.First(x => x.Name.EndsWith("zip"));
                var path = $"{Constant.ExecutePath}tmp";
                var file = $"{path}\\{downloadInfo.Name}";
                if (Directory.Exists(path) && File.Exists(file))
                {
                    var fi = new FileInfo(file);
                    if (fi.Length == downloadInfo.Size)
                    {
                        ExecuteUpdate(file);
                        return;
                    }

                    // 文件不完整, 删除
                    fi.Delete();
                }
                ToastHelper.Show(AppLanguageManager.GetString("About.Downloading"), WindowType.Preference);

                // 开始下载并显示进度
                IsDownloading = true;
                DownloadProgress = 0;

                var progress = new Progress<double>(value =>
                {
                    DownloadProgress = value;
                });
                var ret = await UpdateUtil.DownloadUpdateAsync(downloadInfo, path, progress, token);

                IsDownloading = false;
                LogService.Logger.Info($"软件压缩包下载完成: {ret}");
                ExecuteUpdate(ret);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            MessageBox_S.Show(AppLanguageManager.GetString("About.UpdateFailed"));
            LogService.Logger.Warn($"检查更新出错, 请检查网络情况, {e.Message}");
        }
        finally
        {
            IsChecking = false;
            IsDownloading = false;
        }
    }

    private void ExecuteUpdate(string file)
    {
        var title = AppLanguageManager.GetString("MessageBox.Tip");
        if (MessageBox_S.Show(AppLanguageManager.GetString("About.DownloadSuccess"), title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        if (!File.Exists(Constant.HostExePath))
        {
            MessageBox_S.Show(AppLanguageManager.GetString("About.NoUpdateExe"));
            return;
        }
        File.Copy(Constant.HostExePath, Constant.HostExeTmpPath, true);
        var isClearFiles = MessageBox_S.Show(AppLanguageManager.GetString("About.ClearFiles"), title, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        string[] args = isClearFiles ? ["update", "-a", file, "-w", "3", "-c", "-s"] : ["update", "-a", file, "-w", "3", "-s"];
        CommonUtil.ExecuteProgram(Constant.HostExeTmpPath, args);
        Environment.Exit(0);
    }
}