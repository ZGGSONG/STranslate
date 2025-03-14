using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace STranslate.Log;

public class SerilogLogger : BaseLogger
{
    private readonly Logger _logger;

    public SerilogLogger(LogLevel minLevel = LogLevel.Debug)
    {
        var logConfiguration = new LoggerConfiguration()
            .WriteTo.File(
                string.Format("{0}logs/log.log", AppDomain.CurrentDomain.BaseDirectory),    //使用绝对路径创建日志文件
                //shared: true,
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Verbose,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .MinimumLevel.Is(ConvertToSerilogLevel(minLevel));

        _logger = logConfiguration.CreateLogger();
    }

    /// <summary>
    ///     将自定义的LogLevel转换为Serilog的LogEventLevel
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    private LogEventLevel ConvertToSerilogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Info => LogEventLevel.Information,
            LogLevel.Warn => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Fatal => LogEventLevel.Fatal,
            _ => LogEventLevel.Debug
        };
    }

    public override void Debug(string message)
    {
        base.Debug(message);
        _logger.Debug(message);
    }

    public override void Info(string message)
    {
        base.Info(message);
        _logger.Information(message);
    }

    public override void Warn(string message)
    {
        base.Warn(message);
        _logger.Warning(message);
    }

    public override void Error(string message)
    {
        base.Error(message);
        _logger.Error(message);
    }

    public override void Error(string message, Exception ex)
    {
        base.Error(message);
        _logger.Error(ex, message);
    }

    public override void Fatal(string message)
    {
        base.Fatal(message);
        _logger.Fatal(message);
    }

    public override void Fatal(string message, Exception ex)
    {
        base.Fatal(message);
        _logger.Fatal(ex, message);
    }

    public override void Dispose()
    {
        base.Dispose();
        _logger.Dispose();
    }
}