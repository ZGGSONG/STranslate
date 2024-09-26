using CommunityToolkit.Mvvm.ComponentModel;

namespace STranslate.Model;

public partial class TranslationResult : ObservableObject
{
    [ObservableProperty] private bool _isSuccess = true;
    [ObservableProperty] private string _result = string.Empty;

    [ObservableProperty] private bool _isTranslateBackSuccess = true;
    [ObservableProperty] private string _translateBackResult = string.Empty;

    public Exception? Exception { get; set; }

    /// <summary>
    ///     成功时使用的构造函数
    /// </summary>
    /// <param name="result"></param>
    private TranslationResult(string result)
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
        Exception = exception;
    }

    /// <summary>
    ///     清空时使用的构造函数
    /// </summary>
    private TranslationResult()
    {
        IsSuccess = true;
        Result = string.Empty;
        IsTranslateBackSuccess = true;
        TranslateBackResult = string.Empty;
        Exception = null;
    }

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
    public static TranslationResult Success(string result)
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

    public static void CopyFrom(TranslationResult source, TranslationResult target)
    {
        target.IsSuccess = source.IsSuccess;
        target.Result = source.Result;
        target.IsTranslateBackSuccess = source.IsTranslateBackSuccess;
        target.TranslateBackResult = source.TranslateBackResult;
        target.Exception = source.Exception;
    }
}