using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.Translator;

public partial class TranslatorEcdict : TranslatorBase, ITranslator
{
    #region Constructor

    public TranslatorEcdict()
        : this(Guid.NewGuid(), "", "简明英汉词典")
    {
    }

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

    #endregion Constructor

    #region Properties

    [ObservableProperty] private Guid _identify = Guid.Empty;

    [JsonIgnore] [ObservableProperty] private ServiceType _type = 0;

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.Baidu;

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

    [JsonIgnore] public BindingList<UserDefinePrompt> UserDefinePrompts { get; set; } = [];

    [JsonIgnore] [ObservableProperty] private bool _autoExecute = true;

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    public TranslationResult _data = TranslationResult.Reset;

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _isExecuting;

    [JsonIgnore] public string Tips { get; set; } = "本地服务，需下载词典文件";

    [ObservableProperty] [JsonIgnore] [property: JsonIgnore]
    private double _processValue;

    [ObservableProperty] [JsonIgnore] [property: JsonIgnore]
    private bool _isShowProcessBar;

    private static readonly string CurrentPath = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string SourceFile = Path.Combine(CurrentPath, "ecdict-sqlite-28.zip");

    [ObservableProperty] [JsonIgnore] [property: JsonIgnore]
    private bool _hasDB;

    [ObservableProperty] [JsonIgnore] [property: JsonIgnore]
    private string _dbFileSize = "";

    #endregion Properties

    #region Methods

    [RelayCommand(IncludeCancelCommand = true)]
    [property: JsonIgnore]
    private async Task DownloadResourceAsync(List<object> @params, CancellationToken token)
    {
        ProcessValue = 0;
        IsShowProcessBar = true;
        var control = (ToggleButton)@params[0];
        control.IsChecked = !control.IsChecked;

        var proxy = ((GithubProxy)@params[1]).GetDescription();
        var url = $"{proxy}https://github.com/skywind3000/ECDICT/releases/download/1.0.28/ecdict-sqlite-28.zip";

        var httpClient = new HttpClient(new SocketsHttpHandler());

        try
        {
            if (File.Exists(SourceFile))
            {
                ProcessValue = 100;
                goto extract;
            }

            ToastHelper.Show("开始下载", WindowType.Preference);
            using (var response =
                   await httpClient.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead, token))
            using (var stream = await response.Content.ReadAsStreamAsync(token))
            using (var fileStream = new FileStream(SourceFile, FileMode.Create))
            {
                var totalBytes = response.Content.Headers.ContentLength ?? -1;
                long totalDownloadedByte = 0;
                var buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, token)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), token);

                    totalDownloadedByte += bytesRead;
                    var process = Math.Round((double)totalDownloadedByte / totalBytes * 100, 2);
                    ProcessValue = process;
                }
            }

            extract:
            IsShowProcessBar = false;
            ToastHelper.Show("下载完成", WindowType.Preference);

            // 下载完成后的处理
            await ProcessDownloadedFileAsync(token);
        }
        catch (OperationCanceledException)
        {
            //更新状态
            IsShowProcessBar = false;
            //删除文件
            File.Delete(SourceFile);
            //通知到用户
            ToastHelper.Show("取消下载", WindowType.Preference);
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

    private async Task ProcessDownloadedFileAsync(CancellationToken token)
    {
        ToastHelper.Show("解压资源包", WindowType.Preference);

        var unresult = await Task.Run(() => ZipUtil.DecompressToDirectory(SourceFile, ConstStr.AppData), token);

        if (unresult)
        {
            ToastHelper.Show("加载资源包成功", WindowType.Preference);

            File.Delete(SourceFile);

            HasDB = true;
            DbFileSize = CommonUtil.CountSize(new FileInfo(ConstStr.ECDICTPath).Length);
        }
        else
        {
            ToastHelper.Show("解压失败,请重启再试", WindowType.Preference);
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    private Task DeleteResourceAsync()
    {
        try
        {
            File.Delete(ConstStr.ECDICTPath);

            ToastHelper.Show("删除成功", WindowType.Preference);

            HasDB = false;
        }
        catch (Exception)
        {
            ToastHelper.Show("删除失败,请重启再试", WindowType.Preference);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    [property: JsonIgnore]
    private Task CheckResourceAsync()
    {
        DataIntegrity();

        ToastHelper.Show(HasDB ? "资源包完整" : "资源包缺失", WindowType.Preference);

        return Task.CompletedTask;
    }

    internal bool DataIntegrity()
    {
        HasDB = true;
        HasDB &= File.Exists(ConstStr.ECDICTPath);

        if (HasDB) DbFileSize = CommonUtil.CountSize(new FileInfo(ConstStr.ECDICTPath).Length);

        return HasDB;
    }

    #endregion Methods

    #region Interface Implementation

    public async Task<TranslationResult> TranslateAsync(object request, CancellationToken token)
    {
        if (!File.Exists(ConstStr.ECDICTPath)) return TranslationResult.Success("");

        if (request is not RequestModel req)
            throw new Exception($"请求数据出错: {request}");

        var content = req.Text;

        var isWord = StringUtil.IsWord(content);
        if (!isWord)
            goto Empty;

        var result = await EcdictHelper.GetECDICTAsync(content, token).ConfigureAwait(false);

        if (result is null)
            goto Empty;

        return TranslationResult.Success(result.ToString());

        Empty:
        return TranslationResult.Success("");
    }

    public Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ITranslator Clone()
    {
        return new TranslatorEcdict
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            Data = TranslationResult.Reset,
            AppID = AppID,
            AppKey = AppKey,
            AutoExecute = AutoExecute,
            Tips = Tips,
            ProcessValue = ProcessValue,
            IsShowProcessBar = IsShowProcessBar,
            HasDB = HasDB,
            DbFileSize = DbFileSize,
            IsExecuting = IsExecuting
        };
    }

    public string? LangConverter(LangEnum lang)
    {
        return null;
    }

    #endregion Interface Implementation
}