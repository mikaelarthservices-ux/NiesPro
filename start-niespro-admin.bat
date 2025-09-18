@echo off
echo ================================================
echo   NiesPro Service Administration Platform v3.0
echo   Professional Enterprise Service Management
echo ================================================
echo.

cd /d "C:\Users\HP\Documents\projets\NiesPro\tools\NiesPro.ServiceAdmin.v3"

echo Building NiesPro Service Administration Platform...
dotnet build --no-restore --verbosity quiet

if %ERRORLEVEL% EQU 0 (
    echo Build successful. Starting NiesPro Admin Platform...
    echo.
    dotnet run --no-build
) else (
    echo Build failed. Please check the errors above.
    pause
)