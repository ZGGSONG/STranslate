# 关闭回显
$ErrorActionPreference = "SilentlyContinue"

$main = "src/STranslate/STranslate.csproj"
$updater = "src/STranslate.Updater/STranslate.Updater.csproj"
$mainProfile = "src/STranslate/Properties/PublishProfiles/FolderProfile.pubxml"
$updaterProfile = "src/STranslate.Updater/Properties/PublishProfiles/FolderProfile.pubxml"

Write-Host "Publishing started."
Write-Host "========================================"
Write-Host "Publishing $main..."
Write-Host "========================================"
dotnet publish $main -c Release -p:PublishProfile=$mainProfile

Write-Host ""
Write-Host "========================================"
Write-Host "Publishing $updater..."
Write-Host "========================================"
dotnet publish $updater -c Release -p:PublishProfile=$updaterProfile

Write-Host ""
Write-Host "========================================"
Write-Host "Publishing completed."
Write-Host "========================================"
# 暂停
# Read-Host -Prompt "Press Enter to continue"