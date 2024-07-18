# PowerShell版本压缩文件并计算SHA256哈希值

param(
    [string]$version
)

if ([string]::IsNullOrEmpty($version)) {
    Write-Error "Version argument is required."
    exit
}

# 使用7-Zip创建ZIP文件
& 7z a -tzip "STranslate_${version}_win-x64.zip" ./publish/*

# 使用7-Zip创建7z文件
& 7z a -t7z "STranslate_${version}_win-x64_7z.7z" ./publish/*

# 使用7-Zip计算SHA256哈希值
& 7z h -scrcsha256 "STranslate_${version}_win-x64.zip" "STranslate_${version}_win-x64_7z.7z" | Out-File "STranslate_${version}_win-x64_sha256.txt"

Write-Host ""
Write-Host "========================================"
Write-Host "Compress the file and calculate the SHA256 hash successfully."
Write-Host "========================================"