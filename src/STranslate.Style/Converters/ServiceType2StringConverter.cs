using System.Globalization;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class ServiceType2StringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 提示词 ContextMenu 是否显示
        if (parameter is "prompt" && value is ITranslator service) return service is ITranslatorLLM ? "1" : "0";

        // 服务类型转换为字符串以显示
        return value switch
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}