using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using PaddleOCRSharp;
using STranslate.Helper;
using STranslate.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.OCR
{
    public partial class PaddleOCR : ObservableObject, IOCR
    {
        #region PaddleOCR Related

        protected Architecture _architecture;
        protected PaddleOCREngine? _paddleOCREngine;

        private void InitialPaddleOCREngine()
        {
            _architecture = RuntimeInformation.OSArchitecture;

            if (_architecture != Architecture.X64)
            {
                // 如果不是64位架构，不进行初始化
                return;
            }

            // 使用默认中英文V4模型
            OCRModelConfig? config = null;

            // 使用默认参数
            OCRParameter oCRParameter = new OCRParameter
            {
                cpu_math_library_num_threads = 10, // 预测并发线程数
                enable_mkldnn = true, // web部署该值建议设置为0, 否则出错，内存如果使用很大，建议该值也设置为0.
                cls = false, // 是否执行文字方向分类；默认false
                det = true, // 是否开启方向检测，用于检测识别180旋转
                use_angle_cls = false, // 是否开启方向检测，用于检测识别180旋转
                det_db_score_mode = true // 是否使用多段线，即文字区域是用多段线还是用矩形
            };

            // 建议程序全局初始化一次即可，不必每次识别都初始化，容易报错。
            _paddleOCREngine = new PaddleOCREngine(config, oCRParameter);
        }

        #endregion PaddleOCR Related

        #region Constructor

        public PaddleOCR()
            : this(Guid.NewGuid(), "", "PaddleOCR") { }

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

            InitialPaddleOCREngine();
        }

        #endregion Constructor

        #region Properties

        [ObservableProperty]
        private Guid _identify = Guid.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private OCRType _type = OCRType.PaddleOCR;

        [JsonIgnore]
        [ObservableProperty]
        public bool _isEnabled = true;

        [JsonIgnore]
        [ObservableProperty]
        private string _name = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private IconType _icon = IconType.STranslate;

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
        public Dictionary<IconType, string> Icons { get; private set; } = ConstStr.ICONDICT;

        #endregion Properties

        #region Interface Implementation

        public Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang,  CancellationToken token)
        {
            if (lang != LangEnum.auto)
                ToastHelper.Show("该服务不支持指定语种", WindowType.OCR);

            var result = new OcrResult();
            var tcs = new TaskCompletionSource<OcrResult>();

            // 新建线程执行OCR操作
            var thread = new Thread(() =>
            {
                try
                {
                    token.ThrowIfCancellationRequested();

                    if (_paddleOCREngine is null)
                    {
                        throw new NotSupportedException($"CPU架构不支持({_architecture})");
                    }

                    var ocrResult = _paddleOCREngine.DetectText(bytes);

                    // 在耗时操作后再次检查取消标志
                    token.ThrowIfCancellationRequested();

                    ocrResult?.TextBlocks.ForEach(tb =>
                    {
                        var ocrContent = new OcrContent(tb.Text);
                        tb.BoxPoints.ForEach(bp => ocrContent.BoxPoints.Add(new BoxPoint(bp.X, bp.Y)));
                        result.OcrContents.Add(ocrContent);
                    });

                    // 设置任务结果
                    tcs.SetResult(result);
                }
                catch (OperationCanceledException)
                {
                    // 任务被取消
                    tcs.SetCanceled();
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
                thread.Abort(); // 注意：Thread.Abort() 在 .NET Core 和 .NET 5.0+ 中已经被废弃
                tcs.TrySetCanceled();
            });

            return tcs.Task;
        }


        public IOCR Clone()
        {
            return new PaddleOCR
            {
                Identify = this.Identify,
                Type = this.Type,
                IsEnabled = this.IsEnabled,
                Icon = this.Icon,
                Name = this.Name,
                Url = this.Url,
                AppID = this.AppID,
                AppKey = this.AppKey,
                Icons = this.Icons,
            };
        }

        public string? LangConverter(LangEnum lang) => null;

        #endregion Interface Implementation
    }
}
