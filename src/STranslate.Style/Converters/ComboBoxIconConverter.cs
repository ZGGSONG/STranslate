using STranslate.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

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
                string type = new(str.TakeWhile(c => c != separator).ToArray());
                string icon = new(str.SkipWhile(c => c != separator).Skip(1).ToArray());
                return param == "0" ? icon : type;
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
