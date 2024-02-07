using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Model;
using STranslate.Updater;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STranslate.ViewModels.Preference.Services
{
    public partial class TranslatorEcdict : ObservableObject, ITranslator
    {
        public TranslatorEcdict()
            : this(Guid.NewGuid(), "", "简明英汉词典") { }

        public TranslatorEcdict(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.Ecdict,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            ServiceType type = ServiceType.EcdictService
        )
        {
            Identify = guid;
            Url = url;
            Name = name;
            Icon = icon;
            AppID = appID;
            AppKey = appKey;
            IsEnabled = isEnabled;
            Type = type;
        }

        [ObservableProperty]
        private Guid _identify = Guid.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private ServiceType _type = 0;

        [JsonIgnore]
        [ObservableProperty]
        public bool _isEnabled = true;

        [JsonIgnore]
        [ObservableProperty]
        private string _name = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private IconType _icon = IconType.Baidu;

        [JsonIgnore]
        [ObservableProperty]
        public string _url = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _AppID = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        public string _appKey = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        public TranslationResult _data = TranslationResult.Reset;

        [JsonIgnore]
        public List<IconType> Icons { get; private set; } = Enum.GetValues(typeof(IconType)).OfType<IconType>().ToList();

        #region Show/Hide Encrypt Info

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        private bool _idHide = true;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        private bool _keyHide = true;

        private void ShowEncryptInfo(string? obj)
        {
            if (obj == null)
                return;

            if (obj.Equals(nameof(AppID)))
            {
                IdHide = !IdHide;
            }
            else if (obj.Equals(nameof(AppKey)))
            {
                KeyHide = !KeyHide;
            }
        }

        private RelayCommand<string>? showEncryptInfoCommand;

        [JsonIgnore]
        public IRelayCommand<string> ShowEncryptInfoCommand => showEncryptInfoCommand ??= new RelayCommand<string>(new Action<string?>(ShowEncryptInfo));

        #endregion Show/Hide Encrypt Info

        [JsonIgnore]
        public string Tips { get; set; } = "本地服务，需下载词典文件";

        [ObservableProperty]
        private double _processValue;

        [ObservableProperty]
        private bool isShowProcessBar;

        [RelayCommand]
        private async Task DownloadResource()
        {
            IsShowProcessBar = true;

            var url = "https://github.com/skywind3000/ECDICT/releases/download/1.0.28/ecdict-sqlite-28.zip";
            var httpClient = new HttpClient(new SocketsHttpHandler());

            ToastHelper.Show("开始下载", WindowType.Preference);
            try
            {
                using (var response = await httpClient.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead))
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ecdict-sqlite-28.zip"), FileMode.Create))
                {
                    long totalBytes = response.Content.Headers.ContentLength ?? -1;
                    long totalDownloadedByte = 0;
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);

                        totalDownloadedByte += bytesRead;
                        double process = Math.Round((double)totalDownloadedByte / totalBytes * 100, 2);
                        ProcessValue = process;
                    }
                }
                IsShowProcessBar = false;

                // 下载完成后的处理
                await ProcessDownloadedFile();
            }
            catch (Exception)
            {
                // 下载发生异常
                ToastHelper.Show("下载时发生异常，请重试。", WindowType.Preference);
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        private async Task ProcessDownloadedFile()
        {
            string unpath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.FullName;

            var unresult = await Task.Run(async () =>
            {
                await Task.Delay(3000);
                return Unzip.ExtractZipFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ecdict-sqlite-28.zip"), unpath);
            });

            if (unresult)
            {
                ToastHelper.Show("下载资源包完成！", WindowType.Preference);
            }
            else
            {
                ToastHelper.Show("解压文件时发生异常，请重试！", WindowType.Preference);
            }
        }

        public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
        {
            if (request is RequestModel req)
            {
                var source = req.SourceLang;
                var target = req.TargetLang;
                var content = req.Text;

                var isWord = StringUtil.IsWord(content);
                var isAutoToZhOrEn = source.Equals("AUTO") && (target.Equals("ZH") || target.Equals("EN"));
                if (!(isWord && isAutoToZhOrEn)) goto Empty;

                var result = await EcdictHelper.GetECDICTAsync(content, token);

                if (result is null) goto Empty;


                return TranslationResult.Success(result.ToString());
            }

            throw new Exception($"请求数据出错: {request}");

        Empty:
            return TranslationResult.Fail("");
        }

        public Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}