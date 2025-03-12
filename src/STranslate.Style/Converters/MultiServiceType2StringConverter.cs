using STranslate.Model;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace STranslate.Style.Converters;

public class MultiServiceType2StringConverter : IMultiValueConverter
{
    // 静态构造函数，订阅语言变更事件
    static MultiServiceType2StringConverter()
    {
        // 订阅语言变更事件
        AppLanguageManager.OnAppLanguageChanged += OnLanguageChanged;
    }

    // 存储所有使用此转换器的绑定表达式的弱引用
    private static readonly List<WeakReference<TextBlock>> _bindingTargets = [];

    // 语言变更事件处理
    private static void OnLanguageChanged()
    {
        // 刷新所有注册的绑定目标
        for (int i = _bindingTargets.Count - 1; i >= 0; i--)
        {
            if (_bindingTargets[i].TryGetTarget(out var target))
            {
                // 刷新绑定
                var bindingExpression = BindingOperations.GetMultiBindingExpression(target as TextBlock, TextBlock.TextProperty);
                bindingExpression?.UpdateTarget();
            }
            else
            {
                // 移除失效的弱引用
                _bindingTargets.RemoveAt(i);
            }
        }
    }
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2)
            return values;

        // 注册绑定目标，以便在语言变更时刷新
        if (values[0] is TextBlock target && !_bindingTargets.Any(wr => wr.TryGetTarget(out var t) && t == target))
        {
            _bindingTargets.Add(new WeakReference<TextBlock>(target));
        }

        // 服务类型转换为字符串以显示
        return values[1] switch
        {
            ServiceType sType => sType switch
            {
                ServiceType.DeepLXService => AppLanguageManager.GetString("ServiceType.SelfBuild"),
                ServiceType.GoogleBuiltinService => AppLanguageManager.GetString("ServiceType.BuiltIn"),
                ServiceType.YandexBuiltInService => AppLanguageManager.GetString("ServiceType.BuiltIn"),
                ServiceType.STranslateService => AppLanguageManager.GetString("ServiceType.BuiltIn"),
                ServiceType.EcdictService => AppLanguageManager.GetString("ServiceType.BuiltIn"),
                ServiceType.KingSoftDictService => AppLanguageManager.GetString("ServiceType.BuiltIn"),
                ServiceType.BingDictService => AppLanguageManager.GetString("ServiceType.BuiltIn"),
                ServiceType.MicrosoftBuiltinService => AppLanguageManager.GetString("ServiceType.BuiltIn"),
                _ => AppLanguageManager.GetString("ServiceType.Official")
                //TODO: 新接口需要适配
            },
            OCRType oType => oType switch
            {
                OCRType.PaddleOCR => AppLanguageManager.GetString("ServiceType.BuiltIn"),
                OCRType.WeChatOCR => AppLanguageManager.GetString("ServiceType.BuiltIn"),
                _ => AppLanguageManager.GetString("ServiceType.Official")
                //TODO: 新TTS服务需要适配
            },
            TTSType tType => tType switch
            {
                TTSType.OfflineTTS => AppLanguageManager.GetString("ServiceType.BuiltIn"),
                TTSType.EdgeTTS => AppLanguageManager.GetString("ServiceType.BuiltIn"),
                TTSType.LingvaTTS => AppLanguageManager.GetString("ServiceType.SelfBuild"),
                _ => AppLanguageManager.GetString("ServiceType.Official")
                //TODO: 新OCR服务需要适配
            },
            VocabularyBookType vType => vType switch
            {
                _ => AppLanguageManager.GetString("ServiceType.Official")
            },
            _ => AppLanguageManager.GetString("ServiceType.SelfBuild")
            //TODO: 新生词本服务需要适配
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [.. Enumerable.Repeat(Binding.DoNothing, targetTypes.Length)];
    }
}
