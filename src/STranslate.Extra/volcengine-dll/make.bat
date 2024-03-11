@echo off
go build -ldflags "-s -w" -o volcengine.dll -buildmode=c-shared main.go

REM upx -9 -o compressed-binary original-binary
REM -9 表示使用最高压缩率进行压缩，-o 指定输出文件名，compressed-binary 是压缩后的文件名，original-binary 是原始的可执行文件名。
..\upx-4.2.2-win64\upx -9 -o ..\..\STranslate.Util\volcengine.dll volcengine.dll