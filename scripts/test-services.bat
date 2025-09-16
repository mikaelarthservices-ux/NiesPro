@echo off
echo ========================================
echo   Test de l'architecture NiesPro
echo ========================================
echo.

echo Test du Gateway API (Port 5000)...
curl -k https://localhost:5000/api/gateway/info
echo.
echo.

echo Test du Health Check Gateway...
curl -k https://localhost:5000/health
echo.
echo.

echo Test d'Auth.API (Port 5001)...
curl -k https://localhost:5001/health
echo.
echo.

echo Test d'Order.API (Port 5002)...
curl -k https://localhost:5002/health
echo.
echo.

echo Test de Catalog.API (Port 5003)...
curl -k https://localhost:5003/health
echo.
echo.

echo Test de routage Gateway vers Produits...
curl -k https://localhost:5000/api/products
echo.
echo.

echo ========================================
echo   Tests termines
echo ========================================
pause