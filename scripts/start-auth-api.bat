@echo off
title NiesPro Auth.API (Port 5001)
echo ========================================
echo   NiesPro Auth.API - Port 5001
echo ========================================
echo.

cd /d "C:\Users\HP\Documents\projets\NiesPro\src\Services\Auth\Auth.API"

echo Building Auth.API...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Starting Auth.API on https://localhost:5001...
echo.
dotnet run --urls "https://localhost:5001"

pause