@echo off

echo Deleting ocr dlls...

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

rd /S /Q inference

echo ocr dlls deleted.
pause