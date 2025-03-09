using System.Windows;

namespace STranslate.Model;

public class KeyValueItem : DependencyObject
{
    public static readonly DependencyProperty Column1Property =
        DependencyProperty.Register("Column1", typeof(string), typeof(KeyValueItem),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty Column2Property =
        DependencyProperty.Register("Column2", typeof(object), typeof(KeyValueItem),
            new PropertyMetadata(string.Empty));

    public string Column1
    {
        get { return (string)GetValue(Column1Property); }
        set { SetValue(Column1Property, value); }
    }

    public object Column2
    {
        get { return GetValue(Column2Property); }
        set { SetValue(Column2Property, value); }
    }
}