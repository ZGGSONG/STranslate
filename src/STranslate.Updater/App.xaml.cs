using System.Windows;

namespace STranslate.Updater;

public partial class App : Application
{
    private readonly string defaultVersion = "1.0.0.101";

    private Mutex? mutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        //阻止多开和用户主动启动
        if (e.Args.Length == 0 || IsAlreadyRunning()) Environment.Exit(0);

        base.OnStartup(e);

        var version = e.Args.FirstOrDefault() ?? defaultVersion;
        var mainWindow = new MainWindow(version);
        mainWindow.Show();
    }

    /// <summary>
    ///     获取当前程序是否已运行
    /// </summary>
    private bool IsAlreadyRunning()
    {
        mutex = new Mutex(true, "9B65585D-AEB9-2CB9-AA73-216DA90DD186", out var isCreatedNew);
        return !isCreatedNew;
    }
}