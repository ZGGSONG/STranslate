@echo off
rem 删除 %localappdata%\stranslate 目录及其所有内容
rd /s /q "%localappdata%\stranslate"

rem 获取批处理文件所在的目录路径
set "batchDir=%~dp0"

rd /s /q "%batchDir%"
