using System.Windows;

namespace STranslate.Updater
{
    public partial class App : Application
    {
        private readonly string defaultVersion = "1.0.0.0101";

        private Mutex? mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            // 阻止多开和用户主动启动
            if (e.Args.Length == 0 || IsAlreadyRunning())
            {
                Environment.Exit(0);
            }

            base.OnStartup(e);

            var version = e.Args.FirstOrDefault() ?? defaultVersion;
            var mainWindow = new MainWindow(version) { DataContext = new MainViewModel(version) };
            mainWindow.Show();
        }

        /// <summary>
        /// 获取当前程序是否已运行
        /// </summary>
        private bool IsAlreadyRunning()
        {
            mutex = new Mutex(true, System.Reflection.Assembly.GetEntryAssembly()!.ManifestModule.Name, out bool isCreatedNew);
            return !isCreatedNew;
        }
    }
}
