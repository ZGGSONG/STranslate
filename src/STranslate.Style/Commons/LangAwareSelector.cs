using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using STranslate.Style.Converters;

namespace STranslate.Style.Commons
{
    public static class LangAwareSelector
    {
        public static readonly DependencyProperty IsLangAwareProperty =
            DependencyProperty.RegisterAttached(
                "IsLangAware",
                typeof(bool),
                typeof(LangAwareSelector),
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
            if (d is Selector selector && (bool)e.NewValue)
            {
                // 使用Loaded事件确保在控件完全加载后再尝试获取绑定表达式
                if (selector.IsLoaded)
                {
                    RegisterSelector(selector);
                }
                else
                {
                    selector.Loaded += Selector_Loaded;
                }
            }
        }

        private static void Selector_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Selector selector)
            {
                selector.Loaded -= Selector_Loaded;
                RegisterSelector(selector);
            }
        }

        private static void RegisterSelector(Selector selector)
        {
            // 直接从ItemsSource属性获取MultiBinding
            if (selector.ItemsSource == null && BindingOperations.GetMultiBinding(selector, Selector.ItemsSourceProperty) is MultiBinding multiBinding)
            {
                if (multiBinding.Converter is MultiLangFilterConverter converter)
                {
                    converter.RegisterSelector(selector);
                }
            }
            else
            {
                // 如果ItemsSource已经有值，则需要等待下一个UI更新周期再尝试
                selector.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // 尝试从绑定表达式获取MultiBinding
                    var bindingExpression = BindingOperations.GetMultiBindingExpression(selector, Selector.ItemsSourceProperty);
                    if (bindingExpression?.ParentMultiBinding is MultiBinding mb && 
                        mb.Converter is MultiLangFilterConverter converter)
                    {
                        converter.RegisterSelector(selector);
                    }
                    else
                    {
                        // 最后尝试直接从DataContext中查找MultiLangFilterConverter
                        var resources = Application.Current.Resources;
                        foreach (var key in resources.Keys)
                        {
                            if (resources[key] is MultiLangFilterConverter mlfc)
                            {
                                mlfc.RegisterSelector(selector);
                                break;
                            }
                        }
                    }
                }));
            }
        }
    }
}