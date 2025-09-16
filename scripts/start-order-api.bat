@echo off
title NiesPro Order.API (Port 5002)
echo ========================================
echo   NiesPro Order.API - Port 5002
echo ========================================
echo.

cd /d "C:\Users\HP\Documents\projets\NiesPro\src\Services\Order\Order.API"

echo Building Order.API...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo.
echo Starting Order.API on https://localhost:5002...
echo.
dotnet run --urls "https://localhost:5002"

pause