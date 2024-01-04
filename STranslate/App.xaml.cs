using STranslate.Log;
using STranslate.Style.Controls;
using STranslate.Util;
using STranslate.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace STranslate
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. 检查是否已经具有管理员权限
            if (NeedAdministrator())
            {
                // 如果没有管理员权限，可以提示用户提升权限
                if (TryRunAsAdministrator())
                {
                    // 如果提升权限成功，关闭当前实例
                    Application.Current.Shutdown();
                    return;
                }
            }

            // 2. 多开检测
            if (IsAnotherInstanceRunning())
            {
                MessageBox_S.Show($"{Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()!.Location)} 应用程序已经在运行中。", "多开检测");
                Application.Current.Shutdown();
                return;
            }

            // 3. 启动应用程序
#if DEBUG
            LogService.Register();
#elif !DEBUG
            LogService.Register(minLevel: LogLevel.Info);
#endif
            //TODO: 支持系统代理，仍需优化热重载系统代理切换
            HttpUtil.SupportSystemAgent();

            StartProgram();

            ExceptionHandler();
        }
        private bool NeedAdministrator()
        {
            //加载配置
            var isRole = Singleton<ConfigHelper>.Instance.CurrentConfig?.NeedAdministrator ?? false;

            if (!isRole) return false;

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return !principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private bool TryRunAsAdministrator()
        {
            var dll = Assembly.GetExecutingAssembly().Location;
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = dll.Substring(0, dll.Length - 3) + "exe",
                UseShellExecute = true,
                Verb = "runas" // 提升权限
            };

            try
            {
                Process.Start(startInfo);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsAnotherInstanceRunning()
        {
            //Process currentProcess = Process.GetCurrentProcess();
            //string currentProcessName = Path.GetFileNameWithoutExtension(currentProcess.MainModule.FileName);
            //var currentProcessName = "CE252DD8-179F-4544-9989-453F5DEA378D";
            var currentProcessName = "STranslate";
            Process[] runningProcesses = Process.GetProcessesByName(currentProcessName);
            return runningProcesses.Length > 1;
        }

        private void StartProgram()
        {
            LogService.Logger.Info($"{Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()!.Location)} Opened...");
            new MainView()!.Show();
        }

        /// <summary>
        /// 异常处理监听
        /// </summary>
        private void ExceptionHandler()
        {
            //UI线程未捕获异常处理事件（UI主线程）
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (LogService.Logger != null)
            {
                LogService.Logger.Info($"{Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()!.Location)} Closed...");
                LogService.UnRegister();
            }
            base.OnExit(e);
        }

        //UI线程未捕获异常处理事件（UI主线程）
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            //异常信息 和 调用堆栈信息
            //string msg = String.Format("{0}\n\n{1}", ex.Message, ex.StackTrace);
            LogService.Logger.Error("UI线程异常", ex);
            e.Handled = true;//表示异常已处理，可以继续运行
        }

        //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
        //如果UI线程异常DispatcherUnhandledException未注册，则如果发生了UI线程未处理异常也会触发此异常事件
        //此机制的异常捕获后应用程序会直接终止。没有像DispatcherUnhandledException事件中的Handler=true的处理方式，可以通过比如Dispatcher.Invoke将子线程异常丢在UI主线程异常处理机制中处理
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex && ex != null)
            {
                //string msg = String.Format("{0}\n\n{1}", ex.Message, ex.StackTrace);
                LogService.Logger.Error("非UI线程异常", ex);
            }
        }

        //Task线程内未捕获异常处理事件
        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            //string msg = String.Format("{0}\n\n{1}", ex.Message, ex.StackTrace);
            LogService.Logger.Error("Task异常", ex);
        }
    }
}
