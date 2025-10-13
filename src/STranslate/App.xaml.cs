﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using STranslate.Helper;
using STranslate.Log;
using STranslate.Model;
using STranslate.Style.Controls;
using STranslate.Util;
using STranslate.Views;

namespace STranslate;

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 开启日志服务
        LogService.Register();

        // 检查是否已经具有管理员权限
        if (NeedAdministrator())
        {
            RunAsAdministrator();
            Environment.Exit(0);
            return;
        }

        // 多开检测
        if (IsAnotherInstanceRunning())
        {
            MessageBox_S.Show($"{Constant.AppName} {AppLanguageManager.GetString("MessageBox.AlreadyRunning")}", AppLanguageManager.GetString("MessageBox.MultiOpeningDetection"));
            Environment.Exit(0);
            return;
        }

        // 开启监听系统代理
        ProxyUtil.LoadDynamicProxy();

        // 软件配置涉及初始化操作
        Singleton<ConfigHelper>.Instance.InitialOperate();

        // 启动应用程序
        StartProgram();

        // 全局异常处理
        ExceptionHandler();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        //释放监听系统代理资源
        ProxyUtil.UnLoadDynamicProxy();

        //释放主题帮助类
        Singleton<ThemeHelper>.Instance.Dispose();

        //打印退出日志并释放日志资源
        if (LogService.Logger != null)
        {
            var adminMsg = CommonUtil.IsUserAdministrator() ? "[Administrator]" : "";
            LogService.Logger.Info($"{Constant.AppName}_{Constant.AppVersion}{adminMsg} Closed...\n");
            LogService.UnRegister();
        }

        base.OnExit(e);
    }

    private bool NeedAdministrator()
    {
        // 加载配置
        var mode = Singleton<ConfigHelper>.Instance.CurrentConfig?.StartMode ?? StartModeKind.Normal;

        if (mode == StartModeKind.Normal)
            return false;

        return !CommonUtil.IsUserAdministrator();
    }

    private void RunAsAdministrator()
    {
        var mode = Singleton<ConfigHelper>.Instance.CurrentConfig?.StartMode ?? StartModeKind.Normal;
        var modeStr = mode switch
        {
            StartModeKind.Admin => "elevated",
            StartModeKind.SkipUACAdmin => "task",
            _ => throw new InvalidOperationException("Unsupported start mode for admin")
        };
        var target = mode switch
        {
            StartModeKind.Admin => $"{Constant.ExecutePath}{Constant.AppName}.exe",
            StartModeKind.SkipUACAdmin => Constant.TaskName,
            _ => throw new InvalidOperationException("Unsupported start mode for admin")
        };
        // 如果是跳过UAC管理员模式，则检查，如果缺失则先创建计划任务
        if (mode == StartModeKind.SkipUACAdmin)
        {
            var fileName = $"{Constant.ExecutePath}{Constant.AppName}.exe";
            var info = TaskSchedulerUtil.GetTaskInfo(Constant.TaskName);
            if (!info.Success || !info.Output.Contains(fileName))
            {
                LogService.Logger.Debug($"<App> 启动方式已选择为'{mode.GetDescription()}', 未检测已经存在计划任务'{Constant.TaskName}', 尝试创建");
                string[] args = ["task", "-a", "create", "-n", Constant.TaskName, "-p", fileName, "-f"];
                var isNeedAdmin = !CommonUtil.IsUserAdministrator();
                CommonUtil.ExecuteProgram(Constant.HostExePath, args, isNeedAdmin, true);
                LogService.Logger.Debug($"<App> 启动方式已选择为'{mode.GetDescription()}', 已创建计划任务'{Constant.TaskName}'");
            }
        }
        CommonUtil.ExecuteProgram(Constant.HostExePath, ["start", "-m", modeStr, "-t", target]);
    }

    [Obsolete]
    private bool TryRunAsAdministrator()
    {
        ProcessStartInfo startInfo =
            new()
            {
                FileName = $"{Constant.ExecutePath}{Constant.AppName}.exe",
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
        const string currentProcessName = "STranslate";
        var runningProcesses = Process.GetProcessesByName(currentProcessName);
        return runningProcesses.Length > 1;
    }

    private void StartProgram()
    {
        var adminMsg = CommonUtil.IsUserAdministrator() ? "[Administrator]" : "";
        LogService.Logger.Info($"{Constant.AppName}_{Constant.AppVersion}{adminMsg} Opened...");
        // 当前配置目录
        LogService.Logger.Info($"Config Path: {Constant.CnfPath}");
        new MainView().Show();
    }

    /// <summary>
    ///     异常处理监听
    /// </summary>
    private void ExceptionHandler()
    {
        //UI线程未捕获异常处理事件（UI主线程）
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        //Task线程内未捕获异常处理事件
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    //UI线程未捕获异常处理事件（UI主线程）
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var ex = e.Exception;
        //异常信息 和 调用堆栈信息
        //string msg = String.Format("{0}\n\n{1}", ex.Message, ex.StackTrace);
        LogService.Logger.Error("UI线程异常", ex);
        e.Handled = true; //表示异常已处理，可以继续运行
    }

    //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
    //如果UI线程异常DispatcherUnhandledException未注册，则如果发生了UI线程未处理异常也会触发此异常事件
    //此机制的异常捕获后应用程序会直接终止。没有像DispatcherUnhandledException事件中的Handler=true的处理方式，可以通过比如Dispatcher.Invoke将子线程异常丢在UI主线程异常处理机制中处理
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            //string msg = String.Format("{0}\n\n{1}", ex.Message, ex.StackTrace);
            LogService.Logger.Error("非UI线程异常", ex);
    }

    //Task线程内未捕获异常处理事件
    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var ex = e.Exception;
        //string msg = String.Format("{0}\n\n{1}", ex.Message, ex.StackTrace);
        LogService.Logger.Error("Task异常", ex);
    }
}