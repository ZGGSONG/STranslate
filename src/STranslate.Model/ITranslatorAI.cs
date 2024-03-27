using System.ComponentModel;

namespace STranslate.Model
{
    public interface ITranslatorAI : ITranslator
    {
        BindingList<UserDefinePrompt> UserDefinePrompts { get; set; }
    }
}
