@echo off
title NiesPro Catalog.API (Port 5003)
echo ========================================
echo   NiesPro Catalog.API - Port 5003
echo ========================================
echo.

cd /d "C:\Users\HP\Documents\projets\NiesPro\src\Services\Catalog\Catalog.API"

echo Building Catalog.API...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Starting Catalog.API on https://localhost:5003...
echo.
dotnet run --urls "https://localhost:5003"

pause