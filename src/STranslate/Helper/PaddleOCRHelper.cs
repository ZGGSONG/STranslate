#if false
using OpenCvSharp;
using Sdcb.OpenVINO.PaddleOCR.Models.Online;
using Sdcb.OpenVINO.PaddleOCR.Models;
using Sdcb.OpenVINO.PaddleOCR;
using System;
using System.Threading.Tasks;

namespace STranslate.Helper
{
    public class PaddleOCRHelper
    {
        private FullOcrModel model;

        public PaddleOCRHelper()
        {
            // 在构造函数中调用异步初始化方法
            model = InitializeAsync().GetAwaiter().GetResult();
        }

        // 异步初始化方法
        private async Task<FullOcrModel> InitializeAsync() => await OnlineFullModels.ChineseServerV4.DownloadAsync();

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string Excute(byte[] bytes)
        {
            try
            {
                string ret = string.Empty;

                using Mat src = Cv2.ImDecode(bytes, ImreadModes.Color);

                if (model == null)
                    throw new Exception("模型未下载完成");

                using (PaddleOcrAll all = new(model) { AllowRotateDetection = true, Enable180Classification = true, })
                {
                    // Load local file by following code:
                    // using (Mat src2 = Cv2.ImRead(@"C:\test.jpg"))
                    //Stopwatch sw = Stopwatch.StartNew();
                    PaddleOcrResult result = all.Run(src);
                    ret = result.Text;
                    //Console.WriteLine($"elapsed={sw.ElapsedMilliseconds} ms");
                    //Console.WriteLine("Detected all texts: \n" + result.Text);
                    //foreach (PaddleOcrResultRegion region in result.Regions)
                    //{
                    //    Console.WriteLine($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectSize:    {region.Rect.Size}, Angle: {region.Rect.Angle}");
                    //}
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("OCR出错: {0}", ex.Message));
            }
        }
    }
}

#endif

#if true
using System.Runtime.InteropServices;
using System.Text;
using PaddleOCRSharp;

namespace STranslate.Helper;

[Obsolete("使用OCRSvcViewModel管理")]
public class PaddleOCRHelper : IDisposable
{
    private readonly Architecture _architecture;
    private readonly PaddleOCREngine? _paddleOCREngine;

    public PaddleOCRHelper()
    {
        _architecture = RuntimeInformation.OSArchitecture;

        if (_architecture != Architecture.X64)
            // 如果不是64位架构，不进行初始化
            return;

        // 使用默认中英文V4模型
        OCRModelConfig? config = null;

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
        _paddleOCREngine = new PaddleOCREngine(config, oCRParameter);
    }

    public void Dispose()
    {
        _paddleOCREngine?.Dispose();
    }

    /// <summary>
    ///     执行 OCR
    /// </summary>
    /// <param name="bytes">图像字节数组</param>
    /// <returns>OCR识别结果</returns>
    public string Execute(byte[] bytes)
    {
        try
        {
            if (_paddleOCREngine is null) throw new NotSupportedException($"CPU架构不支持({_architecture})");

            var sb = new StringBuilder();
            var ocrResult = _paddleOCREngine.DetectText(bytes);

            ocrResult?.TextBlocks.ForEach(x => sb.AppendLine(x.Text));

            return sb.ToString();
        }
        catch (NotSupportedException ex)
        {
            // 处理特定的不支持异常
            throw new NotSupportedException($"OCR不支持: {ex.Message}");
        }
        catch (Exception ex)
        {
            // 处理其他异常
            throw new Exception($"OCR出错: {ex.Message}");
        }
    }
}
#endif