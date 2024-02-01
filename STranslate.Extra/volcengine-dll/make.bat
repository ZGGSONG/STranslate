@echo off
go build -ldflags "-s -w" -o ..\..\STranslate.Util\volcengine.dll -buildmode=c-shared main.go