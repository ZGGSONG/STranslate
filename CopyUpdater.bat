@echo off

echo "Start Copy Updater.exe To Publish Directory"

copy ".\src\STranslate.Updater\bin\Release\net8.0-windows\publish\win-x64\Updater.exe" ".\src\STranslate\bin\Release\net8.0-windows\publish\win-x64\Updater.exe"

echo "Copy Updater.exe To Publish Directory Successful!"

pause
exit