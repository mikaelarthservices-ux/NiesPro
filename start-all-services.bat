@echo off
cd /d "c:\Users\HP\Documents\projets\NiesPro"

echo ========================================
echo   Lancement des services NiesPro
echo ========================================

echo Lancement d'Auth.API...
start "Auth.API" cmd /k "dotnet run --project src/Services/Auth/Auth.API/Auth.API.csproj --urls https://localhost:5001"

timeout /t 3

echo Lancement d'Order.API...
start "Order.API" cmd /k "dotnet run --project src/Services/Order/Order.API/Order.API.csproj --urls https://localhost:5002"

timeout /t 3

echo Lancement de Catalog.API...
start "Catalog.API" cmd /k "dotnet run --project src/Services/Catalog/Catalog.API/Catalog.API.csproj --urls https://localhost:5003"

timeout /t 3

echo Lancement du Gateway.API...
start "Gateway.API" cmd /k "dotnet run --project src/Gateway/Gateway.API/Gateway.API.csproj --urls https://localhost:5000"

echo.
echo ========================================
echo   Tous les services sont en cours de démarrage
echo ========================================
echo.
echo URLs d'accès :
echo - Gateway API: https://localhost:5000
echo - Auth API: https://localhost:5001  
echo - Order API: https://localhost:5002
echo - Catalog API: https://localhost:5003
echo.