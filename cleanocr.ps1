Write-Host ""
Write-Host "========================================"
Write-Host "Deleting ocr dlls..."
Write-Host "========================================"

$path_to_files = "publish"
$path_to_back = "../"
Set-Location -Path $path_to_files

$dlls = @(
  "PaddleOCR.dll",
  "common.dll",
  "libiomp5md.dll",
  "mkldnn.dll",
  "mklml.dll",
  "opencv_world470.dll",
  "paddle_inference.dll",
  "tbb12.dll",
  "tbbmalloc.dll",
  "tbbmalloc_proxy.dll",
  "vcruntime140.dll",
  "vcruntime140_1.dll"
)

foreach ($dll in $dlls) {
    Remove-Item -Path $dll -Force -ErrorAction SilentlyContinue
}

Get-ChildItem *CPU*.exe | Remove-Item -Force -ErrorAction SilentlyContinue

Remove-Item -Path inference -Recurse -Force -ErrorAction SilentlyContinue

Set-Location -Path $path_to_back

Write-Host ""
Write-Host "========================================"
Write-Host "Deleted ocr dlls..."
Write-Host "========================================"
