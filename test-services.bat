@echo off
title NiesPro - Test Individual Services
echo Testing individual service builds...
echo.

echo Testing Gateway.API...
cd /d "C:\Users\HP\Documents\projets\NiesPro\src\Gateway\Gateway.API"
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Gateway.API build failed!
    pause
    exit /b 1
)
echo Gateway.API build: OK
echo.

echo Testing Auth.API...
cd /d "C:\Users\HP\Documents\projets\NiesPro\src\Services\Auth\Auth.API"
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Auth.API build failed!
    pause
    exit /b 1
)
echo Auth.API build: OK
echo.

echo Testing Order.API...
cd /d "C:\Users\HP\Documents\projets\NiesPro\src\Services\Order\Order.API"
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Order.API build failed!
    pause
    exit /b 1
)
echo Order.API build: OK
echo.

echo Testing Catalog.API...
cd /d "C:\Users\HP\Documents\projets\NiesPro\src\Services\Catalog\Catalog.API"
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Catalog.API build failed!
    pause
    exit /b 1
)
echo Catalog.API build: OK
echo.

echo All services build successfully!
echo Now starting Gateway.API for 10 seconds...
cd /d "C:\Users\HP\Documents\projets\NiesPro\src\Gateway\Gateway.API"

timeout /t 3 /nobreak
start /b dotnet run
echo Gateway starting... waiting 10 seconds
timeout /t 10 /nobreak

echo Stopping all dotnet processes...
taskkill /f /im dotnet.exe > nul 2>&1

echo Test completed!
pause