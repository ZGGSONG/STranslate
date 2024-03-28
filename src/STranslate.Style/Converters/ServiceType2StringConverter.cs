using STranslate.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace STranslate.Style.Converters
{
    public class ServiceType2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string str && str == "prompt" && value is ServiceType svcType)
            {
                return svcType switch
                {
                    ServiceType.OpenAIService => "true",
                    ServiceType.GeminiService => "true",
                    ServiceType.ChatglmService => "true",
                    _ => "false"
                };
            }
            if (value is ServiceType sType)
            {
                return sType switch
                {
                    ServiceType.ApiService => "自建",
                    ServiceType.STranslateService => "内置",
                    ServiceType.EcdictService => "内置",
                    _ => "官方",
                };
            }
            if (value is TTSType tType)
            {
                return tType switch
                {
                    TTSType.OfflineTTS => "内置",
                    _ => "官方",
                };
            }
            return "自建";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
