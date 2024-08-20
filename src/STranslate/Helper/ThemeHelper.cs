using System.Windows;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.Helper;

public class ThemeHelper : IDisposable
{
    private static readonly RegistryMonitor registryMonitor =
        new(Constant.ThemeRegistryHive, Constant.ThemeRegistry, Constant.ThemeRegistryKey);

    public void Dispose()
    {
        registryMonitor?.Dispose();
    }

    public void StartListenRegistry()
    {
        if (registryMonitor.IsMonitoring)
            return;

        registryMonitor.RegChanged += OnRegChanged;
        registryMonitor.Start();
        InitialTheme();
    }

    public void StopListenRegistry()
    {
        registryMonitor.Stop();
        registryMonitor.RegChanged -= OnRegChanged;
    }

    private void InitialTheme()
    {
        var SystemUsesLightTheme = RegistryMonitor.GetRegistryValue(Constant.ThemeRegistry, Constant.ThemeRegistryKey);
        OnRegChanged(SystemUsesLightTheme);
    }

    public void LightTheme()
    {
        OnRegChanged("1");
    }

    public void DarkTheme()
    {
        OnRegChanged("0");
    }

    private void OnRegChanged(string type)
    {
        Application.Current.Resources.MergedDictionaries.First().Source =
            type == "1" ? Constant.LightUri : Constant.DarkUri;
    }
}