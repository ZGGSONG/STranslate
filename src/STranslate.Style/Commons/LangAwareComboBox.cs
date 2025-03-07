using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using STranslate.Style.Converters;

namespace STranslate.Style.Commons
{
    public static class LangAwareComboBox
    {
        public static readonly DependencyProperty IsLangAwareProperty =
            DependencyProperty.RegisterAttached(
                "IsLangAware",
                typeof(bool),
                typeof(LangAwareComboBox),
                new PropertyMetadata(false, OnIsLangAwareChanged));

        public static bool GetIsLangAware(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsLangAwareProperty);
        }

        public static void SetIsLangAware(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLangAwareProperty, value);
        }

        private static void OnIsLangAwareChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox && (bool)e.NewValue)
            {
                // 使用Loaded事件确保在控件完全加载后再尝试获取绑定表达式
                if (comboBox.IsLoaded)
                {
                    RegisterComboBox(comboBox);
                }
                else
                {
                    comboBox.Loaded += ComboBox_Loaded;
                }
            }
        }

        private static void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.Loaded -= ComboBox_Loaded;
                RegisterComboBox(comboBox);
            }
        }

        private static void RegisterComboBox(ComboBox comboBox)
        {
            // 直接从ItemsSource属性获取MultiBinding
            if (comboBox.ItemsSource == null && BindingOperations.GetMultiBinding(comboBox, ComboBox.ItemsSourceProperty) is MultiBinding multiBinding)
            {
                if (multiBinding.Converter is MultiLangFilterConverter converter)
                {
                    converter.RegisterComboBox(comboBox);
                }
            }
            else
            {
                // 如果ItemsSource已经有值，则需要等待下一个UI更新周期再尝试
                comboBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // 尝试从绑定表达式获取MultiBinding
                    var bindingExpression = BindingOperations.GetMultiBindingExpression(comboBox, ComboBox.ItemsSourceProperty);
                    if (bindingExpression?.ParentMultiBinding is MultiBinding mb && 
                        mb.Converter is MultiLangFilterConverter converter)
                    {
                        converter.RegisterComboBox(comboBox);
                    }
                    else
                    {
                        // 最后尝试直接从DataContext中查找MultiLangFilterConverter
                        var resources = Application.Current.Resources;
                        foreach (var key in resources.Keys)
                        {
                            if (resources[key] is MultiLangFilterConverter mlfc)
                            {
                                mlfc.RegisterComboBox(comboBox);
                                break;
                            }
                        }
                    }
                }));
            }
        }
    }
}