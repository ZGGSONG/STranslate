using System.ComponentModel;

namespace STranslate.Model;

public interface ITranslatorLLM : ITranslator
{
    double Temperature { get; set; }

    string Model { get; set; }

    BindingList<string> Models { get; set; }

    /// <summary>
    ///     手动通知属性更新
    /// </summary>
    /// <param name="name"></param>
    void ManualPropChanged(params string[] name);


    Task TranslateAsync(object request, Action<string> onDataReceived, CancellationToken token);
}
