namespace STranslate.Util;

/// <summary>
///     单例模式
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : class, new()
{
    private static readonly Lazy<T> _instance = new(() => (T)Activator.CreateInstance(typeof(T), true)!, true);

    public static T Instance => _instance.Value;
}