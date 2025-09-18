@echo off
echo ========================================
echo    NiesPro Service Administration v3
echo ========================================
echo.
echo Compilation de l'application...
dotnet build "tools/NiesPro.ServiceAdmin.v3/NiesPro.ServiceAdmin.v3.csproj" --verbosity quiet
if %ERRORLEVEL% NEQ 0 (
    echo Erreur lors de la compilation!
    pause
    exit /b 1
)

echo.
echo Lancement de l'application...
dotnet run --project "tools/NiesPro.ServiceAdmin.v3/NiesPro.ServiceAdmin.v3.csproj"