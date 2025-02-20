using System.ComponentModel;

namespace STranslate.Model;

public interface IOCRLLM : IOCR
{
    double Temperature { get; set; }
    string Model { get; set; }
    BindingList<UserDefinePrompt> UserDefinePrompts { get; set; }
}
