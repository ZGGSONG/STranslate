@echo off

echo Deleting ocr dlls...

set "path_to_files=.\publish\"
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