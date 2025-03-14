using System.Windows;

namespace STranslate.Style.Themes;

public class ThemeManager
{
    private readonly Dictionary<string, ResourceDictionary> _themes = new();

    public void RegisterTheme(string themeName, string assemblyName, string resourcePath)
    {
        var uri = $"/{assemblyName};component/{resourcePath}";

        var resource = new ResourceDictionary
        {
            Source = new Uri(uri, UriKind.RelativeOrAbsolute)
        };

        _themes.Add(themeName, resource);
    }

    public void ApplyTheme(string themeName)
    {
        var resource = _themes[themeName];

        Application.Current.Resources.MergedDictionaries.Add(resource);
    }
}