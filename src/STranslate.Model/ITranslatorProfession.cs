using System.Collections.ObjectModel;

namespace STranslate.Model;

public interface ITranslatorProfession
{
    ObservableCollection<string> Models { get; set; }

    ObservableCollection<Term> Terms { get; set; }
}
