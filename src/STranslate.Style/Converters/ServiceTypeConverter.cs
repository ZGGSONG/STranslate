using STranslate.Model;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace STranslate.Style.Converters;

public class ServiceTypeConverter : MarkupExtension, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ServiceType st) return value;

        // 移除最后的Service
        return st.ToString().Replace("Service", string.Empty);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}