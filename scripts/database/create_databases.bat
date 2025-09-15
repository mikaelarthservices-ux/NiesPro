@echo off
echo 🚀 Création des bases NiesPro avec WAMP MySQL 9.1.0...
echo.

set MYSQL_PATH=C:\wamp64\bin\mysql\mysql9.1.0\bin\mysql.exe
set SCRIPT_PATH=C:\Users\HP\Documents\projets\NiesPro\scripts\database\create_databases_wamp.sql

echo 📋 Vérification de MySQL...
if not exist "%MYSQL_PATH%" (
    echo ❌ MySQL non trouvé dans : %MYSQL_PATH%
    echo 🌐 Utilisez phpMyAdmin : http://localhost/phpmyadmin/
    pause
    exit /b 1
)

echo ✅ MySQL trouvé : %MYSQL_PATH%
echo.

echo 📄 Exécution du script SQL...
"%MYSQL_PATH%" -u root -e "source %SCRIPT_PATH%"

if %errorlevel% equ 0 (
    echo.
    echo ✅ Bases de données créées avec succès !
    echo.
    echo 🔍 Vérification des bases :
    "%MYSQL_PATH%" -u root -e "SHOW DATABASES LIKE 'NiesPro%%';"
) else (
    echo.
    echo ❌ Erreur lors de la création
    echo 🌐 Alternative : http://localhost/phpmyadmin/
)

echo.
echo 🎯 Script terminé.
pause