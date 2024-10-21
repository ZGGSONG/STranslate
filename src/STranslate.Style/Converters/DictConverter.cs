using System.Globalization;
using System.Windows;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class DictConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        System.Diagnostics.Debug.WriteLine(value);
        if (value is not string str || string.IsNullOrEmpty(str) || !int.TryParse(parameter?.ToString(), out var index)) return value;
        var array = str.Split(Environment.NewLine).Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim()).ToArray();
        return index < array.Length ? array[index] : string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DictItemVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DictVisibilityCollapsedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ServiceType serviceType) return value;
        return serviceType switch
        {
            ServiceType.EcdictService => Visibility.Collapsed,
            ServiceType.BingDictService => Visibility.Collapsed,
            ServiceType.KingSoftDictService => Visibility.Collapsed,
            _ => Visibility.Visible,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DictVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ServiceType serviceType) return value;
        return serviceType switch
        {
            ServiceType.EcdictService => Visibility.Visible,
            ServiceType.BingDictService => Visibility.Visible,
            ServiceType.KingSoftDictService => Visibility.Visible,
            _ => Visibility.Collapsed,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}