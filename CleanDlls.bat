@echo off

set "path_to_files=.\src\STranslate\bin\Release\net8.0-windows\publish\win-x64\"

echo Deleting ocr dlls...
cd /D "%path_to_files%"

for %%i in (
    "PaddleOCR.dll"
	"common.dll"
	"libiomp5md.dll"
	"mkldnn.dll"
	"mklml.dll"
	"opencv_world470.dll"
	"paddle_inference.dll"
	"tbb12.dll"
	"tbbmalloc.dll"
	"tbbmalloc_proxy.dll"
	"vcruntime140.dll"
	"vcruntime140_1.dll"
) do (
    del /F /Q "%%~i"
)

del /F /Q *CPU*.exe

rd /S /Q inference

echo ocr dlls deleted.
pause