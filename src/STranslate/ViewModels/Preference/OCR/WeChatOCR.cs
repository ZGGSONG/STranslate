using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STranslate.Helper;
using STranslate.Model;
using STranslate.Util;
using STranslate.WeChatOcr;

namespace STranslate.ViewModels.Preference.OCR;

public partial class WeChatOCR : ObservableObject, IOCR
{
    #region Constructor

    public WeChatOCR()
        : this(Guid.NewGuid(), "", "微信OCR", isEnabled: false)
    {
    }

    public WeChatOCR(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.WeChat,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        OCRType type = OCRType.WeChatOCR
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

    [JsonIgnore] [ObservableProperty] private OCRType _type = OCRType.BaiduOCR;

    [JsonIgnore] [ObservableProperty] private bool _isEnabled = true;

    [JsonIgnore] [ObservableProperty] private string _name = string.Empty;

    [JsonIgnore] [ObservableProperty] private IconType _icon = IconType.BaiduBce;

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

    [JsonIgnore] public Dictionary<IconType, string> Icons { get; private set; } = Constant.IconDict;

    #region Show/Hide Encrypt Info

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
    private bool _idHide = true;

    [JsonIgnore] [ObservableProperty] [property: JsonIgnore]
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
    public IRelayCommand<string> ShowEncryptInfoCommand =>
        showEncryptInfoCommand ??= new RelayCommand<string>(ShowEncryptInfo);

    #endregion Show/Hide Encrypt Info
    
    /// <summary>
    ///     微信OCR可执行文件路径
    /// </summary>
    [ObservableProperty] private string _weChatPath = string.Empty;


    #endregion Properties

    #region Interface Implementation

    public async Task<Model.OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        var extension =
            (Singleton<ConfigHelper>.Instance.CurrentConfig?.OcrImageQuality ?? OcrImageQualityEnum.Medium) switch
            {
                OcrImageQualityEnum.Medium => ".png",
                OcrImageQualityEnum.Low => ".jpg",
                _ => ".bmp"
            };
        var imgPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");

        try
        {
            await File.WriteAllBytesAsync(imgPath, bytes, cancelToken);

            //var weChatOcrExe = WeChatOcrHelper.FindWeChatOcrExe();
            //if (weChatOcrExe == null)
            //    throw new Exception("未找到微信OCR可执行文件");

            var ocr = new ImageOcr();
            ocr.Run(imgPath, (path, result) =>
            {
                if (result == null) return;

                var ocrResult = new Model.OcrResult();
                var list = result?.OcrResult?.SingleResult;
                for (int i = 0; i < list?.Count; i++)
                {
                    SingleResult? item = list[i];
                    Log.LogService.Logger.Debug(item?.SingleStrUtf8);
                    foreach (var item2 in item.OneResult)
                    {
                        Log.LogService.Logger.Debug($"{string.Join(",", item2.OnePos.Pos.Select(x => (x.X, x.Y)))}");
                    }
                }

            });

            // 提取content的值
            var ocrResult = new Model.OcrResult();
            //foreach (var item in ocrResponse)
            //{
            //    var content = new OcrContent(item["text"]?.ToString() ?? "");
            //    var left = double.Parse(item["left"]?.ToString() ?? "0");
            //    var top = double.Parse(item["top"]?.ToString() ?? "0");
            //    var right = double.Parse(item["right"]?.ToString() ?? "0");
            //    var bottom = double.Parse(item["bottom"]?.ToString() ?? "0");
            //    //content.BoxPoints.Add(new BoxPoint())
            //    //Converter(item.location).ForEach(pg =>
            //    //{
            //    //    //仅位置不全为0时添加
            //    //    if (pg.X != pg.Y || pg.X != 0)
            //    //        content.BoxPoints.Add(new BoxPoint(pg.X, pg.Y));
            //    //});
            //    ocrResult.OcrContents.Add(content);
            //}

            return ocrResult;
        }
        finally
        {
            if (File.Exists(imgPath))
                File.Delete(imgPath);
        }
    }

    public IOCR Clone()
    {
        return new WeChatOCR
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            AppID = AppID,
            AppKey = AppKey,
            Icons = Icons,
            WeChatPath = WeChatPath,
        };
    }

    public string? LangConverter(LangEnum lang)
    {
        throw new NotImplementedException();
    }

    #endregion Interface Implementation

    #region Location Support

    public List<BoxPoint> Converter(Location location)
    {
        return
        [
            //left top
            new BoxPoint(location.left, location.top),

            //right top
            new BoxPoint(location.left + location.width, location.top),

            //right bottom
            new BoxPoint(location.left + location.width, location.top + location.height),

            //left bottom
            new BoxPoint(location.left, location.top + location.height)
        ];
    }

    public class Location
    {
        /// <summary>
        /// </summary>
        public int top { get; set; }

        /// <summary>
        /// </summary>
        public int left { get; set; }

        /// <summary>
        /// </summary>
        public int width { get; set; }

        /// <summary>
        /// </summary>
        public int height { get; set; }
    }

    #endregion
}