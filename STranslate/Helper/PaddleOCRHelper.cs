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
using PaddleOCRSharp;
using System;
using System.Text;

namespace STranslate.Helper
{
    public class PaddleOCRHelper : IDisposable
    {
        private PaddleOCREngine paddleOCREngine;

        public PaddleOCRHelper()
        {
            //使用默认中英文V4模型
            OCRModelConfig? config = null;

            //使用默认参数
            OCRParameter oCRParameter = new OCRParameter();
            oCRParameter.cpu_math_library_num_threads = 10;//预测并发线程数
            oCRParameter.enable_mkldnn = true;//web部署该值建议设置为0,否则出错，内存如果使用很大，建议该值也设置为0.
            oCRParameter.cls = false; //是否执行文字方向分类；默认false
            oCRParameter.det = true;//是否开启方向检测，用于检测识别180旋转
            oCRParameter.use_angle_cls = false;//是否开启方向检测，用于检测识别180旋转
            oCRParameter.det_db_score_mode = true;//是否使用多段线，即文字区域是用多段线还是用矩形，
            //识别结果对象
            OCRResult ocrResult = new OCRResult();
            //建议程序全局初始化一次即可，不必每次识别都初始化，容易报错。
            paddleOCREngine = new PaddleOCREngine(config, oCRParameter);
        }

        public void Dispose()
        {
            paddleOCREngine.Dispose();
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string Excute(byte[] bytes)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                var ocrResult = paddleOCREngine.DetectText(bytes);

                ocrResult?.TextBlocks.ForEach(x => sb.AppendLine(x.Text));
                //ret = ocrResult?.Text ?? "";

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("OCR出错: {0}", ex.Message));
            }
        }
    }
}
#endif
