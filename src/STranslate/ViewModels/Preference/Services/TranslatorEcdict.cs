using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Model;
using STranslate.Updater;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.Services
{
    #region Constructor

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

            HasDB = File.Exists(ConstStr.ECDICTPath);
            if (HasDB)
            {
                DbFileSize = CommonUtil.CountSize(new FileInfo(ConstStr.ECDICTPath).Length);
            }
        }

    #endregion Constructor

        #region Properties

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
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string _url = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string _appID = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: DefaultValue("")]
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string _appKey = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        public TranslationResult _data = TranslationResult.Reset;

        [JsonIgnore]
        public Dictionary<IconType, string> Icons { get; private set; } = ConstStr.ICONDICT;

        [JsonIgnore]
        public string Tips { get; set; } = "本地服务，需下载词典文件";

        [ObservableProperty]
        [JsonIgnore]
        [property: JsonIgnore]
        private double _processValue;

        [ObservableProperty]
        [JsonIgnore]
        [property: JsonIgnore]
        private bool _isShowProcessBar;

        private static readonly string CurrentPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string SourceFile = Path.Combine(CurrentPath, "ecdict-sqlite-28.zip");

        [ObservableProperty]
        [JsonIgnore]
        [property: JsonIgnore]
        private bool _hasDB;

        [ObservableProperty]
        [JsonIgnore]
        [property: JsonIgnore]
        private string _dbFileSize = "";

        #endregion Properties

        #region Methods

        [RelayCommand]
        [property: JsonIgnore]
        private async Task DownloadResource()
        {
            ProcessValue = 0;
            IsShowProcessBar = true;

            var url = "https://github.com/skywind3000/ECDICT/releases/download/1.0.28/ecdict-sqlite-28.zip";
            var httpClient = new HttpClient(new SocketsHttpHandler());

            try
            {
                if (File.Exists(SourceFile))
                {
                    ProcessValue = 100;
                    goto extract;
                }

                ToastHelper.Show("开始下载", WindowType.Preference);
                using (var response = await httpClient.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead))
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(SourceFile, FileMode.Create))
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

                extract:
                IsShowProcessBar = false;
                ToastHelper.Show("下载完成", WindowType.Preference);

                // 下载完成后的处理
                ProcessDownloadedFile();
            }
            catch (Exception)
            {
                // 下载发生异常
                ToastHelper.Show("下载时发生异常", WindowType.Preference);
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        private void ProcessDownloadedFile()
        {
            ToastHelper.Show("解压资源包", WindowType.Preference);

            var unresult = Unzip.ExtractZipFile(SourceFile, ConstStr.AppData);

            if (unresult)
            {
                ToastHelper.Show("加载资源包成功", WindowType.Preference);

                File.Delete(SourceFile);

                HasDB = true;
                DbFileSize = CommonUtil.CountSize(new FileInfo(ConstStr.ECDICTPath).Length);
            }
            else
            {
                ToastHelper.Show("解压文件时发生异常", WindowType.Preference);
            }
        }

        [RelayCommand]
        [property: JsonIgnore]
        private void DeleteResource()
        {
            try
            {
                File.Delete(ConstStr.ECDICTPath);

                ToastHelper.Show("删除成功", WindowType.Preference);

                HasDB = false;
            }
            catch (Exception)
            {
                ToastHelper.Show("文件被占用删除失败", WindowType.Preference);
            }
        }

        #endregion Methods

        #region Interface Implementation

        public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
        {
            if (!File.Exists(ConstStr.ECDICTPath))
            {
                return TranslationResult.Success("");
            }

            if (request is RequestModel req)
            {
                var source = req.SourceLang;
                var target = req.TargetLang;
                var content = req.Text;

                var isWord = StringUtil.IsWord(content);
                var isAutoToZhOrEn = source.Equals("AUTO") && (target.Equals("ZH") || target.Equals("EN"));
                if (!(isWord && isAutoToZhOrEn))
                    goto Empty;

                var result = await EcdictHelper.GetECDICTAsync(content, token);

                if (result is null)
                    goto Empty;

                return TranslationResult.Success(result.ToString());
            }

            throw new Exception($"请求数据出错: {request}");

            Empty:
            return TranslationResult.Success("");
        }

        public Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public ITranslator Clone()
        {
            return new TranslatorEcdict
            {
                Identify = this.Identify,
                Type = this.Type,
                IsEnabled = this.IsEnabled,
                Icon = this.Icon,
                Name = this.Name,
                Url = this.Url,
                Data = TranslationResult.Reset,
                AppID = this.AppID,
                AppKey = this.AppKey,
                Icons = this.Icons,
                Tips = this.Tips,
                ProcessValue = this.ProcessValue,
                IsShowProcessBar = this.IsShowProcessBar,
                HasDB = this.HasDB,
                DbFileSize = this.DbFileSize,
            };
        }

        #endregion Interface Implementation
    }
}
