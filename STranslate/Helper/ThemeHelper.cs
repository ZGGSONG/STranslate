using STranslate.Model;
using STranslate.Util;
using System.Linq;
using System.Windows;

namespace STranslate.Helper
{
    public class ThemeHelper
    {
        public static void StartListenRegistry()
        {
            if (registryMonitor.IsMonitoring)
                return;

            registryMonitor.RegChanged += OnRegChanged;
            registryMonitor.Start();
            InitialTheme();
        }

        public static void StopListenRegistry()
        {
            registryMonitor.Dispose();
            registryMonitor.RegChanged -= OnRegChanged;
        }

        private static void InitialTheme()
        {
            var SystemUsesLightTheme = RegistryMonitor.GetRegistryValue(ConstStr.REGISTRY, ConstStr.REGISTRYKEY);
            OnRegChanged(SystemUsesLightTheme);
        }

        public static void LightTheme() => OnRegChanged("1");

        public static void DarkTheme() => OnRegChanged("0");

        private static void OnRegChanged(string type)
        {
            Application.Current.Resources.MergedDictionaries.First().Source = type == "1" ? ConstStr.LIGHTURI : ConstStr.DARKURI;
        }

        private static readonly RegistryMonitor registryMonitor = new(ConstStr.REGISTRYHIVE, ConstStr.REGISTRY, ConstStr.REGISTRYKEY);
    }
}
