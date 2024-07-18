using System.Windows;
using System.Windows.Controls;
using STranslate.Model;

namespace STranslate.Style.Commons;

public class HeaderTemplateSelector : DataTemplateSelector
{
    public required DataTemplate ITranslatorTemplate { get; set; }
    public required DataTemplate IOCRTemplate { get; set; }
    public required DataTemplate ITTSTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        // 根据item的类型或属性选择一个模板
        return item switch
        {
            ITranslator => ITranslatorTemplate,
            IOCR => IOCRTemplate,
            ITTS => ITTSTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}