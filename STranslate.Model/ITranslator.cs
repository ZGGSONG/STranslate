using System.Threading.Tasks;
using System.Threading;
using System;

namespace STranslate.Model
{
    public interface ITranslator
    {
        Guid Identify { get; set; }

        ServiceType Type { get; set; }

        public bool IsEnabled { get; set; }

        IconType Icon { get; set; }

        string Name { get; set; }

        string Url { get; set; }

        object Data { get; set; }

        string AppID { get; set; }

        string AppKey { get; set; }

        Task<object> TranslateAsync(object request, CancellationToken token);
    }
}
