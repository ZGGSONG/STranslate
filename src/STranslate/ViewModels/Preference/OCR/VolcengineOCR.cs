using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Model;
using STranslate.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.ViewModels.Preference.OCR
{
    public partial class VolcengineOCR : ObservableObject, IOCR
    {
        #region Constructor

        public VolcengineOCR()
            : this(Guid.NewGuid(), "https://visual.volcengineapi.com", "火山OCR") { }

        public VolcengineOCR(
            Guid guid,
            string url,
            string name = "",
            IconType icon = IconType.Volcengine,
            string appID = "",
            string appKey = "",
            bool isEnabled = true,
            OCRType type = OCRType.VolcengineOCR
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

        [ObservableProperty]
        private Guid _identify = Guid.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private OCRType _type = OCRType.VolcengineOCR;

        [JsonIgnore]
        [ObservableProperty]
        public bool _isEnabled = true;

        [JsonIgnore]
        [ObservableProperty]
        private string _name = string.Empty;

        [JsonIgnore]
        [ObservableProperty]
        private IconType _icon = IconType.Volcengine;

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
            switch (obj)
            {
                case null:
                    return;
                case nameof(AppID):
                    IdHide = !IdHide;
                    break;
                case nameof(AppKey):
                    KeyHide = !KeyHide;
                    break;
            }
        }

        private RelayCommand<string>? showEncryptInfoCommand;

        [JsonIgnore]
        public IRelayCommand<string> ShowEncryptInfoCommand => showEncryptInfoCommand ??= new RelayCommand<string>(new Action<string?>(ShowEncryptInfo));

        #endregion Show/Hide Encrypt Info

        #endregion Properties

        #region Interface Implementation

        public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
        {
            //https://github.com/Baozisoftware/go-dll/wiki/C%23%E8%B0%83%E7%94%A8Go%E7%89%88DLL#%E5%85%B3%E4%BA%8Ego%E7%9A%84%E6%95%B0%E7%BB%84%E5%88%87%E7%89%87%E8%BF%94%E5%9B%9E%E9%97%AE%E9%A2%98
            //加入这个就不崩溃了
            Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=0");

            var accessKeyBytes = Encoding.UTF8.GetBytes(AppID);
            var secretKeyBytes = Encoding.UTF8.GetBytes(AppKey);
            var result = await Task.Run(() => GoUtil.VolcengineOcr(accessKeyBytes, secretKeyBytes, BitmapUtil.BytesToBase64StringBytes(bytes)), cancelToken).ConfigureAwait(false);
            var tuple = GoUtil.GoTupleToCSharpTuple(result);
            var resp = tuple.Item2 ?? throw new Exception("请求结果为空");
            if (tuple.Item1 != 200)
                throw new Exception(resp);

            var parsedData = JsonConvert.DeserializeObject<Root>(resp) ?? throw new Exception($"反序列化失败: {resp}");
            if (parsedData.code != 10000) return OcrResult.Fail(parsedData.ResponseMetadata.Error.Message);

            if (parsedData.data.line_texts.Count != parsedData.data.line_rects.Count) return OcrResult.Fail("识别和位置结果数量不匹配\n原始数据:" + resp);

            // 提取content的值
            var ocrResult = new OcrResult();
            for (var i = 0; i < parsedData.data.line_texts.Count; i++)
            {
                var content = new OcrContent(parsedData.data.line_texts[i]);
                Converter(parsedData.data.line_rects[i]).ForEach(pg =>
                {
                    //仅位置不全为0时添加
                    if (pg.X != pg.Y || pg.X != 0)
                        content.BoxPoints.Add(new BoxPoint(pg.X, pg.Y));
                });
                ocrResult.OcrContents.Add(content);
            }
            return ocrResult;
        }

        public IOCR Clone()
        {
            return new VolcengineOCR
            {
                Identify = this.Identify,
                Type = this.Type,
                IsEnabled = this.IsEnabled,
                Icon = this.Icon,
                Name = this.Name,
                Url = this.Url,
                AppID = this.AppID,
                AppKey = this.AppKey,
            };
        }

        public string? LangConverter(LangEnum lang) => "auto";

        #endregion Interface Implementation

        #region Volcengine Offcial Support

        public List<BoxPoint> Converter(Line_rectsItem rect) =>
        [
            //left top
            new(rect.x, rect.y),

	        //right top
	        new(rect.x + rect.width, rect.y),

	        //right bottom
	        new(rect.x + rect.width, rect.y + rect.height),

	        //left bottom
	        new(rect.x, rect.y + rect.height)
        ];

        public class Error
        {
            /// <summary>
            /// 
            /// </summary>
            public int CodeN { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Message { get; set; }
        }

        public class ResponseMetadata
        {
            /// <summary>
            /// 
            /// </summary>
            public string RequestId { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Service { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Region { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Action { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Version { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Error Error { get; set; }
        }

        public class Line_rectsItem
        {
            /// <summary>
            /// 
            /// </summary>
            public int x { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int y { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int width { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int height { get; set; }
        }

        public class CharsItemItem
        {
            /// <summary>
            /// 
            /// </summary>
            public int x { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int y { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int width { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int height { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public double score { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string @char { get; set; }
        }

        public class Data
        {
            /// <summary>
            /// 
            /// </summary>
            public List<string> line_texts { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<Line_rectsItem> line_rects { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<List<CharsItemItem>> chars { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<List<List<int>>> polygons { get; set; }
        }

        public class Root
        {
            /// <summary>
            /// 
            /// </summary>
            public ResponseMetadata ResponseMetadata { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string request_id { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string time_elapsed { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int code { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Data data { get; set; }
        }


        #endregion Volcengine Offcial Support
    }
}