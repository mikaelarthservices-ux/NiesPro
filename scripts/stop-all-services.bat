@echo off
echo ========================================
echo   Arrêt de tous les services NiesPro
echo ========================================
echo.

echo Arrêt de tous les processus dotnet.exe...
taskkill /f /im dotnet.exe 2>nul

echo Vérification des ports...
netstat -an | findstr "5000\|5001\|5002\|5003" | findstr "LISTENING"

if %ERRORLEVEL% equ 0 (
    echo Certains ports sont encore utilisés.
) else (
    echo Tous les services ont été arrêtés avec succès.
)

echo.
pause