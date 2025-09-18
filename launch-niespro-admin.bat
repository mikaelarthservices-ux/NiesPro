@echo off
cls
echo ================================================
echo   NiesPro Service Administration Platform v3.0
echo   Professional Enterprise Service Management
echo ================================================
echo.
echo Launching NiesPro Service Admin Platform...
echo.

cd /d "C:\Users\HP\Documents\projets\NiesPro\tools\NiesPro.ServiceAdmin.v3"
start "" dotnet run --verbosity quiet

echo.
echo NiesPro Service Administration Platform v3.0 started!
echo The application window should open momentarily.
echo.
echo Features:
echo  - High-performance reactive architecture
echo  - Professional enterprise design
echo  - Real-time service monitoring
echo  - Clean Architecture patterns
echo.
pause