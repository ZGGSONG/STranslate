using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STranslate.Model
{
    public interface ITTS
    {
        Guid Identify { get; set; }

        TTSType Type { get; set; }

        string Name { get; set; }

        string Url { get; set; }

        string AppID { get; set; }

        string AppKey { get; set; }

        Task SpeakTextAsync(string text, CancellationToken token);
    }
}
