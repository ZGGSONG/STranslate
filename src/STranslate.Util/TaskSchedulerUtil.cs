using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;

namespace STranslate.Util;

public static class TaskSchedulerUtil
{
    private const string DefaultAuthorPrefix = "STranslate";
    private const string DefaultUserSid = "S-1-5-21-0-0-0-1001";

    // 缓存系统编码，避免重复获取
    private static Encoding? _systemEncoding;
    private static Encoding SystemEncoding => _systemEncoding ??= GetSystemEncoding();

    /// <summary>
    /// 静态构造函数 - 注册编码提供程序
    /// </summary>
    static TaskSchedulerUtil()
    {
        try
        {
            // 注册编码提供程序以支持 GBK 等编码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        catch
        {
            // 忽略注册失败
        }
    }

    /// <summary>
    /// 创建任务计划（无触发器，手动执行）
    /// </summary>
    public static TaskOperationResult CreateTask(string exePath, string taskName, string? author = null)
    {
        var config = new TaskConfiguration
        {
            ExecutablePath = exePath,
            TaskName = taskName,
            Author = author
        };
        return CreateTask(config);
    }

    /// <summary>
    /// 创建任务计划（使用配置对象）
    /// </summary>
    public static TaskOperationResult CreateTask(TaskConfiguration config)
    {
        try
        {
            // 参数验证
            var validationResult = ValidateTaskConfiguration(config);
            if (!validationResult.Success)
                return validationResult;

            // 检查管理员权限
            if (!IsRunAsAdministrator())
                return TaskOperationResult.CreateFailure("需要管理员权限才能创建任务计划");

            // 检查文件是否存在
            if (!File.Exists(config.ExecutablePath))
                return TaskOperationResult.CreateFailure($"找不到可执行文件: {config.ExecutablePath}");

            // 设置默认值
            config.WorkingDirectory ??= Path.GetDirectoryName(config.ExecutablePath);
            config.Author ??= $"{DefaultAuthorPrefix} - {Environment.UserName.ToLower()}";

            // 删除现有任务
            var existsResult = TaskExists(config.TaskName);
            if (existsResult.Success)
            {
                var deleteResult = DeleteTask(config.TaskName);
                if (!deleteResult.Success)
                    return TaskOperationResult.CreateFailure($"无法删除现有任务: {deleteResult.Message}");
            }

            // 创建XML配置并执行
            return CreateTaskFromConfiguration(config);
        }
        catch (Exception ex)
        {
            return TaskOperationResult.CreateFailure("创建任务时发生异常", ex.Message, ex);
        }
    }

    /// <summary>
    /// 删除任务计划
    /// </summary>
    public static TaskOperationResult DeleteTask(string taskName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskName))
                return TaskOperationResult.CreateFailure("任务名称不能为空");

            if (!IsRunAsAdministrator())
                return TaskOperationResult.CreateFailure("需要管理员权限才能删除任务计划");

            return ExecuteSchTasksCommand($"/Delete /TN \"{taskName}\" /F", "删除任务");
        }
        catch (Exception ex)
        {
            return TaskOperationResult.CreateFailure("删除任务时发生异常", ex.Message, ex);
        }
    }

    /// <summary>
    /// 检查任务是否存在
    /// </summary>
    public static TaskOperationResult TaskExists(string taskName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskName))
                return TaskOperationResult.CreateFailure("任务名称不能为空");

            var result = ExecuteSchTasksCommand($"/Query /TN \"{taskName}\"", "查询任务");
            result.Message = result.Success ? "任务存在" : "任务不存在";
            return result;
        }
        catch (Exception ex)
        {
            return TaskOperationResult.CreateFailure("查询任务时发生异常", ex.Message, ex);
        }
    }

    /// <summary>
    /// 运行指定的任务计划
    /// </summary>
    public static TaskOperationResult RunTask(string taskName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskName))
                return TaskOperationResult.CreateFailure("任务名称不能为空");

            return ExecuteSchTasksCommand($"/Run /TN \"{taskName}\"", "运行任务");
        }
        catch (Exception ex)
        {
            return TaskOperationResult.CreateFailure("运行任务时发生异常", ex.Message, ex);
        }
    }

    /// <summary>
    /// 获取任务详细信息
    /// </summary>
    public static TaskOperationResult GetTaskInfo(string taskName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(taskName))
                return TaskOperationResult.CreateFailure("任务名称不能为空");

            return ExecuteSchTasksCommand($"/Query /TN \"{taskName}\" /FO LIST /V", "获取任务信息");
        }
        catch (Exception ex)
        {
            return TaskOperationResult.CreateFailure("获取任务信息时发生异常", ex.Message, ex);
        }
    }

    /// <summary>
    /// 列出所有任务（可选择性过滤）
    /// </summary>
    public static TaskOperationResult ListTasks(string? filter = null)
    {
        try
        {
            string args = "/Query /FO LIST";
            if (!string.IsNullOrWhiteSpace(filter))
                args += $" /TN \"{filter}\"";

            return ExecuteSchTasksCommand(args, "列出任务");
        }
        catch (Exception ex)
        {
            return TaskOperationResult.CreateFailure("列出任务时发生异常", ex.Message, ex);
        }
    }

    #region 私有方法

    /// <summary>
    /// 获取系统编码
    /// </summary>
    private static Encoding GetSystemEncoding()
    {
        try
        {
            // 方法1：通过控制台输出代码页获取
            int codePage = GetConsoleOutputCP();
            if (codePage != 0)
            {
                try
                {
                    return Encoding.GetEncoding(codePage);
                }
                catch
                {
                    // 如果代码页不支持，继续下一个方法
                }
            }

            // 方法2：通过系统默认编码获取
            try
            {
                return Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
            }
            catch
            {
                // 如果 OEM 代码页不支持，继续下一个方法
            }

            // 方法3：根据系统语言使用常用编码
            var cultureName = CultureInfo.CurrentCulture.Name;
            if (cultureName.StartsWith("zh"))
            {
                // 中文系统，尝试不同的中文编码
                try
                {
                    return Encoding.GetEncoding("GB2312"); // 简体中文
                }
                catch
                {
                    try
                    {
                        return Encoding.GetEncoding(936); // GBK 代码页
                    }
                    catch
                    {
                        return Encoding.GetEncoding("UTF-8"); // 最后fallback到UTF-8
                    }
                }
            }

            // 方法4：默认使用系统默认编码
            return Encoding.Default;
        }
        catch
        {
            // 最终fallback：UTF-8
            return Encoding.UTF8;
        }
    }

    [DllImport("kernel32.dll")]
    private static extern int GetConsoleOutputCP();

    private static TaskOperationResult ValidateTaskConfiguration(TaskConfiguration config)
    {
        if (config == null)
            return TaskOperationResult.CreateFailure("配置对象不能为空");

        if (string.IsNullOrWhiteSpace(config.ExecutablePath))
            return TaskOperationResult.CreateFailure("可执行文件路径不能为空");

        if (string.IsNullOrWhiteSpace(config.TaskName))
            return TaskOperationResult.CreateFailure("任务名称不能为空");

        if (config.TaskName.Contains('"') || config.TaskName.Contains('\\'))
            return TaskOperationResult.CreateFailure("任务名称包含非法字符");

        return TaskOperationResult.CreateSuccess("配置验证通过");
    }

    private static bool IsRunAsAdministrator()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }

    private static string GetCurrentUserSid()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            return identity.User?.ToString() ?? DefaultUserSid;
        }
        catch
        {
            return DefaultUserSid;
        }
    }

    private static TaskOperationResult CreateTaskFromConfiguration(TaskConfiguration config)
    {
        string? tempXmlPath = null;
        try
        {
            // 生成XML内容
            string xmlContent = CreateTaskXml(config);
            tempXmlPath = Path.Combine(Path.GetTempPath(), $"Task_{Guid.NewGuid():N}.xml");

            // 写入临时文件 - 使用 Unicode 编码（UTF-16LE）
            File.WriteAllText(tempXmlPath, xmlContent, Encoding.Unicode);

            // 执行创建命令
            return ExecuteSchTasksCommand($"/Create /TN \"{config.TaskName}\" /XML \"{tempXmlPath}\" /F", "创建任务");
        }
        finally
        {
            // 清理临时文件
            if (tempXmlPath != null && File.Exists(tempXmlPath))
            {
                try
                {
                    File.Delete(tempXmlPath);
                }
                catch
                {
                    // 忽略清理失败
                }
            }
        }
    }

    private static TaskOperationResult ExecuteSchTasksCommand(string arguments, string operationName)
    {
        using var process = CreateSchTasksProcess(arguments);

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        // 异步读取输出，避免死锁
        process.OutputDataReceived += (_, e) => {
            if (!string.IsNullOrEmpty(e.Data))
                outputBuilder.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (_, e) => {
            if (!string.IsNullOrEmpty(e.Data))
                errorBuilder.AppendLine(e.Data);
        };

        process.Start();

        // 开始异步读取
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();

        string output = outputBuilder.ToString().Trim();
        string error = errorBuilder.ToString().Trim();
        bool success = process.ExitCode == 0;

        return new TaskOperationResult
        {
            Success = success,
            Message = success ? $"{operationName}成功" : $"{operationName}失败 (ExitCode: {process.ExitCode})",
            Output = output,
            Error = error,
            ExitCode = process.ExitCode
        };
    }

    private static Process CreateSchTasksProcess(string arguments)
    {
        return new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                // 使用系统编码来正确处理中文输出
                StandardOutputEncoding = SystemEncoding,
                StandardErrorEncoding = SystemEncoding
            }
        };
    }

    private static string CreateTaskXml(TaskConfiguration config)
    {
        string userSid = GetCurrentUserSid();

        var xml = new StringBuilder();
        xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-16\"?>");
        xml.AppendLine("<Task version=\"1.2\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">");
        xml.AppendLine("  <RegistrationInfo>");
        xml.AppendLine($"    <Author>{SecurityElement.Escape(config.Author!)}</Author>");
        xml.AppendLine($"    <URI>\\{SecurityElement.Escape(config.TaskName)}</URI>");
        xml.AppendLine($"    <Date>{DateTime.Now:yyyy-MM-ddTHH:mm:ss}</Date>");
        xml.AppendLine("  </RegistrationInfo>");
        xml.AppendLine("  <Triggers />");
        xml.AppendLine("  <Principals>");
        xml.AppendLine("    <Principal id=\"Author\">");
        xml.AppendLine($"      <UserId>{userSid}</UserId>");
        xml.AppendLine("      <LogonType>InteractiveToken</LogonType>");
        xml.AppendLine("      <RunLevel>HighestAvailable</RunLevel>");
        xml.AppendLine("    </Principal>");
        xml.AppendLine("  </Principals>");
        xml.AppendLine("  <Settings>");
        xml.AppendLine($"    <DisallowStartIfOnBatteries>{config.DisallowStartIfOnBatteries.ToString().ToLower()}</DisallowStartIfOnBatteries>");
        xml.AppendLine($"    <StopIfGoingOnBatteries>{config.StopIfGoingOnBatteries.ToString().ToLower()}</StopIfGoingOnBatteries>");
        xml.AppendLine("    <AllowHardTerminate>true</AllowHardTerminate>");
        xml.AppendLine("    <StartWhenAvailable>false</StartWhenAvailable>");
        xml.AppendLine("    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>");
        xml.AppendLine("    <IdleSettings>");
        xml.AppendLine("      <StopOnIdleEnd>true</StopOnIdleEnd>");
        xml.AppendLine("      <RestartOnIdle>false</RestartOnIdle>");
        xml.AppendLine("    </IdleSettings>");
        xml.AppendLine($"    <AllowStartOnDemand>{config.AllowStartOnDemand.ToString().ToLower()}</AllowStartOnDemand>");
        xml.AppendLine("    <Enabled>true</Enabled>");
        xml.AppendLine($"    <Hidden>{config.Hidden.ToString().ToLower()}</Hidden>");
        xml.AppendLine("    <RunOnlyIfIdle>false</RunOnlyIfIdle>");
        xml.AppendLine("    <WakeToRun>false</WakeToRun>");
        xml.AppendLine($"    <ExecutionTimeLimit>PT{config.ExecutionTimeLimit.TotalHours:F0}H</ExecutionTimeLimit>");
        xml.AppendLine($"    <Priority>{(int)config.Priority}</Priority>");
        xml.AppendLine("  </Settings>");
        xml.AppendLine("  <Actions Context=\"Author\">");
        xml.AppendLine("    <Exec>");
        xml.AppendLine($"      <Command>{SecurityElement.Escape(config.ExecutablePath)}</Command>");
        xml.AppendLine($"      <WorkingDirectory>{SecurityElement.Escape(config.WorkingDirectory!)}</WorkingDirectory>");
        xml.AppendLine("    </Exec>");
        xml.AppendLine("  </Actions>");
        xml.AppendLine("</Task>");

        return xml.ToString();
    }

    #endregion
}

/// <summary>
/// 操作结果模型
/// </summary>
public class TaskOperationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public int ExitCode { get; set; }
    public Exception? Exception { get; set; }

    public static TaskOperationResult CreateSuccess(string message = "操作成功", string output = "")
        => new() { Success = true, Message = message, Output = output };

    public static TaskOperationResult CreateFailure(string message, string error = "", Exception? ex = null)
        => new() { Success = false, Message = message, Error = error, Exception = ex };
}

/// <summary>
/// 任务计划配置
/// </summary>
public class TaskConfiguration
{
    public string ExecutablePath { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? WorkingDirectory { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;
    public TimeSpan ExecutionTimeLimit { get; set; } = TimeSpan.FromHours(72);
    public bool AllowStartOnDemand { get; set; } = true;
    public bool DisallowStartIfOnBatteries { get; set; } = false;
    public bool StopIfGoingOnBatteries { get; set; } = false;
    public bool Hidden { get; set; } = false;
}

public enum TaskPriority
{
    Critical = 0,
    High = 1,
    Normal = 4,
    Low = 7,
    Idle = 10
}