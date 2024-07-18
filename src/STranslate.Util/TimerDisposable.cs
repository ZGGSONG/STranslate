using System.Diagnostics;

namespace STranslate.Util;

/// <summary>
///     Represents a disposable timer that measures the elapsed time.
/// </summary>
public class TimerDisposable : IDisposable
{
    private readonly Action<TimeSpan>? _onElapsed;
    private readonly Stopwatch _stopwatch;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TimerDisposable" /> class.
    /// </summary>
    /// <param name="onElapsed">The action to be invoked when the timer is disposed.</param>
    public TimerDisposable(Action<TimeSpan>? onElapsed = null)
    {
        _onElapsed = onElapsed;
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    /// <summary>
    ///     Disposes the timer and stops the stopwatch.
    /// </summary>
    public void Dispose()
    {
        _stopwatch.Stop();
        _onElapsed?.Invoke(_stopwatch.Elapsed);
    }
}

/**
 *
 * Demo
 *
 *
    using (var _ = new TimerDisposable(elapsed => Console.WriteLine($"操作耗时: {elapsed}"), () => Console.WriteLine("操作开始")))
    {
        // 在这里执行需要计时的操作
        PerformTimeConsumingOperation();
    }
  */