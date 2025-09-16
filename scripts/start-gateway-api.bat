@echo off
title NiesPro Gateway.API (Port 5000)
echo ========================================
echo   NiesPro Gateway.API - Port 5000
echo ========================================
echo.

cd /d "C:\Users\HP\Documents\projets\NiesPro\src\Gateway\Gateway.API"

echo Building Gateway.API...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Starting Gateway.API on https://localhost:5000...
echo.
dotnet run --urls "https://localhost:5000"

pause