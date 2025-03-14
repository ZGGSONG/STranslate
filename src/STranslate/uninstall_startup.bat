@echo off
setlocal enabledelayedexpansion

set "TASK_NAME=STranslate(Auto Run)"

:: 检查是否以管理员权限运行
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo 请以管理员权限运行此脚本！
    pause
    exit /b 1
)

schtasks /Delete /TN "%TASK_NAME%" /F

if %errorLevel% equ 0 (
    echo 任务计划删除成功！
) else (
    echo 任务计划删除失败！
)

pause