using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace STranslate.Updater;

/// <summary>
///     MainWindow.xaml 的交互逻辑
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    ///     Github相关类
    /// </summary>
    private readonly GithubRelease githubRelease;

    /// <summary>
    ///     Releases版本地址
    /// </summary>
    private readonly string ReleasesURL = "https://api.github.com/repos/zggsong/stranslate/releases/latest";

    /// <summary>
    ///     新版本保存目录路径
    /// </summary>
    private readonly string SaveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

    /// <summary>
    ///     新版本保存名字
    /// </summary>
    private readonly string SaveName = "update.zip";

    /// <summary>
    ///     MainViewModel
    /// </summary>
    private readonly MainViewModel vm;

    /// <summary>
    ///     新版本发布页路径
    /// </summary>
    private string NewVersionURL = "";

    /// <summary>
    ///     新版本下载路径
    /// </summary>
    private string NewVersionZipURL = "";

    public MainWindow(string version)
    {
        InitializeComponent();

        vm = new MainViewModel(version);

        DataContext = vm;

        githubRelease = new GithubRelease(ReleasesURL, version);

        Loaded += Window_Loaded;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Check();
    }

    /// <summary>
    ///     下载新版软件
    /// </summary>
    private async void Download()
    {
        SetStatus("正在下载新版本文件...", false);

        UpdateBtn.Visibility = Visibility.Collapsed;
        ReCheckBtn.Visibility = Visibility.Collapsed;
        ProgressBar.Visibility = Visibility.Visible;
        vm.ProcessValue = 0;

        var httpClient = new HttpClient(new SocketsHttpHandler());

        try
        {
            if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir);

            using (var response =
                   await httpClient.GetAsync(new Uri(NewVersionZipURL), HttpCompletionOption.ResponseHeadersRead))
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(Path.Combine(SaveDir, SaveName), FileMode.Create))
            {
                var totalBytes = response.Content.Headers.ContentLength ?? -1;
                long totalDownloadedByte = 0;
                var buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);

                    totalDownloadedByte += bytesRead;
                    var process = Math.Round((double)totalDownloadedByte / totalBytes * 100, 2);
                    vm.ProcessValue = process;
                }
            }

            // 下载完成后的处理
            await ProcessDownloadedFile();
        }
        catch (Exception)
        {
            // 下载发生异常
            SetStatus("下载时发生异常，请重试。", false);
            UpdateBtn.Visibility = Visibility.Visible;
        }
        finally
        {
            httpClient.Dispose();
        }
    }

    /// <summary>
    ///     处理下载好的新版软件
    /// </summary>
    /// <returns></returns>
    private async Task ProcessDownloadedFile()
    {
        // 准备更新
        var process = Process.GetProcessesByName("STranslate");
        if (process != null && process.Length > 0) process[0].Kill();

        SetStatus("下载完成，正在解压请勿关闭此窗口...");

        var unpath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.FullName;

        var unresult = await Task.Run(async () =>
        {
            await Task.Delay(3000);
            return Unzip.ExtractZipFile(Path.Combine(SaveDir, SaveName), unpath);
        });

        if (unresult)
        {
            SetStatus("更新完成！", false);
            Process.Start(Path.Combine(unpath, "STranslate.exe"));
        }
        else
        {
            SetStatus("解压文件时发生异常，请重试！通常情况可能是因为 STranslate 主程序尚未退出。", false);
            UpdateBtn.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    ///     检查更新
    /// </summary>
    private async void Check()
    {
        NewVersionSP.Visibility = Visibility.Collapsed;
        PreTag.Visibility = Visibility.Collapsed;

        SetStatus("正在检查更新");
        UpdateBtn.Visibility = Visibility.Collapsed;
        ReCheckBtn.IsEnabled = false;

        var info = await githubRelease.GetRequest();

        if (info != null)
        {
            if (githubRelease.IsCanUpdate())
            {
                UpdateBtn.Visibility = Visibility.Visible;

                NewVersionSP.Visibility = Visibility.Visible;
                Version.Text = info.Version;
                VersionTitle.Text = info.Title;
                NewVersionZipURL = info.DownloadUrl;
                NewVersionURL = info.HtmlUrl;
                if (info.IsPre) PreTag.Visibility = Visibility.Visible;
                SetStatus("检测到新的版本！", false);
            }
            else
            {
                SetStatus("目前没有可用的更新。", false);
            }
        }
        else
        {
            SetStatus("无法获取版本信息，请检查代理或网络。", false);
        }

        ReCheckBtn.IsEnabled = true;
    }

    /// <summary>
    ///     设定状态
    /// </summary>
    /// <param name="statusText"></param>
    /// <param name="isLoading"></param>
    private void SetStatus(string statusText, bool isLoading = true)
    {
        StatusLabel.Text = statusText;
        ProgressBar.IsIndeterminate = isLoading;
        if (isLoading)
            ProgressBar.Visibility = Visibility.Visible;
        else
            ProgressBar.Visibility = Visibility.Collapsed;
    }

    private void ReCheckBtn_Click(object sender, RoutedEventArgs e)
    {
        Check();
    }

    private void UpdateBtn_Click(object sender, RoutedEventArgs e)
    {
        Download();
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = NewVersionURL, UseShellExecute = true });
    }
}