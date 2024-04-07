using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.Model
{
    public interface ITranslator : INotifyPropertyChanged
    {
        Guid Identify { get; set; }

        ServiceType Type { get; set; }

        IconType Icon { get; set; }

        bool IsEnabled { get; set; }

        string Name { get; set; }

        string Url { get; set; }

        bool AutoExpander { get; set; }

        TranslationResult Data { get; set; }

        BindingList<UserDefinePrompt> UserDefinePrompts { get; set; }

        string AppID { get; set; }

        string AppKey { get; set; }

        Task<TranslationResult> TranslateAsync(object request, CancellationToken token);

        Task TranslateAsync(object request, Action<string> OnDataReceived, CancellationToken token);

        ITranslator Clone();
    }
}
