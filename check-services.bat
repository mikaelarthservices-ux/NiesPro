@echo off
echo === Verification rapide des services ===
powershell.exe -ExecutionPolicy Bypass -File "%~dp0quick-check.ps1"
echo.
echo Pour plus de details, utilisez: powershell -File check-services-fixed.ps1 -Detailed
pause