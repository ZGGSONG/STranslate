namespace STranslate.Log;

public enum LogLevel
{
    /// <summary>
    ///     Most verbose level. Used for development and seldom enabled in production
    /// </summary>
    Trace = 0,

    /// <summary>
    ///     Debugging the application behavior from internal events of interest
    /// </summary>
    Debug,

    /// <summary>
    ///     Information that highlights progress or application lifetime events
    /// </summary>
    Info,

    /// <summary>
    ///     Warnings about validation issues or temporary failures that can be recovered.
    /// </summary>
    Warn,

    /// <summary>
    ///     Errors where functionality has failed or <see cref="System.Exception" /> have been caught.
    /// </summary>
    Error,

    /// <summary>
    ///     Most critical level. Application is about to abort.
    /// </summary>
    Fatal
}