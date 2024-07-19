# 关闭回显
$ErrorActionPreference = "SilentlyContinue"

$main = "src/STranslate/STranslate.csproj"
$updater = "src/STranslate.Updater/STranslate.Updater.csproj"
$mainProfile = "src/STranslate/Properties/PublishProfiles/FolderProfile.pubxml"
$updaterProfile = "src/STranslate.Updater/Properties/PublishProfiles/FolderProfile.pubxml"

# 检查并删除发布目录
function CheckAndDeletePublishDir($publishDir) {
    if (Test-Path $publishDir) {
		Write-Host ""
		Write-Host "[Cache] Starting clear."
		Write-Host "========================================"
		Write-Host "[Cache] Deleting existing publish directory $publishDir..."
		Write-Host "========================================"
        try {
            Remove-Item -Path $publishDir -Recurse -Force
			Write-Host ""
			Write-Host "========================================"
            Write-Host "[Cache] Deleted $publishDir successfully."
			Write-Host "========================================"
        } catch {
			Write-Host ""
			Write-Host "========================================"
            Write-Host "[Cache] Failed to delete $publishDir. Stopping script."
			Write-Host "========================================"
            exit
        }
    } else {
		Write-Host ""
		Write-Host "========================================"
		Write-Host "[Cache] $publishDir not existing."
		Write-Host "========================================"
	}
}

# 将相对发布目录路径转换为绝对路径
$publishDirAbsolute = [System.IO.Path]::GetFullPath("publish")

CheckAndDeletePublishDir $publishDirAbsolute

Write-Host ""
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