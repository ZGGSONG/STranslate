using System;
using System.Globalization;
using System.Windows.Data;

namespace STranslate.Style.Converters
{
    public class String2IsExpandedConverter : IValueConverter
    {
        /// <summary>
        /// 缓存值
        /// </summary>
        private string _cachedValue = "";

        /// <summary>
        /// 缓存是否为UI操作，防止循环执行Convert方法
        /// </summary>
        private (bool UIOperated, bool UIReturned) _uiOperation = (false, false);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (_uiOperation.UIOperated)
            {
                var uiResult = _uiOperation.UIReturned;
                _uiOperation = (false, false);
                return uiResult;
            }

            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                _cachedValue = str;
                return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            _uiOperation = (true, (bool)value);
            return _cachedValue;
        }
    }
}
