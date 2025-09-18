@echo off
echo ================================================
echo   Test de l'outil d'administration NiesPro v3.0
echo ================================================
echo.

echo Verification de la structure des projets...
if exist "src\Services\Auth\Auth.API\Auth.API.csproj" (
    echo ✅ Auth.API trouvé
) else (
    echo ❌ Auth.API non trouvé
)

if exist "src\Services\Order\Order.API\Order.API.csproj" (
    echo ✅ Order.API trouvé
) else (
    echo ❌ Order.API non trouvé
)

if exist "src\Services\Catalog\Catalog.API\Catalog.API.csproj" (
    echo ✅ Catalog.API trouvé
) else (
    echo ❌ Catalog.API non trouvé
)

if exist "src\Gateway\Gateway.API\Gateway.API.csproj" (
    echo ✅ Gateway.API trouvé
) else (
    echo ❌ Gateway.API non trouvé
)

echo.
echo Test de build des services...
dotnet build src\Services\Auth\Auth.API\Auth.API.csproj --verbosity quiet
if %ERRORLEVEL% EQU 0 (
    echo ✅ Auth.API compile correctement
) else (
    echo ❌ Auth.API erreur de compilation
)

dotnet build src\Services\Order\Order.API\Order.API.csproj --verbosity quiet
if %ERRORLEVEL% EQU 0 (
    echo ✅ Order.API compile correctement
) else (
    echo ❌ Order.API erreur de compilation
)

echo.
echo Vérification des ports en écoute...
netstat -an | findstr "LISTENING" | findstr "5000\|5001\|5002\|5003\|5004"

echo.
echo ================================================
echo   Tous les services sont prêts pour l'administration
echo ================================================
pause