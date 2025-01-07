using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class ServiceTypeFilterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not BindingList<ITranslator> list || parameter is not string type) return value;

        return type switch
        {
            "selfBuild" => list.Where(x => x.Type
                is ServiceType.DeepLXService
                ),
            "local" => list.Where(x => x.Type
                is ServiceType.STranslateService
                or ServiceType.EcdictService
                or ServiceType.KingSoftDictService
                or ServiceType.BingDictService
                or ServiceType.GoogleBuiltinService
                ),
            "official" => list.Where(x => x.Type
                is not (ServiceType.GoogleBuiltinService
                or ServiceType.DeepLXService
                or ServiceType.STranslateService
                or ServiceType.EcdictService
                or ServiceType.KingSoftDictService
                or ServiceType.BingDictService
                )),
            //TODO: 新接口需要适配
            _ => list
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}