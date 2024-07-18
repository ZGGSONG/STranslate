using System.IO;
using System.Net;
using System.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.Views.Preference;
using WebDav;

namespace STranslate.ViewModels.Preference;

public partial class BackupViewModel : ObservableObject
{
    /// <summary>
    ///     当前配置实例
    /// </summary>
    private readonly ConfigHelper _configHelper = Singleton<ConfigHelper>.Instance;

    private readonly WebDavViewModel _webDavVm = Singleton<WebDavViewModel>.Instance;

    /// <summary>
    ///     备份、恢复方式
    /// </summary>
    [ObservableProperty] private BackupType _backupType;

    private ConfigModel? _curConfig;

    /// <summary>
    ///     显示/隐藏密码
    /// </summary>
    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _isWebDavPasswordHide = true;

    private RelayCommand<string>? _showEncryptInfoCommand;

    /// <summary>
    ///     WebDav认证密码
    /// </summary>
    [ObservableProperty] private string _webDavPassword = "";

    /// <summary>
    ///     是否启用代理认证
    /// </summary>
    [ObservableProperty] private string _webDavUrl = "";

    /// <summary>
    ///     WebDav认证用户名
    /// </summary>
    [ObservableProperty] private string _webDavUsername = "";

    public BackupViewModel()
    {
        Init();
    }

    /// <summary>
    ///     显示/隐藏密码Command
    /// </summary>
    [JsonIgnore]
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        _showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

    private void Init()
    {
        _curConfig = _configHelper.CurrentConfig;
        BackupType = _curConfig?.BackupType ?? BackupType.Local;
        WebDavUrl = _curConfig?.WebDavUrl ?? string.Empty;
        WebDavUsername = _curConfig?.WebDavUsername ?? string.Empty;
        WebDavPassword = _curConfig?.WebDavPassword ?? string.Empty;
    }

    private void ShowEncryptInfo(string? obj)
    {
        if (obj != null && obj.Equals(nameof(WebDavPassword))) IsWebDavPasswordHide = !IsWebDavPasswordHide;
    }

    [RelayCommand]
    private async Task BackupAsync()
    {
        switch (BackupType)
        {
            case BackupType.Local:
                LocalBackup();
                break;

            case BackupType.WebDav:
                await WebDavBackupAsync();
                break;
        }

        Save();
    }

    [RelayCommand]
    private async Task RestoreAsync()
    {
        switch (BackupType)
        {
            case BackupType.Local:
                LocalRestore();
                break;

            case BackupType.WebDav:
                await WebDavRestoreAsync();
                break;
        }

        Save();
    }

    [RelayCommand]
    private void Save()
    {
        if (!_configHelper.WriteConfig(this)) LogService.Logger.Debug($"保存配置失败，{JsonConvert.SerializeObject(this)}");
    }

    #region 导出

    private void LocalBackup()
    {
        var saveFileDialog = new SaveFileDialog
            { Filter = "zip(*.zip)|*.zip", FileName = $"stranslate_backup_{DateTime.Now:yyyyMMddHHmmss}" };

        if (saveFileDialog.ShowDialog() == true)
        {
            //文件内容读取
            var zipFilePath = saveFileDialog.FileName;
            ZipUtil.CompressFile(ConstStr.CnfFullName, zipFilePath);

            ToastHelper.Show("导出成功", WindowType.Preference);
        }
    }

    private async Task WebDavBackupAsync()
    {
        // 测试连接是否成功
        var cRet = await CreateClientAsync();
        if (!cRet.Item3)
        {
            ToastHelper.Show("请检查配置是否正确", WindowType.Preference);
            return;
        }

        var client = cRet.Item1;
        var absolutePath = cRet.Item2;

        // 压缩
        var fn = $"stranslate_backup_{DateTime.Now:yyyyMMddHHmmss}.zip";
        var rd = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var tmpPath = Path.Combine(ConstStr.ExecutePath, rd);
        //如果目录不存在则创建
        if (!Directory.Exists(tmpPath)) Directory.CreateDirectory(tmpPath);

        var zipFilePath = Path.Combine(tmpPath, fn);
        //先压缩文件到程序暂存目录
        ZipUtil.CompressFile(ConstStr.CnfFullName, zipFilePath);
        try
        {
            // 上传
            var response = await client.PutFile($"{absolutePath}/{fn}", FileUtil.FileToStream(zipFilePath));

            // 打印通知
            if (response.IsSuccessful && response.StatusCode == 201)
                ToastHelper.Show("导出成功", WindowType.Preference);
            else
                ToastHelper.Show("导出失败", WindowType.Preference);
        }
        finally
        {
            // 最后都要删除目录
            Directory.Delete(tmpPath, true);
        }
    }

    #endregion 导出

    #region 导入

    private void LocalRestore()
    {
        var openFileDialog = new OpenFileDialog { Filter = "zip(*.zip)|*.zip" };

        //创建暂存目录
        var rd = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var tmpPath = Path.Combine(ConstStr.ExecutePath, rd);
        if (!Directory.Exists(tmpPath)) Directory.CreateDirectory(tmpPath);
        try
        {
            if (openFileDialog.ShowDialog() != true)
                return;
            //文件内容读取
            var zipFile = openFileDialog.FileName;

            // 解压到程序暂存目录
            if (ZipUtil.DecompressToDirectory(zipFile, tmpPath))
                // 执行恢复操作
                BackOperate(tmpPath);
        }
        finally
        {
            Directory.Delete(tmpPath, true);
        }
    }

    private async Task WebDavRestoreAsync()
    {
        var cRet = await CreateClientAsync();
        if (!cRet.Item3)
        {
            ToastHelper.Show("请检查配置是否正确", WindowType.Preference);
            return;
        }

        var client = cRet.Item1;
        var absolutePath = cRet.Item2;

        var rd = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var tmpPath = Path.Combine(ConstStr.ExecutePath, rd);
        //如果目录不存在则创建
        if (!Directory.Exists(tmpPath)) Directory.CreateDirectory(tmpPath);

        try
        {
            //检查该路径是否存在
            var ret = await client.Propfind(absolutePath);
            if (!ret.IsSuccessful)
                //不存在则创建目录
                await client.Mkcol(absolutePath);
            else
                //添加结果到viewmodel
                foreach (var res in ret.Resources)
                {
                    if (res.IsCollection || !res.Uri.Contains(ConstStr.CNFNAME))
                        continue;

                    var fullName = res.Uri.Replace(absolutePath, "").Trim('/');
                    //html解码以显示中文
                    var decodeFullName = HttpUtility.UrlDecode(fullName);
                    _webDavVm.WebDavResultList.Add(new WebDavResult(decodeFullName));
                }

            var dialog = new WebDavDialog(client, absolutePath, tmpPath);
            if (dialog.ShowDialog() == true) BackOperate(tmpPath);
            _webDavVm.WebDavResultList.Clear();
        }
        finally
        {
            Directory.Delete(tmpPath, true);
        }
    }

    #endregion 导入

    #region 共有操作

    /// <summary>
    ///     创建WebDavClient
    /// </summary>
    /// <returns></returns>
    private async Task<Tuple<WebDavClient, string, bool>> CreateClientAsync()
    {
        var uri = new Uri(WebDavUrl);
        var absolutePath = $"{uri.LocalPath.TrimEnd('/')}/{ConstStr.CNFNAME}";

        var clientParams = new WebDavClientParams
        {
            Timeout = TimeSpan.FromSeconds(10),
            BaseAddress = uri,
            Credentials = new NetworkCredential(WebDavUsername, WebDavPassword)
        };
        var client = new WebDavClient(clientParams);
        var linkTest = await client.Propfind(string.Empty);
        if (!linkTest.IsSuccessful)
            return new Tuple<WebDavClient, string, bool>(new WebDavClient(), string.Empty, false);

        return new Tuple<WebDavClient, string, bool>(client, absolutePath, true);
    }

    /// <summary>
    ///     导入操作
    /// </summary>
    /// <param name="tmpPath"></param>
    private void BackOperate(string tmpPath)
    {
        //提取后进行校验
        if (!Verification(tmpPath))
        {
            ToastHelper.Show("导入失败", WindowType.Preference);
            return;
        }

        //重新初始化ConfigHelper操作
        Singleton<ConfigHelper>.Instance.InitCurrentCnf();
        Singleton<ConfigHelper>.Instance.InitialOperate();

        //配置页面初始化
        Init();
        Singleton<MainViewModel>.Instance.Reset();
        Singleton<InputViewModel>.Instance.Clear();
        Singleton<CommonViewModel>.Instance.ResetCommand.Execute(null);
        Singleton<TranslatorViewModel>.Instance.ResetCommand.Execute(null);
        Singleton<ReplaceViewModel>.Instance.ResetCommand.Execute(null);
        Singleton<OCRScvViewModel>.Instance.ResetCommand.Execute(null);
        Singleton<TTSViewModel>.Instance.ResetCommand.Execute(null);

        //hotkey
        Singleton<HotkeyViewModel>.Instance.ResetCommand.Execute(null);

        ToastHelper.Show("导入成功", WindowType.Preference);
    }

    /// <summary>
    /// </summary>
    /// <param name="tmpPath"></param>
    /// <returns></returns>
    private bool Verification(string tmpPath)
    {
        var ret = false;
        try
        {
            var root = new DirectoryInfo(tmpPath);
            foreach (var info in root.GetFiles())
            {
                if (info.Extension != ConstStr.CNFEXTENSION && info.Name != ConstStr.CNFNAME)
                    continue;

                ret = Singleton<ConfigHelper>.Instance.VerificateConfig(info.FullName);

                if (!ret)
                    return ret;

                File.Move(info.FullName, ConstStr.CnfFullName, true);

                return ret;
            }
        }
        catch
        {
            // ignored
        }

        return ret;
    }

    #endregion 共有操作
}