# 使用7-Zip创建7z文件
& 7z a -t7z "publish.7z" ./publish/*

# 使用MicaSetup打包成exe安装包
# https://github.com/lemutec/MicaSetup/releases
& makemica micasetup.json

Write-Host ""
Write-Host "========================================"
Write-Host "Build Installer successfully."
Write-Host "========================================"