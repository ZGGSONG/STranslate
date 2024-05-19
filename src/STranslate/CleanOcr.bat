@echo off

echo Deleting ocr dlls...

for %%i in (
    "common.dll"
    "concrt140.dll"
    "libiomp5md.dll"
    "mfc140.dll"
    "mfcm140.dll"
    "mkldnn.dll"
    "mklml.dll"
    "msvcp140.dll"
    "msvcp140_1.dll"
    "msvcp140_2.dll"
    "msvcp140_atomic_wait.dll"
    "msvcp140_codecvt_ids.dll"
    "opencv_world470.dll"
    "PaddleOCR.dll"
    "paddle_inference.dll"
    "vcamp140.dll"
    "vccorlib140.dll"
    "vcomp140.dll"
    "vcruntime140.dll"
    "vcruntime140_1.dll"
) do (
    del /F /Q "%%~i"
)

rd /S /Q inference

echo ocr dlls deleted.
pause