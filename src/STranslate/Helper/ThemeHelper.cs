using System.Windows;
using STranslate.Model;
using STranslate.Util;

namespace STranslate.Helper;

public class ThemeHelper : IDisposable
{
    private RegistryMonitor? _registryMonitor;

    public void Dispose()
    {
        _registryMonitor?.Dispose();
    }

    public void SetTheme(ThemeType themeType)
    {
        switch (themeType)
        {
            case ThemeType.Dark:
                ManualApplyTheme("0");
                break;
            case ThemeType.Light:
                ManualApplyTheme("1");
                break;
            case ThemeType.FollowSystem:
                StartAutoApplyTheme(Constant.SystemThemeRegistryKey);
                break;
            case ThemeType.FollowApp:
                StartAutoApplyTheme(Constant.AppThemeRegistryKey);
                break;
        }
    }

    private void StartAutoApplyTheme(string monitorKey)
    {
        if (_registryMonitor == null)
        {
            _registryMonitor = new RegistryMonitor(Constant.ThemeRegistryHive, Constant.ThemeRegistry, monitorKey);
        }
        else if (_registryMonitor.IsMonitoring)
        {
            StopAutoApplyTheme();
        }

        _registryMonitor.RegChanged += ApplyTheme;
        _registryMonitor.Start();
        InitialTheme(monitorKey);
    }

    private void StopAutoApplyTheme()
    {
        if (_registryMonitor == null)
            return;

        _registryMonitor.Stop();
        _registryMonitor.RegChanged -= ApplyTheme;
        _registryMonitor.Dispose();
        _registryMonitor = null;
    }

    private void InitialTheme(string monitorKey)
    {
        var systemUsesLightTheme = RegistryMonitor.GetRegistryValue(Constant.ThemeRegistry, monitorKey);
        ApplyTheme(systemUsesLightTheme);
    }

    private void ManualApplyTheme(string isLight)
    {
        ApplyTheme(isLight);
        StopAutoApplyTheme();
    }

    private void ApplyTheme(string isLight)
    {
        Application.Current.Resources.MergedDictionaries.First().Source =
            isLight == "1" ? Constant.LightUri : Constant.DarkUri;
    }
}