using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using PaddleOCRSharp;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.ViewModels.Preference.OCR;

public partial class PaddleOCR : ObservableObject, IOCR
{
    #region PaddleOCR Related

    protected Tuple<LangEnum, PaddleOCREngine>? _engineDict;

    private PaddleOCREngine GetEngine(LangEnum lang = LangEnum.auto)
    {
        //获取缓存引擎
        if (_engineDict != null && _engineDict.Item1 == lang) return _engineDict.Item2;

        //如果需要创建，首先释放当前引擎资源
        _engineDict?.Item2?.Dispose();
        _engineDict = null;

        var architecture = RuntimeInformation.OSArchitecture;

        if (architecture != Architecture.X64) throw new Exception($"CPU架构不支持({architecture})");

        var config = GetOcrModelConfig(lang);

        // 使用默认参数
        var oCRParameter = new OCRParameter
        {
            cpu_math_library_num_threads = 10, // 预测并发线程数
            enable_mkldnn = true, // web部署该值建议设置为0, 否则出错，内存如果使用很大，建议该值也设置为0.
            cls = false, // 是否执行文字方向分类；默认false
            det = true, // 是否开启方向检测，用于检测识别180旋转
            use_angle_cls = false, // 是否开启方向检测，用于检测识别180旋转
            det_db_score_mode = true // 是否使用多段线，即文字区域是用多段线还是用矩形
        };

        // 建议程序全局初始化一次即可，不必每次识别都初始化，容易报错。
        var engine = new PaddleOCREngine(config, oCRParameter);

        _engineDict = new Tuple<LangEnum, PaddleOCREngine>(lang, engine);
        return engine;
    }

    private OCRModelConfig? GetOcrModelConfig(LangEnum lang = LangEnum.auto)
    {
        return lang switch
        {
            LangEnum.ja
                => new OCRModelConfig
                {
                    det_infer = ConstStr.PaddleOcrModelPath + "Multilingual_PP-OCRv3_det_slim_infer",
                    rec_infer = ConstStr.PaddleOcrModelPath + "japan_PP-OCRv3_rec_infer",
                    cls_infer = ConstStr.PaddleOcrModelPath + "ch_ppocr_mobile_v2.0_cls_infer",
                    keys = ConstStr.PaddleOcrModelPath + "japan_dict.txt"
                },
            LangEnum.ko
                => new OCRModelConfig
                {
                    det_infer = ConstStr.PaddleOcrModelPath + "Multilingual_PP-OCRv3_det_slim_infer",
                    rec_infer = ConstStr.PaddleOcrModelPath + "korean_PP-OCRv3_rec_infer",
                    cls_infer = ConstStr.PaddleOcrModelPath + "ch_ppocr_mobile_v2.0_cls_infer",
                    keys = ConstStr.PaddleOcrModelPath + "korean_dict.txt"
                },
            // 使用默认中英文V4模型
            _ => null
        };
    }

    #endregion PaddleOCR Related

    #region Constructor

    public PaddleOCR()
        : this(Guid.NewGuid(), "", "PaddleOCR", isEnabled: false)
    {
    }

    public PaddleOCR(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.PaddleOCR,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        OCRType type = OCRType.PaddleOCR
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

    [JsonIgnore] [ObservableProperty] private OCRType _type = OCRType.PaddleOCR;

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.STranslate;

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

    [JsonIgnore] public Dictionary<IconType, string> Icons { get; private set; } = ConstStr.ICONDICT;

    [ObservableProperty] [JsonIgnore] [property: JsonIgnore]
    private bool _hasData;

    [ObservableProperty] [JsonIgnore] [property: JsonIgnore]
    private double _processValue;

    [ObservableProperty] [JsonIgnore] [property: JsonIgnore]
    private bool _isShowProcessBar;

    private const string PaddleOcrFile = "stranslate_paddleocr_data_v4.3.zip";
    private static readonly string CurrentPath = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string SourceFile = Path.Combine(CurrentPath, PaddleOcrFile);

    #endregion Properties

    #region Command

    [RelayCommand(IncludeCancelCommand = true)]
    [property: JsonIgnore]
    private async Task DownloadAsync(List<object> @params, CancellationToken token)
    {
        ProcessValue = 0;
        IsShowProcessBar = true;
        var control = (ToggleButton)@params[0];
        control.IsChecked = !control.IsChecked;

        var proxy = ((GithubProxy)@params[1]).GetDescription();
        var url =
            $"{proxy}https://github.com/ZGGSONG/STranslate/releases/download/0.01/{PaddleOcrFile}";

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
        ToastHelper.Show("解压数据包", WindowType.Preference);

        var result = await Task.Run(() => ZipUtil.DecompressToDirectory(SourceFile, CurrentPath), token);

        if (result)
        {
            ToastHelper.Show("加载数据包成功", WindowType.Preference);

            File.Delete(SourceFile);

            HasData = true;
        }
        else
        {
            ToastHelper.Show("解压失败,请重启再试", WindowType.Preference);
        }
    }

    [RelayCommand]
    [property: JsonIgnore]
    private Task CheckDataAsync()
    {
        DataIntegrity();

        ToastHelper.Show(HasData ? "数据完整" : "数据缺失", WindowType.Preference);

        return Task.CompletedTask;
    }

    internal bool DataIntegrity()
    {
#if DEBUG
        HasData = true;
        return HasData;
#else
        HasData = true;
        // 使用绝对路径进行检查
        HasData &= Directory.Exists(ConstStr.PaddleOcrModelPath);
        ConstStr.PaddleOcrDlls.ForEach(x => HasData &= File.Exists(string.Format("{0}{1}", ConstStr.ExecutePath, x)));
        return HasData;
#endif
    }

    /// <summary>
    ///     一些库存在运行时被依赖占用无法删除: vcruntime140_1.dll
    /// </summary>
    /// <returns></returns>
    [Obsolete]
    [RelayCommand]
    [property: JsonIgnore]
    private Task DeleteDataAsync()
    {
        try
        {
            ConstStr.PaddleOcrDlls.ForEach(File.Delete);
            Directory.Delete(ConstStr.PaddleOcrModelPath, true);    // 使用绝对路径

            ToastHelper.Show("删除成功", WindowType.Preference);

            HasData = false;
        }
        catch (Exception)
        {
            ToastHelper.Show("删除失败,请重启再试", WindowType.Preference);
        }

        return Task.CompletedTask;
    }

    #endregion Command

    #region Interface Implementation

    public Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken token)
    {
        if (LangConverter(lang) == null)
            ToastHelper.Show($"不支持 {lang.GetDescription()}", WindowType.OCR);

        if (!DataIntegrity())
        {
            var msg = "离线数据不完整,请前往PaddleOCR配置页面进行下载";

            ToastHelper.Show(msg, WindowType.OCR);

            LogService.Logger.Error($"PaddleOCR{msg}，请检查下载后重试...");

            return Task.FromResult(OcrResult.Fail(msg));
        }

        var result = new OcrResult();
        var tcs = new TaskCompletionSource<OcrResult>();

        // 新建线程执行OCR操作
        var thread = new Thread(() =>
        {
            try
            {
                token.ThrowIfCancellationRequested();

                using (var _ = new TimerDisposable(timeElapsed => LogService.Logger.Debug($"PaddleOCR 耗时: {timeElapsed} ms")))
                {
                    var ocrResult = GetEngine(lang).DetectText(bytes);

                    // 在耗时操作后再次检查取消标志
                    token.ThrowIfCancellationRequested();

                    ocrResult?.TextBlocks.ForEach(tb =>
                    {
                        var ocrContent = new OcrContent(tb.Text);
                        tb.BoxPoints.ForEach(bp => ocrContent.BoxPoints.Add(new BoxPoint(bp.X, bp.Y)));
                        result.OcrContents.Add(ocrContent);
                    });
                }

                // 设置任务结果
                tcs.SetResult(result);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ThreadInterruptedException)
            {
            }
            catch (NotSupportedException ex)
            {
                // 处理特定的不支持异常
                tcs.SetException(new NotSupportedException($"OCR不支持: {ex.Message}"));
            }
            catch (Exception ex)
            {
                // 处理其他异常
                tcs.SetException(new Exception($"OCR出错: {ex.Message}"));
            }
        })
        {
            IsBackground = true
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start(); // 启动线程

        // 注册取消回调以便在取消时中止线程
        token.Register(() =>
        {
            thread.Interrupt();
            tcs.TrySetCanceled();
        });
        return tcs.Task;
    }

    public IOCR Clone()
    {
        return new PaddleOCR
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            AppID = AppID,
            AppKey = AppKey,
            HasData = HasData
        };
    }

    public string? LangConverter(LangEnum lang)
    {
        return lang switch
        {
            LangEnum.auto => "",
            LangEnum.zh_cn => "",
            LangEnum.zh_tw => "",
            LangEnum.yue => "",
            LangEnum.en => "",
            LangEnum.ja => "",
            LangEnum.ko => "",
            _ => null
        };
    }

    #endregion Interface Implementation
}