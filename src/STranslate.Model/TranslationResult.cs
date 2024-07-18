using CommunityToolkit.Mvvm.ComponentModel;

namespace STranslate.Model;

public partial class TranslationResult : ObservableObject
{
    [ObservableProperty] private bool _isSuccess = true;

    [ObservableProperty] private object? _result;

    /// <summary>
    ///     成功时使用的构造函数
    /// </summary>
    /// <param name="result"></param>
    private TranslationResult(object result)
    {
        IsSuccess = true;
        Result = result;
        Exception = null;
    }

    /// <summary>
    ///     失败时使用的构造函数
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <param name="exception"></param>
    private TranslationResult(string errorMessage, Exception? exception = null)
    {
        IsSuccess = false;
        Result = errorMessage;
        //Exception = null;//暂时不记录异常
        Exception = exception;
    }

    /// <summary>
    ///     清空时使用的构造函数
    /// </summary>
    private TranslationResult()
    {
        IsSuccess = true;
        Result = string.Empty;
        Exception = null;
    }

    public Exception? Exception { get; } // 可选，如果你想保留异常的详细信息

    /// <summary>
    ///     静态方法用于清空
    /// </summary>
    /// <returns></returns>
    public static TranslationResult Reset => new();

    /// <summary>
    ///     静态方法用于创建成功的结果
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static TranslationResult Success(object result)
    {
        return new TranslationResult(result);
    }

    /// <summary>
    ///     静态方法用于创建失败的结果
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static TranslationResult Fail(string errorMessage, Exception? exception = null)
    {
        return new TranslationResult(errorMessage, exception);
    }
}