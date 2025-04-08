@echo off
setlocal enabledelayedexpansion

:: 设置标题和颜色
title STranslate 自动启动移除工具
color 0B

:: 设置任务名称和用户信息
set "TASK_NAME=STranslate(Auto Run)"
set "CURRENT_USER=ZGGSONG"

echo =========================================
echo    STranslate 自动启动移除工具
echo    当前用户: %CURRENT_USER%
echo    当前时间: %DATE% %TIME%
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

:: 检查任务是否存在
schtasks /Query /TN "%TASK_NAME%" >nul 2>&1
if %errorLevel% neq 0 (
    color 0E
    echo [警告] 未找到名为"%TASK_NAME%"的计划任务！
    echo.
    echo 可能的原因:
    echo   - 任务计划尚未创建
    echo   - 任务名称已被更改
    echo   - 任务已被手动删除
    echo.
    choice /C YN /M "是否仍要尝试删除？(Y=是, N=否)"
    if !ERRORLEVEL! equ 2 (
        echo.
        echo 操作已取消。
        goto :end
    )
    echo.
)

echo 正在移除自动启动任务...
echo.

:: 删除任务计划
schtasks /Delete /TN "%TASK_NAME%" /F
set TASK_RESULT=%ERRORLEVEL%

:: 检查任务删除是否成功
if %TASK_RESULT% equ 0 (
    color 0A
    echo [成功] 任务计划"%TASK_NAME%"已成功移除！
    echo.
    echo STranslate 将不再在登录时自动启动。
) else (
    color 0C
    echo [错误] 任务计划删除失败！错误代码: %TASK_RESULT%
    echo.
    echo 可能的原因:
    echo   - 权限不足
    echo   - 任务计划服务未运行
    echo   - 任务正在运行且无法停止
    echo.
    echo 您可以尝试:
    echo   1. 确保以管理员身份运行此脚本
    echo   2. 在任务计划程序中手动删除该任务
    echo   3. 重启计算机后再次尝试
)

:end
echo.
echo =========================================
echo.
pause
exit /b 0