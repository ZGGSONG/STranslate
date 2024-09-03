using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Model;
using OcrResult = STranslate.Model.OcrResult;

namespace STranslate.ViewModels.Preference.OCR;

public partial class WindowsOCR : ObservableObject, IOCR
{
    #region Constructor

    public WindowsOCR()
        : this(Guid.NewGuid(), "", "Windows OCR", isEnabled: false)
    {
    }

    public WindowsOCR(
        Guid guid,
        string url,
        string name = "",
        IconType icon = IconType.Windows,
        string appID = "",
        string appKey = "",
        bool isEnabled = true,
        OCRType type = OCRType.WindowsOCR
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

    #endregion Properties

    #region Interface Implementation

    public async Task<OcrResult> ExecuteAsync(byte[] bytes, LangEnum lang, CancellationToken cancelToken)
    {
        var softwareBitmap = await ConvertToSoftwareBitmapAsync(bytes);
        var ocrEngine = OcrEngine.TryCreateFromLanguage(new Language("zh-CN"));
        var ret = await ocrEngine.RecognizeAsync(softwareBitmap);
        // 提取content的值
        var ocrResult = new OcrResult();
        foreach (var item in ret.Lines)
        {
            var content = new OcrContent(item.Text);
            ocrResult.OcrContents.Add(content);
        }

        return ocrResult;
    }

    public IOCR Clone()
    {
        return new WindowsOCR
        {
            Identify = Identify,
            Type = Type,
            IsEnabled = IsEnabled,
            Icon = Icon,
            Name = Name,
            Url = Url,
            AppID = AppID,
            AppKey = AppKey,
            Icons = Icons
        };
    }

    public string? LangConverter(LangEnum lang)
    {
        throw new NotImplementedException();
    }

    #endregion Interface Implementation

    #region Extra Methods

    internal async Task<SoftwareBitmap> ConvertToSoftwareBitmapAsync(byte[] imageData)
    {
        // 将byte[]转换为IRandomAccessStream
        using var stream = new InMemoryRandomAccessStream();
        await stream.WriteAsync(imageData.AsBuffer());
        stream.Seek(0);

        // 使用BitmapDecoder从流中解码图像
        var decoder = await BitmapDecoder.CreateAsync(stream);
        return await decoder.GetSoftwareBitmapAsync();
    }

    #endregion
}