@echo off
REM =======================================================================
REM MYSQL SIMPLE CHECKER - NiesPro
REM Script batch pour vérifier MySQL et les bases
REM =======================================================================

echo 🗄️  MYSQL SIMPLE CHECKER
echo ========================

REM Chemins possibles pour MySQL
set MYSQL_PATHS="C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" ^
    "C:\wamp64\bin\mysql\mysql8.0.31\bin\mysql.exe" ^
    "C:\xampp\mysql\bin\mysql.exe"

set MYSQL_FOUND=0
set MYSQL_EXE=

REM Rechercher MySQL
for %%P in (%MYSQL_PATHS%) do (
    if exist %%P (
        set MYSQL_EXE=%%P
        set MYSQL_FOUND=1
        echo ✅ MySQL trouvé: %%P
        goto :mysql_found
    )
)

if %MYSQL_FOUND%==0 (
    echo ❌ MySQL non trouvé dans les chemins standards
    echo 💡 Chemins recherchés:
    for %%P in (%MYSQL_PATHS%) do echo    - %%P
    echo.
    echo 💡 Solutions:
    echo    1. Installer MySQL Server
    echo    2. Installer WAMP/XAMPP
    echo    3. Utiliser phpMyAdmin: http://localhost/phpmyadmin/
    pause
    exit /b 1
)

:mysql_found
echo.

REM Test de connexion
echo 🔌 Test de connexion...
%MYSQL_EXE% -u root -e "SELECT 'Connexion OK' AS Status, NOW() AS Timestamp;" 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Échec de connexion (peut-être un mot de passe requis)
    echo 💡 Essayez: mysql -u root -p
    pause
    exit /b 1
)

echo ✅ Connexion MySQL réussie!
echo.

REM Lister toutes les bases
echo 📚 TOUTES LES BASES DE DONNÉES:
%MYSQL_EXE% -u root -e "SHOW DATABASES;" 2>nul
echo.

REM Bases NiesPro spécifiquement
echo 🎯 BASES NIESPRO:
%MYSQL_EXE% -u root -e "SHOW DATABASES;" 2>nul | findstr -i "niespro"
echo.

REM Vérifier la base Catalog spécifiquement
echo 🔍 VÉRIFICATION CATALOG:
%MYSQL_EXE% -u root -e "USE niespro_catalog_dev; SHOW TABLES;" 2>nul
if %ERRORLEVEL% EQU 0 (
    echo ✅ Base niespro_catalog_dev existe et contient des tables
    echo.
    echo 📊 NOMBRE D'ENREGISTREMENTS:
    %MYSQL_EXE% -u root niespro_catalog_dev -e "SELECT 'Categories' as Table_Name, COUNT(*) as Count FROM Categories UNION SELECT 'Products', COUNT(*) FROM Products UNION SELECT 'Brands', COUNT(*) FROM Brands;" 2>nul
) else (
    echo ❌ Base niespro_catalog_dev introuvable ou vide
    echo 💡 Exécutez: tools\catalog-db-setup.ps1 -Force
)

echo.
echo 📋 ACTIONS DISPONIBLES:
echo 1. Créer base niespro_catalog_dev
echo 2. Lancer phpMyAdmin
echo 3. Quitter
echo.
set /p choice="Votre choix: "

if "%choice%"=="1" (
    echo 🔧 Création de la base...
    %MYSQL_EXE% -u root -e "CREATE DATABASE IF NOT EXISTS niespro_catalog_dev CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
    echo ✅ Base créée! Appliquez maintenant les migrations avec:
    echo    cd src\Services\Catalog\Catalog.API
    echo    dotnet ef database update
)

if "%choice%"=="2" (
    echo 🌐 Ouverture de phpMyAdmin...
    start http://localhost/phpmyadmin/
)

echo.
echo 👋 Script terminé
pause