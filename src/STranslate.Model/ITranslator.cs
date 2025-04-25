using System.ComponentModel;

namespace STranslate.Model;

public interface ITranslator : INotifyPropertyChanged
{
    Guid Identify { get; set; }

    ServiceType Type { get; set; }

    IconType Icon { get; set; }

    bool IsEnabled { get; set; }

    string Name { get; set; }

    string Url { get; set; }

    bool AutoExecute { get; set; }

    bool AutoExecuteTranslateBack { get; set; }

    TranslationResult Data { get; set; }

    string AppID { get; set; }

    string AppKey { get; set; }

    bool IsExecuting { get; set; }

    bool IsTranslateBackExecuting { get; set; }

    string? LangConverter(LangEnum lang);

    Task<TranslationResult> TranslateAsync(object request, CancellationToken token);

    BindingList<UserDefinePrompt> UserDefinePrompts { get; set; }

    ITranslator Clone();
}
