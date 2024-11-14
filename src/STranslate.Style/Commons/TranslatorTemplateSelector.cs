using System.Windows;
using System.Windows.Controls;
using STranslate.Model;

namespace STranslate.Style.Commons;

public class TranslatorTemplateSelector : DataTemplateSelector
{
    public required DataTemplate TranslatorTemplate { get; set; }
    public required DataTemplate SeparatorTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            ITranslator => TranslatorTemplate,
            Separator => SeparatorTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}