@echo off
title NiesPro - Build and Start Simple Service Manager
echo Compilation du Simple Service Manager...
echo.
cd /d "C:\Users\HP\Documents\projets\NiesPro\tools\SimpleServiceManager"
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERREUR: Compilation echouee!
    pause
    exit /b 1
)
echo.
echo Compilation reussie! Lancement du Simple Service Manager...
echo.
dotnet run
pause