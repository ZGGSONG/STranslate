using System.Globalization;
using System.Windows;

namespace STranslate.Model;

public class AppLanguageManager
{
    /// <summary>
    /// 语言变更事件
    /// </summary>
    public static event EventHandler? LanguageChanged;

    /// <summary>
    /// 切换应用程序语言
    /// </summary>
    /// <param name="lang"></param>
    public static void SwitchLanguage(AppLanguageKind lang)
    {
        var languageCode = lang.ToString().Replace("_", "-");
        // 设置当前线程的文化信息
        var cultureInfo = new CultureInfo(languageCode);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        // 更新资源字典
        ResourceDictionary resourceDict = new()
        {
            Source = new Uri($"/STranslate.Style;component/Styles/Localizations/Language.{languageCode}.xaml", UriKind.Relative)
        };

        // 移除旧的资源字典并添加新的
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        for (var i = 0; i < dictionaries.Count; i++)
        {
            var dict = dictionaries[i];
            if (dict.Source == null || !dict.Source.OriginalString.Contains("Language")) continue;
            dictionaries.Remove(dict);
            break;
        }

        Application.Current.Resources.MergedDictionaries.Add(resourceDict);

        // 触发语言变更事件
        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// 根据配置初始化应用程序语言
    /// </summary>
    /// <param name="config">应用程序配置</param>
    public static void InitializeLanguage(ConfigModel? config)
    {
        SwitchLanguage(config?.AppLanguage ?? AppLanguageKind.zh_Hans_CN);
    }

    /// <summary>
    /// 获取本地化字符串
    /// </summary>
    /// <param name="key">资源键</param>
    /// <returns>本地化字符串，如果未找到则返回键名</returns>
    public static string GetString(string key)
    {
        return Application.Current.Resources[key] as string ?? key;
    }
}