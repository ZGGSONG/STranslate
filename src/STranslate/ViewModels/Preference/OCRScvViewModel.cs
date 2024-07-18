using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Util;
using STranslate.ViewModels.Preference.OCR;
using STranslate.Views.Preference.OCR;

namespace STranslate.ViewModels.Preference;

public partial class OCRScvViewModel : ObservableObject
{
    /// <summary>
    ///     导航 UI 缓存
    /// </summary>
    private readonly Dictionary<Type, UIElement?> ContentCache = [];

    /// <summary>
    ///     当前激活的OCR服务
    /// </summary>
    private IOCR? _activedOCR =
        Singleton<ConfigHelper>.Instance.CurrentConfig?.OCRList?.FirstOrDefault(x => x.IsEnabled);

    [ObservableProperty]
    private OCRCollection<IOCR> _curOCRServiceList = Singleton<ConfigHelper>.Instance.CurrentConfig?.OCRList ?? [];


    private bool _isPreferenceOperate;

    [ObservableProperty] private int _ocrCounter;

    [ObservableProperty] private UIElement? _ocrServiceContent;

    [ObservableProperty] private BindingList<IOCR> _ocrServices = [];

    [ObservableProperty] private int _selectedIndex;

    private int tmpIndex;

    public OCRScvViewModel()
    {
        //添加默认支持OCR
        //TODO: 新OCR服务需要适配
        OcrServices.Add(new PaddleOCR());
        OcrServices.Add(new TencentOCR());
        OcrServices.Add(new BaiduOCR());
        OcrServices.Add(new VolcengineOCR());

        ResetView();
    }

    public IOCR? ActivedOCR
    {
        get => _activedOCR;
        set
        {
            if (value == null || _activedOCR == value)
                return;

            _activedOCR = value;
            OnPropertyChanged();

            CurOCRServiceList.First(x => x == value).IsEnabled = true;

            if (!_isPreferenceOperate) Save();

            OnChangeActivedOcrService?.Invoke();
        }
    }

    /// <summary>
    ///     执行OCR
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="showErrorToast">当OCR未启用时错误显示的窗口</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<OcrResult> ExecuteAsync(byte[] bytes, WindowType showErrorToast, CancellationToken? token = null,
        LangEnum lang = LangEnum.auto)
    {
        if (ActivedOCR is not null)
            return await ActivedOCR.ExecuteAsync(bytes, lang, token ?? CancellationToken.None);
        ToastHelper.Show("未启用OCR服务", showErrorToast);
        return await Task.FromResult(OcrResult.Fail("未启用OCR服务"));
    }

    [RelayCommand]
    private void SelectedOCR(IOCR ocr)
    {
        _isPreferenceOperate = true;
        ActivedOCR = ocr;
        _isPreferenceOperate = false;
    }

    /// <summary>
    ///     重置选中项
    /// </summary>
    /// <param name="type"></param>
    private void ResetView(ActionType type = ActionType.Initialize)
    {
        OcrCounter = CurOCRServiceList.Count;

        //当全部删除时则清空view绑定属性
        if (OcrCounter < 1)
        {
            SelectedIndex = 0;
            OcrServiceContent = null;
            return;
        }

        switch (type)
        {
            case ActionType.Delete:
            {
                //不允许小于0
                SelectedIndex = Math.Max(tmpIndex - 1, 0);
                TogglePage(CurOCRServiceList[SelectedIndex]);
                break;
            }
            case ActionType.Add:
            {
                //选中最后一项
                SelectedIndex = OcrCounter - 1;
                TogglePage(CurOCRServiceList[SelectedIndex]);
                break;
            }
            default:
            {
                //初始化默认执行选中第一条
                SelectedIndex = 0;
                TogglePage(CurOCRServiceList.First());
                break;
            }
        }
    }

    [RelayCommand]
    private void TogglePage(IOCR ocr)
    {
        if (ocr != null)
        {
            if (SelectedIndex != -1)
                tmpIndex = SelectedIndex;

            const string head = "STranslate.Views.Preference.OCR.";
            //TODO: 新OCR服务需要适配
            var name = ocr.Type switch
            {
                OCRType.PaddleOCR => string.Format($"{head}{nameof(PaddleOCRPage)}"),
                OCRType.TencentOCR => string.Format($"{head}{nameof(TencentOCRPage)}"),
                OCRType.BaiduOCR => string.Format($"{head}{nameof(BaiduOCRPage)}"),
                OCRType.VolcengineOCR => string.Format($"{head}{nameof(VolcengineOCRPage)}"),
                _ => string.Format($"{head}{nameof(PaddleOCRPage)}")
            };

            NavigationPage(name, ocr);
        }
    }

    [RelayCommand]
    private void Popup(Popup control)
    {
        control.IsOpen = true;
    }

    [RelayCommand]
    private void Add(List<object> list)
    {
        if (list?.Count != 2) return;
        var ocr = list.First();

        //TODO: 新OCR服务需要适配
        CurOCRServiceList.Add(
            ocr switch
            {
                PaddleOCR paddleocr => paddleocr.Clone(),
                TencentOCR tencentocr => tencentocr.Clone(),
                BaiduOCR baiduocr => baiduocr.Clone(),
                VolcengineOCR volcengineocr => volcengineocr.Clone(),
                _ => throw new InvalidOperationException($"Unsupported ocr type: {ocr.GetType().Name}")
            }
        );

        (list.Last() as Popup)!.IsOpen = false;

        ResetView(ActionType.Add);
    }

    [RelayCommand]
    private void Delete(IOCR ocr)
    {
        if (ocr != null)
        {
            CurOCRServiceList.Remove(ocr);

            ResetView(ActionType.Delete);

            ToastHelper.Show("删除成功", WindowType.Preference);
        }
    }

    [RelayCommand]
    private void Save()
    {
        if (!Singleton<ConfigHelper>.Instance.WriteConfig(CurOCRServiceList))
        {
            LogService.Logger.Warn($"保存OCR失败，{JsonConvert.SerializeObject(CurOCRServiceList)}");

            ToastHelper.Show("保存失败", WindowType.Preference);
        }

        ToastHelper.Show("保存成功", WindowType.Preference);
    }

    [RelayCommand]
    private void Reset()
    {
        CurOCRServiceList = Singleton<ConfigHelper>.Instance.ResetConfig.OCRList ?? [];
        ActivedOCR = CurOCRServiceList.FirstOrDefault(x => x.IsEnabled);
        ResetView();
        ToastHelper.Show("重置配置", WindowType.Preference);
    }

    /// <summary>
    ///     导航页面
    /// </summary>
    /// <param name="name"></param>
    /// <param name="ocr"></param>
    public void NavigationPage(string name, IOCR ocr)
    {
        UIElement? content = null;

        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("param name is null or empty", nameof(name));

            ArgumentNullException.ThrowIfNull(ocr);

            var type = Type.GetType(name) ?? throw new Exception($"{nameof(NavigationPage)} get {name} exception");

            //读取缓存是否存在，存在则从缓存中获取View实例并通过UpdateVM刷新ViewModel
            if (ContentCache.ContainsKey(type))
            {
                content = ContentCache[type];
                if (content is UserControl uc)
                {
                    var method = type.GetMethod("UpdateVM");
                    method?.Invoke(uc, new[] { ocr });
                }
            }
            else //不存在则创建并通过构造函数传递ViewModel
            {
                content = (UIElement?)Activator.CreateInstance(type, ocr);
                ContentCache.Add(type, content);
            }

            OcrServiceContent = content;
        }
        catch (Exception ex)
        {
            LogService.Logger.Error("OCR导航出错", ex);
        }
    }

    public event Action? OnChangeActivedOcrService;
}