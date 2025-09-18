@echo off
echo ========================================
echo   NiesPro Enterprise Administration
echo   Professional Service Management v3.0
echo ========================================
echo.

REM Cleanup any existing processes
taskkill /f /im "NiesPro.ServiceAdmin.exe" 2>nul >nul

echo [INFO] Compiling modern interface...
dotnet build "tools/NiesPro.ServiceAdmin.v3/NiesPro.ServiceAdmin.v3.csproj" --verbosity quiet

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Compilation failed!
    echo [INFO] Falling back to standard interface...
    dotnet run --project "tools/NiesPro.ServiceAdmin.v3/NiesPro.ServiceAdmin.v3.csproj"
    pause
    exit /b 1
)

echo [SUCCESS] Compilation completed successfully
echo [INFO] Launching NiesPro Enterprise Administration Platform...
echo.

REM Launch the modern interface
dotnet run --project "tools/NiesPro.ServiceAdmin.v3/NiesPro.ServiceAdmin.v3.csproj"

echo.
echo [INFO] Application closed
pause