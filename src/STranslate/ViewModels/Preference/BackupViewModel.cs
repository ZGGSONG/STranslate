using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.Views.Preference;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WebDav;

namespace STranslate.ViewModels.Preference
{
    public partial class BackupViewModel : ObservableObject
    {
        private WebDavViewModel webDavVM = Singleton<WebDavViewModel>.Instance;

        /// <summary>
        /// 当前配置实例
        /// </summary>
        private readonly ConfigHelper configHelper = Singleton<ConfigHelper>.Instance;

        private ConfigModel? _curConfig;

        public BackupViewModel() => Init();

        private void Init()
        {
            _curConfig = configHelper.CurrentConfig;
            BackupType = _curConfig?.BackupType ?? BackupType.Local;
            WebDavUrl = _curConfig?.WebDavUrl ?? string.Empty;
            WebDavUsername = _curConfig?.WebDavUsername ?? string.Empty;
            WebDavPassword = _curConfig?.WebDavPassword ?? string.Empty;
        }

        /// <summary>
        /// 备份、恢复方式
        /// </summary>
        [ObservableProperty]
        private BackupType _backupType;

        /// <summary>
        /// 是否启用代理认证
        /// </summary>
        [ObservableProperty]
        private string _webDavUrl = "";

        /// <summary>
        /// WebDav认证用户名
        /// </summary>
        [ObservableProperty]
        private string _webDavUsername = "";

        /// <summary>
        /// WebDav认证密码
        /// </summary>
        [ObservableProperty]
        private string _webDavPassword = "";

        /// <summary>
        /// 显示/隐藏密码
        /// </summary>
        [JsonIgnore]
        [ObservableProperty]
        [property: JsonIgnore]
        private bool _isWebDavPasswordHide = true;

        private RelayCommand<string>? showEncryptInfoCommand;

        /// <summary>
        /// 显示/隐藏密码Command
        /// </summary>
        [JsonIgnore]
        public IRelayCommand<string> ShowEncryptInfoCommand => showEncryptInfoCommand ??= new RelayCommand<string>(new Action<string?>(ShowEncryptInfo));

        private void ShowEncryptInfo(string? obj)
        {
            if (obj != null && obj.Equals(nameof(WebDavPassword)))
            {
                IsWebDavPasswordHide = !IsWebDavPasswordHide;
            }
        }

        [RelayCommand]
        private void Backup()
        {
            switch (BackupType)
            {
                case BackupType.Local:
                    LocalBackup();
                    break;

                case BackupType.WebDav:
                    WebDavBackup();
                    break;

                default:
                    break;
            }
            Save();
        }

        [RelayCommand]
        private async Task Restore()
        {
            switch (BackupType)
            {
                case BackupType.Local:
                    LocalRestore();
                    break;

                case BackupType.WebDav:
                    await WebDavRestore();
                    break;

                default:
                    break;
            }
            Save();
        }

        #region 功能实现

        protected void Save()
        {
            if (!configHelper.WriteConfig(this))
            {
                LogService.Logger.Debug($"保存配置失败，{JsonConvert.SerializeObject(this)}");
            }
        }

        private void LocalBackup()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "zip(*.zip)|*.zip",
                FileName = $"stranslate_backup_{DateTime.Now:yyyyMMddHHmmss}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                //文件内容读取
                string zipFilePath = saveFileDialog.FileName;
                ZipUtil.CompressFile(ConstStr.CnfFullName, zipFilePath);

                ToastHelper.Show("导出成功", WindowType.Preference);
            }
        }

        private void WebDavBackup()
        {
            // 测试连接是否成功

            // 压缩

            // 上传

            // 打印通知

        }

        private void LocalRestore()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "zip(*.zip)|*.zip"
            };

            if (openFileDialog.ShowDialog() != true) return;
            //文件内容读取
            var zipFile = openFileDialog.FileName;
            var rd = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var tmpPath = Path.Combine(ConstStr.ExecutePath, rd);
            ZipUtil.DecompressToDirectory(zipFile, tmpPath);

            //提取后进行校验
            if (!Verification(tmpPath))
            {
                ToastHelper.Show("导入失败", WindowType.Preference);
                return;
            }
                
            //重新初始化ConfigHelper操作
            Singleton<ConfigHelper>.Instance.InitialCurntCnf();
            Singleton<ConfigHelper>.Instance.InitialOperate();

            //配置页面初始化
            Init();
            Singleton<MainViewModel>.Instance.Reset();
            Singleton<InputViewModel>.Instance.Clear();
            Singleton<OutputViewModel>.Instance.Reset();
            Singleton<CommonViewModel>.Instance.ResetCommand.Execute(null);
            Singleton<ServiceViewModel>.Instance.ResetCommand.Execute(null);
            Singleton<OCRScvViewModel>.Instance.ResetCommand.Execute(null);
            Singleton<TTSViewModel>.Instance.ResetCommand.Execute(null);
            //TODO: hotkey

            ToastHelper.Show("导入成功", WindowType.Preference);
        }

        private async Task WebDavRestore()
        {
            var uri = new Uri(WebDavUrl);
            var absolutePath = $"{uri.LocalPath.TrimEnd('/')}/{ConstStr.CNFNAME}";

            var clientParams = new WebDavClientParams
            {
                BaseAddress = uri,
                Credentials = new NetworkCredential(WebDavUsername, WebDavPassword)
            };
            var client = new WebDavClient(clientParams);
            var linkTest = await client.Propfind(string.Empty);
            if (!linkTest.IsSuccessful)
            {
                ToastHelper.Show("请检查配置是否正确");
                return;
            }

            var ret = await client.Propfind(absolutePath);
            if (!ret.IsSuccessful)
            {
                await client.Mkcol(absolutePath);
            }
            else
            {
                foreach (var res in ret.Resources)
                {
                    if (res.IsCollection || !res.Uri.Contains(ConstStr.CNFNAME)) continue;

                    var name = res.Uri.Replace(absolutePath, "").Trim('/');
                    webDavVM.WebDavResultList.Add(name);
                }
            }

            var dialog = new WebDavDialog(client, absolutePath);
            dialog.ShowDialog();
            webDavVM.WebDavResultList.Clear();
        }


        private bool Verification(string tmpPath)
        {
            var ret = false;
            try
            {
                var root = new DirectoryInfo(tmpPath);
                foreach (var info in root.GetFiles())
                {
                    if (info.Extension != ConstStr.CNFEXTENSION && info.Name != ConstStr.CNFNAME) continue;

                    ret = Singleton<ConfigHelper>.Instance.VerificateConfig(info.FullName);

                    if (!ret) return ret;

                    File.Move(info.FullName, ConstStr.CnfFullName, true);

                    return ret;
                }
            }
            finally
            {
                Directory.Delete(tmpPath, true);
            }
            return ret;
        }
        #endregion 功能实现
    }
}
