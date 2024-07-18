namespace STranslate.Log;

public class LogService
{
#if true

    public static void Register(string name = "", LogLevel minLevel = LogLevel.Debug)
    {
        _logger = name.ToLower() switch
        {
            "serilog" => new SerilogLogger(minLevel),
            //"nlog" => new NLogLogger(level),
            _ => new SerilogLogger(minLevel)
        };
    }

    public static void UnRegister()
    {
        _logger?.Dispose();
    }

    private static ILogger? _logger;

    public static ILogger Logger
    {
        get => _logger!;
        set => _logger = value;
    }

#else
        private static readonly Lazy<ILogger> _logger = new(() => new SerilogLogger());
        public static ILogger Logger => _logger.Value;

        public static void UnRegister()
        {
            _logger.Value.Dispose();
        }
#endif
}