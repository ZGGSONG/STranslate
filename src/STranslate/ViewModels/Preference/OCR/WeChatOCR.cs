using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.WeChatOcr;
using OcrResult = STranslate.Model.OcrResult;

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

    public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        var imgType = (Singleton<ConfigHelper>.Instance.CurrentConfig?.OcrImageQuality ?? OcrImageQualityEnum.Medium) switch
        {
            OcrImageQualityEnum.Medium => ImageType.Png,
            OcrImageQualityEnum.Low => ImageType.Jpeg,
            _ => ImageType.Bmp
        };
        var tcs = new TaskCompletionSource<OcrResult>();

        using var ocr = new ImageOcr(WeChatPath);
        ocr.Run(bytes, (path, result) =>
        {
            if (result == null) return;

            var ocrResult = new OcrResult();
            LogService.Logger.Info(JsonConvert.SerializeObject(result));
            var list = result?.OcrResult?.SingleResult;
            for (var i = 0; i < list?.Count; i++)
            {
                var item = list[i];
                var content = new OcrContent(item?.SingleStrUtf8 ?? "");
                ocrResult.OcrContents.Add(content);
                //LogService.Logger.Debug(item?.SingleStrUtf8);
                //foreach (var item2 in item.OneResult)
                //    LogService.Logger.Debug($"{string.Join(",", item2.OnePos.Pos.Select(x => (x.X, x.Y)))}");
            }

            tcs.SetResult(ocrResult);

        }, imgType);

        var timeoutTask = Task.Delay(10000, cancelToken);
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException("WeChatOCR operation timed out.");
        }
        // 提取content的值
        var finalResult = await tcs.Task;

        return finalResult;
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
            WeChatPath = WeChatPath
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