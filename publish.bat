@echo off

set main=src\STranslate\STranslate.csproj
set updater=src\STranslate.Updater\STranslate.Updater.csproj
set mainProfile=src\STranslate\Properties\PublishProfiles\FolderProfile.pubxml
set updaterProfile=src\STranslate.Updater\Properties\PublishProfiles\FolderProfile.pubxml

echo Publishing started.
echo ========================================
echo Publishing %main%...
echo ========================================
dotnet publish %main% -c Release -o Publish -p:PublishProfile=%mainProfile%

echo.
echo ========================================
echo Publishing %updater%...
echo ========================================
dotnet publish %updater% -c Release -o Publish -p:PublishProfile=%updaterProfile%

echo.
echo ========================================
echo Publishing completed.
echo ========================================
pause