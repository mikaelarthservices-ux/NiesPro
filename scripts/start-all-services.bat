@echo off
echo ========================================
echo   Lancement de l'architecture NiesPro
echo ========================================
echo.
echo Ce script lance tous les microservices dans des terminaux separés :
echo - Auth.API (Port 5001)
echo - Order.API (Port 5002) 
echo - Catalog.API (Port 5003)
echo - Gateway.API (Port 5000)
echo.
echo Chaque service s'executera dans sa propre fenetre de terminal.
echo.
pause

echo Lancement d'Auth.API...
start "Auth.API" cmd /k "C:\Users\HP\Documents\projets\NiesPro\scripts\start-auth-api.bat"
timeout /t 3

echo Lancement d'Order.API...
start "Order.API" cmd /k "C:\Users\HP\Documents\projets\NiesPro\scripts\start-order-api.bat"
timeout /t 3

echo Lancement de Catalog.API...
start "Catalog.API" cmd /k "C:\Users\HP\Documents\projets\NiesPro\scripts\start-catalog-api.bat"
timeout /t 3

echo Lancement du Gateway.API...
start "Gateway.API" cmd /k "C:\Users\HP\Documents\projets\NiesPro\scripts\start-gateway-api.bat"

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
echo Documentation Swagger disponible sur chaque service via /swagger
echo.
pause