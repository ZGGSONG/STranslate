using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Updater
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        GithubRelease githubRelease;

        /// <summary>
        /// 新版本保存目录路径
        /// </summary>
        private string SaveDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        /// <summary>
        /// 新版本保存名字
        /// </summary>
        private string SaveName = "update.zip";
        /// <summary>
        /// 新版本下载路径
        /// </summary>
        private string NewVersionZipURL;
        /// <summary>
        /// 新版本发布页路径
        /// </summary>
        private string NewVersionURL;
        private MainViewModel _mainViewModel;
        public MainWindow()
        {
            InitializeComponent();

            _mainViewModel = new MainViewModel();

            DataContext = _mainViewModel;

            _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
        }

        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_mainViewModel.Version))
            {
                githubRelease = new GithubRelease("https://api.github.com/repos/zggsong/stranslate/releases/latest", _mainViewModel.Version);

                Check();
            }
        }
        private async void Download()
        {
            SetStatus("正在下载新版本文件...", false);

            UpdateBtn.Visibility = Visibility.Collapsed;
            ReCheckBtn.Visibility = Visibility.Collapsed;
            ProgressBar.Visibility = Visibility.Visible;
            _mainViewModel.ProcessValue = 0;

            var res = await Task.Run(() =>
            {
                try
                {
                    //  确认保存目录
                    if (!Directory.Exists(SaveDir))
                    {
                        Directory.CreateDirectory(SaveDir);
                    }

                    Uri uri = new Uri(NewVersionZipURL);
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                    httpWebRequest.Timeout = 120 * 1000;
                    HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();


                    long totalBytes = httpWebResponse.ContentLength;

                    Stream st = httpWebResponse.GetResponseStream();
                    Stream so = new FileStream(System.IO.Path.Combine(SaveDir, SaveName), FileMode.Create);

                    long totalDownloadedByte = 0;
                    byte[] by = new byte[1024];
                    int osize = st.Read(by, 0, (int)by.Length);
                    while (osize > 0)
                    {

                        totalDownloadedByte = osize + totalDownloadedByte;
                        so.Write(by, 0, osize);

                        osize = st.Read(by, 0, (int)by.Length);

                        //进度计算
                        double process = double.Parse(String.Format("{0:F}",
                               ((double)totalDownloadedByte / (double)totalBytes * 100)));
                        _mainViewModel.ProcessValue = process;
                        //Debug.WriteLine(ProcessValue);
                    }
                    //关闭资源
                    httpWebResponse.Close();
                    so.Close();
                    st.Close();

                    return true;

                }
                catch (Exception)
                {
                    return false;
                }
            });

            if (res)
            {
                //  准备更新
                var process = Process.GetProcessesByName("STranslate");
                if (process != null && process.Length > 0)
                {
                    process[0].Kill();
                }

                SetStatus("下载完成，正在解压请勿关闭此窗口...");

                string unpath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;

                var unresult = await Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    return Unzip.ExtractZipFile(System.IO.Path.Combine(SaveDir, SaveName), unpath);
                });
                if (unresult)
                {
                    SetStatus("更新完成！", false);
                    Process p = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo(System.IO.Path.Combine(unpath, "STranslate.exe"));
                    p.StartInfo = startInfo;
                    p.Start();
                }
                else
                {
                    SetStatus("解压文件时发生异常，请重试！通常情况可能是因为 STranslate 主程序尚未退出。", false);
                    UpdateBtn.Visibility = Visibility.Visible;

                }
            }
            else
            {
                //下载发生异常
                SetStatus("下载时发生异常，请重试。", false);
                UpdateBtn.Visibility = Visibility.Visible;
            }
        }


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
                    if (info.IsPre)
                    {
                        PreTag.Visibility = Visibility.Visible;
                    }
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

        private void SetStatus(string statusText, bool isLoading = true)
        {
            StatusLabel.Text = statusText;
            ProgressBar.IsIndeterminate = isLoading;
            if (isLoading)
            {
                ProgressBar.Visibility = Visibility.Visible;
            }
            else
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            }
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
            Process.Start(new ProcessStartInfo(NewVersionURL));
        }
    }
}
