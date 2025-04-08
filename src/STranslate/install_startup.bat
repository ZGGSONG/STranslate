@echo off
setlocal enabledelayedexpansion

:: 设置标题和颜色
title STranslate 自动启动配置
color 0A

:: 获取当前目录和应用信息
set "CURRENT_DIR=%~dp0"
set "EXE_NAME=STranslate.exe"
set "TASK_NAME=STranslate(Auto Run)"
set "CURRENT_USER=%USERNAME%"

echo =========================================
echo    STranslate 自动启动配置工具
echo    当前用户: %CURRENT_USER%
echo    当前目录: %CURRENT_DIR%
echo =========================================
echo.

:: 检查是否以管理员权限运行
net session >nul 2>&1
if %errorLevel% neq 0 (
    color 0C
    echo [错误] 请以管理员权限运行此脚本！
    echo.
    echo 请右键点击此脚本，选择"以管理员身份运行"
    echo.
    pause
    exit /b 1
)

:: 检查目标程序是否存在
if not exist "%CURRENT_DIR%%EXE_NAME%" (
    color 0C
    echo [错误] 未找到 %EXE_NAME%！
    echo 请确保此脚本与 %EXE_NAME% 位于同一目录。
    echo.
    pause
    exit /b 1
)

:: 询问用户是否需要管理员权限运行程序
choice /C YN /M "是否需要以管理员权限运行程序？(Y=是, N=否)"
set ADMIN_RUN=%ERRORLEVEL%

echo.
echo 正在创建计划任务...

:: 删除旧的任务计划（如果存在）
schtasks /Query /TN "%TASK_NAME%" >nul 2>&1
if !errorLevel! equ 0 (
    echo 发现现有任务计划，正在移除...
    schtasks /Delete /TN "%TASK_NAME%" /F >nul 2>&1
)

:: 创建任务计划 XML 文件以便包含工作目录
echo ^<?xml version="1.0" encoding="UTF-16"?^> > "%TEMP%\stask.xml"
echo ^<Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task"^> >> "%TEMP%\stask.xml"
echo   ^<RegistrationInfo^> >> "%TEMP%\stask.xml"
echo     ^<Description^>STranslate 自动启动任务^</Description^> >> "%TEMP%\stask.xml"
echo     ^<Author^>%CURRENT_USER%^</Author^> >> "%TEMP%\stask.xml"
echo   ^</RegistrationInfo^> >> "%TEMP%\stask.xml"
echo   ^<Triggers^> >> "%TEMP%\stask.xml"
echo     ^<LogonTrigger^> >> "%TEMP%\stask.xml"
echo       ^<Enabled^>true^</Enabled^> >> "%TEMP%\stask.xml"
echo     ^</LogonTrigger^> >> "%TEMP%\stask.xml"
echo   ^</Triggers^> >> "%TEMP%\stask.xml"
echo   ^<Principals^> >> "%TEMP%\stask.xml"
echo     ^<Principal id="Author"^> >> "%TEMP%\stask.xml"

:: 根据用户选择设置权限级别
if %ADMIN_RUN% equ 1 (
    echo       ^<RunLevel^>HighestAvailable^</RunLevel^> >> "%TEMP%\stask.xml"
) else (
    echo       ^<RunLevel^>LeastPrivilege^</RunLevel^> >> "%TEMP%\stask.xml"
)

echo       ^<UserId^>%USERDOMAIN%\%USERNAME%^</UserId^> >> "%TEMP%\stask.xml"
echo       ^<LogonType^>InteractiveToken^</LogonType^> >> "%TEMP%\stask.xml"
echo     ^</Principal^> >> "%TEMP%\stask.xml"
echo   ^</Principals^> >> "%TEMP%\stask.xml"
echo   ^<Settings^> >> "%TEMP%\stask.xml"
echo     ^<MultipleInstancesPolicy^>IgnoreNew^</MultipleInstancesPolicy^> >> "%TEMP%\stask.xml"
echo     ^<DisallowStartIfOnBatteries^>false^</DisallowStartIfOnBatteries^> >> "%TEMP%\stask.xml"
echo     ^<StopIfGoingOnBatteries^>false^</StopIfGoingOnBatteries^> >> "%TEMP%\stask.xml"
echo     ^<AllowHardTerminate^>true^</AllowHardTerminate^> >> "%TEMP%\stask.xml"
echo     ^<StartWhenAvailable^>false^</StartWhenAvailable^> >> "%TEMP%\stask.xml"
echo     ^<RunOnlyIfNetworkAvailable^>false^</RunOnlyIfNetworkAvailable^> >> "%TEMP%\stask.xml"
echo     ^<IdleSettings^> >> "%TEMP%\stask.xml"
echo       ^<StopOnIdleEnd^>false^</StopOnIdleEnd^> >> "%TEMP%\stask.xml"
echo       ^<RestartOnIdle^>false^</RestartOnIdle^> >> "%TEMP%\stask.xml"
echo     ^</IdleSettings^> >> "%TEMP%\stask.xml"
echo     ^<AllowStartOnDemand^>true^</AllowStartOnDemand^> >> "%TEMP%\stask.xml"
echo     ^<Enabled^>true^</Enabled^> >> "%TEMP%\stask.xml"
echo     ^<Hidden^>false^</Hidden^> >> "%TEMP%\stask.xml"
echo     ^<RunOnlyIfIdle^>false^</RunOnlyIfIdle^> >> "%TEMP%\stask.xml"
echo     ^<WakeToRun^>false^</WakeToRun^> >> "%TEMP%\stask.xml"
echo     ^<ExecutionTimeLimit^>PT0S^</ExecutionTimeLimit^> >> "%TEMP%\stask.xml"
echo     ^<Priority^>7^</Priority^> >> "%TEMP%\stask.xml"
echo   ^</Settings^> >> "%TEMP%\stask.xml"
echo   ^<Actions Context="Author"^> >> "%TEMP%\stask.xml"
echo     ^<Exec^> >> "%TEMP%\stask.xml"
echo       ^<Command^>%CURRENT_DIR%%EXE_NAME%^</Command^> >> "%TEMP%\stask.xml"
echo       ^<WorkingDirectory^>%CURRENT_DIR%^</WorkingDirectory^> >> "%TEMP%\stask.xml"
echo     ^</Exec^> >> "%TEMP%\stask.xml"
echo   ^</Actions^> >> "%TEMP%\stask.xml"
echo ^</Task^> >> "%TEMP%\stask.xml"

:: 导入任务计划
schtasks /Create /TN "%TASK_NAME%" /XML "%TEMP%\stask.xml" /F
set TASK_RESULT=%ERRORLEVEL%

:: 清理临时文件
del "%TEMP%\stask.xml" >nul 2>&1

:: 检查任务计划是否创建成功
if %TASK_RESULT% equ 0 (
    color 0A
    echo.
    echo [成功] 任务计划创建成功！
    echo.
    echo 程序详情:
    echo   - 程序路径: %CURRENT_DIR%%EXE_NAME%
    echo   - 工作目录: %CURRENT_DIR%
    if %ADMIN_RUN% equ 1 (
        echo   - 权限级别: 管理员权限
    ) else (
        echo   - 权限级别: 普通用户权限
    )
    echo   - 启动触发: 用户登录时
    echo.
    echo 您可以通过"任务计划程序"查看或修改此任务。
) else (
    color 0C
    echo.
    echo [错误] 任务计划创建失败！错误代码: %TASK_RESULT%
    echo.
    echo 可能的原因:
    echo   - 权限不足
    echo   - 任务计划服务未运行
    echo   - XML文件格式错误
    echo.
    echo 请尝试重新运行此脚本或手动创建任务计划。
)

echo.
echo =========================================
echo.
pause
exit /b 0