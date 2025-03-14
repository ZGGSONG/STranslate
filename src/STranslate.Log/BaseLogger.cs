using System.Text;

namespace STranslate.Log;

public class BaseLogger : ILogger
{
    public virtual void Debug(string message)
    {
        WriteLine("DBG", message);
    }

    public virtual void Info(string message)
    {
        WriteLine("INF", message);
    }

    public virtual void Warn(string message)
    {
        WriteLine("WRN", message);
    }

    public virtual void Error(string message)
    {
        WriteLine("ERR", message);
    }

    public virtual void Error(string message, Exception ex)
    {
        WriteLine("ERR", message, ex);
    }

    public virtual void Fatal(string message)
    {
        WriteLine("FTL", message);
    }

    public virtual void Fatal(string message, Exception ex)
    {
        WriteLine("FTL", message, ex);
    }

    public virtual void Dispose()
    {
        WriteLine("DBG", $"{nameof(BaseLogger)} Dispose");
    }

    public Action<string>? OnErrorOccured { get; set; }

    internal void WriteLine(string type, string message, Exception? ex = default)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var sb = new StringBuilder($"{timestamp} [{type}] {message}");
        if (ex != null)
            sb.Append($", Exception: {ex}");

        var combineStr = sb.ToString();
        System.Diagnostics.Debug.WriteLine(combineStr);
        Console.WriteLine(combineStr);
        if (type == "ERR" || type == "FTL")
            OnErrorOccured?.Invoke(combineStr);
    }
}
