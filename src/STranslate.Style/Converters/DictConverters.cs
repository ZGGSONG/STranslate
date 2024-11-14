using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using STranslate.Model;

namespace STranslate.Style.Converters;

public class DictConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Debug.WriteLine(value);
        if (value is not string str || string.IsNullOrEmpty(str) ||
            !int.TryParse(parameter?.ToString(), out var index)) return value;
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

public class DictToCollapsedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ServiceType serviceType) return Visibility.Collapsed;

        return serviceType switch
        {
            ServiceType.EcdictService => Visibility.Collapsed,
            ServiceType.BingDictService => Visibility.Collapsed,
            ServiceType.KingSoftDictService => Visibility.Collapsed,
            _ => Visibility.Visible
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DictGetWordConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string str) return value;
        return str.Trim().Split('\r', '\n').FirstOrDefault() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DictResultMultiVisibilityCollapsedConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [bool b, ServiceType serviceType]) return Visibility.Collapsed;

        if (!b) return Visibility.Visible;

        return serviceType switch
        {
            ServiceType.EcdictService => Visibility.Collapsed,
            ServiceType.BingDictService => Visibility.Collapsed,
            ServiceType.KingSoftDictService => Visibility.Collapsed,
            //TODO: 词典
            _ => Visibility.Visible
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DictResultMultiVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [bool b, ServiceType serviceType]) return Visibility.Collapsed;

        if (!b) return Visibility.Collapsed;
        return serviceType switch
        {
            ServiceType.EcdictService => Visibility.Visible,
            ServiceType.BingDictService => Visibility.Visible,
            ServiceType.KingSoftDictService => Visibility.Visible,
            //TODO: 词典
            _ => Visibility.Collapsed
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DictIconMultiVisibilityCollapsedConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [ServiceType serviceType, bool b]) return Visibility.Collapsed;
        if (serviceType is ServiceType.EcdictService or ServiceType.BingDictService or ServiceType.KingSoftDictService)
            return Visibility.Collapsed;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DictIconMultiVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is not [ServiceType serviceType, bool b]) return Visibility.Collapsed;
        if (serviceType is ServiceType.EcdictService or ServiceType.BingDictService or ServiceType.KingSoftDictService)
            return b ? Visibility.Visible : Visibility.Collapsed;

        return Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}