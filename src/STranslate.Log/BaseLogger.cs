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

    internal static void WriteLine(string type, string message)
    {
        System.Diagnostics.Debug.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{type}] {message}");
    }

    internal static void WriteLine(string type, string message, Exception ex)
    {
        System.Diagnostics.Debug.WriteLine(
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{type}] {message}, Exception: {ex}");
    }
}