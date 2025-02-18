@echo off
setlocal enabledelayedexpansion

:: 获取当前目录
set "CURRENT_DIR=%~dp0"
set "EXE_NAME=STranslate.exe"
set "TASK_NAME=STranslate(Auto Run)"

:: 检查是否以管理员权限运行
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo 请以管理员权限运行此脚本！
    pause
    exit /b 1
)

:: 询问用户是否需要管理员权限运行程序
set /p ADMIN_RUN="是否需要以管理员权限运行程序？(Y/N): "

:: 转换输入为大写
call :ToUpper ADMIN_RUN

if "%ADMIN_RUN%"=="Y" (
    :: 创建需要管理员权限的任务计划
    schtasks /Create /TN "%TASK_NAME%" /TR "\"%CURRENT_DIR%%EXE_NAME%\"" /SC ONLOGON /RL HIGHEST /F
    if !errorLevel! equ 0 (
        echo 任务计划创建成功！
        echo 程序将在用户登录时以管理员权限启动
    ) else (
        echo 任务计划创建失败！
    )
) else (
    :: 创建普通权限的任务计划
    schtasks /Create /TN "%TASK_NAME%" /TR "\"%CURRENT_DIR%%EXE_NAME%\"" /SC ONLOGON /F
    if !errorLevel! equ 0 (
        echo 任务计划创建成功！
        echo 程序将在用户登录时以普通权限启动
    ) else (
        echo 任务计划创建失败！
    )
)

pause
exit /b 0

:ToUpper
:: 将变量转换为大写
for %%L in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do set %1=!%1:%%L=%%L!
exit /b