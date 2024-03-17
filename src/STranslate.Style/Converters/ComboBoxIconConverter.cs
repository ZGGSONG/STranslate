using STranslate.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace STranslate.Style.Converters
{
    public class ComboBoxIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string pkv && parameter is string param)
            {
                var str = pkv.TrimStart('[').TrimEnd(']');
                char separator = ',';
                IconType type = (IconType)Enum.Parse(typeof(IconType), new(str.TakeWhile(c => c != separator).ToArray()));
                string icon = new(str.SkipWhile(c => c != separator).Skip(1).ToArray());
                return param == "0" ? icon : type.GetDescription();
            }
            else if (value is IconType type)
            {
                return type.GetDescription();
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
