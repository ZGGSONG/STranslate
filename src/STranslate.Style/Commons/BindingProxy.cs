using System.Windows;

namespace STranslate.Style.Commons
{
    /// <summary>
    /// 绑定代理，尤其用在ContextMenu的MenuItem和ToolTip的绑定失败上
    /// 出处: https://thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/
    /// </summary>
    public sealed class BindingProxy : Freezable
    {
        protected override Freezable CreateInstanceCore() => new BindingProxy();

        public object Data
        {
            get => (object)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data),
            typeof(object),
            typeof(BindingProxy),
            new PropertyMetadata(default(object))
        );

        public override string ToString() =>
            Data is FrameworkElement fe ? $"{nameof(BindingProxy)}: {fe.Name}" : $"{nameof(BindingProxy)}: {Data?.GetType().FullName}";
    }
}
